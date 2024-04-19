using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAnimator : HullAnimatorBase {

	#region References

	[SerializeField] private List<Transform> legs;

	#endregion

	#region Constants

	//how long to prevent leg movement when moving legs
	private const float LEG_STOP_TIME = 0.07f;

	//leg shifting speed
	private const float LEG_MOVE_SPEED = 18f;

	//determines leg shift
	private const float BODY_SIZE = 1.25f;

	#endregion

	#region Members

	//IK target will be set to legPositions and legPositions interpolates to targetLegPositions
	private List<Vector3> legPositions;
	private List<Vector3> targetLegPositions;

	//timestamp of when it was last moved (this is used to determine priority of leg movement)
	private List<float> legMoveTimestamps;

	//default distance between leg and self with y-axis disabled
	private List<Vector3> standardLegDistances;

	//seconds before allowed to move again (set to LEG_STOP_TIME)
	private float movedLegCountdown = 0f;

	#endregion

	#region Functions

	//resets leg positions
	public override void Teleported() {
		if (legPositions == null) return;
		for (int i = 0; i < legPositions.Count; i++) {
			legPositions[i] = new Vector3(standardLegDistances[i].x + transform.position.x,
				legPositions[i].y, standardLegDistances[i].z + transform.position.z);
			targetLegPositions[i] = legPositions[i];
		}
	}

	protected override void Start() {
		base.Start();

		legPositions = new();
		targetLegPositions = new();
		standardLegDistances = new();
		legMoveTimestamps = new();

		foreach (Transform t in legs) {
			legMoveTimestamps.Add(Time.time);
			legPositions.Add(t.position);
			targetLegPositions.Add(t.position);
			standardLegDistances.Add(new Vector3(t.position.x - transform.position.x, 0,
				t.position.z - transform.position.z));
		}
	}

	protected override void Update() {
		base.Update();

		if (movedLegCountdown > 0) movedLegCountdown -= Time.deltaTime;

		//leg that hasn't moved for the longest time gets to be moved first to prevent dragging
		List<Vector3> queuedLegPositions = new();
		for (int i = 0; i < legs.Count; i++) {
			queuedLegPositions.Add(Vector3.zero);

			//move leg towards target gradually
			legPositions[i] = Vector3.MoveTowards(legPositions[i], targetLegPositions[i],
				Time.deltaTime * LEG_MOVE_SPEED);

			if (Vector3.Distance(legPositions[i], targetLegPositions[i]) > BODY_SIZE * 2f) {
				Teleported();
			}

			//reset leg position from local to saved global position
			legs[i].position = legPositions[i];

			Vector3 newLegDifference = new(targetLegPositions[i].x - transform.position.x, 0,
				targetLegPositions[i].z - transform.position.z);

			float rawMovement = (newLegDifference - standardLegDistances[i]).magnitude;


			float threshold = 0.52f;

			//move out/move in leg
			if (rawMovement > threshold * BODY_SIZE && Vector3.Dot(GetVelocity(), newLegDifference) < 0 &&
				movedLegCountdown <= 0) {

				Vector3 newPosition = transform.position + standardLegDistances[i] +
					threshold * 0.8f * BODY_SIZE * GetVelocity().normalized;

				newPosition.y = 0;

				queuedLegPositions[i] = newPosition;
			}
		}
		//which leg to move, if any (if multiple need movement pick the one that was moved longest ago)
		int priorityIndex = -1;
		float longestTimestamp = 0;
		for (int i = 0; i < queuedLegPositions.Count; i++) {
			if (queuedLegPositions[i] != Vector3.zero && Time.time - legMoveTimestamps[i] > longestTimestamp) {
				priorityIndex = i;
				longestTimestamp = Time.time - legMoveTimestamps[i];
			}
		}
		if (priorityIndex != -1) {
			targetLegPositions[priorityIndex] = queuedLegPositions[priorityIndex];
			legMoveTimestamps[priorityIndex] = Time.time;
			movedLegCountdown = LEG_STOP_TIME;
		}
	}

	#endregion
}
