using UnityEngine;
using UnityEngine.UI;

public class NetworkUIController : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private SimpleTcpNetworkManager networkManager;

    [Header("UI")]
    [SerializeField] private GameObject networkPanel;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Awake()
    {
        if (hostButton != null)
            hostButton.onClick.AddListener(OnClickHost);

        if (clientButton != null)
            clientButton.onClick.AddListener(OnClickClient);
    }

    private void Update()
    {
        if (networkManager == null)
            return;

        if (networkManager.IsConnected)
        {
            if (networkPanel != null && networkPanel.activeSelf)
                networkPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (hostButton != null)
            hostButton.onClick.RemoveListener(OnClickHost);

        if (clientButton != null)
            clientButton.onClick.RemoveListener(OnClickClient);
    }

    private void OnClickHost()
    {
        if (networkManager == null)
            return;

        networkManager.StartHost();

        Debug.Log("Host 버튼 클릭");
    }

    private void OnClickClient()
    {
        if (networkManager == null)
            return;

        networkManager.StartClient();

        Debug.Log("Client 버튼 클릭");
    }
}