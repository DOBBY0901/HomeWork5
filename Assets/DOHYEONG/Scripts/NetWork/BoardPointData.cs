using System;

[Serializable]
public class BoardPointData
{
    public int x;
    public int y;

    public BoardPointData(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}