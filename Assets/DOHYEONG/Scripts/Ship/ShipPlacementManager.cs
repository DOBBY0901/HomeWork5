using System.Collections.Generic;
using UnityEngine;

public class ShipPlacementManager : MonoBehaviour
{
    [SerializeField] private TilemapBoardManager boardManager;

    [Header("Placed Ships")]
    [SerializeField] private List<ShipData> placedShips = new List<ShipData>();

    [Header("Ship Visual")]
    [SerializeField] private GameObject shipVisualPrefab;
    [SerializeField] private Transform shipVisualParent;

    [Header("Ship Visual Settings")]
    [SerializeField] private float shipZ = -0.1f;
    [SerializeField] private float lengthPadding = 0.95f;
    [SerializeField] private float heightPadding = 0.75f;

    public IReadOnlyList<ShipData> PlacedShips => placedShips;

    private void Start()
    {
        placedShips.Clear();
    }

    public bool CanPlaceShip(Vector2Int startBoardPos, int size, ShipDirection direction)
    {
        for (int i = 0; i < size; i++)
        {
            int x = startBoardPos.x;
            int y = startBoardPos.y;

            if (direction == ShipDirection.Horizontal)
                x += i;
            else
                y += i;

            BoardCell cell = boardManager.GetCell(x, y);

            if (cell == null)
                return false;

            if (cell.state != BoardCellState.Sea)
                return false;

            if (HasAdjacentShip(x, y))
                return false;
        }

        return true;
    }

    private bool HasAdjacentShip(int x, int y)
    {
        for (int offsetY = -1; offsetY <= 1; offsetY++)
        {
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                int checkX = x + offsetX;
                int checkY = y + offsetY;

                BoardCell nearbyCell = boardManager.GetCell(checkX, checkY);

                if (nearbyCell == null)
                    continue;

                if (nearbyCell.state == BoardCellState.Ship)
                    return true;
            }
        }

        return false;
    }

    public bool TryPlaceShip(Vector3Int tilemapPos, int size, ShipDirection direction, int shipId)
    {
        if (!boardManager.IsInsideBoard(tilemapPos))
        {
            Debug.Log("보드 밖이라 배치 불가");
            return false;
        }

        Vector2Int startBoardPos = boardManager.TilemapToBoardPosition(tilemapPos);

        if (!CanPlaceShip(startBoardPos, size, direction))
        {
            Debug.Log("배치 불가 위치입니다.");
            return false;
        }

        ShipData newShip = new ShipData(shipId, size);

        for (int i = 0; i < size; i++)
        {
            int boardX = startBoardPos.x;
            int boardY = startBoardPos.y;

            if (direction == ShipDirection.Horizontal)
                boardX += i;
            else
                boardY += i;

            BoardCell cell = boardManager.GetCell(boardX, boardY);

            if (cell == null)
                return false;

            cell.state = BoardCellState.Ship;
            cell.shipId = shipId;

            newShip.AddCell(boardX, boardY);
        }

        placedShips.Add(newShip);

        CreatePlacedShipVisual(startBoardPos, size, direction);

        boardManager.ClearPreview();

        Debug.Log($"배 배치 완료 / Size:{size}, Direction:{direction}, ShipId:{shipId}");
        Debug.Log($"현재 배치된 배 수: {placedShips.Count}");

        return true;
    }

    private void CreatePlacedShipVisual(Vector2Int startBoardPos, int size, ShipDirection direction)
    {
        if (shipVisualPrefab == null)
        {
            Debug.LogWarning("Ship Visual Prefab이 연결되지 않았습니다.");
            return;
        }

        Vector3 startWorld = GetBoardCellCenterWorld(startBoardPos.x, startBoardPos.y);

        int endX = startBoardPos.x;
        int endY = startBoardPos.y;

        if (direction == ShipDirection.Horizontal)
            endX += size - 1;
        else
            endY += size - 1;

        Vector3 endWorld = GetBoardCellCenterWorld(endX, endY);

        Vector3 centerWorld = (startWorld + endWorld) * 0.5f;
        centerWorld.z = shipZ;

        Transform parent = shipVisualParent != null ? shipVisualParent : transform;

        GameObject visual = Instantiate(shipVisualPrefab, centerWorld, Quaternion.identity, parent);

        if (direction == ShipDirection.Vertical)
            visual.transform.rotation = Quaternion.Euler(0f, 0f, 90f);

        FitShipVisualToCells(visual, size);
    }

    private Vector3 GetBoardCellCenterWorld(int boardX, int boardY)
    {
        Vector3Int tilemapPos = boardManager.BoardToTilemapPosition(boardX, boardY);
        return boardManager.CellToWorldCenter(tilemapPos);
    }

    private void FitShipVisualToCells(GameObject visual, int size)
    {
        SpriteRenderer spriteRenderer = visual.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        float cellSize = 1f;

        float targetLength = size * cellSize * lengthPadding;
        float targetHeight = cellSize * heightPadding;

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        visual.transform.localScale = new Vector3(
            targetLength / spriteSize.x,
            targetHeight / spriteSize.y,
            1f
        );
    }

    public ShipData GetShipDataById(int shipId)
    {
        foreach (ShipData ship in placedShips)
        {
            if (ship.shipId == shipId)
                return ship;
        }

        return null;
    }

    public bool AreAllShipsPlaced()
    {
        return placedShips.Count >= 5;
    }

    public void ClearPlacedShips()
    {
        placedShips.Clear();
    }
}