using System;
using System.Collections;
using System.Collections.Generic;
using Lobby;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class HullCard : SelectionCard {
    [SerializeField] private string HullName;
    public override string uid { get => HullName; }
    
    [SerializeField]
    private ProceduralImage outline;
    
    private void Start() {
        LobbyEventsHandler.OnHullSelect += OnDeselect;

        if (PlayerPrefs.GetString("hull_name") == uid) OnSelect();
    }

    private void OnDestroy() {
        LobbyEventsHandler.OnHullSelect -= OnDeselect;
    }

    public override void OnSelect() {
        LobbyEventsHandler.RaiseHullSelect();
        outline.color = Color.white;
        PlayerPrefs.SetString("hull_name", HullName);
    }

    public override void OnDeselect() {
        outline.color = Color.black;
    }
}