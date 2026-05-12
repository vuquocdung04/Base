using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public partial class DevTool : OdinEditorWindow
{
    [MenuItem("Tools/Mở DevTool")]
    public static void OpenWindow()
    {
        GetWindow<DevTool>("My Dev Tool").Show();
    }
    [Button("🚀 Bật Fast Play Mode", ButtonHeight = 35)]
    [FoldoutGroup("⚙️ Editor")]
    [GUIColor(0.2f, 1f, 0.2f)]
    private void EnableFastPlayMode()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
        Debug.Log("<b>[DevTool]</b> Đã BẬT Fast Play Mode!");
    }

    [Button("🐢 Tắt Fast Play Mode", ButtonHeight = 35)]
    [FoldoutGroup("⚙️ Editor")]
    [GUIColor(1f, 1f, 0.2f)]
    private void DisableFastPlayMode()
    {
        EditorSettings.enterPlayModeOptionsEnabled = false;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
        Debug.Log("<b>[DevTool]</b> Đã TẮT Fast Play Mode!");
    }
}
