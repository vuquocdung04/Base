using EventDispatcher;

public partial class BoosterController : InitSingleton<BoosterController>
{
    public override void Init()
    {
        SeedData();
        ApplyConfigToItems();
        BindItems(true);

        ShowTutorialIfAny();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        BindItems(false);
    }

    private void BindItems(bool bind)
    {
        for (int i = 0; i < boosters.Count; i++)
        {
            BoosterItem item = boosters[i].item;
            if (item == null) continue;

            item.OnClickBuy -= HandleBuy;
            item.OnClickUse -= HandleUse;

            if (!bind) continue;

            item.OnClickBuy += HandleBuy;
            item.OnClickUse += HandleUse;
        }
    }

    private void HandleBuy(BoosterItem item) => PopupManager.Show<BuyBoosterBox>().Forget();

    private void HandleUse(BoosterItem item)
    {
        int index = IndexOf(item);
        if (index < 0) return;

        SetTutorialDone(index);
        Consume(index);

        this.PostEvent(EventID.BOOSTER_USED, index);
    }

    private void ShowTutorialIfAny()
    {
        if (FindTutorialIndex() < 0) return;

        PopupManager.Show<BoosterUnlockBox>().Forget();
    }

    private int FindTutorialIndex()
    {
        for (int i = 0; i < boosters.Count; i++)
        {
            if (CurrentLevel == boosters[i].levelUnlock && !IsTutorialDone(i)) return i;
        }
        return -1;
    }
}
