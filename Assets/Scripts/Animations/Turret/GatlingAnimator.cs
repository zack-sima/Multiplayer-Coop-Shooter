using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatlingAnimator : TurretAnimatorBase {

	#region References

	[SerializeField] private Transform gun;
	[SerializeField] private List<GameObject> leftMuzzleFlashes, rightMuzzleFlashes;
	[SerializeField] private Transform leftBarrel, rightBarrel;

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
	public override void FireMainWeapon() {
		int muzzleIndex = Random.Range(0, leftMuzzleFlashes.Count);
		leftMuzzleFlashes[muzzleIndex].SetActive(true);
		StartCoroutine(StopMuzzleFlash(leftMuzzleFlashes[muzzleIndex]));

		int muzzleIndex2 = Random.Range(0, rightMuzzleFlashes.Count);
		rightMuzzleFlashes[muzzleIndex2].SetActive(true);
		StartCoroutine(StopMuzzleFlash(rightMuzzleFlashes[muzzleIndex2]));

		if (gunSound != null) {
			if (AudioSourceController.CanPlaySound(gunSoundName, audioCutoff)) {
				gunSound.PlayOneShot(gunSound.clip);
			}
		}
	}
	void Update() {
		leftBarrel.Rotate(0, 0, Time.deltaTime * 600f);
		rightBarrel.Rotate(0, 0, Time.deltaTime * 600f);
	}

	#endregion
}
