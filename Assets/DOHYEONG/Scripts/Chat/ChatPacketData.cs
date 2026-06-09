using System;

[Serializable]
public class ChatPacketData
{
    public string senderName;
    public string message;

    public ChatPacketData(string senderName, string message)
    {
        this.senderName = senderName;
        this.message = message;
    }
}