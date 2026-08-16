using System;

[Serializable]
public class GameReward
{
    public string id;
    public int quantity;

    public GameReward() { }

    public GameReward(string id, int quantity)
    {
        this.id = id;
        this.quantity = quantity;
    }
}
