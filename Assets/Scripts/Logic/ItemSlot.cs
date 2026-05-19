using UnityEngine;

public abstract class ItemSlot
{
    public Transform Root;
    public SpriteRenderer Frame;
    public ItemView View;
    public bool IsPendingSpawn;

    public void SetActiveFrame(bool active)
    {
        Frame?.gameObject.SetActive(active);
    }
}
