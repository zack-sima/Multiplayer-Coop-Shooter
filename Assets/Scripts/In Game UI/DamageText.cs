using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour {

	[SerializeField] private TMP_Text damageText;

	private float scale = 1f;
	private int damage = 0;
	public int GetDamage() { return damage; }

	float existedFor = 0f;
	bool noBubble = false;
	bool enabled = false;

	Vector3 randOffset = Vector3.zero;
	public Vector3 GetRandOffset() { return randOffset; }

	public void SetDamageText(int damage, Color color, float scale, bool noBubble, Vector3 randOffset) {
		this.damage = damage;
		damageText.text = damage.ToString();
		damageText.color = color;
		this.scale = scale;
		this.noBubble = noBubble;
		enabled = true;
		this.randOffset = randOffset;
	}
	private IEnumerator Timer() {
		float transparencyOffset = 0.1f;
		float start = 0.35f;
		float startPercent = 0f;

		if (noBubble) {
			startPercent = 0f;
			start = 0f;
		}

		for (float i = startPercent; i < start; i += Time.deltaTime) {
			damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b,
				i / start - transparencyOffset);
			transform.GetChild(0).localScale = scale * 0.5f * (i / start + 0.3f) * Vector3.one;
			yield return new WaitForEndOfFrame();
		}
		damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b,
			1f - transparencyOffset);
		transform.GetChild(0).localScale = scale * 0.5f * 1.3f * Vector3.one;

		yield return new WaitForSeconds(0.3f);

		float fade = 1f;
		for (float i = 0; i < fade; i += Time.deltaTime) {
			damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b,
				1f - i / fade - transparencyOffset);
			yield return new WaitForEndOfFrame();
		}
		Destroy(gameObject);
	}
	//if damage was recently done stack it to another hit
	public bool CanStack() {
		return existedFor < 0.2f;
	}
	private void Start() {
		transform.GetChild(0).localScale = scale * 0.5f * Vector3.one;
		damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, 0);
		StartCoroutine(Timer());
	}
	private void Update() {
		if (!enabled) {
			damageText.color = new Color(0, 0, 0, 0);
			Debug.LogWarning("not active");
			return;
		}
		existedFor += Time.deltaTime;

		if (existedFor > 0.15f)
			transform.Translate(Vector3.up * Time.deltaTime * 0.5f);
	}
}
