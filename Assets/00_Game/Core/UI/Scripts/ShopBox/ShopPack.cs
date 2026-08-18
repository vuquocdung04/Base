using System.Collections.Generic;
using UnityEngine;

public class ShopPack : ShopPackBase
{
    [Header("Reward Items")]
    public RewardItemView rewardItemPrefab;

    [Tooltip("Mỗi ô ứng với 1 nhóm thưởng trong catalog, theo thứ tự.")]
    public List<Transform> rewardItemParents = new();

    private readonly List<RewardItemView> _items = new();

    protected override void FillRewards()
    {
        _items.ClearSpawned();

        int groupCount = Config.rewardGroups != null ? Config.rewardGroups.Count : 0;

        for (int i = 0; i < rewardItemParents.Count; i++)
        {
            Transform parent = rewardItemParents[i];
            if (parent == null) continue;

            List<GameReward> group = i < groupCount ? Config.rewardGroups[i]?.rewards : null;
            bool used = group != null && group.Count > 0;

            parent.gameObject.SetActive(used);

            if (!used || rewardItemPrefab == null) continue;

            for (int j = 0; j < group.Count; j++)
            {
                RewardItemView item = Instantiate(rewardItemPrefab, parent);
                item.Setup(group[j]);
                _items.Add(item);
            }

            if (parent is RectTransform holder) holder.FitToBounds();
        }

        if (groupCount > rewardItemParents.Count)
            Debug.LogWarning($"[Shop] '{packId}' có {groupCount} nhóm thưởng nhưng chỉ có {rewardItemParents.Count} ô.", this);
    }
}
