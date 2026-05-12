using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeartBox : MonoBehaviour
{
    [Header("Heart UI")]
    [SerializeField] private TextMeshProUGUI txtHeart;
    [SerializeField] private TextMeshProUGUI txtTimeHeart;
    [SerializeField] private Image imgHeart;
    [SerializeField] private Sprite sprOriginal;
    [SerializeField] private Sprite sprInfinite;
    [SerializeField] private string fullText = "Full";
    [SerializeField] private string infiniteText = "Infinite";
    
    private LivesManager livesManager;
    private string lastDisplayedStatus = "";
    private int lastDisplayedAmount = -1;
    private bool lastIsUnlimited;
    
    private CancellationTokenSource cts;
    
    public void Init()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        livesManager = GameManager.Instance.livesManager;
        
        RefreshHeartUI();
        HeartUIUpdateLoop(cts.Token).Forget();
        
        this.RegisterListener(EventID.CHANGE_HEART, OnHeartAmountChanged);
    }
    private void OnHeartAmountChanged(object obj = null)
    {
        txtHeart.text = UseProfile.Heart.ToString();
        lastDisplayedAmount = -1;
    }

    private async UniTaskVoid HeartUIUpdateLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: true, cancellationToken: token);
            
            if(token.IsCancellationRequested) break;
            
            RefreshHeartUI();
        }
    }
    
    private void RefreshHeartUI()
    {
        bool isUnlimited = UseProfile.IsUnlimitedHeart;
        int currentHeart = UseProfile.Heart;

        if (isUnlimited != lastIsUnlimited || currentHeart != lastDisplayedAmount)
        {
            if (isUnlimited)
            {
                imgHeart.sprite = sprInfinite;
                txtHeart.gameObject.SetActive(false);
            }
            else
            {
                imgHeart.sprite = sprOriginal;
                txtHeart.gameObject.SetActive(true);
                txtHeart.text = currentHeart.ToString();
            }
            
            lastIsUnlimited = isUnlimited;
            lastDisplayedAmount = currentHeart;
        }

        string newStatusText;
        if (isUnlimited)
        {
            newStatusText = infiniteText;
        }
        else if (currentHeart >= livesManager.maxHearts)
        {
            newStatusText = fullText;
        }
        else
        {
            double refillTime = livesManager.GetTimeToNextHeart();
            TimeSpan ts = TimeSpan.FromSeconds(refillTime);
            newStatusText = (refillTime > 0)
                ? $"{ts.Minutes:D2}:{ts.Seconds:D2}"
                : "00:00";
        }

        if (newStatusText != lastDisplayedStatus)
        {
            txtTimeHeart.text = newStatusText;
            lastDisplayedStatus = newStatusText;
        }

        if (!txtTimeHeart.gameObject.activeSelf)
        {
            txtTimeHeart.gameObject.SetActive(true);
        }
    }
    

    private void OnDestroy()
    {
        this.RemoveListener(EventID.CHANGE_HEART, OnHeartAmountChanged);
        
        cts?.Cancel();
        cts?.Dispose();
    }
}