
public class ConsumableManager
{
    public static int TotalCoin() => UseProfile.Coin;
    
    public static void AddCoin(int amount)
    {
        UseProfile.Coin.Value += amount;
    }

    public static bool TrySubtractCoin(int amount)
    {
        if(amount < 0) return false;
        if (UseProfile.Coin < amount) return false;
        
        UseProfile.Coin.Value -= amount;
        return true;
    }

    public static int TotalHeart() => UseProfile.Heart;

    public static void AddHeart(int amount)
    {
        UseProfile.Heart.Value += amount;
        var maxHeart = LivesManager.Instance.maxHearts;
        if (UseProfile.Heart > maxHeart)
        {
            UseProfile.Heart.Value = maxHeart;
        }
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CHANGE_HEART, UseProfile.Heart);
    }

    public static bool TryUseHeart()
    {
        bool success = LivesManager.Instance.TryUseHeart();
        if (success) 
        {
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CHANGE_HEART, UseProfile.Heart);
        }
        return success;
    }

    public static void AddHeartUnlimitedHeart(int minutes)
    {
        LivesManager.Instance.AddUnlimitedHeart(minutes);
    }
}