using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public partial class BoosterController
{
    [Serializable]
    public class BoosterConfig
    {
        [TableColumnWidth(170, resizable: false)]
        public BoosterItem item;
        public int levelUnlock;
        public int quantity;
    }

    [TableList(AlwaysExpanded = true, DrawScrollView = false)]
    [SerializeField] private List<BoosterConfig> boosters = new();

    private int CurrentLevel => UseProfile.Level;

    public int Count => boosters.Count;

    public BoosterItem GetItem(int index)
        => index >= 0 && index < boosters.Count ? boosters[index].item : null;

    private int IndexOf(BoosterItem item)
    {
        for (int i = 0; i < boosters.Count; i++)
        {
            if (boosters[i].item == item) return i;
        }
        return -1;
    }

    private void SeedData()
    {
        for (int i = 0; i < boosters.Count; i++)
        {
            if (BoosterData.Has(i)) continue;
            BoosterData.Get(i).Amount = boosters[i].quantity;
        }
        BoosterData.Save();
    }

    private void ApplyConfigToItems()
    {
        for (int i = 0; i < boosters.Count; i++)
        {
            BoosterConfig config = boosters[i];
            if (config.item == null) continue;

            config.item.Setup(GetQuantity(i), config.levelUnlock, CurrentLevel);
        }
    }

    public int GetQuantity(int index) => BoosterData.Get(index).Amount;

    public void AddQuantity(int index, int amount) => SetQuantity(index, GetQuantity(index) + amount);

    private void Consume(int index) => SetQuantity(index, GetQuantity(index) - 1);

    private void SetQuantity(int index, int amount)
    {
        BoosterData.Get(index).Amount = Mathf.Max(0, amount);
        BoosterData.Save();

        GetItem(index)?.SetQuantity(GetQuantity(index));
    }

    private bool IsTutorialDone(int index) => BoosterData.Get(index).TutorialDone;

    private void SetTutorialDone(int index)
    {
        if (IsTutorialDone(index)) return;

        BoosterData.Get(index).TutorialDone = true;
        BoosterData.Save();
    }
}
