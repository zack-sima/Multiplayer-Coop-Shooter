using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HullAnimatorBase : MonoBehaviour {

	#region References

	[SerializeField] private List<Renderer> teamColorRenderers;

	#endregion

	#region Functions

	//callback when player is spawned in/teleported so spider legs, etc don't try follow
	public virtual void Teleported() { }
	public void SetTeamMaterial(Material m) {
		foreach (Renderer r in teamColorRenderers) {
			List<Material> materials = new(r.materials) { [0] = m };
			r.SetMaterials(materials);
		}
	}

	#endregion
}
