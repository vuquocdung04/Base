using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public static partial class UIUtils
{
    public static async UniTaskVoid BindCountdownRealtime(
        this TextMeshProUGUI textUI,
        Func<double> getTimeRemaining,
        string textWhenZero = "Full",
        Func<bool> checkUnlimited = null,
        CancellationToken token = default)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                double timeRemaining = getTimeRemaining();

                if (timeRemaining <= 0)
                {
                    if (checkUnlimited != null && checkUnlimited.Invoke())
                    {
                        textUI.text = "Unlimited";
                    }
                    else
                    {
                        textUI.text = textWhenZero;
                    }
                }
                else
                {
                    TimeSpan t = TimeSpan.FromSeconds(timeRemaining);
                    textUI.text = string.Format("{0:D2}:{1:D2}", (int)t.TotalMinutes, t.Seconds);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: true, cancellationToken: token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public static async UniTask CountTo(this TextMeshProUGUI textElement, int targetValue, float duration = 0.5f)
    {
        int.TryParse(textElement.text, out int startValue);

        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            int currentValue = (int)Mathf.Lerp(startValue, targetValue, progress);
            textElement.text = currentValue.ToString();

            await UniTask.Yield();
        }

        textElement.text = targetValue.ToString();
    }


    public static void SetCanvasState(this CanvasGroup cg, bool isInteractable, float alpha = -1f)
    {
        if (cg == null) return;
        cg.interactable = isInteractable;
        cg.blocksRaycasts = isInteractable;
        if (alpha >= 0) cg.alpha = alpha;
    }

    // --- SLIDING ANIMATION ---
    public static Tween SlideTo(this RectTransform rect, Vector2 targetPos, float duration, Ease ease = Ease.OutCubic)
    {
        return rect.DOAnchorPos(targetPos, duration).SetEase(ease);
    }

    // --- SCALE ANIMATION ---
    public static Tween ScaleTo(this RectTransform rect, Vector3 targetScale, float duration, Ease ease = Ease.OutBack)
    {
        return rect.DOScale(targetScale, duration).SetEase(ease);
    }
}