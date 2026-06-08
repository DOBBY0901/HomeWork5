using UnityEngine;
using UnityEngine.InputSystem;

public class TargetAttackController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TilemapBoardManager targetBoardManager;
    [SerializeField] private SimpleTcpNetworkManager networkManager;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private GameManager gameManager;

    [Header("Mode")]
    [SerializeField] private bool canAttack = false;

    private void Update()
    {
        if (!canAttack)
            return;

        if (mainCamera == null || targetBoardManager == null || networkManager == null)
            return;

        HandleClick();
    }

    private void HandleClick()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (gameManager != null && gameManager.CurrentPhase == GamePhase.GameOver)
        {
            Debug.Log("게임 종료 상태에서는 공격할 수 없습니다.");
            return;
        }

        Vector3Int tilemapPos = GetMouseCellPosition();

        if (!targetBoardManager.IsInsideBoard(tilemapPos))
        {
            Debug.Log("Target 보드 밖 클릭");
            return;
        }

        Vector2Int boardPos = targetBoardManager.TilemapToBoardPosition(tilemapPos);
        BoardCell cell = targetBoardManager.GetCell(boardPos.x, boardPos.y);

        if (cell == null)
            return;

        if (cell.state == BoardCellState.Land)
        {
            Debug.Log("육지는 공격할 수 없습니다.");
            return;
        }

        if (cell.state == BoardCellState.Hit ||
            cell.state == BoardCellState.Miss ||
            cell.state == BoardCellState.SunkAround)
        {
            Debug.Log("이미 공격한 칸입니다.");
            return;
        }

        if (!networkManager.IsConnected)
        {
            Debug.LogWarning("네트워크가 연결되지 않아 공격할 수 없습니다.");
            return;
        }

        AttackPacketData attackData = new AttackPacketData(boardPos.x, boardPos.y);
        networkManager.SendPacket(PacketType.Attack, attackData);

        Debug.Log($"공격 패킷 전송 / X:{boardPos.x}, Y:{boardPos.y}");

        if (turnManager != null)
            turnManager.OnAttackSent();
    }

    public void ApplyAttackResult(AttackResultPacketData resultData)
    {
        BoardCell cell = targetBoardManager.GetCell(resultData.x, resultData.y);

        if (cell == null)
            return;

        switch (resultData.result)
        {
            case AttackResult.Miss:
                cell.state = BoardCellState.Miss;
                targetBoardManager.DrawMissTile(resultData.x, resultData.y);
                Debug.Log($"공격 결과 적용: 빗나감 / X:{resultData.x}, Y:{resultData.y}");
                break;

            case AttackResult.Hit:
                cell.state = BoardCellState.Hit;
                targetBoardManager.DrawHitTile(resultData.x, resultData.y);
                Debug.Log($"공격 결과 적용: 명중 / X:{resultData.x}, Y:{resultData.y}");
                break;

            case AttackResult.Sunk:
                cell.state = BoardCellState.Hit;
                targetBoardManager.DrawHitTile(resultData.x, resultData.y);

                foreach (BoardPointData point in resultData.sunkAroundCells)
                {
                    BoardCell aroundCell = targetBoardManager.GetCell(point.x, point.y);

                    if (aroundCell == null)
                        continue;

                    if (aroundCell.state == BoardCellState.Sea)
                    {
                        aroundCell.state = BoardCellState.SunkAround;
                        targetBoardManager.DrawMissTile(point.x, point.y);
                    }
                }

                Debug.Log($"공격 결과 적용: 침몰 / X:{resultData.x}, Y:{resultData.y}");
                break;

            case AttackResult.Invalid:
                Debug.LogWarning("상대가 Invalid 공격 결과를 보냈습니다.");
                break;
        }
    }

    private Vector3Int GetMouseCellPosition()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0f)
        );

        mouseWorldPos.z = 0f;

        return targetBoardManager.WorldToCell(mouseWorldPos);
    }

    public void SetCanAttack(bool value)
    {
        canAttack = value;
        Debug.Log($"Target 공격 가능 상태: {canAttack}");
    }
}