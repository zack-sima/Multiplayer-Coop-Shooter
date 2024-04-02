using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps a catalog of all entities (players, AI, etc) that exist in the game
/// </summary>

public class EntityController : MonoBehaviour {

	#region Statics & Consts

	public static EntityController instance;
	public static CombatEntity player; //TODO: spawn player here

	//TODO: only set this to true once game loading is done in multiplayer
	public static bool gameInitialized = true;

	#endregion

	#region References

	//TODO: replace player reference with spawning player prefab
	[SerializeField] private CombatEntity playerRef;

	#endregion

	#region Members

	//TODO: procedurally spawn EVERYTHING, including player
	private List<CombatEntity> combatEntities = new();
	public List<CombatEntity> GetCombatEntities() { return combatEntities; }

	private List<Entity> staticEntities = new();
	public List<Entity> GetStaticEntities() { return staticEntities; }

	#endregion

	#region Functions

	//entities add/remove from entity lists automatically
	public void AddToStaticEntities(Entity e) { staticEntities.Add(e); }
	public void RemoveFromStaticEntities(Entity e) { staticEntities.Remove(e); }
	public void AddToCombatEntities(CombatEntity e) { combatEntities.Add(e); }
	public void RemoveFromCombatEntities(CombatEntity e) { combatEntities.Remove(e); }

	private void Awake() {
		instance = this;

		//TODO: spawn player here
		player = playerRef;
	}

	#endregion

}
