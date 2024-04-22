using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Just animates the tank tracks (as a part of the tank hull)

public class TankTracksAnimator : MonoBehaviour {

	#region References

	[SerializeField] private List<Transform> tracks, wheels;

	#endregion

	#region Members

	private List<Vector3> trackPositions;
	private List<Quaternion> trackRotations;

	//lerps tracks; should be between 0 and 1
	private float trackProgress = 0f;
	private float speed = 0f;
	public void SetSpeed(float s) { speed = s; }

	#endregion

	void Start() {
		trackPositions = new();
		trackRotations = new();

		foreach (Transform t in tracks) {
			trackPositions.Add(t.localPosition);
			trackRotations.Add(t.localRotation);
		}
	}
	void Update() {
		if (speed < 0) {
			trackProgress += Time.deltaTime * speed;
			if (trackProgress < 0f) trackProgress = 1f;
		} else {
			trackProgress = (trackProgress + Time.deltaTime * speed) % 1f;
		}

		for (int index = 0; index < tracks.Count; index++) {
			//wraps around for last track link
			int nextIndex = (index < tracks.Count - 1) ? index + 1 : 0;

			tracks[index].SetLocalPositionAndRotation(
				Vector3.Lerp(trackPositions[index], trackPositions[nextIndex], 1f - trackProgress),
				Quaternion.Lerp(trackRotations[index], trackRotations[nextIndex], 1f - trackProgress)
			);
		}
		foreach (Transform t in wheels) {
			t.localEulerAngles = new Vector3(trackProgress * 45f, 0, 0);
		}
	}
}
