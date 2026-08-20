using Sirenix.OdinInspector;
using UnityEngine;

public class ShopBox : BaseBox, IPopupSlide
{
    [SerializeField] private ShopPackBase[] packs;

    [PropertyOrder(-1)]
    [Button("Auto", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
    public void Auto()
    {
        packs = GetComponentsInChildren<ShopPackBase>(true);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        Debug.Log($"<color=cyan>[Shop]</color> Đã gom {packs.Length} ShopPack.", this);
    }

    protected override void Init()
    {
        CachePacks();

        for (int i = 0; i < packs.Length; i++)
        {
            if (packs[i] != null) packs[i].Init();
        }

        if (PackManager.Instance != null)
        {
            PackManager.Instance.OnPurchased += HandlePurchased;
            PackManager.Instance.OnPricesChanged += RefreshAll;
        }
    }

    protected override void InitState()
    {
        RefreshAll();
    }

    protected override void OnDestroy()
    {
        if (PackManager.Instance != null)
        {
            PackManager.Instance.OnPurchased -= HandlePurchased;
            PackManager.Instance.OnPricesChanged -= RefreshAll;
        }

        base.OnDestroy();
    }

    public void RefreshAll()
    {
        CachePacks();

        for (int i = 0; i < packs.Length; i++)
        {
            if (packs[i] != null) packs[i].Refresh();
        }
    }

    private void HandlePurchased(PackConfig pack) => RefreshAll();

    private void CachePacks()
    {
        if (packs != null && packs.Length > 0) return;

        packs = GetComponentsInChildren<ShopPackBase>(true);
    }

#if UNITY_EDITOR
    private void Reset() => Auto();
#endif
}
