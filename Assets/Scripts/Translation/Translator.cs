using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//to access translations, call Translator.Translate(key)
public class Translator {
	//anything that doesn't need to be translated from code english to written english is left blank
	private static Dictionary<string, string> localizedEN, localizedCN, localizedES, localizedRU, localizedPL,
		localizedFR, localizedJP, localizedPU, localizedUA, localizedKO, localizedDE, localizedVN, localizedAB, localizedHI;
	public static bool isInit;
	public static void Init() {
		localizedEN = new();
		localizedES = new();
		localizedRU = new();
		localizedPL = new();
		localizedFR = new();
		localizedJP = new();
		localizedCN = new();
		localizedPU = new();
		localizedUA = new();
		localizedKO = new();
		localizedDE = new();
		localizedVN = new();
		localizedAB = new();
		localizedHI = new();

		CSVLoader csvLoader = new CSVLoader();
		csvLoader.LoadCSV("Translations");

		localizedEN = localizedEN.Concat(csvLoader.GetDictionaryValuies("English").
			Where(x => !localizedEN.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);
		localizedCN = localizedCN.Concat(csvLoader.GetDictionaryValuies("Chinese").
			Where(x => !localizedCN.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);
		localizedES = localizedES.Concat(csvLoader.GetDictionaryValuies("Spanish").
			Where(x => !localizedES.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);
		localizedRU = localizedRU.Concat(csvLoader.GetDictionaryValuies("Russian").
			Where(x => !localizedRU.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);
		localizedPL = localizedPL.Concat(csvLoader.GetDictionaryValuies("Polish").
			Where(x => !localizedPL.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);
		localizedFR = localizedFR.Concat(csvLoader.GetDictionaryValuies("French").
			Where(x => !localizedFR.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);
		localizedJP = localizedJP.Concat(csvLoader.GetDictionaryValuies("Japanese").
			Where(x => !localizedJP.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);
		localizedPU = localizedPU.Concat(csvLoader.GetDictionaryValuies("Portuguese").
			Where(x => !localizedPU.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);
		localizedUA = localizedUA.Concat(csvLoader.GetDictionaryValuies("Ukrainian").
			Where(x => !localizedUA.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);
		localizedKO = localizedKO.Concat(csvLoader.GetDictionaryValuies("Korean").
			Where(x => !localizedKO.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);
		localizedDE = localizedDE.Concat(csvLoader.GetDictionaryValuies("German").
			Where(x => !localizedDE.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);
		localizedVN = localizedVN.Concat(csvLoader.GetDictionaryValuies("Vietnamese").
			Where(x => !localizedVN.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);
		localizedAB = localizedAB.Concat(csvLoader.GetDictionaryValuies("Arabic").
			Where(x => !localizedAB.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);
		localizedHI = localizedHI.Concat(csvLoader.GetDictionaryValuies("Hindi").
			Where(x => !localizedHI.Keys.Contains(x.Key))).ToDictionary(x => x.Key, x => x.Value);

		isInit = true;
	}
	//note: earmark server text with backticks and translate locally later
	public static string PrepareTranslate(string key) {
		return "`" + key + "`";
	}
	//translates everything enclosed by backticks
	public static string ServerUnpackTranslate(string key) {
		StringBuilder s = new StringBuilder();
		StringBuilder tempTranslate = new StringBuilder();
		bool inTranslation = false;
		foreach (char c in key) {
			if (c == '`') {
				if (!inTranslation) {
					//start saving string
					inTranslation = true;
				} else {
					//translate this whole block
					inTranslation = false;
					s.Append(Translate(tempTranslate.ToString()));
					tempTranslate.Clear();
				}
			} else {
				if (inTranslation) {
					tempTranslate.Append(c);
				} else {
					s.Append(c);
				}
			}
		}
		return s.ToString();
	}
	//for TMP indexing
	public static Dictionary<string, string> GetAllValues(string language) {
		if (!isInit) Init();

		switch (language) {
			case "Chinese":
				return localizedCN;
			case "Spanish":
				return localizedES;
			case "Russian":
				return localizedRU;
			case "French":
				return localizedFR;
			case "Japanese":
				return localizedJP;
			case "Korean":
				return localizedKO;
			case "Polish":
				return localizedPL;
			case "Ukrainian":
				return localizedUA;
			case "German":
				return localizedDE;
			case "Portuguese":
				return localizedPU;
			case "Arabic":
				return localizedAB;
			case "Vietnamese":
				return localizedVN;
			case "Hindi":
				return localizedHI;
			default: //english
				return localizedEN;
		}
	}
	public static string Translate(string key) {
		Dictionary<string, string> dict = GetAllValues(PlayerPrefs.GetString("language"));

		if (dict.TryGetValue(key, out string value)) {
			if (value == "") return key;
			return value;
		} else {
			return key;
		}
	}
}
