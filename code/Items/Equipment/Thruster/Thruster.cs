using Sandbox;

namespace TTT;

[Category( "Equipment" )]
[ClassName( "ttt_equipment_thrustermine" )]
[Title( "Thruster Mine" )]
public partial class ThrusterMine : Deployable<ThrusterMineEntity>
{
	[Net, Local]
	public int RemainingMines { get; protected set; } = 5;
	public override string SlotText => $"{RemainingMines}";

	protected override bool CanDrop => false;

	protected override ThrusterMineEntity Deploy( TraceResult trace )
	{
		var dropped = new ThrusterMineEntity
		{
			Owner = this,
			PhysicsEnabled = true,
			Position = trace.EndPosition,
			Rotation = Rotation.From( trace.Normal.EulerAngles ),
			Velocity = 0,
			Parent = trace.Entity
		};

		RemainingMines -= 1;

		if ( RemainingMines <= 0 )
			Delete();

		return dropped;
	}

	protected override bool IsPlacementValid( TraceResult trace )
	{
		return trace.Body.BodyType != PhysicsBodyType.Static;
	}
}
