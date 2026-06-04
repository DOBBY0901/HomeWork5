using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DockShip
{
    public int shipId;
    public int size;
    public bool isPlaced;

    public Vector3Int dockStartPos;
    public List<Vector3Int> dockCells = new List<Vector3Int>();

    public DockShip(int shipId, int size, Vector3Int dockStartPos)
    {
        this.shipId = shipId;
        this.size = size;
        this.dockStartPos = dockStartPos;
        isPlaced = false;
    }
}