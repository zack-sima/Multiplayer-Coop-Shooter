using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "Team Materials Manager")]
public class TeamMaterialManager : ScriptableObject {
	[SerializeField] private List<Material> teamColors;

	public Material GetTeamColor(int teamIndex) {
		if (teamIndex < 0 || teamIndex >= teamColors.Count) return null;
		return teamColors[teamIndex];
	}
}
