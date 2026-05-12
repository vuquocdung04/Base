using System;
using EventDispatcher;
using TMPro;
using UnityEngine;

public class HeartView : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private TextMeshProUGUI heartCountText; 
    [SerializeField] private TextMeshProUGUI timerText;     
    
    [Header("Optional Icons")]
    [SerializeField] private GameObject normalHeartIcon;   
    [SerializeField] private GameObject unlimitedHeartIcon;

    private int lastTimerSeconds = -1;

    private void OnEnable()
    {
        this.RegisterListener(EventID.CHANGE_HEART,OnHeartChanged );
        UpdateHeartStateVisuals();
    }

    private void OnDisable()
    {
        this.RemoveListener(EventID.CHANGE_HEART,OnHeartChanged);
    }

    private void OnHeartChanged(object param)
    {
        UpdateHeartStateVisuals();
    }
    private void UpdateHeartStateVisuals()
    {
        if (LivesManager.Instance == null) return;

        bool isUnlimited = UseProfile.IsUnlimitedHeart;
        
        if (normalHeartIcon != null) normalHeartIcon.SetActive(!isUnlimited);
        if (unlimitedHeartIcon != null) unlimitedHeartIcon.SetActive(isUnlimited);
        
        if (heartCountText != null) heartCountText.gameObject.SetActive(!isUnlimited);

        if (!isUnlimited)
        {
            int currentHearts = UseProfile.Heart;
            int maxHearts = LivesManager.Instance.GetMaxHearts();
            heartCountText.text = $"{currentHearts}";

            if (currentHearts >= maxHearts)
            {
                timerText.text = "FULL";
                lastTimerSeconds = -2;
            }
        }
    }
    private void Update()
    {
        bool isUnlimited = UseProfile.IsUnlimitedHeart;

        if (isUnlimited)
        {
            TimeSpan timeRemain = LivesManager.Instance.GetUnlimitedTimeRemaining();
            UpdateUnlimitedTimerText(timeRemain);
        }
        else if (UseProfile.Heart < LivesManager.Instance.GetMaxHearts())
        {
            double timeToNext = LivesManager.Instance.GetTimeToNextHeart();
            UpdateNormalTimerText((int)timeToNext);
        }
    }
    private void UpdateUnlimitedTimerText(TimeSpan time)
    {
        int currentTotalSeconds = (int)time.TotalSeconds;

        if (currentTotalSeconds != lastTimerSeconds)
        {
            lastTimerSeconds = currentTotalSeconds;

            if (time.TotalSeconds <= 0)
            {
                timerText.text = "00:00:00";
                return;
            }

            timerText.text = time.TotalDays >= 1 ? $"{(int)time.TotalDays}d {time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}" : $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
        }
    }
    private void UpdateNormalTimerText(int totalSeconds)
    {
        if (totalSeconds != lastTimerSeconds)
        {
            lastTimerSeconds = totalSeconds;
            
            int mins = totalSeconds / 60;
            int secs = totalSeconds % 60;
            timerText.text = $"{mins:D2}:{secs:D2}";
        }
    }
}