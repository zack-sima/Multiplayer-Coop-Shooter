using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraBlur : MonoBehaviour {
	[SerializeField] private Volume volume;

	private DepthOfField dpf = null;

	private float currentBlur = 0f;

	private void Start() {
		volume.profile.TryGet(out dpf);
	}
	private void Update() {
		if (dpf != null) {
			dpf.focalLength.value = currentBlur * 250f;
		}
	}
	public void SetBlur(float blur) {
		StartCoroutine(GradualBlur(blur));
	}
	private IEnumerator GradualBlur(float targetBlur) {
		float speed = 5f;
		while (currentBlur > targetBlur) {
			currentBlur = Mathf.Max(targetBlur, currentBlur - Time.deltaTime * speed);
			yield return new WaitForEndOfFrame();
		}
		while (currentBlur < targetBlur) {
			currentBlur = Mathf.Min(targetBlur, currentBlur + Time.deltaTime * speed);
			yield return new WaitForEndOfFrame();
		}
	}
}
