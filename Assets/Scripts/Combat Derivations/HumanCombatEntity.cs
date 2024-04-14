using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HumanCombatEntity : CombatEntity {

	#region Statics & Consts

	#endregion

	#region References

    [Header("UI")]
    [SerializeField] private GameObject lobUIPrefab;
    [SerializeField] private GameObject lobSectionPrefab;

	#endregion

	#region Members


    private List<GameObject> lobSections = new();
    private Vector3 targetLocation;
    private bool isLobber = false;

	#endregion

	#region Functions

    //0.0001

    private void InitLobUI() {
       
    }

    private void ScaleLobUI(Vector3 location) {

    }

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
        if (GetTurret() is Mortar) isLobber = true;
        InitLobUI();
    }

    // Update is called once per frame
    protected override void Update() {
        base.Update();
        if (isLobber) ScaleLobUI(targetLocation);
    }

    #endregion
}
