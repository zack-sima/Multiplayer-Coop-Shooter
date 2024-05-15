using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using Abilities;
using Unity.VisualScripting;

/// <summary>
/// Autonomously decides on an AI enemy's course of action
/// </summary>

public class AIBrain : MonoBehaviour
{

	#region References

	[SerializeField] private Collider hitbox; //own hitbox, turn off when doing raycast
	[SerializeField] private AINavigator navigator;
	[SerializeField] private CombatEntity entity;

	#endregion

	#region Members

	[SerializeField] private float maxRange;

	//if is PVP bot, execute a different set of behaviors (at least don't always shoot)
	[SerializeField] private bool isPVPBot;
	[SerializeField] private float burstTime = 1f, pauseTime = 0.3f;
	[SerializeField] private float retreatThreshold = 0.42f;

	//for bot intermittent shooting
	private bool inPausePhase = false;
	private float botPhaseTimer = 1f;

	//turret rotates towards target and shoots if there is a line of sight in range
	private CombatEntity target = null;
	private bool canShootTarget = false;

	//bogo target
	private Vector3 bogoTarget = Vector3.zero;
	private Vector3 homeTarget = Vector3.zero;

	//point cap
	private CapturePoint targetPoint = null;

	#endregion

	#region Functions

	public static float GroundDistance(Vector3 a, Vector3 b)
	{
		return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
	}
	private void EnemyDecisionTick()
	{
		if (entity.GetNetworker().GetIsDead()) return;

		if (target != null && target.GetNetworker().GetIsDead())
		{
			canShootTarget = false;
			target = null;
		}

		//wait for NavMeshAgent to initialize
		if (!navigator.GetIsNavigable()) return;

		//try finding target
		float closestDistance = isPVPBot ? 16 : 999;
		foreach (CombatEntity ce in EntityController.instance.GetCombatEntities())
		{
			if (!ce.GetNetworker().GetInitialized() ||
				ce.GetTeam() == entity.GetTeam() || ce.GetNetworker().GetIsDead()) continue;
			float distance = GroundDistance(ce.transform.position, transform.position);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				target = ce;
			}
		}
		//if running home, ignore everything else
		bool runningAway = false;

		//in point capture mode when contesting a point, never retreat (or if no enemies nearby)
		bool order227 = false;
		if (PlayerInfo.GetIsPointCap() && (targetPoint != null &&
			GroundDistance(transform.position, targetPoint.transform.position) < 3f ||
			target == null)) order227 = true;

		if (isPVPBot && entity.GetHealth() < entity.GetMaxHealth() * retreatThreshold && !order227)
		{
			//try and heal; TODO: wait for abilities & effects
			entity.GetNetworker().PushAIAbilityActivation(1);
			//heal.Activate(entity.GetNetworker(), true);
			//entity.GetNetworker().HealthPercentNetworkEntityCall(3f);

			navigator.SetStopped(false);
			if (homeTarget == Vector3.zero || GroundDistance(homeTarget, transform.position) < 5f)
			{
				//run home!
				homeTarget = MapController.instance.GetTeamSpawnpoint(entity.GetTeam() % 2);
			}
			navigator.SetTarget(homeTarget);
			runningAway = true;
		}
		if (target != null)
		{
			//try raycasting to target
			canShootTarget = false;
			bool veryCloseToTarget = GroundDistance(transform.position, target.transform.position) < 3f;

			if (entity.GetTurret() is Mortar)
			{
				//mortar can shoot over obstacles
				canShootTarget = GroundDistance(transform.position, target.transform.position) < 13.5f;
			}
			else
			{
				//line of sight to target check; raycast all prevents other AI from blocking line of sight
				Vector3 directionToPlayer = target.transform.position - transform.position;
				directionToPlayer.y = 0;

				RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up,
					directionToPlayer.normalized, maxRange);

				List<RaycastHit> hitsList = hits.ToList();
				hitsList.Sort((hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

				foreach (var hit in hitsList)
				{
					if (hit.collider.gameObject == target.gameObject)
					{
						canShootTarget = true;
						break;
					}
					else if (hit.collider.GetComponent<CombatEntity>() == null &&
						hit.collider.GetComponent<Bullet>() == null)
					{

						//If hit is not a combat entity, break
						break;
					}
				}
			}
			if (entity.GetTurret().GetIsProximityExploder()) canShootTarget = false;

			if (runningAway)
			{
			}
			else if (!veryCloseToTarget && !canShootTarget)
			{
				navigator.SetStopped(false);
				navigator.SetTarget(target.transform.position);
			}
			else if (isPVPBot)
			{
				if ((bogoTarget == Vector3.zero || GroundDistance(bogoTarget, transform.position) < 3.5f) &&
					(!PlayerInfo.GetIsPointCap() || targetPoint == null ||
					GroundDistance(targetPoint.transform.position, transform.position) > 5f))
				{
					Vector2 circle = Random.insideUnitCircle * Random.Range(2f, 5f);
					bogoTarget = target.transform.position + new Vector3(circle.x, 0, circle.y);
				}
				navigator.SetStopped(false);
				navigator.SetTarget(bogoTarget);
			}
			else
			{
				navigator.SetStopped(true);
			}
		}
		else if (!runningAway)
		{
			if (isPVPBot)
			{
				if (PlayerInfo.GetIsPointCap())
				{
					//in point capture, go for point and stay there
					List<CapturePoint> points = MapController.instance.GetCapturePoints();
					if (targetPoint == null || (targetPoint != null && targetPoint.GetCaptureProgress() >= 1 &&
						targetPoint.GetPointOwnerTeam() == entity.GetTeam())
						 || bogoTarget == Vector3.zero)
					{
						print("POINT CHANGING NOW");

						List<CapturePoint> validPoints = new List<CapturePoint>();
						for (int i = 0; i < points.Count; ++i)
						{
							CapturePoint pointTest = points[i];
							if (pointTest.gameObject.activeInHierarchy)
							{
								if (pointTest.GetCaptureProgress() < 1 || pointTest.GetPointOwnerTeam() != entity.GetTeam())
								{
									validPoints.Add(pointTest);
								}
							}
						}
						int rand = Random.Range(0, validPoints.Count);
						if (validPoints.Count != 0)
						{
							targetPoint = points[rand];
						}
						else
						{
							bogoTarget = MapController.instance.GetTeamSpawnpoint((entity.GetTeam() + 1) % 2);
						}

						if (targetPoint != null && (targetPoint.GetCaptureProgress() < 1 ||
							targetPoint.GetPointOwnerTeam() != entity.GetTeam() ||
							bogoTarget == Vector3.zero))
						{
							Vector2 circle = Random.insideUnitCircle * Random.Range(0f, 2f);
							bogoTarget = targetPoint.transform.position + new Vector3(circle.x, 0, circle.y);
						}
					}
				}
				else
				{
					//in PvP, go to other side if nothing to do
					if (bogoTarget == Vector3.zero || GroundDistance(bogoTarget, transform.position) < 5f)
					{
						bogoTarget = MapController.instance.GetTeamSpawnpoint((entity.GetTeam() + 1) % 2);
					}
				}
				navigator.SetStopped(false);
				if (bogoTarget != Vector3.zero) navigator.SetTarget(bogoTarget);
			}
			else
			{
				navigator.SetStopped(true);
			}
		}
		if (targetPoint == null) { Debug.Log(name); }
	}
	private IEnumerator Tick()
	{
		while (true)
		{
			//not client master
			if (!entity.GetNetworker().HasSyncAuthority())
			{
				navigator.SetActive(false);
				yield return new WaitForSeconds(1f);
				continue;
			}
			else navigator.SetActive(true);

			try
			{
				EnemyDecisionTick();
			}
			catch (System.Exception e) { Debug.LogWarning(e); }

			yield return new WaitForSeconds(0.35f);
		}
	}

