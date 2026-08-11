using TMPro;
using UnityEngine.UI;

public class KeepPlayingBox : BaseBox, IPopupScale
{
    public Button btnBuyTime;
    public TextMeshProUGUI txtCoinDisplay;

    protected override void Init()
    {
    }

    protected override void InitState()
    {
    }

    public void SetCost(string value) => txtCoinDisplay.text = value;
}
