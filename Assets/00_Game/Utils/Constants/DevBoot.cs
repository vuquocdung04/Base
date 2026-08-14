public static class DevBoot
{
    public const string SESSION_TARGET_SCENE = "DevTools.Boot.TargetScene";

    public static string ResolveStartScene(string fallback)
    {
#if UNITY_EDITOR
        string target = UnityEditor.SessionState.GetString(SESSION_TARGET_SCENE, string.Empty);

        if (!string.IsNullOrEmpty(target) && target != SceneName.LOADING_SCENE) return target;
#endif
        return fallback;
    }
}
