using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestNPC : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private QuestType questType = QuestType.Normal;

    [Header("World UI")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TMP_Text interactionText;

    [Header("Messages")]
    [SerializeField] private string acceptMessage = "Äù½ºÆ® ¹Þ±â [E]";
    [SerializeField] private string progressMessage = "ÁøÇà Áß";
    [SerializeField] private string completeMessage = "Äù½ºÆ® ¿Ï·á [E]";
    [SerializeField] private string rewardedMessage = "¿Ï·áµÊ";
    [SerializeField] private string dailyDoneMessage = "¿À´Ã ¿Ï·áµÊ";

    [Header("Option")]
    [SerializeField] private float messageDuration = 1.2f;

    private bool playerInRange;
    private float messageTimer;

    private void Start()
    {
        HideUI();

        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestChanged += RefreshMessage;
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestChanged -= RefreshMessage;
    }

    private void Update()
    {
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;

            if (messageTimer <= 0f && playerInRange)
                RefreshMessage();
        }

        if (!playerInRange)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    private void Interact()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestManager°¡ ¾À¿¡ ¾ø½À´Ï´Ù.");
            return;
        }

        if (QuestManager.Instance.CanAcceptQuest(questType))
        {
            QuestManager.Instance.AcceptQuest(questType);
            ShowTemporaryMessage("Äù½ºÆ® ¼ö¶ô!");
            return;
        }

        if (QuestManager.Instance.IsQuestCompleted(questType) &&
            !QuestManager.Instance.IsQuestRewarded(questType))
        {
            int reward = QuestManager.Instance.CompleteQuestAndGetReward(questType);
            ShowTemporaryMessage($"º¸»ó È¹µæ +{reward}");
            return;
        }

        RefreshMessage();
    }

    private void RefreshMessage()
    {
        if (!playerInRange)
            return;

        if (QuestManager.Instance == null)
            return;

        string message = GetCurrentMessage();
        ShowUI(message);
    }

    private string GetCurrentMessage()
    {
        if (QuestManager.Instance.CanAcceptQuest(questType))
            return acceptMessage;

        if (QuestManager.Instance.IsQuestRewarded(questType))
        {
            if (questType == QuestType.Daily)
                return dailyDoneMessage;

            return rewardedMessage;
        }

        if (QuestManager.Instance.IsQuestCompleted(questType))
            return completeMessage;

        if (QuestManager.Instance.IsQuestAccepted(questType))
        {
            int current = QuestManager.Instance.GetCurrentKillCount(questType);
            int target = QuestManager.Instance.GetTargetKillCount(questType);

            return $"{progressMessage} {current}/{target}";
        }

        return acceptMessage;
    }

    private void ShowTemporaryMessage(string message)
    {
        ShowUI(message);
        messageTimer = messageDuration;
    }

    private void ShowUI(string message)
    {
        if (interactionUI != null)
            interactionUI.SetActive(true);

        if (interactionText != null)
            interactionText.text = message;
    }

    private void HideUI()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        RefreshMessage();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        HideUI();
    }
}