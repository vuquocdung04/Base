using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace DevTools.QuickTools
{
	[System.Serializable]
	public class BootFromLoadingSection
	{
		const string EnabledKey = "DevTools.Boot.FromLoading";
		const string LoadingScenePath = "Assets/00_Game/Scenes/LoadingScene.unity";

		[Title("Boot từ LoadingScene", bold: true)]
		[ShowInInspector, DisplayAsString, HideLabel, PropertyOrder(0)]
		string Status => IsEnabled
			? "Status: ON (Play ở scene nào cũng chạy LoadingScene rồi quay lại scene đó)"
			: "Status: OFF (Play chạy thẳng scene đang mở)";

		[ShowIf(nameof(HasNoLoadingScene)), PropertyOrder(1)]
		[InfoBox("Không tìm thấy " + LoadingScenePath + " — bật lên cũng không có tác dụng.", InfoMessageType.Error)]
		[ShowInInspector, DisplayAsString, HideLabel]
		string MissingScene => string.Empty;

		[HorizontalGroup("Boot", Width = 160), PropertyOrder(2)]
		[Button("Enable", ButtonHeight = 30)]
		[GUIColor(0.4f, 0.9f, 0.5f)]
		void Enable() => Apply(true);

		[HorizontalGroup("Boot", Width = 160), PropertyOrder(3)]
		[Button("Disable", ButtonHeight = 30)]
		[GUIColor(0.85f, 0.85f, 0.85f)]
		void Disable() => Apply(false);

		static bool IsEnabled => EditorPrefs.GetBool(EnabledKey, false);

		static bool HasNoLoadingScene => AssetDatabase.LoadAssetAtPath<SceneAsset>(LoadingScenePath) == null;

		static void Apply(bool enabled)
		{
			EditorPrefs.SetBool(EnabledKey, enabled);
			Sync(enabled);
		}

		static void Sync(bool enabled)
		{
			EditorSceneManager.playModeStartScene = enabled
				? AssetDatabase.LoadAssetAtPath<SceneAsset>(LoadingScenePath)
				: null;
		}

		[InitializeOnLoadMethod]
		static void Hook()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeChanged;
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
			EditorApplication.delayCall += () => Sync(IsEnabled);
		}

		static void OnPlayModeChanged(PlayModeStateChange state)
		{
			if (state != PlayModeStateChange.ExitingEditMode) return;

			SessionState.SetString(
				DevBoot.SESSION_TARGET_SCENE,
				IsEnabled ? SceneManager.GetActiveScene().name : string.Empty);
		}
	}
}
