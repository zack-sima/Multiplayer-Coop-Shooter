using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileTurretAnimator : TurretAnimatorBase {

	#region References

	[SerializeField] private List<GameObject> leftMuzzleFlashes, rightMuzzleFlashes;

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
		for (int i = 0; i < leftMuzzleFlashes.Count; i++) {
			leftMuzzleFlashes[i].SetActive(false);
		}
		for (int i = 0; i < rightMuzzleFlashes.Count; i++) {
			rightMuzzleFlashes[i].SetActive(false);
		}
	}
	public override void FireMainWeapon(int bulletIndex) {
		//int muzzleIndex = Random.Range(0, leftMuzzleFlashes.Count);
		//leftMuzzleFlashes[muzzleIndex].SetActive(true);
		//StartCoroutine(StopMuzzleFlash(leftMuzzleFlashes[muzzleIndex]));

		//int muzzleIndex2 = Random.Range(0, rightMuzzleFlashes.Count);
		//rightMuzzleFlashes[muzzleIndex2].SetActive(true);
		//StartCoroutine(StopMuzzleFlash(rightMuzzleFlashes[muzzleIndex2]));

		if (gunSound != null) {
			if (AudioSourceController.CanPlaySound(gunSoundName, audioCutoff)) {
				gunSound.PlayOneShot(gunSound.clip);
			}
		}
	}

	#endregion
}
