using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleCannonAnimator : TurretAnimatorBase {

	#region References

	[SerializeField] private Transform gun, gun2;
	[SerializeField] private List<GameObject> leftMuzzleFlashes, rightMuzzleFlashes;

	//sounds
	[SerializeField] private AudioSource gunSound;
	[SerializeField] private string gunSoundName;
	[SerializeField] private float audioCutoff;

	#endregion

	#region Members

	private float gunRecoil = 0f;
	private bool addingRecoil = false;

	private float gunRecoil2 = 0f;
	private bool addingRecoil2 = false;

	#endregion

	#region Functions

	private void AddRecoil() {
		StartCoroutine(AddRecoilCoroutine());
	}
	private void AddRecoil2() {
		StartCoroutine(AddRecoil2Coroutine());
	}
	private IEnumerator AddRecoilCoroutine() {
		for (float i = 0; i < 0.03f; i += Time.deltaTime) {
			addingRecoil = true;
			gunRecoil = Mathf.Min(0.5f, gunRecoil + 10f * Time.deltaTime);
			yield return null;
		}
		addingRecoil = false;
	}
	private IEnumerator AddRecoil2Coroutine() {
		for (float i = 0; i < 0.03f; i += Time.deltaTime) {
			addingRecoil2 = true;
			gunRecoil2 = Mathf.Min(0.5f, gunRecoil2 + 10f * Time.deltaTime);
			yield return null;
		}
		addingRecoil2 = false;
	}
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
		if (bulletIndex % 2 == 0) {
			int muzzleIndex = Random.Range(0, leftMuzzleFlashes.Count);
			leftMuzzleFlashes[muzzleIndex].SetActive(true);
			StartCoroutine(StopMuzzleFlash(leftMuzzleFlashes[muzzleIndex]));
			AddRecoil();
		} else {
			int muzzleIndex2 = Random.Range(0, rightMuzzleFlashes.Count);
			rightMuzzleFlashes[muzzleIndex2].SetActive(true);
			StartCoroutine(StopMuzzleFlash(rightMuzzleFlashes[muzzleIndex2]));
			AddRecoil2();
		}
		if (gunSound != null) {
			if (AudioSourceController.CanPlaySound(gunSoundName, audioCutoff)) {
				gunSound.PlayOneShot(gunSound.clip);
			}
		}
	}
	private void Update() {
		if (gunRecoil > 0 && !addingRecoil) {
			gunRecoil = Mathf.Max(0f, gunRecoil - Time.deltaTime);
		}
		gun.localPosition = new Vector3(gun.localPosition.x, gun.localPosition.y, -gunRecoil);

		if (gunRecoil2 > 0 && !addingRecoil2) {
			gunRecoil2 = Mathf.Max(0f, gunRecoil2 - Time.deltaTime);
		}
		gun2.localPosition = new Vector3(gun2.localPosition.x, gun2.localPosition.y, -gunRecoil2);
	}

	#endregion
}