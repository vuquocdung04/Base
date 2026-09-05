using System.Collections.Generic;
using UnityEngine;

public partial class BoosterController
{
    [SerializeField] private BoosterCatalogSO catalog;
    [SerializeField] private BoosterItem[] items = new BoosterItem[0];

    [Header("Tutorial")]
    [SerializeField] private BoosterIntro intro;
    [SerializeField] private string tutorialSortingLayer = "UI";
    [SerializeField] private int tutorialSortingOrder = 100;

    private readonly Dictionary<string, BoosterItem> lookup = new();

    private int CurrentLevel => GameConfigSO.Instance.CurrentLevel;
    private bool Testing => GameConfigSO.Instance.Testing;

    public int Count => items.Length;

    public BoosterItem GetItem(string id)
        => !string.IsNullOrEmpty(id) && lookup.TryGetValue(id, out BoosterItem item) ? item : null;

    public BoosterEntry GetEntry(string id) => catalog != null ? catalog.Find(id) : null;

    public int GetQuantity(string id) => BoosterData.GetAmount(id);

    public void AddQuantity(string id, int amount) => BoosterData.Add(id, amount);

    private void Consume(string id) => BoosterData.Add(id, -1);

    private void BuildLookup()
    {
        lookup.Clear();

        for (int i = 0; i < items.Length; i++)
        {
            BoosterItem item = items[i];
            if (item == null || string.IsNullOrEmpty(item.Id)) continue;

            if (lookup.ContainsKey(item.Id))
            {
                Debug.LogError($"[Booster] Trung boosterId '{item.Id}' tren nhieu BoosterItem.");
                continue;
            }

            lookup[item.Id] = item;
        }
    }

    private void SeedData()
    {
        if (Testing) BoosterData.UseMemoryOnly();
        if (catalog == null) return;

        for (int i = 0; i < catalog.boosters.Count; i++)
        {
            BoosterEntry entry = catalog.boosters[i];
            if (entry == null || string.IsNullOrEmpty(entry.id)) continue;

            BoosterRecord record = BoosterData.Get(entry.id);
            if (record.Seeded) continue;

            record.Amount += entry.startQuantity;
            record.Seeded = true;
        }

        BoosterData.Save();
    }

    private void ApplyConfigToItems()
    {
        for (int i = 0; i < items.Length; i++)
        {
            BoosterItem item = items[i];
            if (item == null || string.IsNullOrEmpty(item.Id)) continue;

            BoosterEntry entry = GetEntry(item.Id);
            if (entry == null) Debug.LogError($"[Booster] boosterId '{item.Id}' khong co trong BoosterCatalogSO.");

            item.SetIcon(BoosterCatalogSO.IconOf(item.Id));
            item.Setup(BoosterData.GetAmount(item.Id), entry != null ? entry.levelUnlock : 0, CurrentLevel);
        }
    }

    public BoosterItem FindTutorialItem()
    {
        for (int i = 0; i < items.Length; i++)
        {
            BoosterItem item = items[i];
            if (item == null || string.IsNullOrEmpty(item.Id)) continue;

            BoosterEntry entry = GetEntry(item.Id);
            if (entry == null || CurrentLevel != entry.levelUnlock) continue;

            if (!BoosterData.IsTutorialDone(item.Id)) return item;
        }

        return null;
    }
}
