using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class ShipPlacementController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [Header("Board")]
    [SerializeField] private TilemapBoardManager boardManager;

    [Header("Placement")]
    [SerializeField] private ShipPlacementManager placementManager;

    [Header("Ready")]
    [SerializeField] private ReadyManager readyManager;

    [Header("Dock Ships")]
    [SerializeField] private List<DockShip> dockShips = new List<DockShip>();

    [Header("Auto Dock Settings")]
    [SerializeField] private bool autoCreateDockShips = true;
    [SerializeField] private Vector3Int defaultDockStartPos = new Vector3Int(13, 2, 0);
    [SerializeField] private int dockRowSpacing = -2;

    [Header("Dock Click Tile")]
    [SerializeField] private TileBase dockShipTile;

    [Header("Dock Ship Visual")]
    [SerializeField] private GameObject dockShipVisualPrefab;
    [SerializeField] private Transform dockShipVisualParent;

    [Header("Dock Visual Settings")]
    [SerializeField] private float dockShipZ = -0.1f;
    [SerializeField] private float dockLengthPadding = 0.95f;
    [SerializeField] private float dockHeightPadding = 0.75f;

    private readonly List<GameObject> dockShipVisuals = new List<GameObject>();

    private DockShip selectedDockShip;
    private ShipDirection currentDirection = ShipDirection.Horizontal;

    private bool isAllPlacedNotified;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!autoCreateDockShips)
            return;

        if (Application.isPlaying)
            return;

        EnsureDefaultDockShips();
    }
