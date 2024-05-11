using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class CapturePoint : MonoBehaviour {

	#region Statics & Consts

	public enum Mode { Points, Money }

	#endregion

	#region References

	[SerializeField] private Image progressBar;

	[SerializeField] private MeshRenderer colorMesh;
	[SerializeField] private int colorMeshIndex; //what index to replace material with

	//materials
	[SerializeField] private Material neutralMaterial;
	[SerializeField] private TeamMaterialManager teamMaterials;

	#endregion

	#region Members

	[SerializeField] private float captureRadius;
	[SerializeField] private float captureSpeed;
	[SerializeField] private float pointsPerSecond;

	[SerializeField] private Mode captureMode;
	public Mode GetCaptureMode() { return captureMode; }

	private float storedPoints = 0;

	private float captureProgress = 0;
	public float GetCaptureProgress() { return captureProgress; }

	//-1 means it is neutral
	private int ownerTeam = -1;
	public int GetPointOwnerTeam() { return ownerTeam; }

	//this will clear stored points (call this every frame)
	//  can be for team points, money, etc
	public int GetPoints() {
		int points = (int)storedPoints;
		storedPoints %= 1;
		return points;
	}

	#endregion

	#region Functions

	//updates the images/color to correspond with values
	private void UpdateVisuals() {
		//set pad material
		Material mat = neutralMaterial;
		if (ownerTeam >= 0)
			mat = teamMaterials.GetTeamColor(ownerTeam);
		if (mat != colorMesh.materials[colorMeshIndex]) {
			List<Material> mats = new(colorMesh.materials) {
				[colorMeshIndex] = mat
			};
			colorMesh.SetMaterials(mats);
		}

		//set progress bar
		progressBar.fillAmount = captureProgress;
	}

	//NOTE: called by GameStatsSyncer non-master client to fetch synced progress
	public void SetClientCaptureProgress(float captureProgress, int ownerTeam) {
		if (this.ownerTeam != ownerTeam || this.captureProgress != captureProgress) {
			Debug.Log("client change");
			this.ownerTeam = ownerTeam;
			this.captureProgress = captureProgress;
			UpdateVisuals();
		}
	}

	//NOTE: called by GameStatsSyncer master client every frame
	public void UpdateCaptureProgress(List<CombatEntity> entities) {
		bool changedOwnership = false;

		//capturing point -1 means no one there, 0/1 means team, -2 means contested
		int capturingTeam = -1;

		foreach (CombatEntity e in entities) {
			if (e == null || e.GetNetworker().GetIsDead()) continue;

			if (AIBrain.GroundDistance(e.transform.position, transform.position) < captureRadius) {
				if (capturingTeam == -1) {
					capturingTeam = e.GetTeam();
				} else if (e.GetTeam() != capturingTeam) {
					capturingTeam = -2; //set as contested
				}
			}
		}
		if (capturingTeam == -1) {
			if (ownerTeam == -1) {
				//case 1: current point is neutral
				captureProgress = Mathf.Max(0, captureProgress - captureSpeed * Time.deltaTime);
			} else {
				//case 2: someone owns the point
				captureProgress = Mathf.Min(1, captureProgress + captureSpeed * Time.deltaTime);
			}
		} else if (capturingTeam > -1) {
			if (ownerTeam == -1) {
				//case 1: current point is neutral
				captureProgress += captureSpeed * Time.deltaTime;
				if (captureProgress >= 1) {
					ownerTeam = capturingTeam;
					captureProgress = 1;
					changedOwnership = true;
				}
			} else if (ownerTeam != capturingTeam) {
				//case 2: current point owner is not capping team
				captureProgress -= captureSpeed * Time.deltaTime;
				if (captureProgress <= 0) {
					ownerTeam = -1;
					captureProgress = 0;
					changedOwnership = true;
				}
			} else {
				//case 3: current point owner is the capping team
				captureProgress = Mathf.Min(1, captureProgress + captureSpeed * Time.deltaTime);
			}
		}
		if (changedOwnership) storedPoints = 0;

		//reward points to team controlling point
		if (ownerTeam != -1) {
			storedPoints += pointsPerSecond * Time.deltaTime;
		}

		UpdateVisuals();
	}

	#endregion

}
