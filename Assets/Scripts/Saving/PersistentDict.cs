using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

// ** This is an old script written ~2020-2021 for Armchair Commander **
// PlayerPrefs, but more secure and with expanded features; it is made to exist in scenes to improve efficiency
//
// Secure data can be stored here (recommended -- the dictionary format prevents class types from being unrecognized)

/// <summary>
/// Use this class to store any information that MUST be preserved;
/// use PlayerPrefs for arguments between scenes, sound settings, etc.
/// </summary>
public class PersistentDict : MonoBehaviour {
	PersistentDictData data = null;
	public static PersistentDict instance;

	//can be used for data export (copy/paste)
	public PersistentDictData GetData() {
		if (data == null) {
			Debug.LogWarning("Export data not found; returning default");
			return new PersistentDictData();
		}
		return data;
	}
	public void OverrideData(PersistentDictData newData) {
		data = newData;
		SaveData();
	}
	/// <summary>
	/// NOTE: DO NOT CALL ANYTHING TO PERSISTENTDICT IN AWAKE!
	/// </summary>
	private void Awake() {
		if (instance == null) {
			DontDestroyOnLoad(gameObject);
			instance = this;
			data = LoadData();
		} else {
			Destroy(gameObject);
			return;
		}
	}
	public static void SetIntList(string key, List<int> value, bool saveNow = true) {
		instance._SetIntList(key, value, saveNow);
	}
	public static void SetFloatList(string key, List<float> value, bool saveNow = true) {
		instance._SetFloatList(key, value, saveNow);
	}
	public static void SetStringList(string key, List<string> value, bool saveNow = true) {
		instance._SetStringList(key, value, saveNow);
	}
	private void _SetIntList(string key, List<int> value, bool saveNow = true) {
		data.SetIntList(key, value);
		if (saveNow) SaveData();
	}
	private void _SetFloatList(string key, List<float> value, bool saveNow = true) {
		data.SetFloatList(key, value);
		if (saveNow) SaveData();
	}
	private void _SetStringList(string key, List<string> value, bool saveNow = true) {
		data.SetStringList(key, value);
		if (saveNow) SaveData();
	}
	public static List<int> GetIntList(string key) {
		return instance._GetIntList(key);
	}
	public static List<float> GetFloatList(string key) {
		return instance._GetFloatList(key);
	}
	public static List<string> GetStringList(string key) {
		return instance._GetStringList(key);
	}
	private List<int> _GetIntList(string key) {
		return data.GetIntList(key);
	}
	private List<float> _GetFloatList(string key) {
		return data.GetFloatList(key);
	}
	private List<string> _GetStringList(string key) {
		return data.GetStringList(key);
	}
	public static void SetInt(string key, int value, bool saveNow = true) {
		instance._SetInt(key, value, saveNow);
	}
	public static void SetFloat(string key, float value, bool saveNow = true) {
		instance._SetFloat(key, value, saveNow);
	}
	public static void SetString(string key, string value, bool saveNow = true) {
		instance._SetString(key, value, saveNow);
	}
	private void _SetInt(string key, int value, bool saveNow = true) {
		data.SetInt(key, value);
		if (saveNow) SaveData();
	}
	private void _SetFloat(string key, float value, bool saveNow = true) {
		data.SetFloat(key, value);
		if (saveNow) SaveData();
	}
	private void _SetString(string key, string value, bool saveNow = true) {
		data.SetString(key, value);
		if (saveNow) SaveData();
	}
	public int _GetInt(string key) {
		return data.GetInt(key);
	}
	public float _GetFloat(string key) {
		return data.GetFloat(key);
	}
	public string _GetString(string key) {
		return data.GetString(key);
	}
	public static int GetInt(string key) {
		return instance._GetInt(key);
	}

	public static float GetFloat(string key) {
		return instance._GetFloat(key);
	}

	public static string GetString(string key) {
		return instance._GetString(key);
	}

	public static bool HasKey(string key) {
		return instance._HasKey(key);
	}
	private bool _HasKey(string key) {
		PersistentDictData d = LoadData();

		return d.intDict.ContainsKey(key) || d.floatDict.ContainsKey(key) || d.stringDict.ContainsKey(key) ||
			d.intListDict.ContainsKey(key) || d.floatListDict.ContainsKey(key) || d.stringListDict.ContainsKey(key);
	}
	public static void DeleteKey(string key) {
		instance._DeleteKey(key);
		instance.SaveData();
	}
	private void _DeleteKey(string key) {
		PersistentDictData d = data;

		if (d.intDict.ContainsKey(key))
			d.intDict.Remove(key);

		if (d.floatDict.ContainsKey(key))
			d.floatDict.Remove(key);

		if (d.stringDict.ContainsKey(key))
			d.stringDict.Remove(key);

		if (d.intListDict.ContainsKey(key))
			d.intListDict.Remove(key);

		if (d.floatListDict.ContainsKey(key))
			d.floatListDict.Remove(key);

		if (d.stringListDict.ContainsKey(key))
			d.stringListDict.Remove(key);
	}

