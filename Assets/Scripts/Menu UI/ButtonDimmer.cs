using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// NOTE: old script
// this class should be attached to buttons; it allows multiple text/images to be dimmed when a button is pressed down,
// replacing the Unity button interface that only allows one image element to be dimmed

[RequireComponent(typeof(Button))]
public class ButtonDimmer : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler,
	IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
	[SerializeField] private bool includeAllChildren; //automatically dim all children
	[SerializeField] private bool excludeTexts; //don't dim texts
	[SerializeField] private bool isTransparent;
	[SerializeField] private bool bounceButton; //bounce animation
	[SerializeField] private List<TMP_Text> dimTexts;
	[SerializeField] private List<Image> dimImages;
	[SerializeField] private float dimDegree = 0.2f, bounceDegree = 0.07f;

	private Dictionary<TMP_Text, Color> dimTextColors = new();
	private Dictionary<Image, Color> dimImageColors = new();

	//set to true when joystick goes inside
	private bool buttonDown = false;
	private bool touchInButton = false;

	private Button self;

	private void ButtonUp() {
		if (self != null && self.interactable) {
			foreach (TMP_Text t in dimTexts)
				if (t) t.color = dimTextColors[t];
			foreach (Image i in dimImages)
				if (i) i.color = dimImageColors[i];
		}
		buttonDown = false;
		touchInButton = false;
	}
	private void OnEnable() {
		ButtonUp();
	}
	public void OnPointerClick(PointerEventData eventData) {
		if (PersistentDict.instance != null && PlayerPrefs.GetInt("mute_audio") == 0) {
			PersistentDict.instance.GetComponent<AudioSource>().Play();
		}
	}
	public void OnPointerDown(PointerEventData eventData) {
		if (self.interactable) {
			foreach (TMP_Text t in dimTexts)
				if (t) t.color = new Color(t.color.r, t.color.g, t.color.b, dimTextColors[t].a - dimDegree);

			if (isTransparent) {
				foreach (Image i in dimImages)
					if (i) i.color = new Color(i.color.r, i.color.g, i.color.b, dimImageColors[i].a - dimDegree);
			} else {
				foreach (Image i in dimImages)
					if (i) i.color = new Color(dimImageColors[i].r - dimDegree * 0.7f, dimImageColors[i].g - dimDegree * 0.7f,
						dimImageColors[i].b - dimDegree * 0.7f, dimImageColors[i].a);
			}
			buttonDown = true;
			touchInButton = true;
		}
	}

	public void OnPointerEnter(PointerEventData eventData) {
		if (buttonDown) touchInButton = true;
	}
	public void OnPointerExit(PointerEventData eventData) {
		touchInButton = false;
	}
	public void OnPointerUp(PointerEventData eventData) {
		ButtonUp();
	}
	void Start() {
		self = GetComponent<Button>();
		if (includeAllChildren) {
			foreach (Image i in GetComponentsInChildren<Image>()) {
				dimImages.Add(i);
			}
			if (!excludeTexts) {
				foreach (TMP_Text t in GetComponentsInChildren<TMP_Text>()) {
					dimTexts.Add(t);
				}
			}
		}
		foreach (Image i in dimImages) {
			dimImageColors.Add(i, i.color);
		}
		foreach (TMP_Text t in dimTexts) {
			dimTextColors.Add(t, t.color);
		}
	}
	float currentSize = 1f;
	bool deltaInteractable = true;
	void Update() {
		if (!self.interactable) {
			if (deltaInteractable) {
				foreach (TMP_Text t in dimTexts)
					if (t) t.color = new Color(t.color.r, t.color.g, t.color.b, dimTextColors[t].a - dimDegree * 2);

				if (isTransparent) {
					foreach (Image i in dimImages)
						if (i) i.color = new Color(i.color.r, i.color.g, i.color.b, dimImageColors[i].a - dimDegree);
				} else {
					foreach (Image i in dimImages)
						if (i) i.color = new Color(dimImageColors[i].r - dimDegree, dimImageColors[i].g - dimDegree,
							dimImageColors[i].b - dimDegree, dimImageColors[i].a);
				}
			}
		} else if (!deltaInteractable) {
			foreach (TMP_Text t in dimTexts)
				if (t) t.color = dimTextColors[t];
			foreach (Image i in dimImages)
				if (i) i.color = dimImageColors[i];
		}
		deltaInteractable = self.interactable;

		//assume frame rate (delta time can't be used when paused!)
		float standardDeltaTime = 1f / 60f;

		if (bounceButton) {
			if (touchInButton) {
				float threshold = 1f - bounceDegree;
				if (currentSize > threshold) {
					currentSize = Mathf.Max(threshold, currentSize - standardDeltaTime * 2f);
					transform.localScale = new Vector2(currentSize, currentSize);
				}
			} else {
				if (currentSize < 1f) {
					currentSize = Mathf.Min(1f, currentSize + standardDeltaTime * 3.5f);
					transform.localScale = new Vector2(currentSize, currentSize);
				}
			}
		}
	}
}
