using System;
using UnityEngine;

public enum QuestType
{
    Normal,
    Daily
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public event Action OnQuestChanged;

    [Header("Normal Quest")]
    [SerializeField] private bool normalAccepted;
    [SerializeField] private bool normalCompleted;
    [SerializeField] private bool normalRewarded;
    [SerializeField] private int normalTargetKillCount = 5;
    [SerializeField] private int normalCurrentKillCount;
    [SerializeField] private int normalRewardMoney = 50;

    [Header("Daily Quest")]
    [SerializeField] private bool dailyAccepted;
    [SerializeField] private bool dailyCompleted;
    [SerializeField] private bool dailyRewarded;
    [SerializeField] private int dailyTargetKillCount = 3;
    [SerializeField] private int dailyCurrentKillCount;
    [SerializeField] private int dailyRewardMoney = 30;

    public int NormalCurrentKillCount => normalCurrentKillCount;
    public int NormalTargetKillCount => normalTargetKillCount;

    public int DailyCurrentKillCount => dailyCurrentKillCount;
    public int DailyTargetKillCount => dailyTargetKillCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (GameClockManager.Instance != null)
        {
            GameClockManager.Instance.OnNewDayStarted += ResetDailyQuest;
        }
    }

    private void OnDestroy()
    {
        if (GameClockManager.Instance != null)
        {
            GameClockManager.Instance.OnNewDayStarted -= ResetDailyQuest;
        }
    }

    public void AcceptQuest(QuestType questType)
    {
        if (questType == QuestType.Normal)
        {
            if (normalRewarded)
                return;

            if (normalAccepted)
                return;

            normalAccepted = true;
            normalCompleted = false;
            normalCurrentKillCount = 0;

            Debug.Log("일반 퀘스트 수락: 좀비 처치");
        }
        else
        {
            if (dailyRewarded)
                return;

            if (dailyAccepted)
                return;

            dailyAccepted = true;
            dailyCompleted = false;
            dailyCurrentKillCount = 0;

            Debug.Log("일일 퀘스트 수락: 좀비 처치");
        }

        OnQuestChanged?.Invoke();
    }

    public void AddZombieKill()
    {
        if (normalAccepted && !normalCompleted && !normalRewarded)
        {
            normalCurrentKillCount++;

            if (normalCurrentKillCount >= normalTargetKillCount)
            {
                normalCurrentKillCount = normalTargetKillCount;
                normalCompleted = true;
                Debug.Log("일반 퀘스트 목표 달성");
            }
        }

        if (dailyAccepted && !dailyCompleted && !dailyRewarded)
        {
            dailyCurrentKillCount++;

            if (dailyCurrentKillCount >= dailyTargetKillCount)
            {
                dailyCurrentKillCount = dailyTargetKillCount;
                dailyCompleted = true;
                Debug.Log("일일 퀘스트 목표 달성");
            }
        }

        OnQuestChanged?.Invoke();
    }

    public bool CanAcceptQuest(QuestType questType)
    {
        if (questType == QuestType.Normal)
            return !normalAccepted && !normalRewarded;

        return !dailyAccepted && !dailyRewarded;
    }

    public bool IsQuestAccepted(QuestType questType)
    {
        if (questType == QuestType.Normal)
            return normalAccepted;

        return dailyAccepted;
    }

    public bool IsQuestCompleted(QuestType questType)
    {
        if (questType == QuestType.Normal)
            return normalCompleted;

        return dailyCompleted;
    }

    public bool IsQuestRewarded(QuestType questType)
    {
        if (questType == QuestType.Normal)
            return normalRewarded;

        return dailyRewarded;
    }

    public int GetCurrentKillCount(QuestType questType)
    {
        if (questType == QuestType.Normal)
            return normalCurrentKillCount;

        return dailyCurrentKillCount;
    }

    public int GetTargetKillCount(QuestType questType)
    {
        if (questType == QuestType.Normal)
            return normalTargetKillCount;

        return dailyTargetKillCount;
    }

    public int CompleteQuestAndGetReward(QuestType questType)
    {
        if (questType == QuestType.Normal)
        {
            if (!normalCompleted || normalRewarded)
                return 0;

            normalRewarded = true;

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.AddMoney(normalRewardMoney);

            Debug.Log($"일반 퀘스트 보상 지급: {normalRewardMoney}");
            OnQuestChanged?.Invoke();

            return normalRewardMoney;
        }
        else
        {
            if (!dailyCompleted || dailyRewarded)
                return 0;

            dailyRewarded = true;

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.AddMoney(dailyRewardMoney);

            Debug.Log($"일일 퀘스트 보상 지급: {dailyRewardMoney}");
            OnQuestChanged?.Invoke();

            return dailyRewardMoney;
        }
    }

    private void ResetDailyQuest()
    {
        dailyAccepted = false;
        dailyCompleted = false;
        dailyRewarded = false;
        dailyCurrentKillCount = 0;

        Debug.Log("06:00 - 일일 퀘스트 초기화 완료");

        OnQuestChanged?.Invoke();
    }
}