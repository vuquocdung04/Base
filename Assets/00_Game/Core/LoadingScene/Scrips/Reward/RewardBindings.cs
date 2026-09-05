using Dispatcher = EventDispatcher.EventDispatcher;

public static class RewardBindings
{
    public static void Bind(RewardManager reward)
    {
        if (reward == null) return;

        reward.Bind(RewardKeys.Coin, r =>
        {
            UseProfile.Coin += r.quantity;
            Dispatcher.Instance.PostEvent(EventID.CHANGE_COIN);
        });

        reward.Bind(RewardKeys.Heart, r => HeartManager.Instance.TryAddHeart(r.quantity));

        reward.Bind(RewardKeys.HeartUnlimited, r => HeartManager.Instance.TryAddUnlimited(r.quantity));

        reward.Bind(RewardKeys.RemoveAds, r => UseProfile.IsRemoveAds = true);
        reward.BindOwned(RewardKeys.RemoveAds, () => UseProfile.IsRemoveAds);

        BindBoosters(reward);
    }

    private static void BindBoosters(RewardManager reward)
    {
        RewardCatalogSO catalog = reward.Catalog;
        if (catalog == null) return;

        for (int i = 0; i < catalog.rewards.Count; i++)
        {
            RewardEntry entry = catalog.rewards[i];
            if (entry == null || entry.category != RewardCategory.Booster) continue;
            if (string.IsNullOrEmpty(entry.id) || reward.IsBound(entry.id)) continue;

            string id = entry.id;
            reward.Bind(id, r => BoosterData.Add(id, r.quantity));
        }
    }
}
