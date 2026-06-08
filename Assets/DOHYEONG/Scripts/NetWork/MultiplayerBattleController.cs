using UnityEngine;

public class MultiplayerBattleController : MonoBehaviour
{
    [SerializeField] private SimpleTcpNetworkManager networkManager;
    [SerializeField] private LocalAttackResolver localAttackResolver;
    [SerializeField] private TargetAttackController targetAttackController;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private GameManager gameManager;
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
            Debug.LogWarning($"패킷 변환 실패: {message}");
            return;
        }

        if (packet == null)
            return;

        switch (packet.type)
        {
            case PacketType.Attack:
                HandleAttackPacket(packet.jsonData);
                break;

            case PacketType.AttackResult:
                HandleAttackResultPacket(packet.jsonData);
                break;

            case PacketType.TurnChange:
                HandleTurnChangePacket(packet.jsonData);
                break;

            case PacketType.GameOver:
                HandleGameOverPacket(packet.jsonData);
                break;
        }
    }

    private void HandleAttackPacket(string jsonData)
    {
        AttackPacketData attackData = JsonUtility.FromJson<AttackPacketData>(jsonData);

        Debug.Log($"공격 패킷 수신 / X:{attackData.x}, Y:{attackData.y}");

        AttackResultPacketData resultData =
            localAttackResolver.ResolveAttack(attackData.x, attackData.y);

        networkManager.SendPacket(PacketType.AttackResult, resultData);

        if (turnManager != null)
        {
            turnManager.OnOpponentAttackResult(resultData.result);
        }

        // 내 배가 모두 침몰했는지 확인
        if (localAttackResolver.AreAllLocalShipsSunk())
        {
            Debug.Log("내 모든 배가 침몰했습니다. 패배 처리");

            // 상대에게 승리 알림
            GameOverPacketData gameOverData = new GameOverPacketData(true);
            networkManager.SendPacket(PacketType.GameOver, gameOverData);

            // 나는 패배
            if (gameManager != null)
                gameManager.SetGameOverPhase(false);
        }
    }

    private void HandleAttackResultPacket(string jsonData)
    {
        AttackResultPacketData resultData =
            JsonUtility.FromJson<AttackResultPacketData>(jsonData);

        Debug.Log($"공격 결과 수신 / X:{resultData.x}, Y:{resultData.y}, Result:{resultData.result}");

        targetAttackController.ApplyAttackResult(resultData);

        if (turnManager != null)
        {
            turnManager.OnMyAttackResult(resultData.result);
        }
    }
    private void HandleTurnChangePacket(string jsonData)
    {
        TurnChangePacketData turnData =
            JsonUtility.FromJson<TurnChangePacketData>(jsonData);

        if (turnManager != null)
            turnManager.ReceiveTurnChange(turnData);
    }

    private void HandleGameOverPacket(string jsonData)
    {
        GameOverPacketData data = JsonUtility.FromJson<GameOverPacketData>(jsonData);

        Debug.Log($"GameOver 패킷 수신 / YouWin:{data.youWin}");

        if (gameManager != null)
            gameManager.SetGameOverPhase(data.youWin);
    }
}