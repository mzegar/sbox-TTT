using System;
using Sandbox;

using TTT.Player;

namespace TTT.Items
{
	[Library( "ttt_equipment_deathstation", Title = "Death Station" )]
	[Buyable( Price = 100 )]
	[Hammer.Skip]
	public partial class DeathStation : BaseCarriable, ICarriableItem
	{
		public override string ViewModelPath => "";
		public SlotType SlotType => SlotType.UtilityEquipment;
		public Type DroppedType => typeof( DeathstationEntity );
		private readonly ItemData _data = new( typeof( DeathStation ) );

		public override void Spawn()
		{
			base.Spawn();

			RenderColor = Color.Transparent;
		}

		public override void Simulate( Client client )
		{
			if ( Owner is not TTTPlayer owner || !IsServer )
			{
				return;
			}

			using ( Prediction.Off() )
			{
				if ( Input.Pressed( InputButton.Attack1 ) )
				{
					owner.Inventory.DropEntity( this, DroppedType );
				}
			}
		}

		public ItemData GetItemData() { return _data; }
		public bool CanDrop() { return false; }
	}
}
