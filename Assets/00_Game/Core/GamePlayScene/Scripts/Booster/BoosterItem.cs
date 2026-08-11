using System;
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

    public Sprite IconSprite => iconBooster.sprite;
    public int Quantity { get; private set; }
    public bool IsLocked { get; private set; }

    public event Action<BoosterItem> OnClickBuy;
    public event Action<BoosterItem> OnClickUse;

    private void Start() => btnMain.OnClicked(HandleClick);

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
