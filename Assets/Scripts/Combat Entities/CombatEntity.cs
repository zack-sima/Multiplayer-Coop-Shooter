using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatEntity : Entity {
	//TODO: when spawning player/enemies, etc, player spawns a combat entity shell and spawns
	//specific hull/turret prefabs to populate it; separate all animators from shell structure

	#region References

	[SerializeField] private Hull hull; //for movement
	public Hull GetHull() { return hull; }

	[SerializeField] private Turret turret; //for combat
	public Turret GetTurret() { return turret; }

	#endregion

	#region Functions

	public override void AddEntityToRegistry() {
		EntitiesController.instance.AddToCombatEntities(this);
	}
	public override void RemoveEntityFromRegistry() {
		EntitiesController.instance.RemoveFromCombatEntities(this);
	}

	#endregion

}
