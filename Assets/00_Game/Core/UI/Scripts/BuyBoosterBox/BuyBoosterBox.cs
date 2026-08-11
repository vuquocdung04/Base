using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyBoosterBox : BaseBox, IPopupScale
{
    public Button btnBuyByCoin;
    public TextMeshProUGUI txtTitle;
    public TextMeshProUGUI textDes;
    public TextMeshProUGUI txtPrice;
    public Image icon;

    protected override void Init()
    {
    }

    protected override void InitState()
    {
    }

    public void SetContent(Sprite iconSprite, string title, string description, string price)
    {
        if (iconSprite != null) icon.sprite = iconSprite;
        txtTitle.text = title;
        textDes.text = description;
        txtPrice.text = price;
    }
}
