using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAnimatorBase : MonoBehaviour {

	#region References

	[SerializeField] private List<Renderer> teamColorRenderers;

	#endregion

	#region Functions

	public void SetTeamMaterial(Material m) {
		foreach (Renderer r in teamColorRenderers) {
			List<Material> materials = new(r.materials) { [0] = m };
			r.SetMaterials(materials);
		}
	}

	public virtual void FireMainWeapon() { }

	#endregion

}