	/// <summary>
	/// WARNING: newData given here will override existing file;
	/// unless saveNow is false when calling functions (for efficiency),
	/// saving is automatically invoked with internal data.
	/// </summary>
	/// <param name="newData">override data</param>
	public void SaveData(PersistentDictData newData = null) {
		if (newData == null) {
			if (data == null) {
				Debug.LogWarning("Current data not present! Aborted save");
				return;
			} else {
				newData = data;
			}
		}

		BinaryFormatter bf = new();
		FileStream stream = new(GetDataPath() + "/persistentDict.sav", FileMode.Create);

		bf.Serialize(stream, newData);
		stream.Close();

		//save backup after current save has completed
		SaveBackupData(newData);
	}
	private void SaveBackupData(PersistentDictData data) {
		BinaryFormatter bf = new();

		FileStream stream = new(GetDataPath() + "/persistentDictBackup.sav", FileMode.Create);

		bf.Serialize(stream, data);

		stream.Close();
	}
	public static string GetDataPath() {
		string dataPath = Application.persistentDataPath;

		//if (!Application.isMobilePlatform) dataPath = Application.dataPath;

#if UNITY_EDITOR
		dataPath = Application.dataPath + "/EditorData";
#endif
		//only use datapath for testing apps (persistent is more secure)
		//if (SystemInfo.deviceType == DeviceType.Desktop && !Application.isEditor) {
		//	dataPath = Application.dataPath;
		//}
		return dataPath;
	}

	private PersistentDictData LoadBackupData() {
		if (File.Exists(GetDataPath() + "/persistentDictBackup.sav")) {
			BinaryFormatter bf = new();
			FileStream stream = new(GetDataPath() + "/persistentDictBackup.sav", FileMode.Open);
			PersistentDictData data;
			try {
				data = bf.Deserialize(stream) as PersistentDictData;
				stream.Close();
			} catch {
				stream.Close();
				Debug.LogError("Cannot parse backup -- data corrupted!");

				return new PersistentDictData();
			}
			return data;

		} else {
			Debug.LogWarning("Backup not found");
			return new PersistentDictData();
		}
	}
	//add integer parameter for different maps
	public PersistentDictData LoadData() {
		if (File.Exists(GetDataPath() + "/persistentDict.sav")) {
			BinaryFormatter bf = new();
			FileStream stream = new(GetDataPath() + "/persistentDict.sav", FileMode.Open);
			PersistentDictData data;
			try {
				data = bf.Deserialize(stream) as PersistentDictData;
				stream.Close();
			} catch {
				stream.Close();
				Debug.LogWarning("Cannot parse old data so trying backup now");

				return LoadBackupData();
			}
			SaveBackupData(data);
			data.intDict ??= new();
			data.floatDict ??= new();
			data.stringDict ??= new();
			data.intListDict ??= new();
			data.floatListDict ??= new();
			data.stringListDict ??= new();
			return data;

		} else if (File.Exists(GetDataPath() + "/persistentDictBackup.sav")) {
			Debug.Log("Trying backup");
			return LoadBackupData();
		} else {
			Debug.Log("New persistent save");
			return new PersistentDictData();
		}
	}
}

[System.Serializable]
public class PersistentDictData {
	public Dictionary<string, int> intDict;
	public Dictionary<string, float> floatDict;
	public Dictionary<string, string> stringDict;
	public Dictionary<string, List<int>> intListDict;
	public Dictionary<string, List<float>> floatListDict;
	public Dictionary<string, List<string>> stringListDict;

	public PersistentDictData() {
		intDict = new();
		floatDict = new();
		stringDict = new();
		intListDict = new();
		floatListDict = new();
		stringListDict = new();
	}
	public void SetIntList(string key, List<int> value) {
		if (intListDict.ContainsKey(key)) {
			intListDict[key] = value;
		} else {
			intListDict.Add(key, value);
		}
	}
	public void SetFloatList(string key, List<float> value) {
		if (floatListDict.ContainsKey(key)) {
			floatListDict[key] = value;
		} else {
			floatListDict.Add(key, value);
		}
	}
	public void SetStringList(string key, List<string> value) {
		if (stringListDict.ContainsKey(key)) {
			stringListDict[key] = value;
		} else {
			stringListDict.Add(key, value);
		}
	}
	public void SetInt(string key, int value) {
		if (intDict.ContainsKey(key)) {
			intDict[key] = value;
		} else {
			intDict.Add(key, value);
		}
	}
	public void SetFloat(string key, float value) {
		if (floatDict.ContainsKey(key)) {
			floatDict[key] = value;
		} else {
			floatDict.Add(key, value);
		}
	}
	public void SetString(string key, string value) {
		if (stringDict.ContainsKey(key)) {
			stringDict[key] = value;
		} else {
			stringDict.Add(key, value);
		}
	}

	public List<int> GetIntList(string key) {
		if (intListDict.ContainsKey(key)) {
			return intListDict[key];
		} else {
			return new List<int>();
		}
	}
	public List<float> GetFloatList(string key) {
		if (floatListDict.ContainsKey(key)) {
			return floatListDict[key];
		} else {
			return new List<float>();
		}
	}
	public List<string> GetStringList(string key) {
		if (stringListDict.ContainsKey(key)) {
			return stringListDict[key];
		} else {
			return new List<string>();
		}
	}
	public int GetInt(string key) {
		if (intDict.ContainsKey(key)) {
			return intDict[key];
		} else {
			return 0;
		}
	}
	public float GetFloat(string key) {
		if (floatDict.ContainsKey(key)) {
			return floatDict[key];
		} else {
			return 0f;
		}
	}
	public string GetString(string key) {
		if (stringDict.ContainsKey(key)) {
			return stringDict[key];
		} else {
			return "";
		}
	}
}