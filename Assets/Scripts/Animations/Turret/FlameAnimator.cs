using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameAnimator : TurretAnimatorBase {

	#region References

	//sounds, NOTE: looping
	[SerializeField] private AudioSource fireSound;

	#endregion

	#region Members

	#endregion

	#region Functions

	public override void FireMainWeapon(int bulletIndex) {
		//TODO: turn on fire sound for n seconds
	}
	void Update() {
	}

	#endregion
}
