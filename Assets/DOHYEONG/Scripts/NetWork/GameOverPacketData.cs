using System;

[Serializable]
public class GameOverPacketData
{
    public bool youWin;

    public GameOverPacketData(bool youWin)
    {
        this.youWin = youWin;
    }
}