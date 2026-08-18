using System;
using System.Collections.Generic;
using UnityEngine;

public class PackManager : MonoBehaviour
{
    public static PackManager Instance { get; private set; }

    private const string PurchasedPrefix = "PACK_PURCHASED_";

    [SerializeField] private PackCatalog catalog;

    private readonly Dictionary<PackCostType, Func<PackConfig, Awaitable<bool>>> _payers = new();
    private readonly Dictionary<PackCostType, Func<PackConfig, string>> _prices = new();

    public PackCatalog Catalog => catalog;

    public static Func<int> DisplayLevelProvider { get; set; }

    public event Action<PackConfig> OnPurchased;

    public void Init()
    {
        Instance = this;
        _payers.Clear();
        _prices.Clear();
        EnsureCatalog();
    }

    private void EnsureCatalog()
    {
        if (catalog != null) return;

        catalog = Resources.Load<PackCatalog>(PackCatalog.RESOURCE_PATH);

        if (catalog == null)
            Debug.LogError($"[Pack] Khong tim thay PackCatalog trong Resources/{PackCatalog.RESOURCE_PATH}.");
    }

    public void BindPayer(PackCostType type, Func<PackConfig, Awaitable<bool>> payer)
    {
        if (payer == null) return;

        if (_payers.ContainsKey(type))
            Debug.LogError($"[Pack] Payer cho {type} da Bind roi, lan sau se ghi de len lan truoc.");

        _payers[type] = payer;
    }

    public void BindPrice(PackCostType type, Func<PackConfig, string> provider)
    {
        if (provider == null) return;

        _prices[type] = provider;
    }

    public bool IsBound(PackCostType type) => _payers.ContainsKey(type);

    public PackConfig Find(string packId) => catalog != null ? catalog.Find(packId) : null;

    public int CurrentLevel => DisplayLevelProvider != null ? DisplayLevelProvider() : UseProfile.Level;

    public bool IsPurchased(string packId)
        => !string.IsNullOrEmpty(packId) && GamePrefs.Get(PurchasedPrefix + packId, false);

    public bool CanShow(PackConfig pack)
    {
        if (pack == null) return false;
        if (!pack.CanShow(CurrentLevel, IsPurchased(pack.packId))) return false;

        return !AllFlagsOwned(pack);
    }

    private bool AllFlagsOwned(PackConfig pack)
    {
        RewardManager reward = RewardManager.Instance;
        if (reward == null || pack.rewardGroups == null) return false;

        bool hasFlag = false;

        for (int i = 0; i < pack.rewardGroups.Count; i++)
        {
            PackRewardGroup group = pack.rewardGroups[i];
            if (group == null || group.rewards == null) continue;

            for (int j = 0; j < group.rewards.Count; j++)
            {
                GameReward item = group.rewards[j];
                if (item == null || !reward.IsFlag(item.id)) continue;

                hasFlag = true;
                if (!reward.IsOwned(item.id)) return false;
            }
        }

        return hasFlag;
    }

    public List<PackConfig> GetVisible()
    {
        var result = new List<PackConfig>();
        if (catalog == null) return result;

        for (int i = 0; i < catalog.packs.Count; i++)
        {
            PackConfig pack = catalog.packs[i];
            if (CanShow(pack)) result.Add(pack);
        }

        return result;
    }

    public string GetPriceText(string packId) => GetPriceText(Find(packId));

    public string GetPriceText(PackConfig pack)
    {
        if (pack == null) return string.Empty;

        return _prices.TryGetValue(pack.cost.type, out Func<PackConfig, string> provider)
            ? provider(pack)
            : pack.cost.amount.ToString();
    }

    public Awaitable<bool> TryPurchase(string packId) => TryPurchase(Find(packId));

    public async Awaitable<bool> TryPurchase(PackConfig pack)
    {
        if (pack == null)
        {
            Debug.LogError("[Pack] Khong tim thay pack.");
            return false;
        }

        if (pack.IsOneTime && IsPurchased(pack.packId))
        {
            Debug.LogWarning($"[Pack] '{pack.packId}' la one-time va da mua roi.");
            return false;
        }

        if (!_payers.TryGetValue(pack.cost.type, out Func<PackConfig, Awaitable<bool>> payer))
        {
            Debug.LogError($"[Pack] Chua BindPayer cho {pack.cost.type} — khong mua duoc '{pack.packId}'.");
            return false;
        }

        if (!await payer(pack)) return false;

        if (pack.IsOneTime) MarkPurchased(pack.packId);

        OnPurchased?.Invoke(pack);

        if (RewardManager.Instance != null) await RewardManager.Instance.ClaimAsync(pack.CollectRewards());

        return true;
    }

    public void RestorePurchase(string packId) => RestorePurchase(Find(packId));

    public void RestorePurchase(PackConfig pack)
    {
        if (pack == null) return;

        MarkPurchased(pack.packId);
        GrantFlags(pack);

        OnPurchased?.Invoke(pack);
    }

    private void GrantFlags(PackConfig pack)
    {
        RewardManager reward = RewardManager.Instance;
        if (reward == null || pack.rewardGroups == null) return;

        for (int i = 0; i < pack.rewardGroups.Count; i++)
        {
            PackRewardGroup group = pack.rewardGroups[i];
            if (group == null || group.rewards == null) continue;

            for (int j = 0; j < group.rewards.Count; j++)
            {
                GameReward item = group.rewards[j];
                if (item != null && reward.IsFlag(item.id)) reward.Grant(item);
            }
        }
    }

    public void MarkPurchased(string packId)
    {
        if (string.IsNullOrEmpty(packId)) return;

        GamePrefs.Set(PurchasedPrefix + packId, true);
    }

    public void ClearPurchased(string packId)
    {
        if (string.IsNullOrEmpty(packId)) return;

        GamePrefs.Delete(PurchasedPrefix + packId);
    }
}
