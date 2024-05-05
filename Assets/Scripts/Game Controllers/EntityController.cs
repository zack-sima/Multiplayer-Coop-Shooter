using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps a catalog of all entities (players, AI, etc) that exist in the game
/// </summary>

public class EntityController : MonoBehaviour {

	#region Statics & Consts

	public static EntityController instance;
	public static CombatEntity player;

	//TODO: only set this to true once game loading is done in multiplayer
	public static bool gameInitialized = true;

	#endregion

	#region References

	#endregion

	#region Members

	private readonly List<CombatEntity> combatEntities = new();
	public List<CombatEntity> GetCombatEntities() { return combatEntities; }

	private readonly List<Entity> staticEntities = new();
	public List<Entity> GetStaticEntities() { return staticEntities; }

	#endregion

	#region Functions

	//entities add/remove from entity lists automatically
	public void AddToStaticEntities(Entity e) { staticEntities.Add(e); }
	public void RemoveFromStaticEntities(Entity e) {
		if (staticEntities.Contains(e)) staticEntities.Remove(e);
	}
	public void AddToCombatEntities(CombatEntity e) { combatEntities.Add(e); }
	public void RemoveFromCombatEntities(CombatEntity e) {
		if (combatEntities.Contains(e)) combatEntities.Remove(e);
	}

	private void Awake() {
		instance = this;

		Application.targetFrameRate = 90;
	}

	#endregion

}

