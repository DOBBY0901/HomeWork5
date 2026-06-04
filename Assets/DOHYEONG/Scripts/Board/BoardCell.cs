[System.Serializable]
public class BoardCell
{
    public int x;
    public int y;
    public BoardCellState state;
    public int shipId = -1;

    public BoardCell(int x, int y)
    {
        this.x = x;
        this.y = y;
        state = BoardCellState.Sea;
        shipId = -1;
    }
}