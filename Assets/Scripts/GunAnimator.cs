using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnimator : MonoBehaviour {

	#region References

	[SerializeField] private Transform gun;
	[SerializeField] private List<GameObject> muzzleFlashes;

	#endregion

	#region Members

	private float gunRecoil = 0f;
	private bool addingRecoil = false;

	#endregion

	#region Functions

	public void AddRecoil() {
		int muzzleIndex = Random.Range(0, muzzleFlashes.Count);
		muzzleFlashes[muzzleIndex].SetActive(true);
		StartCoroutine(StopMuzzleFlash(muzzleFlashes[muzzleIndex]));

		StartCoroutine(AddRecoilCoroutine());
	}
	private IEnumerator StopMuzzleFlash(GameObject muzzleRef) {
		yield return new WaitForSeconds(0.05f);
		muzzleRef.SetActive(false);
	}

	private IEnumerator AddRecoilCoroutine() {
		for (float i = 0; i < 0.03f; i += Time.deltaTime) {
			addingRecoil = true;
			gunRecoil = Mathf.Min(0.5f, gunRecoil + 10f * Time.deltaTime);
			yield return null;
		}
		addingRecoil = false;
	}

	void Update() {
		//TODO: migrate to human controller
		if (Input.GetKeyDown(KeyCode.Space)) {
			AddRecoil();
		}

		if (gunRecoil > 0 && !addingRecoil) {
			gunRecoil = Mathf.Max(0f, gunRecoil - Time.deltaTime);
		}
		gun.localPosition = new Vector3(gun.localPosition.x, gun.localPosition.y, -gunRecoil);
	}

	#endregion
}
