using System;
using System.Threading;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BoosterType
{
    Booster0 = 0,
    Booster1 = 1,
    Booster2 = 2
}

public enum BoosterState
{
    Locked = 0,
    Available = 1,
    Empty = 2,
    InUse = 3,
    Cooldown = 4
}


public class BoosterItem : MonoBehaviour
{
    [SerializeField] private BoosterType type;
    public BoosterType Type => type;

    public Button btnMain;
    public Image iconBooster;

    [Header("State UI Containers")]
    [SerializeField] private GameObject unlockedContainer;
    [SerializeField] private GameObject lockedContainer;

    [Header("Available & Empty UI")]
    [SerializeField] private GameObject quantityInfoGroup;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private GameObject addIconOverlay;

    [Header("InUse UI")]
    [SerializeField] private GameObject inUseHighlight;

    [Header("Cooldown UI")]
    [SerializeField] private GameObject cooldownGroup;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private TextMeshProUGUI cooldownText;

    [Header("Lock UI")]
    [SerializeField] private TextMeshProUGUI unlockLevelText;

    private BoosterState _currentState;
    private Tween _cooldownTween;

    private void Start()
    {
        btnMain.OnClicked(OnButtonClicked);
    }
    private void OnDestroy()
    {
        CancelCooldownTask();
    }

    public void SetSize(float targetSize)
    {
        iconBooster.FitToTargetHeight(targetSize);
    }
    public void ChangeState(BoosterState newState)
    {
        _currentState = newState;

        addIconOverlay.SetActive(false);
        quantityInfoGroup.SetActive(false);
        inUseHighlight.SetActive(false);
        cooldownGroup.SetActive(false);
        switch (_currentState)
        {
            case BoosterState.Locked:
                unlockedContainer.SetActive(false);
                lockedContainer.SetActive(true);
                break;

            case BoosterState.Available:
                unlockedContainer.SetActive(true);
                lockedContainer.SetActive(false);
                quantityInfoGroup.SetActive(true);
                break;

            case BoosterState.Empty:
                unlockedContainer.SetActive(true);
                lockedContainer.SetActive(false);
                addIconOverlay.SetActive(true);
                break;

            case BoosterState.InUse:
                unlockedContainer.SetActive(true);
                lockedContainer.SetActive(false);
                inUseHighlight.SetActive(true);
                break;

            case BoosterState.Cooldown:
                unlockedContainer.SetActive(true);
                lockedContainer.SetActive(false);
                cooldownGroup.SetActive(true);
                break;
        }

    }
    private void UpdateCooldownUI(float timeLeft, float totalCooldown)
    {
        if (_currentState != BoosterState.Cooldown) return;

        cooldownFillImage.fillAmount = timeLeft / totalCooldown;
        cooldownText.text = Mathf.CeilToInt(timeLeft).ToString();
    }
    public void StartCooldown(float duration)
    {
        ChangeState(BoosterState.Cooldown);


        CancelCooldownTask();

        _cooldownTween = DOVirtual.Float(duration, 0f, duration, (timeLeft) =>
        {
            UpdateCooldownUI(timeLeft, duration);
        })
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            ChangeState(BoosterState.Available);
        });
    }
    private void CancelCooldownTask()
    {
        if (_cooldownTween != null)
        {
            _cooldownTween.Kill();
            _cooldownTween = null;
        }
    }
    private void OnButtonClicked()
    {
        if (_currentState == BoosterState.Available)
        {
            this.PostEvent(EventID.BOOSTER_USE_REQUEST, type);
        }
        else if (_currentState == BoosterState.InUse)
        {
            this.PostEvent(EventID.BOOSTER_CANCEL_REQUEST, type);
        }
        else if (_currentState == BoosterState.Empty)
        {
            Debug.Log("Mở popup mua thêm!");
        }
    }


}
