using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Audio sources that need frequency limiting & pitch randomness call this to make sure sounds don't clip and are unique.
/// </summary>

public class AudioSourceController : MonoBehaviour {

	public static AudioSourceController instance;

	private readonly Dictionary<string, float> soundsLastTimestamps = new();

	/// <summary>
	/// If true is returned, it is assumed the sound will be played
	/// </summary>
	public static bool CanPlaySound(string soundName, float cutoffThresholdSeconds) {
		if (!instance.soundsLastTimestamps.ContainsKey(soundName)) {
			instance.soundsLastTimestamps.Add(soundName, Time.time);
			return true;
		}
		if (Time.time - instance.soundsLastTimestamps[soundName] > cutoffThresholdSeconds) {
			instance.soundsLastTimestamps[soundName] = Time.time;
			return true;
		}
		return false;
	}

	private void Awake() {
		instance = this;
	}
}
