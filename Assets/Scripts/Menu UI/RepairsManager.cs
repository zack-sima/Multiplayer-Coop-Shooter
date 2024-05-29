using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class RepairsManager : MonoBehaviour {

	#region Statics & Consts

	public static RepairsManager instance;

	//TODO: change dynamically
	public const int MAX_REPAIRS = 3;

	public class UTCParser {
		public int unixtime;
	}

	#endregion

	#region Prefabs

	[SerializeField] private GameObject repairBarPrefab;

	#endregion

	#region References

	[SerializeField] private RectTransform repairsScreen;
	[SerializeField] private RectTransform repairsParent;
	[SerializeField] private TMP_Text repairsTitle;

	#endregion

	#region Members

	private Vector3 normalCameraPosition;

	//interpolate to this
	private Vector3 targetGarageCameraPosition = new(1.9f, 0.5f, -7f);
	private Vector3 targetGarageCameraRotation = new(2f, 78f, 0);

	private List<GameObject> repairBars = new();

	#endregion

	#region Functions

	//can player go play? if this is true prevent player from starting matches
	public bool PlayerNeedsRepair() {
		return GarageManager.instance.GetHullDurability(MenuManager.instance.GetPlayerHull()) -
			PersistentDict.GetInt("repair_uses_" + MenuManager.instance.GetPlayerHull()) <= 0 ||

			GarageManager.instance.GetTurretDurability(MenuManager.instance.GetPlayerTurret()) -
			PersistentDict.GetInt("repair_uses_" + MenuManager.instance.GetPlayerTurret()) <= 0;
	}
	//is repair shop full yet?
	public bool HasRepairRoom() {
		return PersistentDict.GetIntList("repair_timers").Count < MAX_REPAIRS;
	}
	//called by AccountDataSyncer
	public void UpdateRepairs() {
		//re-create dropdown & check garage
		foreach (GameObject g in repairBars) {
			Destroy(g);
		}
		repairBars.Clear();

		List<int> repairTimers = PersistentDict.GetIntList("repair_timers");
		List<string> repairNames = PersistentDict.GetStringList("repair_names");

		for (int i = 0; i < repairNames.Count; i++) {
			GameObject g = Instantiate(repairBarPrefab, repairsParent);
			g.transform.GetChild(0).GetComponent<TMP_Text>().text = repairNames[i];

			int time = repairTimers[i];
			g.transform.GetChild(1).GetComponent<TMP_Text>().text =
				$"{time / 3600:00}:{time % 3600 / 60:00}:{time % 60:00}";

			Sprite s = GarageManager.instance.GetHullSprite(repairNames[i]);
			if (s == null) s = GarageManager.instance.GetTurretSprite(repairNames[i]);

			g.transform.GetChild(3).GetComponent<Image>().sprite = s;

			repairBars.Add(g);
		}

		repairsTitle.text = Translator.Translate("REPAIRS") + $" ({repairNames.Count}/{MAX_REPAIRS})";

		GarageManager.instance.UpdateRepairStatus();
	}
	private void JumpTime(int seconds) {
		if (seconds < 0) return;

		//if there are repairs, etc, allow jump
		AccountDataSyncer.instance.UpdateRepairs(seconds);

		Debug.Log($"jumped {seconds} seconds outside of game");
	}

	//TODO: NOTE -- A FALLBACK TO PERSONAL/GOOGLE CLOUD LINUX SERVER SHOULD BE USED IN CASE WEBSITE GOES DOWN
	//tries to fetch world time. If successful, allow time to "jump" if it is the first time the user got it in a session.
	private IEnumerator GetWorldTime() {
		while (true) {
			using (UnityWebRequest webRequest = UnityWebRequest.Get("http://worldtimeapi.org/api/timezone/Etc/UTC")) {
				yield return webRequest.SendWebRequest();

				if (webRequest.result != UnityWebRequest.Result.Success) {
					Debug.LogError("Error retrieving world time: " + webRequest.error);
				} else {
					string jsonResponse = webRequest.downloadHandler.text;

					int worldTime = ((UTCParser)MyJsonUtility.FromJson(typeof(UTCParser), jsonResponse)).unixtime;

					if (PersistentDict.GetInt("unixtime") != 0 && !ServerLinker.instance.GetWorldTimeFetched()) {
						JumpTime(worldTime - PersistentDict.GetInt("unixtime"));
					}
					PersistentDict.SetInt("unixtime", worldTime);

					ServerLinker.instance.SetWorldTimeFetched();
					//Debug.Log("World time in UTC: " + worldTime);
				}
			}
			yield return new WaitForSeconds(5f);
		}
	}

	public bool GetIsInRepairs() {
		return repairsScreen.gameObject.activeInHierarchy;
	}
	public void OpenRepairs() {
		repairsScreen.gameObject.SetActive(true);
		MenuManager.instance.SetMenuScreen(false);
		GarageManager.instance.GetBlur().SetBlur(0);
	}
	public void CloseRepairs() {
		repairsScreen.gameObject.SetActive(false);
		MenuManager.instance.SetMenuScreen(true);
		GarageManager.instance.GetBlur().SetBlur(0);

		MenuManager.instance.SetLastClosed(1);
	}
	private void Awake() {
		instance = this;
	}

	private void Start() {
		normalCameraPosition = GarageManager.instance.GetPlayerCamera().transform.position;

		StartCoroutine(GetWorldTime());

		UpdateRepairs();
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
		} else if (MenuManager.instance.GetLastClosedId() == 1 && !GarageManager.instance.GetIsInGarage() &&
			!UpgradesManager.instance.GetIsInUpgrades()) {
			GarageManager.instance.GetPlayerCamera().transform.SetPositionAndRotation(Vector3.MoveTowards(
				GarageManager.instance.GetPlayerCamera().transform.position,
				normalCameraPosition, Time.deltaTime * 10f), Quaternion.RotateTowards(
				GarageManager.instance.GetPlayerCamera().transform.rotation,
				Quaternion.identity, Time.deltaTime * 150f));
		}

#if UNITY_EDITOR

		if (Input.GetKeyDown(KeyCode.P)) {
			JumpTime(60);
		}

#endif
	}

	#endregion

}
