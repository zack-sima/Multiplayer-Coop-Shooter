using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The base class for all objects that are spawned in at runtime/can be destroyed (e.g. having HP)
/// </summary>

public class Entity : MonoBehaviour {

	#region Type Declarations

	//TODO: implement other two types of healthbars
	public enum HealthBarType { AlwaysShow, HideWhenFull, AlwaysHide };

	#endregion

	#region Prefabs

	[SerializeField] private GameObject explosionPrefab;

	#endregion

	#region References

	[SerializeField] private NetworkedEntity networker;
	public NetworkedEntity GetNetworker() { return networker; }
	[SerializeField] private RectTransform healthBarRect;
	[SerializeField] private RectTransform healthCanvas;
	[SerializeField] private TeamMaterialManager teamMaterials;
	public TeamMaterialManager GetTeamMaterials() { return teamMaterials; }

	#endregion

	#region Members

	//TODO: set in controller
	[SerializeField] private bool isPlayer;
	public bool GetIsPlayer() { return isPlayer; }

	//set in inspector; TODO: change via upgrades, etc & set at init function
	[SerializeField] private float maxHealth;
	public float GetMaxHealth() { return maxHealth; }

	[SerializeField] private HealthBarType healthBarType;

	//NOTE: if networked, this field is not used
	private float localHealth;

	//for autohealing
	private float lastDamageTimestamp = 0f;
	public float GetLastDamageTimestamp() { return lastDamageTimestamp; }

	#endregion

	#region Callback Functions

	public int GetTeam() {
		return networker.GetTeam();
	}
	public virtual void SetTeam(int newTeam) {
		if (newTeam == EntityController.player.GetTeam()) {
			healthBarRect.GetComponent<Image>().color = Color.green;
		} else {
			healthBarRect.GetComponent<Image>().color = Color.red;
		}
	}
	public float GetHealth() {
		return networker.GetHealth();
	}
	private void EntityDied() {
		//TODO: handle networking despawning here: add networked despawn & a "onstopserver" to
		//remove self from list of entities
		Debug.Log("entity has been killed");

		if (this == EntityController.player) {
			//TODO: if entity is a player (need marking), use separate system of respawns
			//  and hiding of canvas/hull
			Debug.Log("player needs to respawn/be hidden right now");
			return;
		}

		Instantiate(explosionPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);

		RemoveEntityFromRegistry();

		Destroy(gameObject);
	}
	public void UpdateHealthBar() {
		healthBarRect.localScale = new Vector2(GetHealth() / maxHealth, 1f);
	}

	#endregion

	#region Functions

	public void PreventHealing() {
		lastDamageTimestamp = Time.time;
	}
	public void LostHealth() {
		UpdateHealthBar();
		PreventHealing();

		if (GetHealth() <= 0) EntityDied();
	}
	//adds to/removes from staticEntities list in EntitiesController (combat overrides these functions)
	public virtual void AddEntityToRegistry() {
		EntityController.instance.AddToStaticEntities(this);
	}
	public virtual void RemoveEntityFromRegistry() {
		EntityController.instance.RemoveFromStaticEntities(this);
	}
	protected virtual void Awake() {
		healthCanvas.rotation = Camera.main.transform.rotation;
	}
	protected virtual void Start() {
		UpdateHealthBar();
	}
	protected virtual void Update() {

	}

	#endregion

}