	private void Update()
	{
		if (!entity.GetNetworker().HasSyncAuthority()) return;
		if (entity.GetNetworker().GetIsDead())
		{
			bogoTarget = Vector3.zero;
			targetPoint = null;
			canShootTarget = false;
			target = null;
			return;
		}
		if (target == null)
		{
			//turret follows movement
			if (entity.GetVelocity() != Vector3.zero)
				entity.GetTurret().SetTargetTurretRotation(Mathf.Atan2(entity.GetVelocity().x,
					entity.GetVelocity().z) * Mathf.Rad2Deg);
			return;
		}

		//target position -- in competitive/smart mode, enemies will try to predict movement
		Vector3 targetPosition = target.transform.position;

		if (isPVPBot)
		{
			float timeToTarget = (entity.GetTurret() is Mortar) ? 2f :
				GroundDistance(targetPosition, transform.position) / 15f;

			targetPosition += target.GetVelocity() * timeToTarget;
		}

		if (entity.GetTurret().GetIsRotatable())
		{
			entity.GetTurret().SetTargetTurretRotation(
				Mathf.Atan2(targetPosition.x - transform.position.x,
				targetPosition.z - transform.position.z) * Mathf.Rad2Deg
			);
		}

		navigator.SetActive(true);

		if (entity.GetTurret() is Mortar)
			((Mortar)entity.GetTurret()).SetDistance(GroundDistance(transform.position, targetPosition));

		if (!entity.GetTurret().GetIsProximityExploder() && canShootTarget)
		{
			//regulate shooting
			if (isPVPBot)
			{
				botPhaseTimer -= Time.deltaTime;
				if (botPhaseTimer <= 0f)
				{
					botPhaseTimer = inPausePhase ? Random.Range(burstTime, burstTime + 0.5f) : Random.Range(pauseTime, pauseTime + 0.3f);
					inPausePhase = !inPausePhase;
				}
				if (!inPausePhase) entity.TryFireMainWeapon();
			}
			else
			{
				entity.TryFireMainWeapon();
			}
		}
		else if (entity.GetTurret().GetIsProximityExploder() &&
			GroundDistance(targetPosition, transform.position) < 2.5f)
		{

			//same sender as target
			entity.GetNetworker().RPC_TakeDamage(entity.GetNetworker().Object,
				entity.GetNetworker().Object, entity.GetMaxHealth(), 0);
		}
		else if (entity.GetTurret().GetIsProximityExploder() && //close enough to walk straight
			GroundDistance(target.transform.position, transform.position) < 5f)
		{
			navigator.SetActive(false);

			Vector3 newPos = Vector3.MoveTowards(transform.position, target.transform.position,
				navigator.GetSpeed() * Time.deltaTime);

			transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
		}
	}
	private void Start()
	{
		navigator.SetRotatable(false);
		navigator.SetSpeed(entity.GetHull().GetSpeed());

		if (isPVPBot) entity.GetNetworker().UpdateAbilityListForAI(); // CHANGE IF NEEDED, idk what you want.

		StartCoroutine(Tick());
	}

	#endregion

}
