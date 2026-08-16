using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RewardBox : BaseBox, IPopupScale
{
    [Header("Reward Items")]
    public RewardItemView rewardItemPrefab;
    public Transform rewardItemParent;

    [Header("Claim")]
    public Button btnClaim;

    [Header("Reveal")]
    [Min(0.01f)] public float revealDuration = 0.3f;
    [Min(0f)] public float revealStagger = 0.08f;

    private readonly List<RewardItemView> _items = new();
    private Sequence _reveal;

    protected override void Init()
    {
        if (btnClaim != null) btnClaim.OnClicked(Close);
    }

    protected override void InitState()
    {
    }

    protected override void OnDestroy()
    {
        KillReveal();
        base.OnDestroy();
    }

    public void Populate(IReadOnlyList<GameReward> rewards)
    {
        KillReveal();

        foreach (RewardItemView item in _items)
        {
            if (item == null) continue;

            item.gameObject.SetActive(false);
            Destroy(item.gameObject);
        }
        _items.Clear();

        SetClaimReady(false);

        if (rewards == null) return;

        for (int i = 0; i < rewards.Count; i++)
        {
            RewardItemView item = Instantiate(rewardItemPrefab, rewardItemParent);
            item.Setup(rewards[i]);
            _items.Add(item);
        }

        FitHolder();
        PlayReveal();
    }

    private void FitHolder()
    {
        if (rewardItemParent is not RectTransform holder) return;

        holder.localScale = Vector3.one;
        LayoutRebuilder.ForceRebuildLayoutImmediate(holder);

        float availableWidth = holder.rect.width;
        float availableHeight = holder.rect.height;

        float neededWidth = LayoutUtility.GetPreferredWidth(holder);
        float neededHeight = LayoutUtility.GetPreferredHeight(holder);

        float scaleX = neededWidth > availableWidth && neededWidth > 0f ? availableWidth / neededWidth : 1f;
        float scaleY = neededHeight > availableHeight && neededHeight > 0f ? availableHeight / neededHeight : 1f;

        float scale = Mathf.Min(scaleX, scaleY);
        holder.localScale = new Vector3(scale, scale, 1f);
    }

    private void PlayReveal()
    {
        if (_items.Count == 0)
        {
            SetClaimReady(true);
            return;
        }

        _reveal = DOTween.Sequence().SetUpdate(true);

        for (int i = 0; i < _items.Count; i++)
        {
            _reveal.Insert(i * revealStagger, _items[i].DoScale(revealDuration));
        }

        _reveal.OnComplete(() =>
        {
            _reveal = null;
            SetClaimReady(true);
        });
    }

    private void KillReveal()
    {
        if (_reveal == null) return;

        _reveal.Kill();
        _reveal = null;
    }

    private void SetClaimReady(bool ready)
    {
        if (btnClaim != null) btnClaim.interactable = ready;
    }
}
