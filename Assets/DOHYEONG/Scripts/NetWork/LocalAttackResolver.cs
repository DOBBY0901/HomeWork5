using System.Collections.Generic;
using UnityEngine;

public class LocalAttackResolver : MonoBehaviour
{
    [SerializeField] private TilemapBoardManager playerBoardManager;
    [SerializeField] private ShipPlacementManager playerShipPlacementManager;

    public AttackResultPacketData ResolveAttack(int x, int y)
    {
        BoardCell cell = playerBoardManager.GetCell(x, y);

        if (cell == null)
        {
            Debug.Log("상대 공격 좌표가 보드 밖입니다.");
            return new AttackResultPacketData(x, y, AttackResult.Invalid);
        }

        if (cell.state == BoardCellState.Land)
        {
            Debug.Log("상대가 육지를 공격했습니다.");
            return new AttackResultPacketData(x, y, AttackResult.Invalid);
        }

        if (cell.state == BoardCellState.Hit ||
            cell.state == BoardCellState.Miss ||
            cell.state == BoardCellState.SunkAround)
        {
            Debug.Log("상대가 이미 공격된 칸을 공격했습니다.");
            return new AttackResultPacketData(x, y, AttackResult.Invalid);
        }

        if (cell.state == BoardCellState.Sea)
        {
            cell.state = BoardCellState.Miss;
            playerBoardManager.DrawMissTile(x, y);

            Debug.Log($"상대 공격 결과: 빗나감 / X:{x}, Y:{y}");

            return new AttackResultPacketData(x, y, AttackResult.Miss);
        }

        if (cell.state == BoardCellState.Ship)
        {
            cell.state = BoardCellState.Hit;
            playerBoardManager.DrawHitTile(x, y);

            ShipData ship = playerShipPlacementManager.GetShipDataById(cell.shipId);

            if (ship != null)
            {
                ship.AddHit(x, y);

                if (ship.IsSunk())
                {
                    Debug.Log($"내 배 침몰 / ShipId:{ship.shipId}, Size:{ship.size}");

                    AttackResultPacketData sunkResult =
                        new AttackResultPacketData(x, y, AttackResult.Sunk);

                    List<BoardPointData> aroundCells = MarkSunkAround(ship);
                    sunkResult.sunkAroundCells = aroundCells;

                    return sunkResult;
                }
            }

            Debug.Log($"상대 공격 결과: 명중 / X:{x}, Y:{y}");

            return new AttackResultPacketData(x, y, AttackResult.Hit);
        }

        return new AttackResultPacketData(x, y, AttackResult.Invalid);
    }

    private List<BoardPointData> MarkSunkAround(ShipData ship)
    {
        List<BoardPointData> markedCells = new List<BoardPointData>();

        foreach (Vector2Int shipCell in ship.occupiedCells)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    int checkX = shipCell.x + offsetX;
                    int checkY = shipCell.y + offsetY;

                    BoardCell nearbyCell = playerBoardManager.GetCell(checkX, checkY);

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
                        playerBoardManager.DrawMissTile(checkX, checkY);

                        markedCells.Add(new BoardPointData(checkX, checkY));
                    }
                }
            }
        }

        return markedCells;
    }

    public bool AreAllLocalShipsSunk()
    {
        foreach (ShipData ship in playerShipPlacementManager.PlacedShips)
        {
            if (!ship.IsSunk())
                return false;
        }

        return playerShipPlacementManager.PlacedShips.Count >= 5;
    }
}