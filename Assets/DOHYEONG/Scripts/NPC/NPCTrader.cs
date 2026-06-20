using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCTrader : MonoBehaviour
{
    [Header("World Interaction UI")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TMP_Text interactionText;

    [Header("Messages")]
    [SerializeField] private string canTradeMessage = "아이템 판매 [E]";
    [SerializeField] private string noItemMessage = "판매할 아이템이 없습니다";
    [SerializeField] private string soldMessage = "판매 완료";

    [Header("Option")]
    [SerializeField] private float messageDuration = 1.2f;

    private bool playerInRange;
    private float messageTimer;

    private void Start()
    {
        HideInteractionUI();
    }

    private void Update()
    {
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;

            if (messageTimer <= 0f && playerInRange)
            {
                ShowInteractionUI(canTradeMessage);
            }
        }

        if (!playerInRange)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.eKey.wasPressedThisFrame)
        {
            Trade();
        }
    }

    private void Trade()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager가 씬에 없습니다.");
            return;
        }

        int earnedMoney = InventoryManager.Instance.SellAllItems();

        if (earnedMoney <= 0)
        {
            ShowTemporaryMessage(noItemMessage);
            Debug.Log("판매할 아이템이 없습니다.");
            return;
        }

        ShowTemporaryMessage($"{soldMessage} +{earnedMoney}");
        Debug.Log($"거래 완료. 획득 돈: {earnedMoney}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        ShowInteractionUI(canTradeMessage);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        HideInteractionUI();
    }

    private void ShowTemporaryMessage(string message)
    {
        ShowInteractionUI(message);
        messageTimer = messageDuration;
    }

    private void ShowInteractionUI(string message)
    {
        if (interactionUI != null)
            interactionUI.SetActive(true);

        if (interactionText != null)
            interactionText.text = message;
    }

    private void HideInteractionUI()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }
}