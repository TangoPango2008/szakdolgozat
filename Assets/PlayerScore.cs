using System;

[Serializable]
public class PlayerScore
{
    public string playerName;
    public float time;

    public PlayerScore(string name, float time)
    {
        playerName = name;
        this.time = time;
    }
}