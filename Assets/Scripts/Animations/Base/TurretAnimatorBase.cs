using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAnimatorBase : MonoBehaviour {

	#region References

	[SerializeField] private List<Renderer> teamColorRenderers;

	//for blend models where the material ordering got messed up
	[SerializeField] private List<Renderer> teamColor2ndMatRenderers;

	[SerializeField] private List<int> teamColorRendererIndices;

	#endregion

	#region Functions

	public void SetTeamMaterial(Material m) {
		int index = 0;
		foreach (Renderer r in teamColorRenderers) {
			int i = 0;
			if (teamColorRendererIndices.Count > index) {
				i = teamColorRendererIndices[index];
			}
			List<Material> materials = new(r.materials) { [i] = m };
			r.SetMaterials(materials);
			index++;
		}
		foreach (Renderer r in teamColor2ndMatRenderers) {
			List<Material> materials = new(r.materials) { [1] = m };
			r.SetMaterials(materials);
		}
	}

	//to fix the bug where muzzle is on when dead
	public virtual void ResetAnimations() { }
	public virtual void FireMainWeapon(int bulletIndex) { }

	#endregion

}
