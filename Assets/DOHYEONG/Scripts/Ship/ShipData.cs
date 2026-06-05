using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShipData
{
    public int shipId; //배 ID
    public int size; //배 크기

    public List<Vector2Int> occupiedCells = new List<Vector2Int>();
    public List<Vector2Int> hitCells = new List<Vector2Int>();

    public ShipData(int shipId, int size)
    {
        this.shipId = shipId;
        this.size = size;
    }

    public void AddCell(int x, int y) //배가 해당 좌표를 차지한다는 것을 기록
    {
        Vector2Int cell = new Vector2Int(x, y);

        if (!occupiedCells.Contains(cell))
            occupiedCells.Add(cell);
    }

    public void AddHit(int x, int y) //배가 해당 좌표에서 공격을 받았음을 기록
    {
        Vector2Int cell = new Vector2Int(x, y);

        if (!hitCells.Contains(cell))
            hitCells.Add(cell);
    }

    public bool ContainsCell(int x, int y) //배가 해당 좌표를 차지하는지 여부
    {
        return occupiedCells.Contains(new Vector2Int(x, y));
    }

    public bool IsSunk() //배가 침몰했는지 여부
    {
        return hitCells.Count >= size;
    }
}