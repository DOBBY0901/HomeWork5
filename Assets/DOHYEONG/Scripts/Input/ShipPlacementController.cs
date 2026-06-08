using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class ShipPlacementController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TilemapBoardManager boardManager;
    [SerializeField] private ShipPlacementManager shipPlacementManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ReadyManager readyManager;

    [Header("Tiles")]
    [SerializeField] private TileBase dockShipTile;

    [Header("Dock Settings")]
    [SerializeField] private Vector3Int dockOrigin = new Vector3Int(9, -6, 0);
    [SerializeField] private int dockLineGap = 2;

    private readonly int[] shipSizes = { 2, 3, 3, 4, 5 };

    private List<DockShip> dockShips = new List<DockShip>();

    private DockShip selectedShip;
    private ShipDirection currentDirection = ShipDirection.Horizontal;

    private void Start()
    {
        CreateDockShips();
        DrawDockShips();
    }

    private void Update()
    {
        if (mainCamera == null || boardManager == null || shipPlacementManager == null)
            return;

        HandleRotate();

        if (selectedShip == null)
        {
            HandlePickShipFromDock();
        }
        else
        {
            HandleShipPreviewOnBoard();
            HandlePlaceSelectedShip();
            HandleCancel();
        }
    }

    private void CreateDockShips()
    {
        dockShips.Clear();

        for (int i = 0; i < shipSizes.Length; i++)
        {
            Vector3Int startPos = dockOrigin + new Vector3Int(0, i * dockLineGap, 0);
            DockShip ship = new DockShip(i, shipSizes[i], startPos);
            dockShips.Add(ship);
        }
    }

    private void DrawDockShips()
    {
        boardManager.ClearDockArea();

        foreach (DockShip ship in dockShips)
        {
            ship.dockCells.Clear();

            if (ship.isPlaced)
                continue;

            for (int i = 0; i < ship.size; i++)
            {
                Vector3Int pos = ship.dockStartPos + new Vector3Int(i, 0, 0);
                ship.dockCells.Add(pos);
                boardManager.SetDockTile(pos, dockShipTile);
            }
        }
    }

    private void HandleRotate()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.rKey.wasPressedThisFrame)
            return;

        if (selectedShip == null)
            return;

        currentDirection = currentDirection == ShipDirection.Horizontal
            ? ShipDirection.Vertical
            : ShipDirection.Horizontal;

        Debug.Log($"���� �� ȸ��: {currentDirection}");
    }

    private void HandlePickShipFromDock()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector3Int clickedCell = GetMouseCellPosition();

        foreach (DockShip ship in dockShips)
        {
            if (ship.isPlaced)
                continue;

            if (ship.dockCells.Contains(clickedCell))
            {
                selectedShip = ship;
                currentDirection = ShipDirection.Horizontal;

                RemoveDockShipVisual(ship);

                Debug.Log($"�� ���õ� / Size:{ship.size}, ShipId:{ship.shipId}");
                return;
            }
        }
    }

    private void RemoveDockShipVisual(DockShip ship)
    {
        foreach (Vector3Int pos in ship.dockCells)
        {
            boardManager.ClearDockTile(pos);
        }
    }

    private void HandleShipPreviewOnBoard()
    {
        if (Mouse.current == null)
            return;

        Vector3Int cellPos = GetMouseCellPosition();
        boardManager.PreviewShip(cellPos, selectedShip.size, currentDirection);
    }

    private void HandlePlaceSelectedShip()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector3Int cellPos = GetMouseCellPosition();

        bool placed = shipPlacementManager.TryPlaceShip(
            cellPos,
            selectedShip.size,
            currentDirection,
            selectedShip.shipId
        );

        if (placed)
        {   
            selectedShip.isPlaced = true;
            selectedShip = null;
            currentDirection = ShipDirection.Horizontal;

            boardManager.ClearPreview();

            Debug.Log("�� ��ġ ����");

            if (IsAllShipsPlaced())
            {
                Debug.Log("모든 배 배치 완료! Ready 전송");

                if (readyManager != null)
                    readyManager.SetLocalReady();
            }

            return;
        }

        ReturnSelectedShipToDock();
    }

    private void HandleCancel()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        ReturnSelectedShipToDock();
    }

    private void ReturnSelectedShipToDock()
    {
        if (selectedShip == null)
            return;

        selectedShip.isPlaced = false;

        boardManager.ClearPreview();
        DrawDockShips();

        Debug.Log("�谡 ��⼮���� ���ư�");

        selectedShip = null;
        currentDirection = ShipDirection.Horizontal;
    }

    private bool IsAllShipsPlaced()
    {
        foreach (DockShip ship in dockShips)
        {
            if (!ship.isPlaced)
                return false;
        }

        return true;
    }

    private Vector3Int GetMouseCellPosition()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0f)
        );

        mouseWorldPos.z = 0f;

        return boardManager.WorldToCell(mouseWorldPos);
    }

    [ContextMenu("��⼮ �� �׸���")]
    private void DrawDockShipsInEditor()
    {
        if (boardManager == null)
        {
            Debug.LogWarning("BoardManager�� ������� �ʾҽ��ϴ�.");
            return;
        }

        CreateDockShips();
        DrawDockShips();

        Debug.Log("�����Ϳ��� ��⼮ �踦 �׷Ƚ��ϴ�.");
    }

    [ContextMenu("��⼮ �� �����")]
    private void ClearDockShipsInEditor()
    {
        if (boardManager == null)
        {
            Debug.LogWarning("BoardManager�� ������� �ʾҽ��ϴ�.");
            return;
        }

        boardManager.ClearDockArea();

        Debug.Log("�����Ϳ��� ��⼮ �踦 �������ϴ�.");
    }
}