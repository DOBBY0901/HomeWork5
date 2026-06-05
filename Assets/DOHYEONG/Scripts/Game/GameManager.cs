using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private ShipPlacementController shipPlacementController;
    [SerializeField] private BattleInputController battleInputController;

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

        if (battleInputController != null)
            battleInputController.SetAttackMode(false);

        Debug.Log("배치 단계 시작");
    }

    public void SetBattlePhase()
    {
        CurrentPhase = GamePhase.Battle;

        if (shipPlacementController != null)
            shipPlacementController.enabled = false;

        if (battleInputController != null)
            battleInputController.SetAttackMode(true);

        Debug.Log("전투 단계 시작");
    }

    public void SetGameOverPhase()
    {
        CurrentPhase = GamePhase.GameOver;

        if (shipPlacementController != null)
            shipPlacementController.enabled = false;

        if (battleInputController != null)
            battleInputController.SetAttackMode(false);

        Debug.Log("게임 종료");
    }
}