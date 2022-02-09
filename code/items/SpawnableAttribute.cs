using System;

namespace TTT.Items
{

	/// <summary>
	/// Prevents this Entity to be spawned from a TTTWeaponRandom
	/// </summary>
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
	public class SpawnableAttribute : Attribute
	{

	}
}
