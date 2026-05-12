using UnityEngine;

// --- INTERFACE CHUNG ---
public interface IStaff { void BindInstance(); }
public interface ILeader { void ForceBindAllStaffs(); }


public abstract class ManagerSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }
    protected virtual void Awake()
    {
        if (Instance == null) { Instance = this as T; DontDestroyOnLoad(gameObject); }
        else if (this != Instance) Destroy(gameObject);
        OnAwake();
    }
    protected virtual void OnAwake() { }
}

public abstract class LeaderSingleton<T> : MonoBehaviour, ILeader where T : MonoBehaviour
{
    public static T Instance { get; private set; }
    protected virtual void Awake()
    {
        if (Instance == null) Instance = this as T;
        else if (this != Instance) { Destroy(gameObject); return; }

        ForceBindAllStaffs();
        OnAwake();
    }

    public void ForceBindAllStaffs()
    {
        var staffs = GetComponentsInChildren<IStaff>(true);
        foreach (var s in staffs) s.BindInstance();
    }
    protected virtual void OnAwake() { }
}

public abstract class StaffSingleton<T> : MonoBehaviour, IStaff where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    public void BindInstance()
    {
        if (Instance == null) Instance = this as T;
    }

    protected virtual void OnDestroy() { if (Instance == this) Instance = null; }
    public abstract void Init();
}