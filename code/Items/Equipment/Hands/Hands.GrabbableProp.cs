using Sandbox;
using System.Threading.Tasks;

namespace TTT;

public class GrabbableProp : IGrabbable
{
	public const float THROW_FORCE = 500;
	public ModelEntity GrabbedEntity { get; set; }
	public Player _owner;

	public bool IsHolding => GrabbedEntity is not null || _isThrowing;
	private bool _isThrowing = false; // Needed to maintain the Holding animation.

	public GrabbableProp( Player player, ModelEntity ent, Hands hands )
	{
		_owner = player;

		GrabbedEntity = ent;
		GrabbedEntity.EnableTouch = false;
		GrabbedEntity.EnableHideInFirstPerson = false;
		GrabbedEntity.SetParent( hands, Hands.MIDDLE_HANDS_ATTACHMENT, new Transform( Vector3.Zero, Rotation.FromRoll( -90 ) ) );
	}

	public void Drop()
	{
		if ( GrabbedEntity.IsValid() )
		{
			GrabbedEntity.EnableTouch = true;
			GrabbedEntity.EnableHideInFirstPerson = true;
			GrabbedEntity.SetParent( null );
		}

		GrabbedEntity = null;
	}

	public void Update( Player player )
	{
		if ( !GrabbedEntity.IsValid() || !_owner.IsValid() )
		{
			Drop();
			return;
		}
	}

	public void SecondaryAction()
	{
		_isThrowing = true;
		_owner.SetAnimParameter( "b_attack", true );

		if ( GrabbedEntity.IsValid() )
			GrabbedEntity.Velocity += _owner.EyeRotation.Forward * THROW_FORCE;
		Drop();

		_ = WaitForAnimationFinish();
	}

	private async Task WaitForAnimationFinish()
	{
		await GameTask.DelaySeconds( 0.6f );
		_isThrowing = false;
	}
}
