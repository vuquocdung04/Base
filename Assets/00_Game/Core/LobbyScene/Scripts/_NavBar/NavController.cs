using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class NavController : MonoBehaviour
{
    public static NavController Instance { get; private set; }

    [SerializeField] private RectTransform rectsParent;

    [SerializeField] private List<NavButton> navButtons = new List<NavButton>();
    [SerializeField] private List<RectTransform> rects = new List<RectTransform>();
    [SerializeField] private List<RectTransform> rectItems = new List<RectTransform>();

    [Header("Width")]
    [SerializeField] private bool useSelectedLarge = true;
    [SerializeField, Range(0f, 1f)] private float selectedPercent = 0.25f;

    [Header("Anim")]
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;
    [SerializeField] private float scaleSelected = 1.3f;
    [SerializeField] private float raiseY = 100f;
    [SerializeField] private Ease easeSelected = Ease.OutBack;
    [SerializeField] private Ease easeUnselected = Ease.OutQuad;

    [Header("Selector")]
    [SerializeField] private RectTransform selector;

    [SerializeField] private int defaultIndex = 2;

    private int currentIndex = -1;
    private bool hasInit;

    public IReadOnlyList<NavButton> NavButtons => navButtons;
    public int CurrentIndex => currentIndex;
    public NavButton CurrentNav => GetNav(currentIndex);

    public event Action<int, int> OnSelectedChanged;

    private void OnEnable()
    {
        for (int i = 0; i < navButtons.Count; i++)
        {
            if (navButtons[i] != null)
                navButtons[i].OnClick += OnClickNav;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < navButtons.Count; i++)
        {
            if (navButtons[i] != null)
                navButtons[i].OnClick -= OnClickNav;
        }
    }

    private void OnDestroy()
    {
        KillTween();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!hasInit || currentIndex < 0)
            return;

        Layout(true);
    }

    public void Init()
    {
        if (hasInit)
            return;

        hasInit = true;
        Instance = this;

        ApplyConfig();
        InitAfterLayoutAsync().Forget();
    }

    public void ApplyConfig()
    {
        for (int i = 0; i < navButtons.Count; i++)
        {
            if (navButtons[i] == null)
                continue;

            navButtons[i].Init();
            navButtons[i].SetConfig(scaleSelected, raiseY, duration, easeSelected, easeUnselected);
        }
    }

    public void NavigateTo(int index) => Select(index);

    public void Select(int index, bool instant = false)
    {
        if (navButtons.Count == 0)
            return;

        index = Mathf.Clamp(index, 0, navButtons.Count - 1);

        if (index == currentIndex)
            return;

        int previous = currentIndex;
        currentIndex = index;

        for (int i = 0; i < navButtons.Count; i++)
        {
            if (navButtons[i] == null)
                continue;

            navButtons[i].SetSelected(i == currentIndex, instant);
        }

        Layout(instant);

        if (previous >= 0)
            OnSelectedChanged?.Invoke(previous, currentIndex);
    }

    public void Select(NavButton navButton, bool instant = false)
    {
        int index = navButtons.IndexOf(navButton);

        if (index < 0)
            return;

        Select(index, instant);
    }

    public NavButton GetNav(int index)
    {
        if (index < 0 || index >= navButtons.Count)
            return null;

        return navButtons[index];
    }

    public void Layout(bool instant = true)
    {
        int count = navButtons.Count;

        if (count == 0)
            return;

        float total = GetTotalWidth();

        if (total <= 0f)
            return;

        float otherWidth = total / count;
        float selectedWidth = otherWidth;
        bool hasSelectedLarge = useSelectedLarge && count > 1 && currentIndex >= 0 && currentIndex < count;

        if (hasSelectedLarge)
        {
            selectedWidth = total * selectedPercent;
            otherWidth = total * (1f - selectedPercent) / (count - 1);
        }

        float positionX = -total * 0.5f;
        float selectedWorldX = 0f;

        for (int i = 0; i < count; i++)
        {
            float width = hasSelectedLarge && i == currentIndex ? selectedWidth : otherWidth;
            float worldX = AnchoredToWorldX(positionX + width * 0.5f);

            SetSlot(GetRect(rects, i), width, worldX, instant);
            SetSlot(GetRect(rectItems, i), width, worldX, instant);

            if (i == currentIndex)
                selectedWorldX = worldX;

            positionX += width;
        }

        if (selector != null)
            ApplyRect(selector, selectedWidth, selectedWorldX, instant);
    }

    [PropertyOrder(-1)]
    [Button("Auto", ButtonSizes.Medium)]
    public void Auto()
    {
        navButtons.Clear();
        rects.Clear();
        rectItems.Clear();

        navButtons.AddRange(GetComponentsInChildren<NavButton>(true));

        for (int i = 0; i < navButtons.Count; i++)
        {
            if (navButtons[i] == null)
                continue;

            navButtons[i].AutoGetField();
            rectItems.Add(navButtons[i].RectMain);
        }

        if (rectsParent == null)
            return;

        for (int i = 0; i < rectsParent.childCount; i++)
        {
            RectTransform rect = rectsParent.GetChild(i) as RectTransform;

            if (rect == null || rect == selector || rect.GetComponent<NavButton>() != null)
                continue;

            rects.Add(rect);
        }
    }

    private async Awaitable InitAfterLayoutAsync()
    {
        await Awaitable.EndOfFrameAsync(destroyCancellationToken);
        Select(defaultIndex, true);
    }

    private void SetSlot(RectTransform rect, float width, float worldX, bool instant)
    {
        if (rect == null || rect == selector)
            return;

        ApplyRect(rect, width, worldX, instant);
    }

    private void ApplyRect(RectTransform rect, float width, float worldX, bool instant)
    {
        SetAnchorCenter(rect);

        rect.DOKill();

        if (instant || duration <= 0f)
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            Vector3 position = rect.position;
            position.x = worldX;
            rect.position = position;
            return;
        }

        Vector2 sizeDelta = rect.sizeDelta;
        sizeDelta.x = width;

        rect.DOSizeDelta(sizeDelta, duration).SetEase(moveEase).SetTarget(rect);
        rect.DOMoveX(worldX, duration).SetEase(moveEase).SetTarget(rect);
    }

    private void SetAnchorCenter(RectTransform rect)
    {
        Vector2 anchorMin = rect.anchorMin;
        Vector2 anchorMax = rect.anchorMax;
        Vector2 pivot = rect.pivot;

        anchorMin.x = 0.5f;
        anchorMax.x = 0.5f;
        pivot.x = 0.5f;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
    }

    private float AnchoredToWorldX(float anchoredX)
    {
        RectTransform parent = rectsParent != null ? rectsParent : transform as RectTransform;

        if (parent == null)
            return anchoredX;

        float localX = anchoredX + parent.rect.width * (0.5f - parent.pivot.x);

        return parent.TransformPoint(new Vector3(localX, 0f, 0f)).x;
    }

    private RectTransform GetRect(List<RectTransform> list, int index)
    {
        if (index < 0 || index >= list.Count)
            return null;

        return list[index];
    }

    private float GetTotalWidth()
    {
        if (rectsParent != null && rectsParent.rect.width > 0f)
            return rectsParent.rect.width;

        RectTransform rect = transform as RectTransform;

        if (rect != null && rect.rect.width > 0f)
            return rect.rect.width;

        return Screen.width;
    }

    private void KillTween()
    {
        if (selector != null)
            selector.DOKill();

        for (int i = 0; i < rects.Count; i++)
        {
            if (rects[i] != null)
                rects[i].DOKill();
        }

        for (int i = 0; i < rectItems.Count; i++)
        {
            if (rectItems[i] != null)
                rectItems[i].DOKill();
        }
    }

    private void OnClickNav(NavButton navButton) => Select(navButton);
}
