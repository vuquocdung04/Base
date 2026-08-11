using System;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(GraphicRaycaster))]
public abstract class BaseBox : MonoBehaviour
{
    [SerializeField] protected RectTransform mainPanel;
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected List<Button> closeButtons = new();

    private PopupSetting _setting;
    private CancellationTokenSource _animCts;
    private Tween _peekTween;
    private float _peekFactor = 1f;
    private bool _postedOpen;

    private PopupSetting Setting => _setting ??= PopupManager.SettingFor(GetType().Name);

    internal float BackdropAlpha => Setting.backdropAlpha * _peekFactor;

    protected abstract void Init();
    protected abstract void InitState();

    internal void Bootstrap()
    {
        canvasGroup.SetCanvasState(false, 0f);

        for (int i = 0; i < closeButtons.Count; i++)
        {
            if (closeButtons[i] != null) closeButtons[i].OnClicked(Close);
        }

        Init();
    }

    protected virtual void OnDestroy()
    {
        PopupManager.Unregister(this);
        CancelAnim();
        KillPeekTween();
    }

    // ========== SHOW / CLOSE ==========
    public void Show(bool cover = false)
    {
        ShowInternal(null, false);
        PopupManager.Push(this, cover);
    }

    public void ShowDetached() => ShowInternal(null, true);

    public void ShowDetached(SlideSide from) => ShowInternal(from, true);

    public void Close() => CloseInternal(null, true);

    public void CloseDetached(SlideSide to) => CloseInternal(to, false);

    private void ShowInternal(SlideSide? side, bool detached)
    {
        InitState();
        CancelPeek();

        PopupManager.Attach(transform, detached);
        transform.SetAsLastSibling();

        if (!_postedOpen)
        {
            _postedOpen = true;
            PostPopupEvent(EventID.POPUP_OPENED);
        }

        RunShow(side).Forget();
    }

    private void CloseInternal(SlideSide? side, bool notifyStack)
    {
        if (_postedOpen)
        {
            _postedOpen = false;
            PostPopupEvent(EventID.POPUP_CLOSED);
        }

        CancelPeek();
        RunClose(side, notifyStack).Forget();
    }

    private async Awaitable RunShow(SlideSide? side)
    {
        await PlayShow(side, RestartAnim());
    }

    private async Awaitable RunClose(SlideSide? side, bool notifyStack)
    {
        CancellationToken token = RestartAnim();

        try { await PlayHide(side, token); }
        catch (OperationCanceledException) { return; }

        if (token.IsCancellationRequested) return;
        FinishClose(notifyStack);
    }

    private Awaitable PlayShow(SlideSide? side, CancellationToken token)
    {
        float duration = Setting.animDuration;

        if (side.HasValue)
            return PopupAnims.SlideShow(mainPanel, canvasGroup, side.Value, duration, token);

        if (this is IPopupSlide)
            return PopupAnims.SlideShow(mainPanel, canvasGroup, SlideSide.Right, duration, token);

        if (this is IPopupScale)
            return PopupAnims.ScaleShow(mainPanel, canvasGroup, duration, token);

        return PopupAnims.Instant(mainPanel, canvasGroup, true);
    }

    private Awaitable PlayHide(SlideSide? side, CancellationToken token)
    {
        float duration = Setting.animDuration;

        if (side.HasValue)
            return PopupAnims.SlideHide(mainPanel, canvasGroup, side.Value, duration, token);

        if (this is IPopupSlide)
            return PopupAnims.SlideHide(mainPanel, canvasGroup, SlideSide.Right, duration, token);

        if (this is IPopupScale)
            return PopupAnims.ScaleHide(mainPanel, canvasGroup, duration, token);

        return PopupAnims.Instant(mainPanel, canvasGroup, false);
    }

    private void FinishClose(bool notifyStack)
    {
        canvasGroup.SetCanvasState(false, 0f);

        if (notifyStack)
        {
            BaseBox toResume = PopupManager.Pop(this);
            if (toResume != null) toResume.Resume();
            PopupManager.RefreshBackdrop(PopupManager.IsEmpty);
        }
    }

    // ========== STACK ==========
    internal void Suspend()
    {
        CancelAnim();
        CancelPeek();
        canvasGroup.SetCanvasState(false, 0f);
    }

    internal void Resume()
    {
        transform.SetAsLastSibling();
        RunShow(null).Forget();
    }

    // ========== BACKDROP INPUT ==========
    internal void OnBackdropPress()
    {
        if (Setting.peekOnBackdropHold) SetPeek(true);
    }

    internal void OnBackdropRelease()
    {
        if (Setting.peekOnBackdropHold) SetPeek(false);
    }

    internal void OnBackdropClick()
    {
        if (Setting.peekOnBackdropHold || !Setting.closeOnBackdropClick) return;
        Close();
    }

    // ========== PEEK ==========
    private void SetPeek(bool on)
    {
        KillPeekTween();
        _peekFactor = on ? 0f : 1f;
        _peekTween = canvasGroup.DOFade(_peekFactor, Setting.peekFadeDuration).SetUpdate(true);

        PopupManager.RefreshBackdrop(true);
    }

    private void CancelPeek()
    {
        KillPeekTween();
        if (_peekFactor >= 1f) return;

        _peekFactor = 1f;
        canvasGroup.alpha = 1f;
    }

    private void KillPeekTween()
    {
        _peekTween?.Kill();
        _peekTween = null;
    }

    // ========== HELPERS ==========
    private CancellationToken RestartAnim()
    {
        CancelAnim();
        _animCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        return _animCts.Token;
    }

    private void CancelAnim()
    {
        if (_animCts == null) return;

        _animCts.Cancel();
        _animCts.Dispose();
        _animCts = null;
    }

    private void PostPopupEvent(EventID id)
    {
        if (!EventDispatcher.EventDispatcher.HasInstance()) return;
        if (!EventDispatcher.EventDispatcher.Instance.HasListener(id)) return;

        this.PostEvent(id);
    }

}
