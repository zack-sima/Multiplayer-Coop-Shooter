using UnityEngine;
public abstract class SelectionCard: MonoBehaviour {
    public abstract string uid { get; }
    
    public abstract void OnSelect();

    public abstract void OnDeselect();
}
