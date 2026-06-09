using UnityEngine;
using UnityEngine.UI;

public class NetworkUIController : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private SimpleTcpNetworkManager networkManager;

    [Header("Menu")]
    [SerializeField] private MainMenuController mainMenuController;

    [Header("Game")]
    [SerializeField] private GameManager gameManager;

    [Header("Buttons")]
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;

    private void Awake()
    {
        if (createGameButton != null)
            createGameButton.onClick.AddListener(OnClickCreateGame);

        if (joinGameButton != null)
            joinGameButton.onClick.AddListener(OnClickJoinGame);
    }

    private void OnEnable()
    {
        if (networkManager != null)
            networkManager.OnDisconnected += HandleDisconnected;
    }

    private void OnDisable()
    {
        if (networkManager != null)
            networkManager.OnDisconnected -= HandleDisconnected;
    }

    private void OnDestroy()
    {
        if (createGameButton != null)
            createGameButton.onClick.RemoveListener(OnClickCreateGame);

        if (joinGameButton != null)
            joinGameButton.onClick.RemoveListener(OnClickJoinGame);
    }

    private void OnClickCreateGame()
    {
        if (networkManager != null)
            networkManager.StartHost();

        if (mainMenuController != null)
            mainMenuController.ShowGamePanel();

        Debug.Log("게임 생성 버튼 클릭");
    }

    private void OnClickJoinGame()
    {
        if (networkManager != null)
            networkManager.StartClient();

        if (mainMenuController != null)
            mainMenuController.ShowGamePanel();

        Debug.Log("게임 참가 버튼 클릭");
    }

    private void HandleDisconnected()
    {
        if (gameManager != null)
            gameManager.OnNetworkDisconnected();

        if (mainMenuController != null)
            mainMenuController.ShowNetworkPanel();

        Debug.Log("연결 끊김: 네트워크 패널로 복귀");
    }
}