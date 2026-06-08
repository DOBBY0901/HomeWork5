using System;

[Serializable]
public class ReadyPacketData
{
    public bool isReady;

    public ReadyPacketData(bool isReady)
    {
        this.isReady = isReady;
    }
}