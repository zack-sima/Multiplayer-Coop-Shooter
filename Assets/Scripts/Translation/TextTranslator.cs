using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class TextTranslator : MonoBehaviour {
	public static List<TextTranslator> allTexts = new List<TextTranslator>();

	private string englishTranslation;

	void Start() {
		englishTranslation = GetComponent<TMP_Text>().text;

		ReTranslate();

		allTexts.Add(this);
	}
	public void ReTranslate() {
		GetComponent<TMP_Text>().text = Translator.Translate(englishTranslation);
	}

	public static void UpdateTexts() {
		foreach (TextTranslator t in allTexts) {
			if (t) t.ReTranslate();
		}
	}
}