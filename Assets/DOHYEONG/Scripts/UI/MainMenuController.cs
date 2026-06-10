using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private static bool openNetworkPanelAfterReload = false;

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
        if (openNetworkPanelAfterReload)
        {
            openNetworkPanelAfterReload = false;
            ShowNetworkPanel();
        }
        else
        {
            ShowMainMenu();
        }
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

    // 결과창 - 다시하기
    public void OnClickRestartGame()
    {
        if (networkManager != null && networkManager.IsConnected)
            networkManager.Disconnect();

        Time.timeScale = 1f;

        // 씬 재로드 후 메인메뉴가 아니라 네트워크 선택 화면으로 이동
        openNetworkPanelAfterReload = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 결과창 - 메인메뉴
    public void OnClickBackToMainMenu()
    {
        if (networkManager != null && networkManager.IsConnected)
            networkManager.Disconnect();

        Time.timeScale = 1f;

        // 메인메뉴 버튼은 진짜 메인메뉴로 이동
        openNetworkPanelAfterReload = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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