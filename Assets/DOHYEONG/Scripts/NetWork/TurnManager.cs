using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SimpleTcpNetworkManager networkManager;
    [SerializeField] private TargetAttackController targetAttackController;
    [SerializeField] private GameStatusUI gameStatusUI;

    [Header("Turn Timer")]
    [SerializeField] private float turnTimeLimit = 15f;

    private float currentTurnTime;
    private bool isTimerRunning;

    public bool IsMyTurn { get; private set; }

    private void Update()
    {
        UpdateTurnTimer();
    }

    public void StartTurnByRole()
    {
        if (networkManager == null)
        {
            Debug.LogWarning("NetworkManager가 없습니다.");
            return;
        }

        // Host 선공, Client 후공
        SetMyTurn(networkManager.IsHost, false);

        if (networkManager.IsHost)
            Debug.Log("Host 선공: 내 턴 시작");
        else
            Debug.Log("Client 후공: 상대 턴 대기");
    }

    public void SetMyTurn(bool value)
    {
        SetMyTurn(value, false);
    }

    public void SetMyTurn(bool value, bool notifyOpponent)
    {
        IsMyTurn = value;

        if (targetAttackController != null)
            targetAttackController.SetCanAttack(IsMyTurn);

        if (IsMyTurn)
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }

        UpdateStatusText();

        Debug.Log(IsMyTurn ? "내 턴" : "상대 턴");

        if (notifyOpponent && networkManager != null && networkManager.IsConnected)
        {
            // 내가 턴을 끝냈으므로 상대에게 "이제 너 턴"이라고 보냄
            TurnChangePacketData data = new TurnChangePacketData(true);
            networkManager.SendPacket(PacketType.TurnChange, data);

            Debug.Log("TurnChange 패킷 전송");
        }
    }

    public void ReceiveTurnChange(TurnChangePacketData data)
    {
        // 상대가 보낸 패킷의 isMyTurn은 내 입장에서 적용
        SetMyTurn(data.isMyTurn, false);

        Debug.Log($"TurnChange 패킷 수신 / 내 턴:{data.isMyTurn}");
    }

    public void OnAttackSent()
    {
        if (targetAttackController != null)
            targetAttackController.SetCanAttack(false);

        StopTimer();

        if (gameStatusUI != null)
        {
            gameStatusUI.SetStatus("공격 결과 대기 중...");
            gameStatusUI.HideTimer();
        }
    }

    public void OnMyAttackResult(AttackResult result)
    {
        if (result == AttackResult.Miss)
        {
            // 빗나가면 내 턴 종료 + 상대에게 턴 넘김
            SetMyTurn(false, true);
            Debug.Log("빗나감: 턴 종료");
            return;
        }

        if (result == AttackResult.Hit || result == AttackResult.Sunk)
        {
            // 맞추면 연속 공격 가능 + 타이머 리셋
            SetMyTurn(true, false);
            Debug.Log("명중: 연속 공격 가능");
            return;
        }

        if (result == AttackResult.Invalid)
        {
            // 무효 공격은 턴 유지
            SetMyTurn(true, false);
            Debug.Log("무효 공격: 턴 유지");
        }
    }

    public void OnOpponentAttackResult(AttackResult result)
    {
        if (result == AttackResult.Miss)
        {
            // 상대가 나를 공격했는데 빗나감 → 이제 내 턴
            SetMyTurn(true, false);
            Debug.Log("상대 빗나감: 내 턴 시작");
            return;
        }

        if (result == AttackResult.Hit || result == AttackResult.Sunk)
        {
            // 상대가 맞췄으면 상대 연속 턴 유지
            SetMyTurn(false, false);
            Debug.Log("상대 명중: 상대 턴 유지");
            return;
        }

        if (result == AttackResult.Invalid)
        {
            SetMyTurn(false, false);
            Debug.Log("상대 무효 공격: 상대 턴 유지");
        }
    }

    private void StartTimer()
    {
        currentTurnTime = turnTimeLimit;
        isTimerRunning = true;
    }

    private void StopTimer()
    {
        isTimerRunning = false;
    }

    private void UpdateTurnTimer()
    {
        if (!isTimerRunning)
            return;

        if (!IsMyTurn)
            return;

        currentTurnTime -= Time.deltaTime;

        UpdateStatusText();
        if (currentTurnTime <= 0f)
        {
            currentTurnTime = 0f;
            isTimerRunning = false;

            Debug.Log("턴 시간 초과! 자동으로 턴을 넘깁니다.");

            if (gameStatusUI != null)
            {
                gameStatusUI.SetStatus("시간 초과! 상대 턴입니다.");
                gameStatusUI.HideTimer();
            }

            SetMyTurn(false, true);
        }
    }

    private void UpdateStatusText()
    {
        if (gameStatusUI == null)
            return;

        if (IsMyTurn)
        {
            gameStatusUI.SetStatus("내 턴입니다.");
            gameStatusUI.SetTimer(currentTurnTime);
        }
        else
        {
            gameStatusUI.SetStatus("상대 턴입니다.");
            gameStatusUI.HideTimer();
        }
    }

    public void StopTurn()
    {
        IsMyTurn = false;
        StopTimer();

        if (targetAttackController != null)
            targetAttackController.SetCanAttack(false);

        if (gameStatusUI != null)
            gameStatusUI.HideTimer();

        Debug.Log("턴 시스템 정지");
    }
}