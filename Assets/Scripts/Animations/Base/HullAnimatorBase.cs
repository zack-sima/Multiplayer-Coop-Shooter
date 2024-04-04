using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HullAnimatorBase : MonoBehaviour {

	#region References

	[SerializeField] private List<Renderer> teamColorRenderers;

	#endregion

	#region Functions

	public void SetTeamMaterial(Material m) {
		Debug.Log(m.name);
		foreach (Renderer r in teamColorRenderers) {
			List<Material> materials = new(r.materials) { [0] = m };
			r.SetMaterials(materials);
		}
	}

	#endregion
}
