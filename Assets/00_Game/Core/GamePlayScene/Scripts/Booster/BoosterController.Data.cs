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
        public bool doneTutorial;
    }

    [TableList(AlwaysExpanded = true, DrawScrollView = false)]
    [SerializeField] private List<BoosterConfig> boosters = new();

    [Header("Tutorial")]
    [SerializeField] private string tutorialSortingLayer = "UI";
    [SerializeField] private int tutorialSortingOrder = 100;

    private int CurrentLevel => GamePlayController.Instance.CurrentLevel;
    private bool Testing => GamePlayController.Instance.Testing;

    public int Count => boosters.Count;

    public BoosterItem GetItem(int index) => ConfigOf(index)?.item;

    private BoosterConfig ConfigOf(int index)
        => index >= 0 && index < boosters.Count ? boosters[index] : null;

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
        if (Testing) return;

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

    public int GetQuantity(int index)
    {
        BoosterConfig config = ConfigOf(index);
        if (config == null) return 0;

        return Testing ? config.quantity : BoosterData.Get(index).Amount;
    }

    public void AddQuantity(int index, int amount) => SetQuantity(index, GetQuantity(index) + amount);

    private void Consume(int index) => SetQuantity(index, GetQuantity(index) - 1);

    private void SetQuantity(int index, int amount)
    {
        BoosterConfig config = ConfigOf(index);
        if (config == null) return;

        amount = Mathf.Max(0, amount);

        if (Testing)
        {
            config.quantity = amount;
        }
        else
        {
            BoosterData.Get(index).Amount = amount;
            BoosterData.Save();
        }

        config.item?.SetQuantity(amount);
    }

    private bool IsTutorialDone(int index)
    {
        BoosterConfig config = ConfigOf(index);
        if (config == null) return true;

        return Testing ? config.doneTutorial : BoosterData.Get(index).TutorialDone;
    }

    private void SetTutorialDone(int index)
    {
        BoosterConfig config = ConfigOf(index);
        if (config == null || IsTutorialDone(index)) return;

        if (Testing)
        {
            config.doneTutorial = true;
            return;
        }

        BoosterData.Get(index).TutorialDone = true;
        BoosterData.Save();
    }
}
