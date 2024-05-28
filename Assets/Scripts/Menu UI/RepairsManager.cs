using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairsManager : MonoBehaviour {

	#region Statics & Consts

	public static RepairsManager instance;

	#endregion

	#region References

	[SerializeField] private RectTransform repairsScreen;

	#endregion

	#region Members

	private Vector3 normalCameraPosition;

	//interpolate to this
	private Vector3 targetGarageCameraPosition = new(1.9f, 0.5f, -7f);
	private Vector3 targetGarageCameraRotation = new(0, 90f, 0);

	#endregion

	#region Functions

	public bool GetIsInRepairs() {
		return repairsScreen.gameObject.activeInHierarchy;
	}
	public void OpenRepairs() {
		repairsScreen.gameObject.SetActive(true);
		MenuManager.instance.SetMenuScreen(false);
		//GarageManager.instance.GetPlayerHealthCanvas().transform.parent.gameObject.SetActive(false);
		GarageManager.instance.GetBlur().SetBlur(0);
	}
	public void CloseRepairs() {
		repairsScreen.gameObject.SetActive(false);
		MenuManager.instance.SetMenuScreen(true);
		//GarageManager.instance.GetPlayerHealthCanvas().transform.parent.gameObject.SetActive(true);
		GarageManager.instance.GetBlur().SetBlur(0);

		MenuManager.instance.SetLastClosed(1);
	}

	private void Awake() {
		instance = this;
	}

	private void Start() {
		normalCameraPosition = GarageManager.instance.GetPlayerCamera().transform.position;

	}

	private void Update() {
		if (repairsScreen.gameObject.activeInHierarchy) {
			Vector3 target = targetGarageCameraPosition;

			//standard aspect ratio adjustment
			float aspectRatio = (float)Screen.width / Screen.height / (1920f / 1080f);
			aspectRatio = (aspectRatio + 1f) / 2f;
			target.x *= aspectRatio;

			GarageManager.instance.GetPlayerCamera().transform.SetPositionAndRotation(Vector3.MoveTowards(
				GarageManager.instance.GetPlayerCamera().transform.position,
				target, Time.deltaTime * 10f), Quaternion.RotateTowards(
				GarageManager.instance.GetPlayerCamera().transform.rotation,
				Quaternion.Euler(targetGarageCameraRotation), Time.deltaTime * 150f));
		} else if (MenuManager.instance.GetLastClosedId() == 1 && !GarageManager.instance.GetIsInGarage()) {
			GarageManager.instance.GetPlayerCamera().transform.SetPositionAndRotation(Vector3.MoveTowards(
				GarageManager.instance.GetPlayerCamera().transform.position,
				normalCameraPosition, Time.deltaTime * 10f), Quaternion.RotateTowards(
				GarageManager.instance.GetPlayerCamera().transform.rotation,
				Quaternion.identity, Time.deltaTime * 150f));
		}
	}

	#endregion

}
