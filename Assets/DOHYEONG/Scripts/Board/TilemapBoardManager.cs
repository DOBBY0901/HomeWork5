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

    [Header("Marker Tiles")]
    [SerializeField] private TileBase hitTile;
    [SerializeField] private TileBase missTile;
    [SerializeField] private TileBase selectTile;

    private BoardCell[,] cells;

    public int Width => width;
    public int Height => height;

    private void Awake()
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
        if (cells == null)
        {
            Debug.LogError($"{gameObject.name}의 BoardCell 배열이 아직 초기화되지 않았습니다.");
            return null;
        }

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

    public Vector3Int DockWorldToCell(Vector3 worldPos)
    {
        if (dockTilemap == null)
            return Vector3Int.zero;

        return dockTilemap.WorldToCell(worldPos);
    }

    public Vector3 CellToWorldCenter(Vector3Int tilemapPos)
    {
        return mapTilemap.GetCellCenterWorld(tilemapPos);
    }

    public Vector3 DockCellToWorldCenter(Vector3Int dockPos)
    {
        if (dockTilemap == null)
            return Vector3.zero;

        return dockTilemap.GetCellCenterWorld(dockPos);
    }

    public void ClearPreview()
    {
        if (markerTilemap != null)
            markerTilemap.ClearAllTiles();
    }

    public void ClearShipTiles()
    {
        if (shipTilemap != null)
            shipTilemap.ClearAllTiles();
    }

    public void PreviewShip(Vector3Int tilemapPos, int size, ShipDirection direction)
    {
        ClearPreview();

        if (!IsInsideBoard(tilemapPos))
            return;

        Vector2Int startBoardPos = TilemapToBoardPosition(tilemapPos);

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

            if (markerTilemap != null)
                markerTilemap.SetTile(drawPos, selectTile);
        }
    }

    public void SetMarkerPreview(Vector3Int tilemapPos)
    {
        ClearPreview();

        if (!IsInsideBoard(tilemapPos))
            return;

        Vector2Int boardPos = TilemapToBoardPosition(tilemapPos);
        BoardCell cell = GetCell(boardPos.x, boardPos.y);

        if (cell == null)
            return;

        if (cell.state == BoardCellState.Land)
            return;

        if (markerTilemap != null)
            markerTilemap.SetTile(tilemapPos, selectTile);
    }

    public void DrawHitTile(int boardX, int boardY)
    {
        if (markerTilemap == null)
            return;

        Vector3Int drawPos = BoardToTilemapPosition(boardX, boardY);
        markerTilemap.SetTile(drawPos, hitTile);
    }

    public void DrawMissTile(int boardX, int boardY)
    {
        if (markerTilemap == null)
            return;

        Vector3Int drawPos = BoardToTilemapPosition(boardX, boardY);
        markerTilemap.SetTile(drawPos, missTile);
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

        if (cell == null)
            return;

        Debug.Log($"Board X:{cell.x}, Y:{cell.y} / Tilemap X:{tilemapPos.x}, Y:{tilemapPos.y} / State:{cell.state} / ShipId:{cell.shipId}");
    }

    public void PrintTilemapPositionOnly(Vector3Int tilemapPos)
    {
        TileBase clickedTile = mapTilemap.GetTile(tilemapPos);
        string tileName = clickedTile != null ? clickedTile.name : "None";

        Debug.Log($"클릭한 Tilemap 좌표: X={tilemapPos.x}, Y={tilemapPos.y}, Z={tilemapPos.z}, Tile={tileName}");
    }

    public void SetDockTile(Vector3Int pos, TileBase tile)
    {
        if (dockTilemap == null)
            return;

        dockTilemap.SetTile(pos, tile);
    }

    public TileBase GetDockTile(Vector3Int pos)
    {
        if (dockTilemap == null)
            return null;

        return dockTilemap.GetTile(pos);
    }

    public void ClearDockTile(Vector3Int pos)
    {
        if (dockTilemap == null)
            return;

        dockTilemap.SetTile(pos, null);
    }

    public void ClearDockArea()
    {
        if (dockTilemap != null)
            dockTilemap.ClearAllTiles();
    }
}