using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapBoardManager : MonoBehaviour
{
    [Header("Board Size")]
    [SerializeField] private int width = 11;
    [SerializeField] private int height = 11;

    [Header("Board Origin")]
    [SerializeField] private Vector3Int boardOrigin = Vector3Int.zero;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap mapTilemap;
    [SerializeField] private Tilemap shipTilemap;
    [SerializeField] private Tilemap markerTilemap;
    [SerializeField] private Tilemap dockTilemap;

    [Header("Map Tiles")]
    [SerializeField] private TileBase seaTile;
    [SerializeField] private TileBase landTile;

    [Header("Ship / Marker Tiles")]
    [SerializeField] private TileBase shipTile;
    [SerializeField] private TileBase hitTile;
    [SerializeField] private TileBase missTile;
    [SerializeField] private TileBase selectTile;

    private BoardCell[,] cells;

    public int Width => width;
    public int Height => height;

    private void Start()
    {
        LoadBoardFromTilemap();

        if (shipTilemap != null)
            shipTilemap.ClearAllTiles();

        if (markerTilemap != null)
            markerTilemap.ClearAllTiles();
    }

    private void LoadBoardFromTilemap()
    {
        cells = new BoardCell[width, height];

        int seaCount = 0;
        int landCount = 0;
        int emptyCount = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                cells[x, y] = new BoardCell(x, y);

                Vector3Int tilePos = boardOrigin + new Vector3Int(x, y, 0);
                TileBase currentTile = mapTilemap.GetTile(tilePos);

                if (currentTile == landTile)
                {
                    cells[x, y].state = BoardCellState.Land;
                    landCount++;
                }
                else if (currentTile == seaTile)
                {
                    cells[x, y].state = BoardCellState.Sea;
                    seaCount++;
                }
                else
                {
                    cells[x, y].state = BoardCellState.Sea;
                    emptyCount++;
                }
            }
        }

        Debug.Log($"맵 로드 완료 / Sea:{seaCount}, Land:{landCount}, Empty:{emptyCount}, Origin:{boardOrigin}");
    }

    public BoardCell GetCell(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return null;

        return cells[x, y];
    }

    public bool IsInsideBoard(Vector3Int tilemapPos)
    {
        Vector2Int boardPos = TilemapToBoardPosition(tilemapPos);

        return boardPos.x >= 0 &&
               boardPos.x < width &&
               boardPos.y >= 0 &&
               boardPos.y < height;
    }

    public Vector2Int TilemapToBoardPosition(Vector3Int tilemapPos)
    {
        return new Vector2Int(
            tilemapPos.x - boardOrigin.x,
            tilemapPos.y - boardOrigin.y
        );
    }

    public Vector3Int BoardToTilemapPosition(int x, int y)
    {
        return boardOrigin + new Vector3Int(x, y, 0);
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        return mapTilemap.WorldToCell(worldPos);
    }

    public void SetMarkerPreview(Vector3Int tilemapPos)
    {
        markerTilemap.ClearAllTiles();

        if (!IsInsideBoard(tilemapPos))
            return;

        Vector2Int boardPos = TilemapToBoardPosition(tilemapPos);
        BoardCell cell = GetCell(boardPos.x, boardPos.y);

        if (cell == null)
            return;

        if (cell.state == BoardCellState.Land)
            return;

        markerTilemap.SetTile(tilemapPos, selectTile);
    }

    public void PrintCellInfo(Vector3Int tilemapPos)
    {
        if (!IsInsideBoard(tilemapPos))
        {
            Debug.Log("보드 밖 클릭");
            return;
        }

        Vector2Int boardPos = TilemapToBoardPosition(tilemapPos);
        BoardCell cell = GetCell(boardPos.x, boardPos.y);

        Debug.Log($"Board X:{cell.x}, Y:{cell.y} / Tilemap X:{tilemapPos.x}, Y:{tilemapPos.y} / State:{cell.state} / ShipId:{cell.shipId}");
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

            BoardCell cell = GetCell(x, y);

            if (cell == null)
                return false;

            if (cell.state != BoardCellState.Sea)
                return false;
        }

        return true;
    }

    public void PreviewShip(Vector3Int tilemapPos, int size, ShipDirection direction)
    {
        markerTilemap.ClearAllTiles();

        if (!IsInsideBoard(tilemapPos))
            return;

        Vector2Int startBoardPos = TilemapToBoardPosition(tilemapPos);
        bool canPlace = CanPlaceShip(startBoardPos, size, direction);

        for (int i = 0; i < size; i++)
        {
            int boardX = startBoardPos.x;
            int boardY = startBoardPos.y;

            if (direction == ShipDirection.Horizontal)
                boardX += i;
            else
                boardY += i;

            BoardCell cell = GetCell(boardX, boardY);

            if (cell == null)
                continue;

            Vector3Int drawPos = BoardToTilemapPosition(boardX, boardY);

            // 지금은 배치 가능/불가능 색 구분 없이 SelectTile로 표시
            // 나중에 invalidTile을 따로 만들어서 빨간색 표시 가능
            markerTilemap.SetTile(drawPos, selectTile);
        }
    }

    public void PlaceShip(Vector3Int tilemapPos, int size, ShipDirection direction, int shipId)
    {
        if (!IsInsideBoard(tilemapPos))
        {
            Debug.Log("보드 밖이라 배치 불가");
            return;
        }

        Vector2Int startBoardPos = TilemapToBoardPosition(tilemapPos);

        if (!CanPlaceShip(startBoardPos, size, direction))
        {
            Debug.Log("배치 불가 위치입니다.");
            return;
        }

        for (int i = 0; i < size; i++)
        {
            int boardX = startBoardPos.x;
            int boardY = startBoardPos.y;

            if (direction == ShipDirection.Horizontal)
                boardX += i;
            else
                boardY += i;

            BoardCell cell = GetCell(boardX, boardY);
            cell.state = BoardCellState.Ship;
            cell.shipId = shipId;

            Vector3Int drawPos = BoardToTilemapPosition(boardX, boardY);
            shipTilemap.SetTile(drawPos, shipTile);
        }

        markerTilemap.ClearAllTiles();

        Debug.Log($"배 배치 완료 / Size:{size}, Direction:{direction}, ShipId:{shipId}");
    }
    public Vector3Int WorldToDockCell(Vector3 worldPos)
    {
        return dockTilemap.WorldToCell(worldPos);
    }

    public void SetDockTile(Vector3Int pos, TileBase tile)
    {
        dockTilemap.SetTile(pos, tile);
    }

    public TileBase GetDockTile(Vector3Int pos)
    {
        return dockTilemap.GetTile(pos);
    }

    public void ClearDockTile(Vector3Int pos)
    {
        dockTilemap.SetTile(pos, null);
    }

    public void ClearDockArea()
    {
        if (dockTilemap != null)
            dockTilemap.ClearAllTiles();
    }
    public void ClearPreview()
    {
        if (markerTilemap != null)
            markerTilemap.ClearAllTiles();
    }

    public bool TryPlaceShip(Vector3Int tilemapPos, int size, ShipDirection direction, int shipId)
    {
        if (!IsInsideBoard(tilemapPos))
        {
            Debug.Log("보드 밖이라 배치 불가");
            return false;
        }

        Vector2Int startBoardPos = TilemapToBoardPosition(tilemapPos);

        if (!CanPlaceShip(startBoardPos, size, direction))
        {
            Debug.Log("배치 불가 위치입니다.");
            return false;
        }

        for (int i = 0; i < size; i++)
        {
            int boardX = startBoardPos.x;
            int boardY = startBoardPos.y;

            if (direction == ShipDirection.Horizontal)
                boardX += i;
            else
                boardY += i;

            BoardCell cell = GetCell(boardX, boardY);

            if (cell == null)
                return false;

            cell.state = BoardCellState.Ship;
            cell.shipId = shipId;

            Vector3Int drawPos = BoardToTilemapPosition(boardX, boardY);
            shipTilemap.SetTile(drawPos, shipTile);
        }

        ClearPreview();

        Debug.Log($"배 배치 완료 / Size:{size}, Direction:{direction}, ShipId:{shipId}");
        return true;
    }
}