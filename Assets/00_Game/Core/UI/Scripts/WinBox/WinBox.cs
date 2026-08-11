using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinBox : BaseBox, IPopupScale
{
    public Button btnReward;

    public Transform coinTarget;

    [Header("Progress")]
    public Image imageIconFill;
    public Image imageProgressFill;
    public Image imageIconBg;
    public TextMeshProUGUI txtPercent;

    [Header("Sprites (theo thứ tự unlock)")]
    public Sprite[] propSprites;

    protected override void Init()
    {
    }

    protected override void InitState()
    {
    }

    public void SetPropSprite(int index)
    {
        if (index < 0 || index >= propSprites.Length) return;

        imageIconBg.sprite = propSprites[index];
        imageIconFill.sprite = propSprites[index];
    }

    public void AnimateFill(float targetPercent)
    {
        btnReward.interactable = false;

        imageIconFill.fillAmount = 0f;
        imageProgressFill.fillAmount = 0f;

        DOTween.To(() => imageIconFill.fillAmount, x =>
        {
            imageIconFill.fillAmount = x;
            imageProgressFill.fillAmount = x;
            txtPercent.text = $"{(int)(x * 100)}%";
        }, targetPercent, 0.6f)
        .SetEase(Ease.OutCubic)
        .SetLink(gameObject)
        .OnComplete(() => btnReward.interactable = true);
    }
}
