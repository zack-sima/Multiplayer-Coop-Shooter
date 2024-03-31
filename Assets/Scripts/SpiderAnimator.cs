using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAnimator : MonoBehaviour {

	#region References

	[SerializeField] private List<Transform> legs;

	//TODO: migrate to separate turret
	[SerializeField] private Transform turret;

	#endregion

	#region Constants

	//how long to prevent leg movement when moving legs
	private const float LEG_STOP_TIME = 0.08f;

	//leg shifting speed
	private const float LEG_MOVE_SPEED = 16f;

	//determines leg shift
	private const float BODY_SIZE = 1.5f;

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

	void Awake() {
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
	private void Start() {
		lastPosition = transform.position;
	}

	private void Update() {
		if (movedLegCountdown > 0) movedLegCountdown -= Time.deltaTime;

		//movement TODO: migrate
		float speed = 2.5f;
		if (Input.GetKey(KeyCode.LeftShift)) {
			speed = 3.5f;
		}
		if (Input.GetKey(KeyCode.W)) {
			transform.Translate(Vector3.forward * Time.deltaTime * speed);
		}
		if (Input.GetKey(KeyCode.S)) {
			transform.Translate(Vector3.back * Time.deltaTime * speed);
		}
		if (Input.GetKey(KeyCode.A)) {
			transform.Translate(Vector3.left * Time.deltaTime * speed);
		}
		if (Input.GetKey(KeyCode.D)) {
			transform.Translate(Vector3.right * Time.deltaTime * speed);
		}

		//turret TODO: migrate
		if (Input.GetKey(KeyCode.Q)) {
			turret.Rotate(0, Time.deltaTime * -150f, 0);
		}
		if (Input.GetKey(KeyCode.E)) {
			turret.Rotate(0, Time.deltaTime * 150f, 0);
		}

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
