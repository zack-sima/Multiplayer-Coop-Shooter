using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatlingSentryAnimator : TurretAnimatorBase {

	#region References

	[SerializeField] private Transform gun;
	[SerializeField] private List<GameObject> muzzleFlashes;
	[SerializeField] private Transform barrel;

	//sounds
	[SerializeField] private AudioSource gunSound;
	[SerializeField] private string gunSoundName;
	[SerializeField] private float audioCutoff;

	#endregion

	#region Members

	#endregion

	#region Functions

	private IEnumerator StopMuzzleFlash(GameObject muzzleRef) {
		yield return new WaitForSeconds(0.05f);
		muzzleRef.SetActive(false);
	}
	public override void ResetAnimations() {
		for (int i = 0; i < muzzleFlashes.Count; i++) {
			muzzleFlashes[i].SetActive(false);
		}
	}
	public override void FireMainWeapon(int bulletIndex) {
		int muzzleIndex = Random.Range(0, muzzleFlashes.Count);
		muzzleFlashes[muzzleIndex].SetActive(true);
		StartCoroutine(StopMuzzleFlash(muzzleFlashes[muzzleIndex]));

		if (gunSound != null) {
			if (AudioSourceController.CanPlaySound(gunSoundName, audioCutoff)) {
				gunSound.PlayOneShot(gunSound.clip);
			}
		}
	}
	void Update() {
		barrel.Rotate(0, 0, Time.deltaTime * 600f);
	}

	#endregion
}
