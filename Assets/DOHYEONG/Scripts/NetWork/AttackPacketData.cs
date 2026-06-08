using System;

[Serializable]
public class AttackPacketData
{
    public int x;
    public int y;

    public AttackPacketData(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}