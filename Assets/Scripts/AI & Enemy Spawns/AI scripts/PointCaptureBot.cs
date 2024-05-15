using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using Abilities;

public class PointCaptureBot : baseBrain
{

    public float accuracy = 0.1f;
    #region Functions
    protected override void EnemyDecisionTick()
    {
        if (entity.GetNetworker().GetIsDead()) return;

        if (target != null && target.GetNetworker().GetIsDead())
        {
            canShootTarget = false;
            target = null;
        }

        //wait for NavMeshAgent to initialize
        if (!navigator.GetIsNavigable()) return;
        float weight = -100000;
        Dictionary<CombatEntity, float> potentialTargets = new Dictionary<CombatEntity, float>();
        //try finding target
        foreach (CombatEntity ce in EntityController.instance.GetCombatEntities())
        {
            if (!ce.GetNetworker().GetInitialized() ||
                ce.GetTeam() == entity.GetTeam() || ce.GetNetworker().GetIsDead()) continue;
            if (InVisionRange(ce))
            {
                // add weight based on target health
                weight = 0;
                if (TargetInRange(ce))
                {

                    weight += 100000;
                    canShootTarget = true;
                    print($"BOT: IN RANGE {canShootTarget}");
                }
                weight += 10 * GroundDistance(ce.transform.position, transform.position);
                weight -= ce.GetHealth();
                potentialTargets.Add(ce, weight);
            }
        }
        foreach (KeyValuePair<CombatEntity, float> entry in potentialTargets)
        {
            if (weight <= entry.Value)
            {
                weight = entry.Value;
                target = entry.Key;
            }
        }




        //if running home, ignore everything else
        bool runningAway = false;

        if (entity.GetHealth() * 0.75 < entity.GetMaxHealth() * retreatThreshold)
        {
            entity.GetNetworker().PushAIAbilityActivation(1);
            //try and heal; TODO: wait for abilities & effects

            //heal.Activate(entity.GetNetworker(), true);
            //entity.GetNetworker().HealthPercentNetworkEntityCall(3f);
        }
        else if (entity.GetHealth() < entity.GetMaxHealth() * retreatThreshold)
        {
            entity.GetNetworker().PushAIAbilityActivation(1);
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
            bool veryCloseToTarget = GroundDistance(transform.position, target.transform.position) < 3f;
            //try raycasting to target

            if (GroundDistance(targetPoint.transform.position, transform.position) < 2)
            {
                bogoTarget = targetPoint.transform.position;
            }

            if (runningAway)
            {
            }
            else if (!veryCloseToTarget && !canShootTarget)
            {
                navigator.SetStopped(false);
                navigator.SetTarget(target.transform.position);
            }
            else
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
        }
        else if (!runningAway)
        {

            List<CapturePoint> points = MapController.instance.GetCapturePoints();
            if (targetPoint == null || (targetPoint != null && targetPoint.GetCaptureProgress() >= 1 &&
                targetPoint.GetPointOwnerTeam() == entity.GetTeam())
                 || bogoTarget == Vector3.zero) // If there is no target 
                                                // or the point is already captured by the team
            {
                List<CapturePoint> validPoints = new List<CapturePoint>();
                foreach (CapturePoint pointTest in points)
                {
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


                    targetPoint = validPoints[rand];
                    Vector2 circle = Random.insideUnitCircle * Random.Range(0f, 2f);
                    bogoTarget = targetPoint.transform.position + new Vector3(circle.x, 0, circle.y);
                }
                else
                {
                    bogoTarget = MapController.instance.GetTeamSpawnpoint((entity.GetTeam() + 1) % 2);
                    targetPoint = null;
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
    void Start()
    {
        BaseStart();
    }



    // Update is called once per frame
    void Update()
    {
        bool endIter = BaseUpdate();
        if (!endIter) return;
    }
    #endregion
}
