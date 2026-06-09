using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject networkPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject chatPanel;

    [Header("Buttons")]
    [SerializeField] private GameObject chatToggleButton;

    [Header("Game Root")]
    [SerializeField] private GameObject gameRoot;

    [Header("Network")]
    [SerializeField] private SimpleTcpNetworkManager networkManager;

    private void Start()
    {
        ShowMainMenu();
    }

    public void OnClickStart()
    {
        ShowNetworkPanel();
    }

    public void OnClickHowToPlay()
    {
        SetActive(mainMenuPanel, false);
        SetActive(howToPlayPanel, true);
        SetActive(networkPanel, false);
        SetActive(gamePanel, false);

        SetActive(chatPanel, false);
        SetActive(chatToggleButton, false);

        SetActive(gameRoot, false);
    }

    public void OnClickCloseHowToPlay()
    {
        ShowMainMenu();
    }

    public void OnClickCloseNetworkPanel()
    {
        if (networkManager != null && networkManager.IsConnected)
            networkManager.Disconnect();

        ShowMainMenu();
    }

    public void OnClickToggleChat()
    {
        if (chatPanel == null)
            return;

        chatPanel.SetActive(!chatPanel.activeSelf);
    }

    public void OnClickExit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ShowMainMenu()
    {
        SetActive(mainMenuPanel, true);
        SetActive(howToPlayPanel, false);
        SetActive(networkPanel, false);
        SetActive(gamePanel, false);

        // 메인메뉴에서는 채팅 완전 숨김
        SetActive(chatPanel, false);
        SetActive(chatToggleButton, false);

        SetActive(gameRoot, false);
    }

    public void ShowNetworkPanel()
    {
        SetActive(mainMenuPanel, false);
        SetActive(howToPlayPanel, false);
        SetActive(networkPanel, true);
        SetActive(gamePanel, false);

        // 네트워크 패널에서는 채팅 사용 안 함
        SetActive(chatPanel, false);
        SetActive(chatToggleButton, false);

        SetActive(gameRoot, false);
    }

    public void ShowGamePanel()
    {
        SetActive(mainMenuPanel, false);
        SetActive(howToPlayPanel, false);
        SetActive(networkPanel, false);
        SetActive(gamePanel, true);

        // 게임 화면에서만 채팅 버튼 표시
        SetActive(chatPanel, false);
        SetActive(chatToggleButton, true);

        SetActive(gameRoot, true);
    }

    private void SetActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }
}