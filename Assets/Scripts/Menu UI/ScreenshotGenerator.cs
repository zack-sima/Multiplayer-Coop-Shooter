using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScreenshotGenerator : MonoBehaviour {
	public Camera mainCam;
	IEnumerator ReturnCanvas(GameObject canvas) {
		for (float i = 0; i < 0.5f; i += Time.deltaTime)
			yield return null;
		canvas.SetActive(true);
	}
	void Update() {
		//NOTE: this is for screenshotting and is only for the editor
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.N) && Input.GetKey(KeyCode.P)) {
			//no canvas screenshot
			GameObject go = GameObject.Find("Canvas");
			if (go == null)
				return;
			go.SetActive(false);
			ScreenCapture.CaptureScreenshot("/Users/Zack/Desktop/Screenshots/SC_" + Screen.width + "x" + Screen.height + "_" +
				System.DateTime.UtcNow.ToLongTimeString() + ".png");

			StartCoroutine(ReturnCanvas(go));
		} else if (Input.GetKey(KeyCode.C) && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.P)) {
			RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
			mainCam.targetTexture = renderTexture;
			mainCam.Render();

			RenderTexture.active = renderTexture;
			Texture2D texture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
			texture.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
			texture.Apply();

			byte[] byteArray = texture.EncodeToPNG();
			File.WriteAllBytes(Application.dataPath + $"/EditorData/SC_{System.DateTime.UtcNow.ToLongTimeString()}.png", byteArray);

			Debug.LogWarning("shot photo");

			//ScreenCapture.CaptureScreenshot($"/Users/Zack/Desktop/Screenshots/SC_{GetString("language")}" +
			//	Screen.width + "x" + Screen.height + "_" + System.DateTime.UtcNow.ToLongTimeString() + ".png");
		}
#endif
	}
}
