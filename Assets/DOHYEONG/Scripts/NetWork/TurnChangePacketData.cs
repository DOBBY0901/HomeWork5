using System;

[Serializable]
public class TurnChangePacketData
{
    public bool isMyTurn;

    public TurnChangePacketData(bool isMyTurn)
    {
        this.isMyTurn = isMyTurn;
    }
}