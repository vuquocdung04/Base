using System;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace DevTools.Heart
{
	[System.Serializable]
	public class HeartPanel : IDevToolPanel
	{
		public string Title => "Heart";
		public int Order => 35;
		public SdfIconType Icon => SdfIconType.HeartFill;

		const string BypassKey = "DevTools.Heart.BypassHeartCost";
		const string ConfigDir = "Assets/00_Game/Configs/Heart/Resources";
		const float LabelWidth = 120f;
		const float ButtonHeight = 26f;

		HeartConfig _config;
		bool _located;
		float _unlimitedMinutes = 1f;

		static HeartManager Live
		{
			get
			{
				if (!Application.isPlaying) return null;

				HeartManager manager = HeartManager.Instance;
				return manager != null ? manager : null;
			}
		}

		[InitializeOnLoadMethod]
		static void Hook()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeChanged;
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
		}

		static void OnPlayModeChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredPlayMode)
			{
				HeartManager.BypassHeartCost = EditorPrefs.GetBool(BypassKey, false);
			}
		}

		[BoxGroup("Config"), PropertyOrder(0)]
		[ShowInInspector, HideLabel]
		[InlineEditor(InlineEditorObjectFieldModes.Boxed, Expanded = true)]
		HeartConfig Config
		{
			get
			{
				if (_config == null && !_located)
				{
					_located = true;
					_config = Locate();
				}

				return _config;
			}
			set
			{
				_config = value;
				_located = true;
			}
		}

		[BoxGroup("Config"), PropertyOrder(1)]
		[ShowIf(nameof(HasNoConfig))]
		[InfoBox("Chưa có HeartConfig — game đang chạy bằng giá trị mặc định (5 tim / 30 phút).", InfoMessageType.Warning)]
		[Button("Tạo HeartConfig", ButtonHeight = 28), GUIColor(0.4f, 0.9f, 0.5f)]
		void CreateConfig()
		{
			Directory.CreateDirectory(ConfigDir);

			HeartConfig created = ScriptableObject.CreateInstance<HeartConfig>();
			string path = AssetDatabase.GenerateUniqueAssetPath($"{ConfigDir}/{HeartConfig.RESOURCE_PATH}.asset");

			AssetDatabase.CreateAsset(created, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			_config = created;
			_located = true;
			Selection.activeObject = created;
		}

		bool HasNoConfig => Config == null;

		[OnInspectorGUI, PropertyOrder(10)]
		void DrawBody()
		{
			HeartManager live = Live;

			DrawLive(live);
			GUILayout.Space(6);
			DrawTools(live);
			GUILayout.Space(6);
			DrawMaintenance();

			if (Application.isPlaying) GUIHelper.RequestRepaint();
		}

		void DrawLive(HeartManager live)
		{
			SirenixEditorGUI.BeginBox();
			SirenixEditorGUI.BeginBoxHeader();
			GUILayout.Label("Live", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			GUILayout.Label(live != null ? "Play Mode" : "chưa vào Play", EditorStyles.miniLabel);
			SirenixEditorGUI.EndBoxHeader();

			if (live == null)
			{
				SirenixEditorGUI.InfoMessageBox("Vào Play để xem và chỉnh tim đang chạy.");
				SirenixEditorGUI.EndBox();
				return;
			}

			bool mismatch = live.Config != null && Config != null && live.Config != Config;
			if (mismatch)
			{
				SirenixEditorGUI.WarningMessageBox(
					$"Đang chạy bằng '{live.Config.name}', khác asset ở khung Config phía trên. Sửa ở trên không ăn vào game.");
			}

			Stat("Tim", $"{live.CurrentHeart} / {live.MaxHearts}");
			Stat("Unlimited", live.IsUnlimited
				? $"còn {TimeManager.Format(live.GetUnlimitedTimeRemaining())}"
				: "tắt");
			Stat("Tới tim kế", live.IsFull || live.IsUnlimited
				? "—"
				: TimeManager.Format(TimeSpan.FromSeconds(live.GetTimeToNextHeart())));
			Stat("Tới khi đầy", live.IsFull || live.IsUnlimited
				? "—"
				: TimeManager.Format(TimeSpan.FromSeconds(live.GetTimeToFull())));

			GUILayout.Space(2);

			Stat("Lần đầu chơi", live.WasFirstPlay ? "có" : "không");
			Stat("Comeback", live.WasComebackReward ? $"có (vắng {live.HoursSinceLastLogin:0.0}h)" : $"không (vắng {live.HoursSinceLastLogin:0.0}h)");

			SirenixEditorGUI.EndBox();
		}

		void DrawTools(HeartManager live)
		{
			SirenixEditorGUI.BeginBox();
			SirenixEditorGUI.BeginBoxHeader();
			GUILayout.Label("Điều khiển", EditorStyles.boldLabel);
			SirenixEditorGUI.EndBoxHeader();

			using (new EditorGUI.DisabledScope(live == null))
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("-1", GUILayout.Height(ButtonHeight))) live.SetHearts(live.CurrentHeart - 1);
				if (GUILayout.Button("+1", GUILayout.Height(ButtonHeight))) live.SetHearts(live.CurrentHeart + 1);
				if (GUILayout.Button("Hết tim", GUILayout.Height(ButtonHeight))) live.SetHearts(0);

				GUI.color = new Color(0.45f, 0.85f, 0.5f);
				if (GUILayout.Button("Đổ đầy", GUILayout.Height(ButtonHeight))) live.Refill();
				GUI.color = Color.white;
				GUILayout.EndHorizontal();

				GUILayout.Space(4);

				GUILayout.BeginHorizontal();
				GUILayout.Label("Unlimited", GUILayout.Width(70));
				_unlimitedMinutes = Mathf.Max(0f, EditorGUILayout.FloatField(_unlimitedMinutes, GUILayout.Width(60)));
				GUILayout.Label("phút", GUILayout.Width(34));

				if (GUILayout.Button("Cộng thêm", GUILayout.Height(ButtonHeight))) live.TryAddUnlimited(_unlimitedMinutes);
				if (GUILayout.Button("Tắt", GUILayout.Height(ButtonHeight), GUILayout.Width(70))) live.ClearUnlimited();
				GUILayout.EndHorizontal();
			}

			GUILayout.Space(6);

			bool bypass = EditorPrefs.GetBool(BypassKey, false);
			bool next = EditorGUILayout.ToggleLeft(
				new GUIContent("Chơi không trừ tim", "Chỉ chạy trong Editor. Thoát Play vẫn nhớ lựa chọn."),
				bypass);

			if (next != bypass)
			{
				EditorPrefs.SetBool(BypassKey, next);
				HeartManager.BypassHeartCost = next;
			}

			SirenixEditorGUI.EndBox();
		}

		void DrawMaintenance()
		{
			SirenixEditorGUI.BeginBox();
			SirenixEditorGUI.BeginBoxHeader();
			GUILayout.Label("Save", EditorStyles.boldLabel);
			SirenixEditorGUI.EndBoxHeader();

			GUI.color = new Color(0.95f, 0.65f, 0.4f);
			bool clicked = GUILayout.Button("Xóa save tim (mô phỏng lần đầu chơi)", GUILayout.Height(28));
			GUI.color = Color.white;

			if (clicked && EditorUtility.DisplayDialog(
					"Xóa save tim",
					"Xóa HEART, IS_UNLIMITER_HEART, TIME_UNLIMITER_HEART, TIME_LAST_OVER_HEART, LAST_TIME_LOGIN.\n\nVào lại Play để chạy nhánh first play.",
					"Xóa", "Hủy"))
			{
				HeartManager.ClearSave();
			}

			SirenixEditorGUI.EndBox();
		}

		static void Stat(string label, string value)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(label, GUILayout.Width(LabelWidth));
			GUILayout.Label(value, EditorStyles.boldLabel);
			GUILayout.EndHorizontal();
		}

		static HeartConfig Locate()
		{
			string guid = AssetDatabase.FindAssets("t:HeartConfig").FirstOrDefault();

			return string.IsNullOrEmpty(guid)
				? null
				: AssetDatabase.LoadAssetAtPath<HeartConfig>(AssetDatabase.GUIDToAssetPath(guid));
		}
	}
}
