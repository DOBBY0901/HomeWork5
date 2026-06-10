using TMPro;
using UnityEngine;

public class GameStatusUI : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Result")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject victoryImage;
    [SerializeField] private GameObject defeatImage;

    private void Start()
    {
        HideResult();
    }

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
        timerText.text = $"남은 시간: {seconds}초";
    }

    public void HideTimer()
    {
        if (timerText == null)
            return;

        timerText.text = "남은 시간: -";
    }

    public void ShowResult(bool isWin)
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        // 먼저 둘 다 끄기
        if (victoryImage != null)
            victoryImage.SetActive(false);

        if (defeatImage != null)
            defeatImage.SetActive(false);

        // 그 다음 하나만 켜기
        if (isWin)
        {
            if (victoryImage != null)
                victoryImage.SetActive(true);
        }
        else
        {
            if (defeatImage != null)
                defeatImage.SetActive(true);
        }

        Debug.Log(isWin ? "결과 이미지: 승리 표시" : "결과 이미지: 패배 표시");
    }

    public void HideResult()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (victoryImage != null)
            victoryImage.SetActive(false);

        if (defeatImage != null)
            defeatImage.SetActive(false);
    }
}