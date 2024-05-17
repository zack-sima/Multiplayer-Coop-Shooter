using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using Abilities;
using System;
using System.Security;
using UnityEngine.AI;
using Fusion;
using UnityEditor.Experimental.GraphView;
using Palmmedia.ReportGenerator.Core;

public class PointCaptureBot : baseBrain
{
    #region DecisionValues
    public float valueOfTank = 1000;// the value of a 1hp tank, in hp/tank units
    public float wallAvoidance = 3f; // how far the tank stays from walls when running away

    #endregion

    public float accuracy = 0.1f;
    public CombatEntity pursuitTarget;
    public enum allModes { fight, chase, capture, run };
    protected allModes currentMode;
    public float chaseValue = 0.5f; // determines how likely it is to chase the opponent
    // if high, more likely to chase
    #region Functions
    protected override void EnemyDecisionTick()
    {
        currentMode = allModes.capture;
        if (entity.GetNetworker().GetIsDead()) return;

        if (target != null && target.GetNetworker().GetIsDead())
        {
            canShootTarget = false;
            target = null;
        }

        //wait for NavMeshAgent to initialize
        if (!navigator.GetIsNavigable()) return;


        if (entity.GetHealth() * 0.75 < entity.GetMaxHealth() * retreatThreshold)
        {
            entity.GetNetworker().PushAIAbilityActivation(1);
            //try and heal; TODO: wait for abilities & effects

            //heal.Activate(entity.GetNetworker(), true);
            //entity.GetNetworker().HealthPercentNetworkEntityCall(3f);
        }

        float weight = -100000;
        float pursuitWeight = -100000;
        float totalAllies = 0;
        float totalEnemies = 0;
        Dictionary<CombatEntity, (float, float)> potentialTargets = new Dictionary<CombatEntity, (float, float)>();
        //try finding target
        foreach (CombatEntity ce in EntityController.instance.GetCombatEntities())
        {
            print($"BOT#: {ce}");
            if (!ce.GetNetworker().GetInitialized() ||
                ce.GetNetworker().GetIsDead()) continue;
            if (InVisionRange(ce))
            {
                if (ce.GetTeam() == entity.GetTeam())
                {
                    float allyHealth = ce.GetHealth();
                    totalAllies += allyHealth;
                    totalAllies += valueOfTank;

                }
                else
                {
                    totalEnemies += ce.GetHealth();
                    totalAllies += valueOfTank;
                    // add weight based on target health
                    weight = 0;
                    pursuitWeight = 0;
                    weight += 10 * GroundDistance(ce.transform.position, transform.position);
                    weight -= ce.GetHealth();
                    pursuitWeight = weight;
                    if (TargetInRange(ce))
                    {
                        weight += 100000;
                        canShootTarget = true;
                        print($"BOT: IN RANGE {canShootTarget}");
                    }

                    potentialTargets.Add(ce, (weight, pursuitWeight));
                }

            }
        }
        print($"BOT#: {totalAllies}, {totalEnemies}");
        if (totalAllies * 0.9 + 200 < totalEnemies)
        {
            currentMode = allModes.run;
        }
        foreach (KeyValuePair<CombatEntity, (float, float)> entry in potentialTargets)
        {
            if (weight <= entry.Value.Item1)
            {
                weight = entry.Value.Item1;
                target = entry.Key;
            }
            if (pursuitWeight <= entry.Value.Item2)
            {
                pursuitWeight = entry.Value.Item2;
                pursuitTarget = entry.Key;
            }
        }




        //if running home, ignore everything else
        /*
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
            currentMode = allModes.run;
        }
        */

        if (currentMode == allModes.run)
        {
            print($"BOT3: RUNNING AWAU");

            Vector3 averageEnemy = new Vector3(0, 0, 0);
            int counter = 0;
            foreach (KeyValuePair<CombatEntity, (float, float)> entry in potentialTargets)
            {
                averageEnemy += entry.Key.transform.position;
                counter++;
            }
            averageEnemy = averageEnemy / counter;
            Vector3 runDirection = (averageEnemy - transform.position).normalized;
            Dictionary<int, Vector3> potentialRunDirections = new Dictionary<int, Vector3>();
            for (int angle = 0; angle == 360; angle += 10)
            {
                float radian = angle * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(radian), 0, Mathf.Sin(radian));
                RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up,
                direction, wallAvoidance);
                bool hasHitWall = false;
                foreach (RaycastHit hit in hits)
                {
                    print($"BOT3: {hit}");
                    Entity isEntity = hit.collider.GetComponent<Entity>();
                    if (isEntity == null)
                    {
                        hasHitWall = true;
                        break;
                    }
                }
                if (!hasHitWall)
                {
                    potentialRunDirections.Add(angle, direction);
                }
            }

            if (potentialRunDirections.Count != 0)
            {
                float minSimilarity = 10f;
                Vector3 targetDirection = new Vector3(0, 0, 0);
                foreach (KeyValuePair<int, Vector3> entry in potentialRunDirections)
                {
                    float similarity = Vector3.Dot(runDirection, entry.Value);
                    // want vector to point away 
                    // runDirection points to the averae enemy location
                    if (similarity < minSimilarity)
                    {
                        minSimilarity = similarity;
                        targetDirection = entry.Value;
                    }
                }
                bogoTarget = transform.position + wallAvoidance * targetDirection;
                print($"BOT3: {bogoTarget}, {transform.position}, {averageEnemy}");
                return;
            }


        }

        if (target != null)
        {
            bool veryCloseToTarget = GroundDistance(transform.position, target.transform.position) < 3f;
            //try raycasting to target

            if (GroundDistance(targetPoint.transform.position, transform.position) < 2)
            {
                bogoTarget = targetPoint.transform.position;
            }

            if (currentMode == allModes.run)
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
                    Vector2 circle = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(2f, 5f);
                    bogoTarget = pursuitTarget.transform.position + new Vector3(circle.x, 0, circle.y);
                }
                navigator.SetStopped(false);
                navigator.SetTarget(bogoTarget);
            }
        }
        else if (!(currentMode == allModes.run))
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
                int rand = UnityEngine.Random.Range(0, validPoints.Count);
                if (validPoints.Count != 0)
                {


                    targetPoint = validPoints[rand];
                    Vector2 circle = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0f, 2f);
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
        currentMode = allModes.capture;
    }



    // Update is called once per frame
    void Update()
    {
        bool endIter = BaseUpdate();
        if (!endIter) return;
    }
    #endregion
}
