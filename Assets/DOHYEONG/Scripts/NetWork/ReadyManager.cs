using UnityEngine;

public class ReadyManager : MonoBehaviour
{
    [SerializeField] private SimpleTcpNetworkManager networkManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameStatusUI gameStatusUI;

    private bool localReady;
    private bool remoteReady;
    private bool battleStarted;

    private void OnEnable()
    {
        if (networkManager != null)
            networkManager.OnMessageReceived += HandleNetworkMessage;
    }

    private void OnDisable()
    {
        if (networkManager != null)
            networkManager.OnMessageReceived -= HandleNetworkMessage;
    }

    public void SetLocalReady()
    {
        if (battleStarted)
            return;

        if (localReady)
            return;

        localReady = true;

        ReadyPacketData readyData = new ReadyPacketData(true);

        if (networkManager != null && networkManager.IsConnected)
            networkManager.SendPacket(PacketType.Ready, readyData);

        Debug.Log("내 Ready 완료");

        if (gameStatusUI != null)
            gameStatusUI.SetStatus("상대 준비를 기다리는 중...");

        CheckBothReady();
    }

    private void HandleNetworkMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        if (!message.StartsWith("{"))
            return;

        NetworkPacket packet;

        try
        {
            packet = JsonUtility.FromJson<NetworkPacket>(message);
        }
        catch
        {
            return;
        }

        if (packet == null)
            return;

        if (packet.type != PacketType.Ready)
            return;

        ReadyPacketData readyData = JsonUtility.FromJson<ReadyPacketData>(packet.jsonData);

        remoteReady = readyData.isReady;

        Debug.Log($"상대 Ready 수신: {remoteReady}");

        CheckBothReady();
    }

    private void CheckBothReady()
    {
        if (battleStarted)
            return;

        if (!localReady || !remoteReady)
        {

            if (gameStatusUI != null)
                gameStatusUI.SetStatus($"대기 중 / 나:{localReady}, 상대:{remoteReady}");

            return;
        }

        battleStarted = true;

        if (gameStatusUI != null)
            gameStatusUI.SetStatus("전투 시작!");

        if (gameManager != null)
            gameManager.SetBattlePhase();
    }

    public void ResetReady()
    {
        localReady = false;
        remoteReady = false;
        battleStarted = false;

        Debug.Log("Ready 상태 초기화");
    }
}