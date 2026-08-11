using TMPro;
using UnityEngine.UI;

public class MoreLivesBox : BaseBox, IPopupScale
{
    public TextMeshProUGUI txtDisplayLives;
    public TextMeshProUGUI txtDisplayCooldownLives;
    public Button btnRefill;
    public Button btnRefillByAds;
    public TextMeshProUGUI txtDisplayCoin;

    protected override void Init()
    {
    }

    protected override void InitState()
    {
    }

    public void SetLives(string value) => txtDisplayLives.text = value;

    public void SetCooldown(string value) => txtDisplayCooldownLives.text = value;

    public void SetCost(string value) => txtDisplayCoin.text = value;
}
