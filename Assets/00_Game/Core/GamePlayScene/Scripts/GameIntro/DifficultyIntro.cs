using DG.Tweening;
using UnityEngine;

public class DifficultyIntro : IntroStep
{
    [Header("Refs")]
    [SerializeField] private GameObject hardObj;
    [SerializeField] private CanvasGroup content;

    [Header("Anim")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float holdDuration = 1f;

    [Header("Testing")]
    [SerializeField] private bool isHard;

    private Tween _fadeTween;

    public bool IsHard
    {
        get => isHard;
        set => isHard = value;
    }

    public override void Prepare() => SetHard(false);

    private void OnDestroy() => Kill();

    public override async Awaitable Play()
    {
        if (!isHard || hardObj == null) return;

        Kill();

        if (content != null) content.alpha = 0f;

        SetHard(true);

        if (content != null)
        {
            _fadeTween = content.DOFade(1f, Mathf.Max(0f, fadeDuration)).SetTarget(this).SetUpdate(true);

            await _fadeTween.ToAwaitable(destroyCancellationToken);

            _fadeTween = null;
        }

        await AwaitableEx.WaitRealtimeAsync(Mathf.Max(0f, holdDuration), destroyCancellationToken);

        SetHard(false);
    }

    public void Hide()
    {
        Kill();
        SetHard(false);
    }

    private void SetHard(bool value)
    {
        if (hardObj != null) hardObj.SetActive(value);
    }

    private void Kill()
    {
        if (_fadeTween == null) return;

        _fadeTween.Kill();
        _fadeTween = null;
    }
}
