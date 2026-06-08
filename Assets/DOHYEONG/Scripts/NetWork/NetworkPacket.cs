using System;

[Serializable]
public class NetworkPacket
{
    public PacketType type;
    public string jsonData;

    public NetworkPacket(PacketType type, string jsonData)
    {
        this.type = type;
        this.jsonData = jsonData;
    }
}