using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject questPanel;

    [Header("Quest Text")]
    [SerializeField] private TMP_Text normalQuestText;
    [SerializeField] private TMP_Text dailyQuestText;

    private void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestChanged += Refresh;
        }

        Refresh();
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        if (QuestManager.Instance == null)
        {
            HideAll();
            return;
        }

        bool hasNormalQuest = QuestManager.Instance.IsQuestAccepted(QuestType.Normal)
                              && !QuestManager.Instance.IsQuestRewarded(QuestType.Normal);

        bool hasDailyQuest = QuestManager.Instance.IsQuestAccepted(QuestType.Daily)
                             && !QuestManager.Instance.IsQuestRewarded(QuestType.Daily);

        if (normalQuestText != null)
        {
            normalQuestText.gameObject.SetActive(hasNormalQuest);

            if (hasNormalQuest)
            {
                int current = QuestManager.Instance.GetCurrentKillCount(QuestType.Normal);
                int target = QuestManager.Instance.GetTargetKillCount(QuestType.Normal);

                string state = QuestManager.Instance.IsQuestCompleted(QuestType.Normal)
                    ? "완료! NPC에게 보고"
                    : $"좀비 {target}마리 처치 {current}/{target}";

                normalQuestText.text =
                    $"일반퀘스트\n{state}";
            }
        }

        if (dailyQuestText != null)
        {
            dailyQuestText.gameObject.SetActive(hasDailyQuest);

            if (hasDailyQuest)
            {
                int current = QuestManager.Instance.GetCurrentKillCount(QuestType.Daily);
                int target = QuestManager.Instance.GetTargetKillCount(QuestType.Daily);

                string state = QuestManager.Instance.IsQuestCompleted(QuestType.Daily)
                    ? "완료! NPC에게 보고"
                    : $"좀비 {target}마리 처치 {current}/{target}";

                dailyQuestText.text =
                    $"일일퀘스트\n{state}";
            }
        }

        if (questPanel != null)
            questPanel.SetActive(hasNormalQuest || hasDailyQuest);
    }

    private void HideAll()
    {
        if (questPanel != null)
            questPanel.SetActive(false);

        if (normalQuestText != null)
            normalQuestText.gameObject.SetActive(false);

        if (dailyQuestText != null)
            dailyQuestText.gameObject.SetActive(false);
    }
}