#endif

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        EnsureDefaultDockShips();
    }

    private void Start()
    {
        DrawDockShips();
    }

    private void Update()
    {
        if (placementManager == null || boardManager == null)
            return;

        HandleRotationInput();
        HandleMouseHover();
        HandleMouseClick();
        HandleCancelInput();
    }

    private void EnsureDefaultDockShips()
    {
        if (!autoCreateDockShips)
            return;

        if (dockShips == null)
            dockShips = new List<DockShip>();

        if (dockShips.Count > 0)
            return;

        dockShips.Add(new DockShip(0, 5, defaultDockStartPos + new Vector3Int(0, dockRowSpacing * 0, 0)));
        dockShips.Add(new DockShip(1, 4, defaultDockStartPos + new Vector3Int(0, dockRowSpacing * 1, 0)));
        dockShips.Add(new DockShip(2, 3, defaultDockStartPos + new Vector3Int(0, dockRowSpacing * 2, 0)));
        dockShips.Add(new DockShip(3, 3, defaultDockStartPos + new Vector3Int(0, dockRowSpacing * 3, 0)));
        dockShips.Add(new DockShip(4, 2, defaultDockStartPos + new Vector3Int(0, dockRowSpacing * 4, 0)));

        Debug.Log("대기석 배 5척 자동 생성 완료");
    }

    private void HandleRotationInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (currentDirection == ShipDirection.Horizontal)
                currentDirection = ShipDirection.Vertical;
            else
                currentDirection = ShipDirection.Horizontal;

            Debug.Log($"배 방향 변경: {currentDirection}");
        }
    }

    private void HandleMouseHover()
    {
        if (selectedDockShip == null)
            return;

        Vector3 worldPos = GetMouseWorldPosition();
        Vector3Int tilemapPos = boardManager.WorldToCell(worldPos);

        boardManager.PreviewShip(tilemapPos, selectedDockShip.size, currentDirection);
    }

    private void HandleMouseClick()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector3 worldPos = GetMouseWorldPosition();

        if (selectedDockShip == null)
        {
            TrySelectDockShip(worldPos);
        }
        else
        {
            TryPlaceSelectedShip(worldPos);
        }
    }

    private void HandleCancelInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelSelection();
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0f;
        return worldPos;
    }

    private void TrySelectDockShip(Vector3 worldPos)
    {
        Vector3Int dockCellPos = boardManager.DockWorldToCell(worldPos);

        DockShip clickedShip = GetDockShipAtCell(dockCellPos);

        if (clickedShip == null)
            return;

        if (clickedShip.isPlaced)
            return;

        selectedDockShip = clickedShip;
        currentDirection = ShipDirection.Horizontal;

        Debug.Log($"대기석 배 선택 / Size:{selectedDockShip.size}, ShipId:{selectedDockShip.shipId}");
    }

    private DockShip GetDockShipAtCell(Vector3Int dockCellPos)
    {
        foreach (DockShip dockShip in dockShips)
        {
            if (dockShip == null)
                continue;

            if (dockShip.isPlaced)
                continue;

            foreach (Vector3Int cell in dockShip.dockCells)
            {
                if (cell == dockCellPos)
                    return dockShip;
            }
        }

        return null;
    }

    private void TryPlaceSelectedShip(Vector3 worldPos)
    {
        Vector3Int tilemapPos = boardManager.WorldToCell(worldPos);

        bool placed = placementManager.TryPlaceShip(
            tilemapPos,
            selectedDockShip.size,
            currentDirection,
            selectedDockShip.shipId
        );

        if (!placed)
            return;

        selectedDockShip.isPlaced = true;
        selectedDockShip = null;

        boardManager.ClearPreview();

        DrawDockShips();

        CheckAllShipsPlaced();
    }

    private void CheckAllShipsPlaced()
    {
        if (isAllPlacedNotified)
            return;

        if (!placementManager.AreAllShipsPlaced())
            return;

        isAllPlacedNotified = true;

        Debug.Log("모든 배 배치 완료");

        if (readyManager != null)
            readyManager.SetLocalReady();
    }

    private void CancelSelection()
    {
        selectedDockShip = null;
        boardManager.ClearPreview();

        Debug.Log("배 선택 취소");
    }

    private void DrawDockShips()
    {
        EnsureDefaultDockShips();

        boardManager.ClearDockArea();
        ClearDockShipVisuals();

        foreach (DockShip dockShip in dockShips)
        {
            if (dockShip == null)
                continue;

            dockShip.dockCells.Clear();

            if (dockShip.isPlaced)
                continue;

            for (int i = 0; i < dockShip.size; i++)
            {
                Vector3Int drawPos = dockShip.dockStartPos + new Vector3Int(i, 0, 0);

                dockShip.dockCells.Add(drawPos);

                // 클릭 인식용 타일
                boardManager.SetDockTile(drawPos, dockShipTile);
            }

            // 실제로 보이는 대기석 가로 배 스프라이트
            CreateDockShipVisual(dockShip);
        }
    }

    private void ClearDockShipVisuals()
    {
        for (int i = 0; i < dockShipVisuals.Count; i++)
        {
            if (dockShipVisuals[i] != null)
                Destroy(dockShipVisuals[i]);
        }

        dockShipVisuals.Clear();
    }

    private void CreateDockShipVisual(DockShip dockShip)
    {
        if (dockShipVisualPrefab == null)
        {
            Debug.LogWarning("Dock Ship Visual Prefab이 연결되지 않았습니다.");
            return;
        }

        if (dockShip == null)
            return;

        Vector3 startWorld = boardManager.DockCellToWorldCenter(dockShip.dockStartPos);
        Vector3 endWorld = boardManager.DockCellToWorldCenter(
            dockShip.dockStartPos + new Vector3Int(dockShip.size - 1, 0, 0)
        );

        Vector3 centerWorld = (startWorld + endWorld) * 0.5f;
        centerWorld.z = dockShipZ;

        Transform parent = dockShipVisualParent != null ? dockShipVisualParent : transform;

        GameObject visual = Instantiate(dockShipVisualPrefab, centerWorld, Quaternion.identity, parent);

        FitDockShipVisualToCells(visual, dockShip.size);

        dockShipVisuals.Add(visual);
    }

    private void FitDockShipVisualToCells(GameObject visual, int size)
    {
        SpriteRenderer spriteRenderer = visual.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        float cellSize = 1f;

        float targetLength = size * cellSize * dockLengthPadding;
        float targetHeight = cellSize * dockHeightPadding;

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        visual.transform.localScale = new Vector3(
            targetLength / spriteSize.x,
            targetHeight / spriteSize.y,
            1f
        );
    }

    private void OnDisable()
    {
        if (boardManager != null)
            boardManager.ClearPreview();
    }
}