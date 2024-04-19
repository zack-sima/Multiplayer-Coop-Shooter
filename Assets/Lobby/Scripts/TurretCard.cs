using System;
using System.Collections;
using System.Collections.Generic;
using Lobby;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class TurretCard : MonoBehaviour {
    [SerializeField] private int TurretIndex;
    [SerializeField] private string TurretName;
    private ProceduralImage outline;


    private void Start() {
        outline = transform.GetChild(0).GetComponent<ProceduralImage>();

        LobbyEventsHandler.OnTurretSelect += OnDeselect;

        if (PlayerPrefs.GetInt("turret_index") == TurretIndex) OnSelect();
    }

    private void OnDestroy() {
        LobbyEventsHandler.OnTurretSelect -= OnDeselect;
    }

    public void OnSelect() {
        LobbyEventsHandler.RaiseTurretSelect();
        outline.color = Color.white;
        PlayerPrefs.SetInt("turret_index", TurretIndex);
        PlayerPrefs.SetString("turret_name", TurretName);
    }

    private void OnDeselect() {
        outline.color = Color.black;
    }
}