using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggleController : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Option")]
    [SerializeField] private bool startOpened = false;

    private void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(startOpened);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (inventoryPanel == null)
            return;

        bool nextState = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(nextState);

        if (nextState && inventoryUI != null)
        {
            inventoryUI.Refresh();
        }
    }
}