using Sandbox;
using System;
using System.Collections.Generic;

namespace TTT;

public partial class InProgressRound : BaseRound
{
	public List<Player> AlivePlayers { get; set; }
	public List<Player> Spectators { get; set; }

	public int InnocentTeamCount { get; set; }
	private int InnocentTeamDeathCount { get; set; }

	public int TraitorTeamCount { get; set; }

	/// <summary>
	/// Unique case where InProgressRound has a seperate fake timer for Innocents.
	/// The real timer is only displayed to Traitors as it increments every player death during the round.
	/// </summary>
	[Net]
	public TimeUntil FakeTime { get; private set; }
	public string FakeTimeFormatted => FakeTime.Relative.TimerString();

	public override string RoundName => "In Progress";
	public override int RoundDuration => Game.InProgressRoundTime;

	private readonly List<RoleButton> _logicButtons = new();

	public override void OnPlayerKilled( Player player )
	{
		base.OnPlayerKilled( player );

		TimeLeft += Game.InProgressSecondsPerDeath;


		if ( player.Team is Team.Innocents )
			InnocentTeamDeathCount += 1;

		var percentDead = (float)InnocentTeamDeathCount / InnocentTeamCount;
		if ( percentDead >= Game.CreditsAwardPercentage )
		{
			GivePlayersCredits( new Traitor(), Game.CreditsAwarded );
			InnocentTeamDeathCount = 0;
		}

		if ( player.Role is Traitor )
			GivePlayersCredits( new Detective(), Game.DetectiveTraitorDeathReward );
		else if ( player.Role is Detective && player.LastAttacker is Player p && p.IsAlive() && p.Team == Team.Traitors )
			GiveTraitorCredits( p );

		AlivePlayers.Remove( player );
		Spectators.Add( player );

		Karma.OnPlayerKilled( player );
		player.UpdateMissingInAction();
		ChangeRoundIfOver();
	}

	public override void OnPlayerJoin( Player player )
	{
		base.OnPlayerJoin( player );

		Spectators.Add( player );
		SyncPlayer( player );
	}

	public override void OnPlayerLeave( Player player )
	{
		base.OnPlayerLeave( player );

		AlivePlayers.Remove( player );
		Spectators.Remove( player );

		ChangeRoundIfOver();
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( Host.IsClient && Local.Pawn is Player localPlayer )
		{
			UI.InfoFeed.Instance?.AddEntry( "Roles have been selected and the round has begun..." );
			UI.InfoFeed.Instance?.AddEntry( $"Traitors will receive an additional {Game.InProgressSecondsPerDeath} seconds per death." );

			float karma = MathF.Round( localPlayer.BaseKarma );
			UI.InfoFeed.Instance?.AddEntry( karma >= 1000 ?
											$"Your karma is {karma}, so you'll deal full damage this round." :
											$"Your karma is {karma}, so you'll deal reduced damage this round." );

			return;
		}

		FakeTime = TimeLeft;

		// For now, if the RandomWeaponCount of the map is zero, let's just give the players
		// a fixed weapon loadout.
		if ( MapHandler.RandomWeaponCount == 0 )
		{
			foreach ( var player in AlivePlayers )
			{
				GiveFixedLoadout( player );
			}
		}

		foreach ( var ent in Entity.All )
		{
			if ( ent is RoleButton button )
				_logicButtons.Add( button );
			else if ( ent is Corpse corpse )
				corpse.Delete();
		}
	}

	private void GiveFixedLoadout( Player player )
	{
		if ( player.Inventory.Add( new MP5() ) )
			player.GiveAmmo( AmmoType.PistolSMG, 120 );

		if ( player.Inventory.Add( new Revolver() ) )
			player.GiveAmmo( AmmoType.Magnum, 20 );
	}

	protected override void OnTimeUp()
	{
		base.OnTimeUp();

		LoadPostRound( Team.Innocents );
	}

	private Team IsRoundOver()
	{
		List<Team> aliveTeams = new();

		foreach ( var player in AlivePlayers )
		{
			if ( !aliveTeams.Contains( player.Team ) )
				aliveTeams.Add( player.Team );
		}

		if ( aliveTeams.Count == 0 )
			return Team.None;

		return aliveTeams.Count == 1 ? aliveTeams[0] : Team.None;
	}

	public void LoadPostRound( Team winningTeam )
	{
		Game.Current.TotalRoundsPlayed++;
		Game.Current.ForceRoundChange( new PostRound() );

		UI.PostRoundMenu.DisplayWinner( winningTeam );
	}

	public override void OnSecond()
	{
		if ( !Host.IsServer )
			return;

		if ( Game.PreventWin )
			TimeLeft += 1f;

		if ( TimeLeft )
			OnTimeUp();

		_logicButtons.ForEach( x => x.OnSecond() ); // Tick role button delay timer.

		if ( !Utils.HasMinimumPlayers() && IsRoundOver() == Team.None )
			Game.Current.ForceRoundChange( new WaitingRound() );
	}

	private bool ChangeRoundIfOver()
	{
		var result = IsRoundOver();

		if ( result != Team.None && !Game.PreventWin )
		{
			LoadPostRound( result );
			return true;
		}

		return false;
	}

	private void GivePlayersCredits( BaseRole role, int credits )
	{
		var clients = Utils.GetAliveClientsWithRole( role );

		clients.ForEach( ( cl ) =>
		{
			if ( cl.Pawn is Player p )
				p.Credits += credits;
		} );
		UI.InfoFeed.DisplayRoleEntry
		(
			To.Multiple( clients ),
			Asset.GetInfo<RoleInfo>( role.Title ),
			$"You have been awarded {credits} credits for your performance."
		);
	}

	private void GiveTraitorCredits( Player traitor )
	{
		traitor.Credits += Game.TraitorDetectiveKillReward;
		UI.InfoFeed.DisplayClientEntry( To.Single( traitor.Client ), $"have received {Game.TraitorDetectiveKillReward} credits for killing a Detective" );
	}

	[TTTEvent.Player.RoleChanged]
	private static void OnPlayerRoleChange( Player player, BaseRole oldRole )
	{
		if ( Host.IsClient )
			return;

		if ( Game.Current.Round is InProgressRound inProgressRound )
			inProgressRound.ChangeRoundIfOver();
	}
}
