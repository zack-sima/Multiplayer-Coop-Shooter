using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class DropdownTranslator : MonoBehaviour {
	public static List<DropdownTranslator> allDropdowns = new List<DropdownTranslator>();

	private string[] englishTranslation;

	void Start() {
		TMP_Dropdown d = GetComponent<TMP_Dropdown>();

		englishTranslation = new string[d.options.Count];
		int index = 0;
		foreach (TMP_Dropdown.OptionData i in d.options) {
			englishTranslation[index] = i.text;
			index++;
		}

		ReTranslate();

		allDropdowns.Add(this);

		GetComponent<TMP_Dropdown>().RefreshShownValue();
	}
	public static void UpdateDropdowns() {
		foreach (DropdownTranslator t in allDropdowns) {
			if (t) t.ReTranslate();
		}
	}
	public void ReTranslate() {
		TMP_Dropdown d = GetComponent<TMP_Dropdown>();

		int index = 0;
		foreach (TMP_Dropdown.OptionData i in d.options) {
			i.text = Translator.Translate(englishTranslation[index]);
			index++;
		}

		d.captionText.text = d.options[0].text;
	}
}
