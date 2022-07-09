﻿using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TTT;

[GameResource( "Role", "role", "TTT role template.", Icon = "🎭" )]
public class RoleInfo : GameResource
{
	public Team Team { get; set; } = Team.None;

	[Category( "UI" )]
	public Color Color { get; set; }

	[Title( "Icon" ), Category( "UI" ), ResourceType( "png" )]
	public string IconPath { get; set; } = "ui/none.png";

	[Category( "Shop" )]
	[Description( "The amount of credits the player spawns with." )]
	public int DefaultCredits { get; set; } = 0;

	[Category( "Shop" ), ResourceType( "weapon" )]
	public List<string> Weapons { get; set; } = new();

	[Category( "Shop" ), ResourceType( "carri" )]
	public List<string> Carriables { get; set; } = new();

	[Category( "Shop" ), ResourceType( "perk" )]
	public List<string> Perks { get; set; } = new();

	public bool CanRetrieveCredits { get; set; } = false;

	public bool CanTeamChat { get; set; } = false;

	public bool CanAttachCorpses { get; set; } = false;

	public KarmaConfig Karma { get; set; }

	public ScoringConfig Scoring { get; set; }

	[HideInEditor]
	[JsonIgnore]
	public HashSet<ItemInfo> ShopItems { get; private set; } = new();

	[HideInEditor]
	[JsonIgnore]
	public Texture Icon { get; private set; }

	protected override void PostLoad()
	{
		base.PostLoad();

		if ( ResourceLibrary is null )
			return;

		var itemPaths = Weapons.Concat( Carriables ).Concat( Perks );
		foreach ( var itemPath in itemPaths )
		{
			var itemInfo = ResourceLibrary.Get<ItemInfo>( itemPath );
			if ( itemInfo is null )
				continue;

			ShopItems.Add( itemInfo );
		}

		if ( Host.IsClient )
			Icon = Texture.Load( FileSystem.Mounted, GetPNGPath( IconPath ) );
	}

	public struct KarmaConfig
	{
		[Description( "This gets calculated like the attacker dealing damage to a player with this role." )]
		[Property]
		public int AttackerKillReward { get; set; } = 0;

		[Description( "This gets calculated like the attacker dealing damage to a teammate." )]
		[Property]
		public int TeamKillPenalty { get; set; } = 0;

		[Description( "This gets multiplied with the damage dealt to a player with this role to calculate the hurt reward for the enemy attacker." )]
		[Property]
		public float AttackerHurtRewardMultiplier { get; set; } = 0;

		[Description( "This gets multiplied with the damage dealt to a teammate to calculate the hurt penalty." )]
		[Property]
		public float TeamHurtPenaltyMultiplier { get; set; } = 0;

		public KarmaConfig() { }
	}

	public struct ScoringConfig
	{
		[Description( "The amount of score points rewarded for confirming a corpse." )]
		[Property]
		public int CorpseFoundReward { get; set; } = 0;

		[Description( "The amount of score points rewarded for killing a player with this role." )]
		[Property]
		public int AttackerKillReward { get; set; } = 0;

		[Description( "The amount of score points penalized for killing a player on the same team." )]
		[Property]
		public int TeamKillPenalty { get; set; } = 0;

		[Description( "The amount of score points rewarded for surviving the round." )]
		[Property]
		public int SurviveBonus { get; set; } = 0;

		[Description( "The amount of score points penalized for commiting suicide." )]
		[Property]
		public int SuicidePenalty { get; set; } = 0;

		public ScoringConfig() { }
	}
}
