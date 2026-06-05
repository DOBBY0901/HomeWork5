using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private TilemapBoardManager boardManager;
    [SerializeField] private ShipPlacementManager shipPlacementManager;

    public AttackResult AttackCell(Vector3Int tilemapPos)
    {
        if (!boardManager.IsInsideBoard(tilemapPos))
        {
            Debug.Log("보드 밖은 공격할 수 없습니다.");
            return AttackResult.Invalid;
        }

        Vector2Int boardPos = boardManager.TilemapToBoardPosition(tilemapPos);
        BoardCell cell = boardManager.GetCell(boardPos.x, boardPos.y);

        if (cell == null)
            return AttackResult.Invalid;

        if (cell.state == BoardCellState.Land)
        {
            Debug.Log("육지는 공격할 수 없습니다.");
            return AttackResult.Invalid;
        }

        if (cell.state == BoardCellState.Hit ||
            cell.state == BoardCellState.Miss ||
            cell.state == BoardCellState.SunkAround)
        {
            Debug.Log("이미 공격한 칸입니다.");
            return AttackResult.Invalid;
        }

        if (cell.state == BoardCellState.Ship)
        {
            return HandleHit(cell, boardPos);
        }

        if (cell.state == BoardCellState.Sea)
        {
            return HandleMiss(cell, boardPos);
        }

        return AttackResult.Invalid;
    }

    private AttackResult HandleHit(BoardCell cell, Vector2Int boardPos)
    {
        cell.state = BoardCellState.Hit;
        boardManager.DrawHitTile(boardPos.x, boardPos.y);

        ShipData ship = shipPlacementManager.GetShipDataById(cell.shipId);

        if (ship != null)
        {
            ship.AddHit(boardPos.x, boardPos.y);

            if (ship.IsSunk())
            {
                Debug.Log($"침몰! ShipId:{ship.shipId}, Size:{ship.size}");
                MarkSunkAround(ship);
                return AttackResult.Sunk;
            }
        }

        Debug.Log($"명중! X:{boardPos.x}, Y:{boardPos.y}");
        return AttackResult.Hit;
    }

    private AttackResult HandleMiss(BoardCell cell, Vector2Int boardPos)
    {
        cell.state = BoardCellState.Miss;
        boardManager.DrawMissTile(boardPos.x, boardPos.y);

        Debug.Log($"빗나감! X:{boardPos.x}, Y:{boardPos.y}");
        return AttackResult.Miss;
    }

    private void MarkSunkAround(ShipData ship)
    {
        foreach (Vector2Int shipCell in ship.occupiedCells)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    int checkX = shipCell.x + offsetX;
                    int checkY = shipCell.y + offsetY;

                    BoardCell nearbyCell = boardManager.GetCell(checkX, checkY);

                    if (nearbyCell == null)
                        continue;

                    if (nearbyCell.state == BoardCellState.Hit)
                        continue;

                    if (nearbyCell.state == BoardCellState.Land)
                        continue;

                    if (nearbyCell.state == BoardCellState.Miss)
                        continue;

                    if (nearbyCell.state == BoardCellState.Sea)
                    {
                        nearbyCell.state = BoardCellState.SunkAround;
                        boardManager.DrawMissTile(checkX, checkY);
                    }
                }
            }
        }
    }

    public bool AreAllShipsSunk()
    {
        foreach (ShipData ship in shipPlacementManager.PlacedShips)
        {
            if (!ship.IsSunk())
                return false;
        }

        return shipPlacementManager.PlacedShips.Count >= 5;
    }
}