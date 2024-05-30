using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class UpgradesManager : MonoBehaviour {

	public static UpgradesManager instance;

	private Vector3 normalCameraPosition;

	//interpolate to this
	private Vector3 targetGarageCameraPosition = new(-0.61f, 0.5f, -7.64f);
	private Vector3 targetGarageCameraRotation = new(2f, -100f, 0);

	[SerializeField] private RectTransform upgradesScreen;

	public bool GetIsInUpgrades() {
		return upgradesScreen.gameObject.activeInHierarchy;
	}
	public void OpenUpgrades() {
		upgradesScreen.gameObject.SetActive(true);
		MenuManager.instance.SetMenuScreen(false);
		GarageManager.instance.GetBlur().SetBlur(0);
	}
	public void CloseUpgrades() {
		upgradesScreen.gameObject.SetActive(false);
		MenuManager.instance.SetMenuScreen(true);
		GarageManager.instance.GetBlur().SetBlur(0);

		MenuManager.instance.SetLastClosed(2);
	}
	private void Awake() {
		instance = this;
	}

	private void Start() {
		normalCameraPosition = GarageManager.instance.GetPlayerCamera().transform.position;
	}

	private void Update() {
		if (upgradesScreen.gameObject.activeInHierarchy) {
			Vector3 target = targetGarageCameraPosition;

			//standard aspect ratio adjustment
			float aspectRatio = (float)Screen.width / Screen.height / (1920f / 1080f);
			aspectRatio = (aspectRatio + 1f) / 2f;
			target.x *= aspectRatio;

			GarageManager.instance.GetPlayerCamera().transform.SetPositionAndRotation(Vector3.MoveTowards(
				GarageManager.instance.GetPlayerCamera().transform.position,
				target, Time.deltaTime * 10f), Quaternion.RotateTowards(
				GarageManager.instance.GetPlayerCamera().transform.rotation,
				Quaternion.Euler(targetGarageCameraRotation), Time.deltaTime * 200f));
		} else if (MenuManager.instance.GetLastClosedId() == 2 && !GarageManager.instance.GetIsInGarage() &&
			!RepairsManager.instance.GetIsInRepairs()) {
			GarageManager.instance.GetPlayerCamera().transform.SetPositionAndRotation(Vector3.MoveTowards(
				GarageManager.instance.GetPlayerCamera().transform.position,
				normalCameraPosition, Time.deltaTime * 10f), Quaternion.RotateTowards(
				GarageManager.instance.GetPlayerCamera().transform.rotation,
				Quaternion.identity, Time.deltaTime * 200f));
		}
	}

}
