using UnityEngine;

public class EnemyShipAutoPlacer : MonoBehaviour
{
    [SerializeField] private TilemapBoardManager enemyBoardManager;
    [SerializeField] private ShipPlacementManager enemyShipPlacementManager;

    [Header("Enemy Ship Settings")]
    [SerializeField] private bool placeOnStart = true;
    [SerializeField] private bool showEnemyShips = false;

    private readonly int[] shipSizes = { 2, 3, 3, 4, 5 };

    private void Start()
    {
        if (placeOnStart)
        {
            PlaceEnemyShips();
        }
    }

    [ContextMenu("상대 배 자동 배치")]
    public void PlaceEnemyShips()
    {
        if (enemyBoardManager == null || enemyShipPlacementManager == null)
        {
            Debug.LogWarning("EnemyBoardManager 또는 EnemyShipPlacementManager가 연결되지 않았습니다.");
            return;
        }

        int shipId = 0;

        for (int i = 0; i < shipSizes.Length; i++)
        {
            bool placed = TryPlaceRandomShip(shipSizes[i], shipId);

            if (!placed)
            {
                Debug.LogWarning($"상대 배 자동 배치 실패 / Size:{shipSizes[i]}, ShipId:{shipId}");
                return;
            }

            shipId++;
        }

        Debug.Log("상대 배 자동 배치 완료");
    }

    private bool TryPlaceRandomShip(int size, int shipId)
    {
        const int maxTryCount = 500;

        for (int i = 0; i < maxTryCount; i++)
        {
            int x = Random.Range(0, enemyBoardManager.Width);
            int y = Random.Range(0, enemyBoardManager.Height);

            ShipDirection direction = Random.value < 0.5f
                ? ShipDirection.Horizontal
                : ShipDirection.Vertical;

            Vector3Int tilemapPos = enemyBoardManager.BoardToTilemapPosition(x, y);

            bool placed = enemyShipPlacementManager.TryPlaceShip(
                tilemapPos,
                size,
                direction,
                shipId
            );

            if (placed)
            {
                if (!showEnemyShips)
                {
                    HideEnemyShipVisuals();
                }

                return true;
            }
        }

        return false;
    }

    private void HideEnemyShipVisuals()
    {
        // 상대 배는 플레이어에게 보이면 안 되므로 ShipTilemap 표시만 지움.
        // BoardCell 데이터와 ShipData는 유지됨.
        enemyBoardManager.ClearShipTiles();
    }
}