using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class ShopPackBase : MonoBehaviour
{
    [ValueDropdown(nameof(PackIds))]
    public string packId;

    [Header("Refs")]
    public Image iconPack;
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtPrice;
    public Button btnPurchase;

    [Tooltip("Object bị ẩn/hiện. Để trống = chính GameObject này.")]
    public GameObject visibilityTarget;

    protected PackConfig Config { get; private set; }

    private bool _buying;

    private GameObject Target => visibilityTarget != null ? visibilityTarget : gameObject;

    protected abstract void FillRewards();

    public void Init()
    {
        if (btnPurchase != null) btnPurchase.OnClicked(Purchase);
    }

    public void Refresh()
    {
        PackManager manager = PackManager.Instance;
        Config = manager != null ? manager.Find(packId) : null;

        bool show = Config != null && manager.CanShow(Config);
        if (Target.activeSelf != show) Target.SetActive(show);

        if (!show) return;

        if (iconPack != null)
        {
            iconPack.sprite = Config.icon;
            iconPack.preserveAspect = true;
            iconPack.enabled = Config.icon != null;
        }

        if (txtName != null) txtName.text = Config.displayName;
        if (txtPrice != null) txtPrice.text = manager.GetPriceText(Config);

        FillRewards();
    }

    private void Purchase()
    {
        if (_buying || Config == null || PackManager.Instance == null) return;

        BuyAsync().Forget();
    }

    private async Awaitable BuyAsync()
    {
        _buying = true;
        SetInteractable(false);

        await PackManager.Instance.TryPurchase(Config);

        if (this == null) return;

        _buying = false;
        SetInteractable(true);
        Refresh();
    }

    private void SetInteractable(bool value)
    {
        if (btnPurchase != null) btnPurchase.interactable = value;
    }

    private static PackCatalog _catalogCache;
    private static string[] _idsCache = System.Array.Empty<string>();

    private static IEnumerable<string> PackIds()
    {
        PackCatalog catalog = PackManager.Instance != null ? PackManager.Instance.Catalog : _catalogCache;

        if (catalog == null)
        {
            catalog = Resources.Load<PackCatalog>(PackCatalog.RESOURCE_PATH);
            if (catalog == null) return _idsCache;
        }

        if (!ReferenceEquals(catalog, _catalogCache) || IsCacheStale(catalog)) RebuildCache(catalog);

        return _idsCache;
    }

    private static bool IsCacheStale(PackCatalog catalog)
    {
        int index = 0;

        for (int i = 0; i < catalog.packs.Count; i++)
        {
            PackConfig pack = catalog.packs[i];
            if (pack == null || string.IsNullOrEmpty(pack.packId)) continue;

            if (index >= _idsCache.Length || _idsCache[index] != pack.packId) return true;
            index++;
        }

        return index != _idsCache.Length;
    }

    private static void RebuildCache(PackCatalog catalog)
    {
        var ids = new List<string>(catalog.packs.Count);

        for (int i = 0; i < catalog.packs.Count; i++)
        {
            PackConfig pack = catalog.packs[i];
            if (pack != null && !string.IsNullOrEmpty(pack.packId)) ids.Add(pack.packId);
        }

        _catalogCache = catalog;
        _idsCache = ids.ToArray();
    }
}
