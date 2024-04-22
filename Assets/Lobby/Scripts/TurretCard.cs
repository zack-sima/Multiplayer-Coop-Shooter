using System;
using System.Collections;
using System.Collections.Generic;
using Lobby;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class TurretCard : SelectionCard {
    [SerializeField] private string TurretName;
    public override string uid { get => TurretName; }
    
    [SerializeField]
    private ProceduralImage outline;
    
    private void Start() {
        LobbyEventsHandler.OnTurretSelect += OnDeselect;

        if (PlayerPrefs.GetString("turret_name") == uid) OnSelect();
    }

    private void OnDestroy() {
        LobbyEventsHandler.OnTurretSelect -= OnDeselect;
    }

    public override void OnSelect() {
        LobbyEventsHandler.RaiseTurretSelect();
        outline.color = Color.white;
        PlayerPrefs.SetString("turret_name", TurretName);
    }

    public override void OnDeselect() {
        outline.color = Color.black;
    }
}