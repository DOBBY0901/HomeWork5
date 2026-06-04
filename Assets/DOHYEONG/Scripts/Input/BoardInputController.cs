using UnityEngine;
using UnityEngine.InputSystem;

public class BoardInputController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TilemapBoardManager boardManager;

    private void Update()
    {
        if (mainCamera == null || boardManager == null)
            return;

        HandleMouseHover();
        HandleMouseClick();
    }

    private void HandleMouseHover()
    {
        if (Mouse.current == null)
            return;

        Vector3Int cellPos = GetMouseCellPosition();
        boardManager.SetMarkerPreview(cellPos);
    }

    private void HandleMouseClick()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector3Int cellPos = GetMouseCellPosition();

        boardManager.PrintCellInfo(cellPos);
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
}