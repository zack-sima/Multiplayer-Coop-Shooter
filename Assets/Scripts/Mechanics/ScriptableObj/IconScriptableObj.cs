using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [CreateAssetMenu(menuName = "IconStorage")]
public class IconScriptableObj : ScriptableObject {

	[System.Serializable]
	public class IconBundle { // Purpose is ONE icon for the upgrade card and the a seperate DISTINCT icon for top right.
		public string id;
		public Sprite icon;
	}

	[System.Serializable]
	public class IconBundleGadget { // Purpose is ONE icon for both the upgrade card and the top right.
		public string id;
		public string childrenid;
		public string GetChildrenId() { return id + childrenid; }
		public Sprite icon;
	}

	[Header("Active Icons")]
	[SerializeField] private List<IconBundle> activesIcons;

	[Header("Ability Icons")] //actives except only whole abilities
	[SerializeField] private List<IconBundle> abilitiesIcons;

	[Header("Gadget Icons")]
	[SerializeField] private List<IconBundleGadget> gadgetIcons;

	public List<IconBundle> GetAbilitiesIcons() { return abilitiesIcons; }
	public List<IconBundle> GetActivesIcons() { return activesIcons; }
	public List<IconBundleGadget> GetGadgetIcons() { return gadgetIcons; }

	public bool TryGetIcon(string id, out Sprite icon) {
		foreach (IconBundle bundle in activesIcons) {
			if (bundle.id == id) {
				icon = bundle.icon;
				return true;
			}
		}
		foreach (IconBundleGadget bundle in gadgetIcons) {
			if (bundle.id == id) {
				icon = bundle.icon;
				return true;
			}
			if (bundle.GetChildrenId() == id) {
				icon = bundle.icon;
				return true;
			}
		}
		icon = null;
		DebugUIManager.instance.LogError("IconScriptObj", "Icon with id " + id + " not found.");
		return false;
	}
}
