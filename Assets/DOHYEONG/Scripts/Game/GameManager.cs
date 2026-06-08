using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private ShipPlacementController shipPlacementController;
    [SerializeField] private TargetAttackController targetAttackController;
    [SerializeField] private TurnManager turnManager;

    [Header("UI")]
    [SerializeField] private GameStatusUI gameStatusUI;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Placement;

    private void Start()
    {
        SetPlacementPhase();
    }

    public void SetPlacementPhase()
    {
        CurrentPhase = GamePhase.Placement;

        if (shipPlacementController != null)
            shipPlacementController.enabled = true;

        if (targetAttackController != null)
            targetAttackController.SetCanAttack(false);

        if (gameStatusUI != null)
        {
            gameStatusUI.SetStatus("배를 배치하세요");
            gameStatusUI.HideTimer();
        }

        Debug.Log("배치 단계 시작");
    }

    public void SetBattlePhase()
    {
        CurrentPhase = GamePhase.Battle;

        if (shipPlacementController != null)
            shipPlacementController.enabled = false;

        if (turnManager != null)
            turnManager.StartTurnByRole();
        else if (targetAttackController != null)
            targetAttackController.SetCanAttack(false);

        if (gameStatusUI != null)
            gameStatusUI.SetStatus("전투 시작");

        Debug.Log("전투 단계 시작");
    }

    public void SetGameOverPhase()
    {
        SetGameOverPhase(false);
    }

    public void SetGameOverPhase(bool isWin)
    {
        CurrentPhase = GamePhase.GameOver;

        if (shipPlacementController != null)
            shipPlacementController.enabled = false;

        if (targetAttackController != null)
            targetAttackController.SetCanAttack(false);

        if (turnManager != null)
            turnManager.StopTurn();

        if (gameStatusUI != null)
        {
            gameStatusUI.HideTimer();

            if (isWin)
                gameStatusUI.SetStatus("승리!");
            else
                gameStatusUI.SetStatus("패배...");
        }

        Debug.Log(isWin ? "게임 종료: 승리" : "게임 종료: 패배");
    }
}