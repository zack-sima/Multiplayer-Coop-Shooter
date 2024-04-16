using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerModelInteractions : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    private Vector3 mPrevPos = Vector3.zero;
    private Vector3 mPosDelta = Vector3.zero;

    private Transform background;

    private bool isDragging = false;

    private Coroutine coroutine;

    public void OnPointerDown(PointerEventData pointerEventData) {
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData pointerEventData) {
        isDragging = false;
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = null;
        coroutine = StartCoroutine(ResetRotation());
    }

    private void Start() {
        background = transform.GetChild(0);
    }

    private void Update() {
        if (isDragging) {
            mPosDelta = Input.mousePosition - mPrevPos;
            background.Rotate(background.up, Vector3.Dot(mPosDelta, Camera.main.transform.right), Space.World);
        }

        mPrevPos = Input.mousePosition;
    }

    private IEnumerator ResetRotation() {
        while (background.localRotation.y != 0) {
            if (!isDragging) {
                yield return new WaitForSeconds(3f);
                if (!isDragging) {
                    background.DORotate(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.OutQuint);
                }
            }
            yield return null;
        }
    }
}