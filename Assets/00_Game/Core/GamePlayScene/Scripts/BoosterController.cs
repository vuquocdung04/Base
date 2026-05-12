using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;

public partial class BoosterController : StaffSingleton<BoosterController>
{
    public Transform boosterHolder;
    private List<BoosterItem> boosterItems;
    private BoosterItem _currentActiveBooster;

    public float targetSize = 150f;

    public override void Init()
    {
        boosterItems = new List<BoosterItem>(boosterHolder.GetComponentsInChildren<BoosterItem>());
        foreach (var item in boosterItems)
        {
            item.SetSize(targetSize);
            item.ChangeState(BoosterState.Available);
        }

        this.RegisterListener(EventID.BOOSTER_USE_REQUEST, OnBoosterUseRequest);
        this.RegisterListener(EventID.BOOSTER_CANCEL_REQUEST, OnBoosterCancelRequest);

        CheckTutorialHighlight();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        this.RemoveListener(EventID.BOOSTER_USE_REQUEST, OnBoosterUseRequest);
        this.RemoveListener(EventID.BOOSTER_CANCEL_REQUEST, OnBoosterCancelRequest);
    }

    public BoosterItem GetBoosterItemByIndex(int index)
    {
        if (boosterItems != null && index >= 0 && index < boosterItems.Count) return boosterItems[index];
        Debug.LogError($"[BoosterController] Lỗi tìm Booster! Index {index} không hợp lệ.");
        return null;
    }

    private void OnBoosterUseRequest(object param)
    {
        BoosterType type = (BoosterType)param;
        BoosterItem item = GetBoosterItem(type);

        if (item == null) return;
        
        CheckAndClearTutorialPhase1(type, item);

        if (_currentActiveBooster != null)
        {
            ToastManager.Instance.ShowToast("Another Booster is in use!");
            return;
        }

        _currentActiveBooster = item;
        _currentActiveBooster.ChangeState(BoosterState.InUse);

        HandleBoosterSpecificLogic(type);
    }

    private void OnBoosterCancelRequest(object param)
    {
        if (_currentActiveBooster != null)
        {
            HandleTutorialCancel(_currentActiveBooster.Type); 
        }
        
        StopCurrentBooster();
    }

    private void HandleBoosterSpecificLogic(BoosterType type)
    {
        switch (type)
        {
            case BoosterType.Booster0:
                StopCurrentBooster();
                break;

            case BoosterType.Booster1:
                SetupPhase2Tutorial(type);
                break;

            case BoosterType.Booster2:
                ReleaseAndStartCooldown(60);
                break;
        }
    }

    public void OnBoosterActionSuccess()
    {
        if (_currentActiveBooster != null)
        {
            CompletePhase2Tutorial(_currentActiveBooster.Type);
            StopCurrentBooster();
        }
    }

    public void ReleaseAndStartCooldown(float cooldownTime)
    {
        if (_currentActiveBooster != null)
        {
            _currentActiveBooster.StartCooldown(cooldownTime);
            _currentActiveBooster = null; 
        }
    }

    private void StopCurrentBooster()
    {
        if (_currentActiveBooster != null)
        {
            _currentActiveBooster.ChangeState(BoosterState.Available);
            _currentActiveBooster = null;
        }
    }

    private BoosterItem GetBoosterItem(BoosterType type)
    {
        for (int i = 0; i < boosterItems.Count; i++)
        {
            if (boosterItems[i].Type == type) return boosterItems[i];
        }
        return null;
    }
}