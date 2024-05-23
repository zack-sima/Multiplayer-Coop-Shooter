using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;


public class baseBrain : MonoBehaviour
{
    #region References

    [SerializeField] protected Collider hitbox; //own hitbox, turn off when doing raycast
    [SerializeField] protected AINavigator navigator;
    [SerializeField] protected CombatEntity entity;

    #endregion

    #region Members

    [SerializeField] protected float maxRange;

    //if is PVP bot, execute a different set of behaviors (at least don't always shoot)
    [SerializeField] protected bool isPVPBot;
    [SerializeField] protected float burstTime = 1f, pauseTime = 0.3f;
    [SerializeField] protected float retreatThreshold = 0.42f;

    //for bot intermittent shooting
    protected bool inPausePhase = false;
    protected float botPhaseTimer = 1f;

    //turret rotates towards target and shoots if there is a line of sight in range
    protected CombatEntity target = null;
    protected bool canShootTarget = false;

    //bogo target
    protected Vector3 bogoTarget = Vector3.zero;
    protected Vector3 homeTarget = Vector3.zero;

    //point cap
    protected CapturePoint targetPoint = null;

    protected int screenHeight = 9; // SCREEN HEIGHT IS Z AXIS
    // Technically height of screen / 2
    protected int screenWidth = 16;
    #endregion
    // Start is called before the first frame update
    public bool TargetInRange(CombatEntity enemy)
    {
        bool inRange = false;
        if (entity.GetTurret() is Mortar)
        {
            //mortar can shoot over obstacles
            inRange = GroundDistance(transform.position, enemy.transform.position) < 13.5f;
            return inRange;
        }
        else
        {
            //line of sight to target check; raycast all prevents other AI from blocking line of sight
            Vector3 directionToPlayer = enemy.transform.position - transform.position;
            directionToPlayer.y = 0;

            RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up,
                directionToPlayer.normalized, maxRange);

            List<RaycastHit> hitsList = hits.ToList();
            hitsList.Sort((hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

            foreach (var hit in hitsList)
            {
                if (hit.collider.gameObject == enemy.gameObject)
                {
                    inRange = true;
                    break;
                }
                else if (hit.collider.GetComponent<CombatEntity>() == null &&
                    hit.collider.GetComponent<Bullet>() == null)
                {

                    //If hit is not a combat entity, break
                    break;
                }
            }
            return inRange;
        }
    }
    public bool InVisionRange(CombatEntity enemy)
    {
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 selfPosition = transform.position;
        if (Math.Abs(enemyPosition.x - selfPosition.x) < screenWidth
        && Math.Abs(enemyPosition.z - selfPosition.z) < screenHeight)
        {
            return true;
        }
        else return false;
    }
    public static float GroundDistance(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
    protected virtual void EnemyDecisionTick()
    {
        print("CALLED WRONG FUNCTION");
    }

    protected IEnumerator Tick()
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

            yield return new WaitForSeconds(1f);
        }
    }
    protected void BaseStart()
    {
        navigator.SetRotatable(false);
        navigator.SetSpeed(entity.GetHull().GetSpeed());

        if (isPVPBot) entity.GetNetworker().UpdateAbilityListForAI(); // CHANGE IF NEEDED, idk what you want.

        StartCoroutine(Tick());
    }
    protected bool BaseUpdate()
    {
        if (!entity.GetNetworker().HasSyncAuthority()) return false;
        if (entity.GetNetworker().GetIsDead())
        {
            bogoTarget = Vector3.zero;
            targetPoint = null;
            canShootTarget = false;
            target = null;
            return false;
        }
        if (target == null)
        {

            //turret follows movement
            if (entity.GetVelocity() != Vector3.zero)
                entity.GetTurret().SetTargetTurretRotation(Mathf.Atan2(entity.GetVelocity().x,
                    entity.GetVelocity().z) * Mathf.Rad2Deg);
            return false;
        }
        Vector3 targetPosition = target.transform.position;
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
                    botPhaseTimer = inPausePhase ? UnityEngine.Random.Range(burstTime, burstTime + 0.5f) : UnityEngine.Random.Range(pauseTime, pauseTime + 0.3f);
                    inPausePhase = !inPausePhase;
                }
                if (!inPausePhase) { print("SHOOTING"); entity.TryFireMainWeapon(); }
            }
            else
            {
                print("SHOOTING");
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
        return true;
    }
}
