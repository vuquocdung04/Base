using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum PackCostType
{
    Free,
    Coin,
    Ads,
    Iap
}

public enum PackPurchaseMode
{
    MultipleTimes,
    OneTime
}

[Serializable, HideReferenceObjectPicker]
public sealed class PackCost
{
    private const float CostLabelW = 90f;

    [LabelWidth(CostLabelW)]
    public PackCostType type = PackCostType.Coin;

    [ShowIf("@type == PackCostType.Coin || type == PackCostType.Ads")]
    [MinValue(0), LabelWidth(CostLabelW)]
    public int amount;

    [ShowIf(nameof(type), PackCostType.Iap), LabelWidth(CostLabelW)]
    public string productId;

    [ShowIf(nameof(type), PackCostType.Iap), LabelWidth(CostLabelW)]
    [Tooltip("Giá hiện tạm khi chưa nối store thật.")]
    public string fakePrice = "$0.99";
}

[Serializable, HideReferenceObjectPicker]
public sealed class PackRewardGroup
{
    [HideLabel]
    [ListDrawerSettings(ShowFoldout = false, DraggableItems = true, ShowItemCount = false)]
    public List<GameReward> rewards = new();

    [Button("＋ Thêm thưởng", ButtonHeight = 20), GUIColor(0.45f, 0.85f, 0.5f)]
    private void AddReward()
    {
        rewards ??= new List<GameReward>();
        rewards.Add(new GameReward());
    }
}

[Serializable, HideReferenceObjectPicker]
public sealed class PackConfig
{
    private const float LabelW = 150f;

    private string Label => string.IsNullOrWhiteSpace(packId) ? "(chưa đặt packId)" : packId;

    [FoldoutGroup("$Label")]
    [HorizontalGroup("$Label/Split", 0.56f, MarginRight = 6)]
    [BoxGroup("$Label/Split/Left", LabelText = "Thiết lập")]
    [Title("Catalog"), LabelWidth(LabelW)]
    public string packId;

    [BoxGroup("$Label/Split/Left"), LabelWidth(LabelW)]
    public string displayName;

    [BoxGroup("$Label/Split/Left"), LabelWidth(LabelW)]
    [PreviewField(52, ObjectFieldAlignment.Left)]
    public Sprite icon;

    [BoxGroup("$Label/Split/Left"), Title("Hiển thị"), LabelWidth(LabelW)]
    public bool active = true;

    [BoxGroup("$Label/Split/Left"), LabelWidth(LabelW), MinValue(1)]
    public int showFromLevel = 1;

    [BoxGroup("$Label/Split/Left"), Title("Mua"), LabelWidth(LabelW)]
    public PackPurchaseMode purchaseMode = PackPurchaseMode.MultipleTimes;

    [BoxGroup("$Label/Split/Left"), LabelWidth(LabelW)]
    public bool hideAfterPurchased = true;

    [BoxGroup("$Label/Split/Right", LabelText = "Giá & Thưởng")]
    [Title("Giá"), HideLabel]
    public PackCost cost = new();

    [BoxGroup("$Label/Split/Right"), Title("Thưởng — mỗi nhóm là 1 ô"), HideLabel]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowItemCount = false)]
    public List<PackRewardGroup> rewardGroups = new();

    [BoxGroup("$Label/Split/Right")]
    [Button("＋ Thêm ô", ButtonHeight = 22), GUIColor(0.4f, 0.7f, 1f)]
    private void AddGroup()
    {
        rewardGroups ??= new List<PackRewardGroup>();
        rewardGroups.Add(new PackRewardGroup());
    }

    public int RewardCount
    {
        get
        {
            int total = 0;

            for (int i = 0; i < rewardGroups.Count; i++)
            {
                PackRewardGroup group = rewardGroups[i];
                if (group != null && group.rewards != null) total += group.rewards.Count;
            }

            return total;
        }
    }

    public List<GameReward> CollectRewards()
    {
        var all = new List<GameReward>();

        for (int i = 0; i < rewardGroups.Count; i++)
        {
            PackRewardGroup group = rewardGroups[i];
            if (group == null || group.rewards == null) continue;

            for (int j = 0; j < group.rewards.Count; j++)
            {
                GameReward reward = group.rewards[j];
                if (reward != null && !string.IsNullOrEmpty(reward.id)) all.Add(reward);
            }
        }

        return all;
    }

    public bool IsOneTime => purchaseMode == PackPurchaseMode.OneTime;

    public bool CanShow(int level, bool purchased)
    {
        if (!active) return false;
        if (level < Mathf.Max(1, showFromLevel)) return false;

        return !(IsOneTime && hideAfterPurchased && purchased);
    }
}

[CreateAssetMenu(menuName = "Base/Shop/Pack Catalog", fileName = "PackCatalog")]
public class PackCatalog : ScriptableObject
{
    public const string RESOURCE_PATH = "PackCatalog";

    public List<PackConfig> packs = new();

    public PackConfig Find(string packId)
    {
        if (string.IsNullOrEmpty(packId)) return null;

        for (int i = 0; i < packs.Count; i++)
        {
            if (packs[i] != null && packs[i].packId == packId) return packs[i];
        }
        return null;
    }
}
