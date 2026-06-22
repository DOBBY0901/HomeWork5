using System;
using TMPro;
using UnityEngine;

public class GameClockManager : MonoBehaviour
{
    public static GameClockManager Instance { get; private set; }

    public event Action OnNewDayStarted;

    [Header("Game Time")]
    [SerializeField] private int day = 1;
    [SerializeField] private int hour = 5;
    [SerializeField] private int minute = 50;

    [Header("Time Speed")]
    [Tooltip("현실 1초에 게임 시간이 몇 분 흐를지. 1일 10분 기준 = 2.4")]
    [SerializeField] private float gameMinutesPerRealSecond = 2.4f;

    [Header("Daily Reset")]
    [SerializeField] private int dailyResetHour = 6;

    [Header("UI")]
    [SerializeField] private TMP_Text clockText;

    private float minuteTimer;
    private bool resetTriggeredToday;

    public int Day => day;
    public int Hour => hour;
    public int Minute => minute;

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
        resetTriggeredToday = hour >= dailyResetHour;
        RefreshUI();
    }

    private void Update()
    {
        TickTime();
        RefreshUI();
    }

    private void TickTime()
    {
        minuteTimer += Time.deltaTime * gameMinutesPerRealSecond;

        while (minuteTimer >= 1f)
        {
            minuteTimer -= 1f;
            AddMinute();
        }
    }

    private void AddMinute()
    {
        minute++;

        if (minute >= 60)
        {
            minute = 0;
            hour++;
        }

        if (hour >= 24)
        {
            hour = 0;
            day++;
            resetTriggeredToday = false;
        }

        CheckDailyReset();
    }

    private void CheckDailyReset()
    {
        if (resetTriggeredToday)
            return;

        if (hour >= dailyResetHour)
        {
            resetTriggeredToday = true;
            Debug.Log($"Day {day} 06:00 - 일일 퀘스트 초기화");
            OnNewDayStarted?.Invoke();
        }
    }

    private void RefreshUI()
    {
        if (clockText == null)
            return;

        clockText.text = $"{hour:00}:{minute:00}";
    }
}