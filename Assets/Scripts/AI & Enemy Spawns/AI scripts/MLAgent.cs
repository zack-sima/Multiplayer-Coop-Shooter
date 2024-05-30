using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;
public class MLAgent : Agent
{
    [SerializeField]
    protected CombatEntity entity;
    private bool shooting = false;
    private CombatEntity target;
    public Vector3 moveDirection = new Vector3();
    public Camera nonOverrideCamera;
    public CameraSensor cameraSensor;

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        //sensor.AddObservation(cameraSensor);
        base.CollectObservations(sensor);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
    }
    // Start is called before the first frame update
    private IEnumerator Tick()
    {
        float baseDist = 10;
        foreach (CombatEntity ce in EntityController.instance.GetCombatEntities())
        {
            if (!ce.GetNetworker().GetInitialized() ||
                ce.GetTeam() == entity.GetTeam() || ce.GetNetworker().GetIsDead()) continue;
            float dist = GroundDistance(ce.transform.position, transform.position);
            if (dist < baseDist)
            {
                baseDist = dist;
                target = ce;
            }
        }
        yield return new WaitForSeconds(0.35f);
    }
    void Start()
    {
        nonOverrideCamera = GetComponent<Camera>();
        if (nonOverrideCamera == null)
        {
            throw new NullReferenceException("Camera not found");
        }
        print($"{(nonOverrideCamera.transform.position - transform.position).magnitude}");
        cameraSensor = new CameraSensor(nonOverrideCamera, 128, 128, true, "VisualObservation", 0);
        cameraSensor.GetCompressedObservation();
        StartCoroutine(Tick());
    }

    // Update is called once per frame
    void Update()
    {
        if (!entity.GetNetworker().HasSyncAuthority()) return;
        if (entity.GetNetworker().GetIsDead()) return;
        if (!shooting)
        {
            Vector3 targetPosition = target.transform.position;
            if (entity.GetTurret().GetIsRotatable())
            {
                entity.GetTurret().SetTargetTurretRotation(
                    Mathf.Atan2(targetPosition.x - transform.position.x,
                    targetPosition.z - transform.position.z) * Mathf.Rad2Deg
                );
            }
        }
        if (target == null)
        {

            //turret follows movement
            if (entity.GetVelocity() != Vector3.zero)
                entity.GetTurret().SetTargetTurretRotation(Mathf.Atan2(entity.GetVelocity().x,
                    entity.GetVelocity().z) * Mathf.Rad2Deg);
        }








    }
    public static float GroundDistance(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
