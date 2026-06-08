using System;
using System.Collections.Generic;

[Serializable]
public class AttackResultPacketData
{
    public int x;
    public int y;
    public AttackResult result;

    public List<BoardPointData> sunkAroundCells = new List<BoardPointData>();

    public AttackResultPacketData(int x, int y, AttackResult result)
    {
        this.x = x;
        this.y = y;
        this.result = result;
    }
}