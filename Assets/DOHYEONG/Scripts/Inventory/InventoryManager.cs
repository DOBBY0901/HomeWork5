using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory")]
    [SerializeField] private int maxSlotCount = 10;
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

    [Header("Money")]
    [SerializeField] private int money = 0;

    public IReadOnlyList<InventorySlot> Slots => slots;
    public int Money => money;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
            return false;

        InventorySlot slot = FindSlot(item);

        if (slot != null)
        {
            slot.AddAmount(amount);
            Debug.Log($"아이템 획득: {item.itemName} x{amount} / 보유 {slot.amount}");
            return true;
        }

        if (slots.Count >= maxSlotCount)
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
            return false;
        }

        slots.Add(new InventorySlot(item, amount));
        Debug.Log($"새 아이템 획득: {item.itemName} x{amount}");

        return true;
    }

    public int SellAllItems()
    {
        int totalMoney = 0;

        for (int i = slots.Count - 1; i >= 0; i--)
        {
            InventorySlot slot = slots[i];

            if (slot == null || slot.IsEmpty)
                continue;

            int price = slot.item.sellPrice * slot.amount;
            totalMoney += price;

            Debug.Log($"판매: {slot.item.itemName} x{slot.amount} = {price}");

            slots.RemoveAt(i);
        }

        money += totalMoney;

        Debug.Log($"전체 판매 완료. 획득 돈: {totalMoney}, 현재 돈: {money}");

        return totalMoney;
    }

    public int GetItemCount(ItemData item)
    {
        InventorySlot slot = FindSlot(item);

        if (slot == null)
            return 0;

        return slot.amount;
    }

    private InventorySlot FindSlot(ItemData item)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
                return slots[i];
        }

        return null;
    }
}