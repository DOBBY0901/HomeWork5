using TMPro;
using UnityEngine;

public class GameStatusUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI timerText;

    public void SetStatus(string message)
    {
        if (statusText == null)
            return;

        statusText.text = message;
    }

    public void SetTimer(float time)
    {
        if (timerText == null)
            return;

        int seconds = Mathf.CeilToInt(time);
        timerText.text = $"남은 시간 : {seconds}초";
    }

    public void HideTimer()
    {
        if (timerText == null)
            return;

        timerText.text = "남은 시간: -";
    }
}