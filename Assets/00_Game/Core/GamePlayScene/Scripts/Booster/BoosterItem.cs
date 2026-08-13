using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterItem : MonoBehaviour
{
    public Button btnMain;
    public Image iconBooster;

    [Header("Containers")]
    [SerializeField] private GameObject unlockedContainer;
    [SerializeField] private GameObject lockedContainer;

    [Header("State UI")]
    [SerializeField] private GameObject quantityInfoGroup;
    [SerializeField] private GameObject addIconOverlay;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI unlockLevelText;

    [Header("Tutorial")]
    [SerializeField] private Transform hand;
    [SerializeField] private float handRaiseY = 30f;
    [SerializeField] private float handDuration = 0.5f;
    [SerializeField] private Ease handEase = Ease.InOutSine;

    private float _handBaseY;
    private Tween _handTween;
    private Canvas _tutorialCanvas;
    private GraphicRaycaster _tutorialRaycaster;

    public Sprite IconSprite => iconBooster.sprite;
    public RectTransform IconRect => iconBooster != null ? iconBooster.rectTransform : null;
    public int Quantity { get; private set; }
    public bool IsLocked { get; private set; }
    public bool IsTutorial { get; private set; }

    public event Action<BoosterItem> OnClickBuy;
    public event Action<BoosterItem> OnClickUse;

    private void Awake()
    {
        if (hand == null) return;

        _handBaseY = hand.localPosition.y;
        hand.gameObject.SetActive(false);
    }

    private void Start() => btnMain.OnClicked(HandleClick);

    private void OnDestroy() => KillHand();

    public void Setup(int quantity, int unlockLevel, int currentLevel)
    {
        IsLocked = currentLevel < unlockLevel;
        if (unlockLevelText != null) unlockLevelText.text = unlockLevel.ToString();

        SetQuantity(quantity);
    }

    public void SetQuantity(int value)
    {
        Quantity = Mathf.Max(0, value);
        Refresh();
    }

    private void Refresh()
    {
        lockedContainer.SetActive(IsLocked);
        unlockedContainer.SetActive(!IsLocked);
        quantityInfoGroup.SetActive(!IsLocked && Quantity > 0);
        addIconOverlay.SetActive(!IsLocked && Quantity <= 0);

        if (quantityText != null) quantityText.text = Quantity.ToString();
    }

    public void SetTutorial(bool value, string sortingLayer = null, int sortingOrder = 0)
    {
        if (IsTutorial == value) return;

        IsTutorial = value;
        SetSorting(value, sortingLayer, sortingOrder);

        if (hand != null) hand.gameObject.SetActive(value);

        if (value) PlayHand();
        else KillHand();
    }

    private void SetSorting(bool value, string sortingLayer, int sortingOrder)
    {
        if (value)
        {
            if (_tutorialCanvas == null) _tutorialCanvas = gameObject.AddComponent<Canvas>();

            _tutorialCanvas.overrideSorting = true;
            _tutorialCanvas.sortingOrder = sortingOrder;
            if (!string.IsNullOrEmpty(sortingLayer)) _tutorialCanvas.sortingLayerName = sortingLayer;

            if (_tutorialRaycaster == null) _tutorialRaycaster = gameObject.AddComponent<GraphicRaycaster>();
            return;
        }

        if (_tutorialRaycaster != null)
        {
            Destroy(_tutorialRaycaster);
            _tutorialRaycaster = null;
        }

        if (_tutorialCanvas != null)
        {
            Destroy(_tutorialCanvas);
            _tutorialCanvas = null;
        }
    }

    private void PlayHand()
    {
        if (hand == null) return;

        KillHand();

        _handTween = hand.DOLocalMoveY(_handBaseY + handRaiseY, handDuration)
                         .SetEase(handEase)
                         .SetLoops(-1, LoopType.Yoyo)
                         .SetUpdate(true);
    }

    private void KillHand()
    {
        if (_handTween != null)
        {
            _handTween.Kill();
            _handTween = null;
        }

        if (hand == null) return;

        Vector3 position = hand.localPosition;
        position.y = _handBaseY;
        hand.localPosition = position;
    }

    private void HandleClick()
    {
        if (IsLocked) return;

        if (Quantity <= 0)
        {
            OnClickBuy?.Invoke(this);
            return;
        }

        OnClickUse?.Invoke(this);
    }
}
