using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HullAnimatorBase : MonoBehaviour {

	#region References

	[SerializeField] private List<Renderer> teamColorRenderers;

	#endregion

	#region Members

	//spider legs pre-move based on velocity
	private Vector3 velocity = Vector3.zero;
	public Vector3 GetVelocity() { return velocity; }

	private Vector3 lastPosition = Vector3.zero;

	//NOTE: only locally for track-based things on PC (-1 means not set)
	private int inReverse = -1;
	public int GetInReverse() { return inReverse; }

	private Vector3 targetDirection = Vector3.zero;
	protected Vector3 GetTargetDirection() { return targetDirection; }

	//bots shouldn't reverse drive
	protected bool isBot = false;
	public void SetAsAI() { isBot = true; }

	#endregion

	#region Functions

	//for determining reversal
	public void SetTargetDirection(Vector3 direction) {
		targetDirection = direction;
	}
	public void SetInReverse(bool reversed) {
		inReverse = reversed ? 1 : 0;
	}
	//callback when player is spawned in/teleported so spider legs, etc don't try follow
	public virtual void Teleported() { }
	public void SetTeamMaterial(Material m) {
		foreach (Renderer r in teamColorRenderers) {
			List<Material> materials = new(r.materials) { [0] = m };
			r.SetMaterials(materials);
		}
	}
	protected virtual void Start() {
		lastPosition = transform.position;
	}
	protected virtual void Update() {

	}
	protected virtual void FixedUpdate() {
		//v = dx/dt, non-zero
		velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
		lastPosition = transform.position;
	}

	#endregion
}
