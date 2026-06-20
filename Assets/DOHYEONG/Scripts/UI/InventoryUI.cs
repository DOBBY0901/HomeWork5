using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Target Item")]
    [SerializeField] private ItemData sellItemData;

    [Header("Slot UI")]
    [SerializeField] private GameObject itemSlotRoot;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TMP_Text itemCountText;

    [Header("Money UI")]
    [SerializeField] private TMP_Text moneyText;

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (InventoryManager.Instance == null)
            return;

        int itemCount = 0;

        if (sellItemData != null)
            itemCount = InventoryManager.Instance.GetItemCount(sellItemData);

        if (itemSlotRoot != null)
            itemSlotRoot.SetActive(itemCount > 0);

        if (itemIconImage != null && sellItemData != null)
            itemIconImage.sprite = sellItemData.icon;

        if (itemCountText != null && sellItemData != null)
            itemCountText.text = $"좀비 부산물 X {itemCount}";

        if (moneyText != null)
            moneyText.text = $"돈 : {InventoryManager.Instance.Money}";
    }
}