using UnityEngine;
using UnityEngine.InputSystem;

public class BattleInputController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TilemapBoardManager boardManager;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private GameManager gameManager;

    [Header("Mode")]
    [SerializeField] private bool attackMode = false;

    private void Update()
    {
        if (!attackMode)
            return;

        if (mainCamera == null || boardManager == null || battleManager == null)
            return;

        HandleAttack();
    }

    private void HandleAttack()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector3Int cellPos = GetMouseCellPosition();

        boardManager.PrintTilemapPositionOnly(cellPos);
    }

    // private void HandleAttack()
    // {
    //     if (Mouse.current == null)
    //         return;
    //
    //     if (!Mouse.current.leftButton.wasPressedThisFrame)
    //         return;
    //
    //     Vector3Int cellPos = GetMouseCellPosition();
    //     AttackResult result = battleManager.AttackCell(cellPos);
    //
    //     if (result == AttackResult.Invalid)
    //         return;
    //
    //     if (battleManager.AreAllShipsSunk())
    //     {
    //         Debug.Log("모든 배 침몰! 게임 종료");
    //
    //         if (gameManager != null)
    //             gameManager.SetGameOverPhase();
    //         else
    //             attackMode = false;
    //     }
    // }

    private Vector3Int GetMouseCellPosition()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0f)
        );

        mouseWorldPos.z = 0f;

        return boardManager.WorldToCell(mouseWorldPos);
    }

    public void SetAttackMode(bool value)
    {
        attackMode = value;
        Debug.Log($"공격 모드: {attackMode}");
    }
}