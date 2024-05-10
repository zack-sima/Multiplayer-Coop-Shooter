using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

/// <summary>
/// The base class for all objects that are spawned in at runtime/can be destroyed (e.g. having HP)
/// </summary>

public class Entity : MonoBehaviour {

	#region Type Declarations & Consts

	//TODO: implement other two types of healthbars
	public enum HealthBarType { AlwaysShow, HideWhenFull, AlwaysHide };

	public const float COOP_RESPAWN_TIME = 15f;
	public const float PVP_RESPAWN_TIME = 10f;

	#endregion

	#region Prefabs

	[SerializeField] private GameObject explosionPrefab;

	#endregion

	#region References

	[SerializeField] private NetworkedEntity networker;
	public NetworkedEntity GetNetworker() { return networker; }

	[SerializeField] private HealthCanvas healthCanvas;
	public HealthCanvas GetHealthCanvas() { return healthCanvas; }

	//if a player is non-local, switch to fallback
	[SerializeField] private HealthCanvas fallbackHealthCanvas;
	protected void SetHealthCanvasToFallback() {
		healthCanvas.gameObject.SetActive(false);
		fallbackHealthCanvas.gameObject.SetActive(true);
		fallbackHealthCanvas.transform.rotation = healthCanvas.transform.rotation;
		healthCanvas = fallbackHealthCanvas;
	}

	[SerializeField] private TeamMaterialManager teamMaterials;
	public TeamMaterialManager GetTeamMaterials() { return teamMaterials; }

	[SerializeField] private Collider hitbox;

	#endregion

	#region Members

	[SerializeField] private bool isPlayer;
	public bool GetIsPlayer() { return isPlayer; }

	//set in inspector; TODO: change via upgrades, etc & set at init function
	[SerializeField] private float maxHealth;
	private float baseHealth;
	public float GetBaseHealth() { return baseHealth; }
	public float GetMaxHealth() { return maxHealth; }
	public void SetMaxHealth(float maxHealth) { this.maxHealth = maxHealth; }

	[SerializeField] private HealthBarType healthBarType;

	//for autohealing
	private float lastDamageTimestamp = 0f;
	public float GetLastDamageTimestamp() { return lastDamageTimestamp; }

	//kill cost (enemies)
	[SerializeField] private int killReward;
	public int GetKillReward() { return killReward; }

	#endregion

	#region Callback Functions

