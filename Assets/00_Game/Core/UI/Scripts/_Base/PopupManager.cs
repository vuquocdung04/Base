using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PopupManager : Singleton<PopupManager>
{
    [Header("Layers")]
    [SerializeField] private Transform popupRoot;
    [SerializeField] private Transform screenRoot;

    [Header("Backdrop")]
    [SerializeField] private PopupBackdrop backdrop;

    [Header("Settings")]
    [SerializeField] private PopupCatalog catalog;

    private class StackEntry
    {
        public BaseBox Box;
        public BaseBox Covered;
    }

    private readonly Dictionary<Type, BaseBox> _instances = new();
    private readonly Dictionary<Type, AsyncOperationHandle<GameObject>> _handles = new();
    private readonly HashSet<Type> _loading = new();
    private readonly List<StackEntry> _stack = new();

    private Transform PopupRoot => popupRoot != null ? popupRoot : transform;
    private Transform ScreenRoot => screenRoot != null ? screenRoot : PopupRoot;

    protected override void OnDestroy()
    {
        ReleaseAll();
        base.OnDestroy();
    }

    internal static PopupSetting SettingFor(string typeName)
    {
        PopupManager m = Instance;
        PopupCatalog cat = m != null ? m.ResolveCatalog() : null;

        return cat != null ? cat.Find(typeName) ?? PopupSetting.Fallback : PopupSetting.Fallback;
    }

    private PopupCatalog ResolveCatalog()
    {
        if (catalog == null) catalog = Resources.Load<PopupCatalog>("PopupCatalog");
        return catalog;
    }

    // ========== LOAD ==========
    public static T Peek<T>() where T : BaseBox
    {
        PopupManager m = Instance;
        if (m == null) return null;

        return m._instances.TryGetValue(typeof(T), out BaseBox box) ? box as T : null;
    }

    public static async Awaitable<T> GetAsync<T>() where T : BaseBox
    {
        PopupManager m = Instance;
        if (m == null)
        {
            Debug.LogError($"[PopupManager] Scene chua co PopupManager, khong mo duoc {typeof(T).Name}.");
            return null;
        }

        return await m.LoadAsync<T>();
    }

    public static async Awaitable Preload<T>() where T : BaseBox
    {
        await GetAsync<T>();
    }

    private async Awaitable<T> LoadAsync<T>() where T : BaseBox
    {
        Type type = typeof(T);

        if (_instances.TryGetValue(type, out BaseBox cached)) return cached as T;

        if (_loading.Contains(type))
        {
            await AwaitableEx.WaitUntil(() => !_loading.Contains(type), destroyCancellationToken);
            return Peek<T>();
        }

        _loading.Add(type);

        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(type.Name, PopupRoot);
        GameObject obj = await handle.Task;

        if (this == null) return null;

        _loading.Remove(type);

        if (obj == null)
        {
            Debug.LogError($"[PopupManager] Khong tim thay Addressable key: {type.Name}");
            return null;
        }

        T box = obj.GetComponent<T>();
        if (box == null)
        {
            Debug.LogError($"[PopupManager] Prefab '{type.Name}' thieu component {type.Name}.");
            Addressables.ReleaseInstance(obj);
            return null;
        }

        _instances[type] = box;
        _handles[type] = handle;
        box.Bootstrap();
        return box;
    }

    // ========== SHOW ==========
    public static async Awaitable Show<T>(Action<T> setup = null, bool cover = false) where T : BaseBox
    {
        T box = await GetAsync<T>();
        if (box == null) return;

        setup?.Invoke(box);
        box.Show(cover);
    }

    public static async Awaitable ShowDetached<T>(Action<T> setup = null) where T : BaseBox
    {
        T box = await GetAsync<T>();
        if (box == null) return;

        setup?.Invoke(box);
        box.ShowDetached();
    }

    // ========== STACK ==========
    internal static bool IsEmpty => Instance == null || Instance._stack.Count == 0;

    internal static BaseBox Top
    {
        get
        {
            PopupManager m = Instance;
            if (m == null) return null;

            for (int i = m._stack.Count - 1; i >= 0; i--)
            {
                if (m._stack[i].Box != null) return m._stack[i].Box;
            }
            return null;
        }
    }

    internal static void Attach(Transform target, bool detached)
    {
        PopupManager m = Instance;
        if (m == null) return;

        Transform root = detached ? m.ScreenRoot : m.PopupRoot;
        if (target.parent != root) target.SetParent(root, false);
    }

    internal static void Push(BaseBox box, bool cover)
    {
        PopupManager m = Instance;
        if (m == null) return;

        m.RemoveEntry(box);

        bool wasEmpty = m._stack.Count == 0;
        BaseBox covered = null;
        if (cover && m._stack.Count > 0)
        {
            covered = m._stack[m._stack.Count - 1].Box;
            if (covered != null) covered.Suspend();
        }

        m._stack.Add(new StackEntry { Box = box, Covered = covered });
        RefreshBackdrop(wasEmpty);
    }

    internal static BaseBox Pop(BaseBox box)
    {
        PopupManager m = Instance;
        if (m == null) return null;

        int index = m.FindLast(box);
        if (index < 0) return null;

        BaseBox covered = m._stack[index].Covered;
        m._stack.RemoveAt(index);

        return covered != null ? covered : null;
    }

    internal static void RefreshBackdrop(bool fade)
    {
        PopupManager m = Instance;
        if (m == null || m.backdrop == null) return;

        BaseBox top = Top;
        if (top == null)
        {
            m.backdrop.HideBackdrop(fade);
            return;
        }

        m.backdrop.ShowBackdrop(top.BackdropAlpha, fade);
    }

    internal static void Unregister(BaseBox box)
    {
        PopupManager m = Instance;
        if (m == null) return;

        m.RemoveEntry(box);
        foreach (StackEntry entry in m._stack)
        {
            if (ReferenceEquals(entry.Covered, box)) entry.Covered = null;
        }
        RefreshBackdrop(false);

        Type type = box.GetType();
        if (m._instances.TryGetValue(type, out BaseBox known) && ReferenceEquals(known, box))
        {
            m._instances.Remove(type);
            m.ReleaseHandle(type);
        }
    }

    // ========== INTERNAL ==========
    private void RemoveEntry(BaseBox box)
    {
        int index = FindLast(box);
        if (index >= 0) _stack.RemoveAt(index);
    }

    private int FindLast(BaseBox box)
    {
        for (int i = _stack.Count - 1; i >= 0; i--)
        {
            if (ReferenceEquals(_stack[i].Box, box)) return i;
        }
        return -1;
    }

    private void ReleaseHandle(Type type)
    {
        if (!_handles.TryGetValue(type, out AsyncOperationHandle<GameObject> handle)) return;

        _handles.Remove(type);
        if (handle.IsValid()) Addressables.ReleaseInstance(handle);
    }

    private void ReleaseAll()
    {
        foreach (var pair in _handles)
        {
            if (pair.Value.IsValid()) Addressables.ReleaseInstance(pair.Value);
        }

        _handles.Clear();
        _instances.Clear();
        _loading.Clear();
        _stack.Clear();
    }
}
