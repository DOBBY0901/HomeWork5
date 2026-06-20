using UnityEngine;

public class WorldItem : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        TryPickup();
    }

    public void TryPickup()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager가 씬에 없습니다.");
            return;
        }

        bool added = InventoryManager.Instance.AddItem(itemData, amount);

        if (added)
        {
            Destroy(gameObject);
        }
    }
}