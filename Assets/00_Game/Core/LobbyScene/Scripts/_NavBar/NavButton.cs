using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NavButton : MonoBehaviour
{
    [SerializeField] private RectTransform rectMain;
    [SerializeField] private Button btnMain;
    [SerializeField] private RectTransform icon;
    [SerializeField] private TextMeshProUGUI textName;

    [SerializeField] private bool isSelected;

    private float scaleSelected = 1.3f;
    private float raiseY = 100f;
    private float duration = 0.25f;
    private Ease easeSelected = Ease.OutBack;
    private Ease easeUnselected = Ease.OutQuad;

    private float iconPositionY;
    private bool hasInit;

    public RectTransform RectMain
    {
        get
        {
            if (rectMain == null)
                rectMain = transform as RectTransform;

            return rectMain;
        }
    }

    public RectTransform Icon => icon;
    public bool IsSelected => isSelected;

    public event Action<NavButton> OnClick;

    private void OnDestroy()
    {
        KillTween();
    }

    public void Init()
    {
        if (hasInit)
            return;

        hasInit = true;

        if (icon != null)
            iconPositionY = icon.anchoredPosition.y;

        if (btnMain != null)
            btnMain.OnClicked(() => OnClick?.Invoke(this));
    }

    public void SetConfig(float scaleSelected, float raiseY, float duration, Ease easeSelected, Ease easeUnselected)
    {
        this.scaleSelected = scaleSelected;
        this.raiseY = raiseY;
        this.duration = duration;
        this.easeSelected = easeSelected;
        this.easeUnselected = easeUnselected;
    }

    public void SetSelected(bool value, bool instant = false)
    {
        Init();

        isSelected = value;

        if (isSelected)
            Selected(instant);
        else
            Unselected(instant);
    }

    public void SetName(string value)
    {
        if (textName != null)
            textName.text = value;
    }

    public void SetInteractable(bool value)
    {
        if (btnMain != null)
            btnMain.interactable = value;
    }

    public void AutoGetField()
    {
        rectMain = GetComponent<RectTransform>();
        btnMain = GetComponent<Button>();
        icon = transform.childCount > 0 ? transform.GetChild(0) as RectTransform : null;
        textName = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void Selected(bool instant)
    {
        KillTween();
        SetTextActive(true);

        if (icon == null)
            return;

        if (instant || duration <= 0f)
        {
            icon.localScale = Vector3.one * scaleSelected;
            SetIconY(iconPositionY + raiseY);
            return;
        }

        icon.DOScale(scaleSelected, duration).SetEase(easeSelected).SetTarget(icon);
        icon.DOAnchorPosY(iconPositionY + raiseY, duration).SetEase(easeSelected).SetTarget(icon);
    }

    private void Unselected(bool instant)
    {
        KillTween();
        SetTextActive(false);

        if (icon == null)
            return;

        if (instant || duration <= 0f)
        {
            icon.localScale = Vector3.one;
            SetIconY(iconPositionY);
            return;
        }

        icon.DOScale(1f, duration).SetEase(easeUnselected).SetTarget(icon);
        icon.DOAnchorPosY(iconPositionY, duration).SetEase(easeUnselected).SetTarget(icon);
    }

    private void SetTextActive(bool value)
    {
        if (textName != null)
            textName.gameObject.SetActive(value);
    }

    private void SetIconY(float value)
    {
        Vector2 position = icon.anchoredPosition;
        position.y = value;
        icon.anchoredPosition = position;
    }

    private void KillTween()
    {
        if (icon != null)
            icon.DOKill();
    }
}
