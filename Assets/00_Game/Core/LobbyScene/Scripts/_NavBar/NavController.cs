using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class NavController : MonoBehaviour
{
    public static NavController Instance { get; private set; }

    [SerializeField] private List<NavButton> navButtons = new List<NavButton>();
    [SerializeField] private RectTransform selector;

    [Header("Anim")]
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;
    [SerializeField] private float scaleSelected = 1.3f;
    [SerializeField] private float raiseY = 100f;
    [SerializeField] private Ease easeSelected = Ease.OutBack;
    [SerializeField] private Ease easeUnselected = Ease.OutQuad;

    private int currentIndex = -1;
    private bool hasInit;

    public IReadOnlyList<NavButton> NavButtons => navButtons;
    public int CurrentIndex => currentIndex;
    public NavButton CurrentNav => GetNav(currentIndex);
    public int DefaultIndex => navButtons.Count / 2;

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
        if (selector != null)
            selector.DOKill();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!hasInit || currentIndex < 0)
            return;

        UpdateSelector(true);
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

        UpdateSelector(instant);

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

    [PropertyOrder(-1)]
    [Button("Auto", ButtonSizes.Medium)]
    public void Auto()
    {
        navButtons.Clear();
        navButtons.AddRange(GetComponentsInChildren<NavButton>(true));

        for (int i = 0; i < navButtons.Count; i++)
        {
            if (navButtons[i] != null)
                navButtons[i].AutoGetField();
        }
    }

    private async Awaitable InitAfterLayoutAsync()
    {
        await Awaitable.EndOfFrameAsync(destroyCancellationToken);
        Select(DefaultIndex, true);
    }

    private void UpdateSelector(bool instant)
    {
        NavButton navButton = CurrentNav;

        if (selector == null || navButton == null)
            return;

        RectTransform rect = navButton.RectMain;

        if (rect == null)
            return;

        float width = rect.rect.width;
        float worldX = rect.TransformPoint(rect.rect.center).x;

        selector.DOKill();

        if (instant || duration <= 0f)
        {
            selector.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            Vector3 position = selector.position;
            position.x = worldX;
            selector.position = position;
            return;
        }

        Vector2 sizeDelta = selector.sizeDelta;
        sizeDelta.x = width - (selector.anchorMax.x - selector.anchorMin.x) * GetParentWidth();

        selector.DOSizeDelta(sizeDelta, duration).SetEase(moveEase).SetTarget(selector);
        selector.DOMoveX(worldX, duration).SetEase(moveEase).SetTarget(selector);
    }

    private float GetParentWidth()
    {
        RectTransform parent = selector.parent as RectTransform;

        return parent != null ? parent.rect.width : 0f;
    }

    private void OnClickNav(NavButton navButton) => Select(navButton);
}
