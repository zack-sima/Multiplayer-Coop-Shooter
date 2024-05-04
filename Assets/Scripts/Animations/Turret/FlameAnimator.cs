using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameAnimator : TurretAnimatorBase {

	#region References

	//sounds, NOTE: looping
	[SerializeField] private AudioSource fireSound;

	#endregion

	#region Members

	float fireVolume = 0f;

	#endregion

	#region Functions

	public override void FireMainWeapon(int bulletIndex) {
		//turn on fire sound for ~0.5 seconds
		fireVolume = 1.5f;
	}
	void Update() {
		if (fireVolume > 0) fireVolume -= Time.deltaTime * 3f;
		fireSound.volume = Mathf.Clamp(fireVolume, 0f, 1f);
	}

	#endregion
}
