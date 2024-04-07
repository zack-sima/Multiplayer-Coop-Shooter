using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAnimator : HullAnimatorBase {

	#region References

	[SerializeField] private List<Transform> legs;

	#endregion

	#region Constants

	//how long to prevent leg movement when moving legs
	private const float LEG_STOP_TIME = 0.08f;

	//leg shifting speed
	private const float LEG_MOVE_SPEED = 16f;

	//determines leg shift
	private const float BODY_SIZE = 1.3f;

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

	//spider legs pre-move based on velocity
	private Vector3 velocity = Vector3.zero;
	private Vector3 lastPosition = Vector3.zero;

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

	private void Start() {
		legPositions = new();
		targetLegPositions = new();
		standardLegDistances = new();
		legMoveTimestamps = new();
		lastPosition = transform.position;

		foreach (Transform t in legs) {
			legMoveTimestamps.Add(Time.time);
			legPositions.Add(t.position);
			targetLegPositions.Add(t.position);
			standardLegDistances.Add(new Vector3(t.position.x - transform.position.x, 0,
				t.position.z - transform.position.z));
		}
	}

	private void Update() {
		if (movedLegCountdown > 0) movedLegCountdown -= Time.deltaTime;

		//leg that hasn't moved for the longest time gets to be moved first to prevent dragging
		List<Vector3> queuedLegPositions = new();
		for (int i = 0; i < legs.Count; i++) {
			queuedLegPositions.Add(Vector3.zero);

			//move leg towards target gradually
			legPositions[i] = Vector3.MoveTowards(legPositions[i], targetLegPositions[i],
				Time.deltaTime * LEG_MOVE_SPEED);

			//reset leg position from local to saved global position
			legs[i].position = legPositions[i];

			Vector3 newLegDistance = new(targetLegPositions[i].x - transform.position.x, 0,
				targetLegPositions[i].z - transform.position.z);

			//move out
			if ((newLegDistance.magnitude < standardLegDistances[i].magnitude - 0.3f * BODY_SIZE) && movedLegCountdown <= 0) {
				Vector3 newPosition = transform.position + standardLegDistances[i] + 0.5f * BODY_SIZE * velocity.normalized;
				newPosition.y = 0;

				queuedLegPositions[i] = newPosition;
			}
			//move in
			if ((newLegDistance.magnitude > standardLegDistances[i].magnitude + 0.75f * BODY_SIZE ||
				(newLegDistance - standardLegDistances[i]).magnitude > 0.5f * BODY_SIZE) && movedLegCountdown <= 0) {
				Vector3 newPosition = transform.position + standardLegDistances[i] + 0.2f * BODY_SIZE * velocity.normalized;
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

		//v = dx/dt, non-zero
		velocity = (transform.position - lastPosition) / Time.deltaTime;
		lastPosition = transform.position;
	}

	#endregion
}
