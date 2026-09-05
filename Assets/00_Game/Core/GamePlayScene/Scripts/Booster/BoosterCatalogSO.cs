using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable, HideReferenceObjectPicker]
public sealed class BoosterEntry
{
    [TableColumnWidth(60, false)]
    [ShowInInspector, PreviewField(48, ObjectFieldAlignment.Center), HideLabel]
    public Sprite Icon => BoosterCatalogSO.IconOf(id);

    [TableColumnWidth(130), HideLabel]
    [ValueDropdown("@BoosterCatalogSO.BoosterIds()")]
    public string id;

    [TableColumnWidth(90, false), MinValue(0)]
    public int levelUnlock;

    [TableColumnWidth(90, false), MinValue(0)]
    public int startQuantity;

    [TableColumnWidth(80, false), MinValue(0)]
    public int price;

    [TableColumnWidth(140)]
    public string nameKey;

    [TableColumnWidth(180)]
    public string descKey;
}

[CreateAssetMenu(menuName = "Base/Booster/Booster Catalog", fileName = "BoosterCatalogSO")]
public class BoosterCatalogSO : ScriptableObject
{
    [TableList(AlwaysExpanded = true, DrawScrollView = false)]
    public List<BoosterEntry> boosters = new();

    private static RewardCatalogSO _rewardsCache;

    private static RewardCatalogSO Rewards
    {
        get
        {
            if (RewardManager.Instance != null && RewardManager.Instance.Catalog != null)
                return RewardManager.Instance.Catalog;

            if (_rewardsCache == null) _rewardsCache = Resources.Load<RewardCatalogSO>(RewardCatalogSO.RESOURCE_PATH);

            return _rewardsCache;
        }
    }

    public BoosterEntry Find(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        for (int i = 0; i < boosters.Count; i++)
        {
            if (boosters[i] != null && boosters[i].id == id) return boosters[i];
        }
        return null;
    }

    public BoosterEntry FindByLevelUnlock(int levelUnlock)
    {
        for (int i = 0; i < boosters.Count; i++)
        {
            if (boosters[i] != null && boosters[i].levelUnlock == levelUnlock) return boosters[i];
        }
        return null;
    }

    public static Sprite IconOf(string id) => Rewards != null ? Rewards.GetIcon(id) : null;

    public static IEnumerable<string> BoosterIds()
    {
        RewardCatalogSO catalog = Rewards;
        if (catalog == null) yield break;

        for (int i = 0; i < catalog.rewards.Count; i++)
        {
            RewardEntry entry = catalog.rewards[i];
            if (entry == null || entry.category != RewardCategory.Booster) continue;
            if (string.IsNullOrEmpty(entry.id)) continue;

            yield return entry.id;
        }
    }
}
