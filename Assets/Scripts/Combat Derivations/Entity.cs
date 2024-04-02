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

	[SerializeField] private RectTransform healthBarRect;
	[SerializeField] private RectTransform healthCanvas;

	#endregion

	#region Members

	//determines whether bullets pass by, health bar color, etc;
	//TODO: set at init function
	//TODO: make team -1 be for neutral (explosion barrels, etc)
	[SerializeField] private int team;
	public int GetTeam() { return team; }

	//TODO: set in controller
	[SerializeField] private bool isPlayer;
	public bool GetIsPlayer() { return isPlayer; }

	//set in inspector; TODO: change via upgrades, etc & set at init function
	[SerializeField] private float maxHealth;

	[SerializeField] private HealthBarType healthBarType;

	//set at awake
	private float health;

	//for autohealing
	private float lastDamageTimestamp = 0f;

	#endregion

	#region Callback Functions

	private void EntityDied() {
		//TODO: handle networking despawning here
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
	private void UpdateHealthBar() {
		healthBarRect.localScale = new Vector2(health / maxHealth, 1f);

		if (GetTeam() == EntityController.player.GetTeam()) {
			healthBarRect.GetComponent<Image>().color = Color.green;
		} else {
			healthBarRect.GetComponent<Image>().color = Color.red;
		}
	}

	#endregion

	#region Functions

	public void PreventHealing() {
		lastDamageTimestamp = Time.time;
	}
	public void LoseHealth(float damage) {
		health = Mathf.Max(0f, health - damage);

		UpdateHealthBar();
		PreventHealing();

		if (health <= 0) EntityDied();
	}

	public void Init(int team) {
		//TODO: initialize health, etc here; called by EntitiesController or whatever spawned this entity
		this.team = team;
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
		health = maxHealth;
	}
	protected virtual void Start() {
		UpdateHealthBar();
	}
	protected virtual void Update() {
		if (isPlayer && health < maxHealth && Time.time - lastDamageTimestamp > 3.5f) {
			health = Mathf.Min(maxHealth, health + Time.deltaTime * maxHealth / 12f);
			UpdateHealthBar();
		}
	}

	#endregion

}
