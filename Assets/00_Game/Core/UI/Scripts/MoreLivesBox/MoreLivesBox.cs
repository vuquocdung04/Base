using Cysharp.Threading.Tasks;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoreLivesBox : BaseBox<MoreLivesBox>
{
    public Button btnClose;
    public Button btnCloseByPanel;
    public TextMeshProUGUI txtDisplayLives;
    public TextMeshProUGUI txtDisplayCooldownLives;
    public Button btnRefill;
    public Button btnRefillByAds;
    public TextMeshProUGUI txtDisplayCoin;

    private int cost;

    protected override void Init()
    {
        cost = 900;
        btnClose.OnClicked(Close);
        btnCloseByPanel.OnClicked(Close);

        btnRefill.OnClicked(delegate
        {
            if (ConsumableManager.TotalHeart() < LivesManager.Instance.maxHearts)
            {
                if (ConsumableManager.TrySubtractCoin(cost))
                {
                    ConsumableManager.AddHeart(1);
                    this.PostEvent(EventID.CHANGE_COIN);
                    AudioManager.Instance.PlaySfx("Reward");
                    Close();
                }
                else
                {
                    ToastManager.Instance.ShowToast("Not Enough Coins");
                }
            }
        });

        btnRefillByAds.OnClicked(delegate
        {
            if (ConsumableManager.TotalHeart() >= LivesManager.Instance.maxHearts)
            {
                AudioManager.Instance.PlaySfx("Heart is full");
                //return;
            }

            // LevelPlaySystem.Instance.ShowRewardAds(AdPlacement.RefillLife,
            //     delegate
            //     {
            //         CurrencyManager.AddHeart(1);
            //         AudioManager.Instance.PlaySfx("Reward");
            //         Close();
            //     },
            //     delegate
            //     {
            //         ToastManager.Instance.ShowToast("Ad skipped. No Heart rewarded.");
            //     }
            // );
        });
        txtDisplayCoin.text = cost.ToString();
        this.RegisterListener(EventID.CHANGE_HEART, UpdateHeartUI);
    }

    protected override void InitState()
    {
        Refresh();
    }

    private void Refresh()
    {
        UpdateHeartUI(null);

        txtDisplayCooldownLives.BindCountdownRealtime(
            getTimeRemaining: () => LivesManager.Instance.GetTimeToNextHeart(),
            textWhenZero: "Full",
            checkUnlimited: () => UseProfile.IsUnlimitedHeart,
            token: this.GetCancellationTokenOnDestroy()
        ).Forget();
    }

    private void UpdateHeartUI(object param)
    {
        txtDisplayLives.text = ConsumableManager.TotalHeart().ToString();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        this.RemoveListener(EventID.CHANGE_HEART, UpdateHeartUI);
    }
}