	public int GetTeam() {
		return networker.GetTeam();
	}
	public virtual void SetTeam(int newTeam) {
		if (EntityController.player == null) return;
		if (newTeam == EntityController.player.GetTeam()) {
			healthCanvas.GetHealthBar().GetComponent<Image>().color = Color.green;
		} else {
			healthCanvas.GetHealthBar().GetComponent<Image>().color = Color.red;
		}
	}
	public float GetHealth() {
		return networker.GetHealth();
	}
	//invoked on local player or master client enemy
	private void EntityDied() {
		//PvP give other team score
		if (PlayerInfo.GetIsPVP()) {
			GameStatsSyncer.instance.AddTeamScore((GetTeam() + 1) % 2, 1);
		}
		//respawn player/cause game over
		if (this == EntityController.player || PlayerInfo.GetIsPVP()) {
			if (networker.GetIsDead()) return;

			networker.EntityDied();
			SetEntityToDead();

			if (isPlayer) {
				StartCoroutine(PlayerRespawnTimer());
			} else {
				//AI disable pathfinding
				if (TryGetComponent(out AINavigator nav)) nav.SetStopped(true);

				StartCoroutine(BotRespawnTimer());
			}
			return;
		} else {
			networker.EntityDied();

			//add score
			if (GameStatsSyncer.instance != null && !isPlayer) {
				GameStatsSyncer.instance.AddScore(killReward);
			}
		}
		//remove self from list of entities
		networker.DeleteEntity();
	}
	//for respawnable entities (now: only players)
	public void SetEntityToDead() {
		if (!networker.GetLocalIsDead())
			SpawnExplosion();
		DisableEntity();
	}
	public virtual void DisableEntity() {
		healthCanvas.gameObject.SetActive(false);
		hitbox.enabled = false;
	}
	public virtual void RespawnEntity() {
		healthCanvas.gameObject.SetActive(true);
		hitbox.enabled = true;
	}
	//called by networked entity right before destroying object
	public virtual void EntityRemoved() {
		if (!networker.GetLocalIsDead())
			SpawnExplosion();
	}
	protected void SpawnExplosion() {
		if (ServerLinker.instance != null && ServerLinker.instance.GetGameIsStopped()) return;
		Instantiate(explosionPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
	}
	public void UpdateHealthBar() {
		healthCanvas.GetHealthBar().localScale = new Vector2(GetHealth() / maxHealth, 1f);
	}
	public void UpdateMaxHealth(float maxHealth) {
		this.maxHealth = maxHealth;
	}

	#endregion

	#region Functions

	public void PreventHealing() {
		lastDamageTimestamp = Time.time;
	}
	//invoked on local player by networker
	public void LostHealth() {
		UpdateHealthBar();
		PreventHealing();

		if (GetHealth() <= 0) EntityDied();
	}
	public static float GetRespawnTime() {
		return PlayerInfo.GetIsPVP() ? PVP_RESPAWN_TIME : COOP_RESPAWN_TIME;
	}
	//NOTE: similar to player, but without the UI calls
	private IEnumerator BotRespawnTimer() {
		yield return new WaitForSeconds(GetRespawnTime());

		PVPRespawn();
	}
	//local timer to respawn player
	private IEnumerator PlayerRespawnTimer() {
		UIController.instance.SetRespawnUIEnabled(true);
		for (float i = GetRespawnTime(); i > 0f; i -= Time.deltaTime) {
			//if game is over, don't respawn
			if (GameStatsSyncer.instance.GetGameOver()) yield break;

			//UIController screen
			UIController.instance.SetRespawnTimerText($"Respawn in:\n{Mathf.CeilToInt(i)}");
			yield return null;
		}
		UIController.instance.SetRespawnUIEnabled(false);

		if (!PlayerInfo.GetIsPVP()) {
			CoopRespawn();
		} else {
			PVPRespawn();
		}
	}
	private void PVPRespawn() {
		Vector3 spawnpoint = MapController.instance.GetTeamSpawnpoint(GetTeam());
		transform.position = spawnpoint;

		//AI reactivate pathfinding
		if (TryGetComponent(out AINavigator nav)) {
			nav.SetStopped(false);
			nav.TeleportTo(spawnpoint);
		}
		RespawnEntity();
		networker.EntityRespawned();
	}
	private void CoopRespawn() {
		//if single player/only player, spawn at spawnpoint; otherwise, spawn near first other player
		bool foundOtherPlayer = false;
		foreach (CombatEntity e in EntityController.instance.GetCombatEntities()) {
			if (e == null || !e.isPlayer || e == (CombatEntity)this || e.networker.GetIsDead()) continue;

			Vector2 rc = Random.insideUnitCircle * 2f;

			if (NavMesh.SamplePosition(e.transform.position + new Vector3(rc.x, 0, rc.y),
					out NavMeshHit h, 2f, NavMesh.AllAreas)) {
				foundOtherPlayer = true;
				transform.position = h.position;
				break;
			}
		}
		if (!foundOtherPlayer) {
			transform.position = MapController.instance.GetPlayerSpawnpoint();
		}
		RespawnEntity();
		networker.EntityRespawned();
	}
	//adds to/removes from staticEntities list in EntitiesController (combat overrides these functions)
	public virtual void AddEntityToRegistry() {
		EntityController.instance.AddToStaticEntities(this);
	}
	public virtual void RemoveEntityFromRegistry() {
		EntityController.instance.RemoveFromStaticEntities(this);
	}
	protected virtual void Awake() {
		//set it back alive when Spawned() is run in the networker and it is not dead
		DisableEntity();
		AddEntityToRegistry();

		if (!isPlayer) transform.position = new Vector3(99999, 0, 99999);
		healthCanvas.transform.rotation = Camera.main.transform.rotation;
	}
	protected virtual void Start() {
		UpdateHealthBar();
		baseHealth = maxHealth;
	}
	protected virtual void Update() {
		RectTransform healthBar = healthCanvas.GetHealthBar();
		RectTransform healthChange = healthCanvas.GetHealthBarChange();

		if (healthBar.localScale.x > healthChange.localScale.x) {
			healthChange.localScale = new Vector2(healthBar.localScale.x, 1);
		} else if (healthBar.localScale.x < healthChange.localScale.x) {
			healthChange.localScale = new Vector2(healthChange.localScale.x - Time.deltaTime * 0.5f, 1);
		}
		healthCanvas.GetHealthText().text = Mathf.CeilToInt(GetHealth()).ToString();
	}

	#endregion

}


