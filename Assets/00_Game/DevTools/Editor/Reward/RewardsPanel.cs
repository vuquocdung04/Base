using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace DevTools.Reward
{
	[System.Serializable]
	public class RewardsPanel : IDevToolPanel
	{
		public string Title => "Rewards";
		public int Order => 36;
		public SdfIconType Icon => SdfIconType.GiftFill;

		const string CatalogFolder = "Assets/00_Game/Configs/Reward/Resources";
		const string RewardScriptFolder = "Assets/00_Game/Core/LoadingScene/Scrips/Reward";
		const string KeysPath = RewardScriptFolder + "/RewardKeys.cs";
		const string BindingsPath = RewardScriptFolder + "/RewardBindings.cs";
		const float ButtonHeight = 26f;
		const float ActionWidth = 190f;

		static readonly Regex IdentifierRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");

		RewardCatalog _catalog;
		bool _located;
		List<RewardEntry> _rows;
		List<TestRow> _testRows = new List<TestRow> { new TestRow() };
		string[] _ids = System.Array.Empty<string>();

		[System.Serializable]
		class TestRow
		{
			public string id = "";
			public int quantity = 1;
		}

		[Title("Catalog", bold: true), PropertyOrder(0)]
		[ShowInInspector, LabelText("Asset"), LabelWidth(70)]
		[OnValueChanged(nameof(SyncRows))]
		RewardCatalog Catalog
		{
			get
			{
				if (_catalog == null && !_located)
				{
					_located = true;
					_catalog = Locate();
					SyncRows();
				}

				return _catalog;
			}
			set
			{
				_catalog = value;
				_located = true;
			}
		}

		[PropertyOrder(1)]
		[InfoBox("Chưa có RewardCatalog. Bấm 'Tạo Catalog' để tạo trong Configs/Reward/Resources.",
			InfoMessageType.Warning, VisibleIf = "@Catalog == null")]
		[Button("Tạo Catalog", ButtonSizes.Large), GUIColor(0.4f, 0.9f, 0.5f)]
		[ShowIf("@Catalog == null")]
		void CreateCatalog()
		{
			Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), CatalogFolder));
			AssetDatabase.Refresh();

			RewardCatalog created = ScriptableObject.CreateInstance<RewardCatalog>();
			string path = AssetDatabase.GenerateUniqueAssetPath($"{CatalogFolder}/{RewardCatalog.RESOURCE_PATH}.asset");

			AssetDatabase.CreateAsset(created, path);
			AssetDatabase.SaveAssets();

			Catalog = created;
			SyncRows();
			EditorGUIUtility.PingObject(created);
		}

		[Title("Danh sách Reward", bold: true), PropertyOrder(10)]
		[ShowInInspector, HideLabel, Searchable]
		[ShowIf("@Catalog != null")]
		[TableList(AlwaysExpanded = true, DrawScrollView = true, MinScrollViewHeight = 160, MaxScrollViewHeight = 440,
			ShowIndexLabels = true)]
		List<RewardEntry> Rows
		{
			get => _rows;
			set => _rows = value;
		}

		[OnInspectorGUI, PropertyOrder(15)]
		void DrawRowActions()
		{
			if (Catalog == null) return;

			SyncRows();

			GUILayout.Space(2);
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("＋ Thêm reward", GUILayout.Width(ActionWidth), GUILayout.Height(ButtonHeight)))
			{
				Undo.RecordObject(Catalog, "Add Reward");
				_rows.Add(new RewardEntry());
				EditorUtility.SetDirty(Catalog);
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		[OnInspectorGUI, PropertyOrder(20)]
		void DrawValidation()
		{
			if (Catalog == null) return;

			SyncRows();
			RebuildIds();

			var errors = new List<string>();
			var warnings = new List<string>();
			var seen = new HashSet<string>();

			for (int i = 0; i < _rows.Count; i++)
			{
				RewardEntry entry = _rows[i];
				string id = entry?.id ?? "";
				string label = $"Dòng {i + 1}";

				if (string.IsNullOrWhiteSpace(id)) errors.Add($"{label}: chưa đặt id.");
				else if (!IdentifierRegex.IsMatch(id)) errors.Add($"{label}: id '{id}' không phải C# identifier hợp lệ.");
				else if (!seen.Add(id)) errors.Add($"{label}: id '{id}' bị trùng.");

				if (entry != null && entry.icon == null && !string.IsNullOrWhiteSpace(id)) warnings.Add(id);
			}

			GUILayout.Space(4);

			foreach (string error in errors)
			{
				SirenixEditorGUI.ErrorMessageBox(error);
			}

			if (warnings.Count > 0)
			{
				SirenixEditorGUI.WarningMessageBox($"Chưa có icon: {string.Join(", ", warnings)}");
			}

			if (errors.Count == 0 && warnings.Count == 0 && _rows.Count > 0)
			{
				SirenixEditorGUI.InfoMessageBox($"{_rows.Count} reward, không có lỗi.");
			}

			if (GUI.changed) EditorUtility.SetDirty(Catalog);
		}

		[OnInspectorGUI, PropertyOrder(30)]
		void DrawCodeGen()
		{
			if (Catalog == null) return;

			GUILayout.Space(6);
			SirenixEditorGUI.BeginBox();
			SirenixEditorGUI.BeginBoxHeader();
			GUILayout.Label("Sinh code", EditorStyles.boldLabel);
			SirenixEditorGUI.EndBoxHeader();

			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical(GUILayout.Width(ActionWidth));

			GUI.color = new Color(0.45f, 0.85f, 0.5f);
			if (GUILayout.Button("Generate Keys", GUILayout.Height(ButtonHeight)))
			{
				RewardKeysGenerator.Generate(Catalog, KeysPath);
			}
			GUI.color = Color.white;

			if (GUILayout.Button("Mở RewardKeys", GUILayout.Height(ButtonHeight))) OpenScript(KeysPath);
			if (GUILayout.Button("Mở RewardBindings", GUILayout.Height(ButtonHeight))) OpenScript(BindingsPath);

			GUILayout.EndVertical();

			GUILayout.Space(12);

			GUILayout.BeginVertical();
			PathRow("RewardKeys.cs", KeysPath);
			PathRow("RewardBindings.cs", BindingsPath);
			GUILayout.Space(4);
			DrawCoverage();
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			SirenixEditorGUI.EndBox();
		}

		[OnInspectorGUI, PropertyOrder(40)]
		void DrawTest()
		{
			if (Catalog == null) return;

			GUILayout.Space(6);
			SirenixEditorGUI.BeginBox();
			SirenixEditorGUI.BeginBoxHeader();
			GUILayout.Label("Thử nhanh", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			GUILayout.Label(Live != null ? "Play Mode" : "vào Play để dùng", EditorStyles.miniLabel);
			SirenixEditorGUI.EndBoxHeader();

			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical(GUILayout.Width(ActionWidth));
			using (new EditorGUI.DisabledScope(Live == null))
			{
				if (GUILayout.Button("Cộng thưởng", GUILayout.Height(ButtonHeight))) Live.Grant(BuildTestRewards());
				if (GUILayout.Button("Mở popup", GUILayout.Height(ButtonHeight))) Live.ShowAsync(BuildTestRewards()).Forget();

				GUI.color = new Color(0.45f, 0.85f, 0.5f);
				if (GUILayout.Button("Cộng + Popup", GUILayout.Height(ButtonHeight))) Live.ClaimAsync(BuildTestRewards()).Forget();
				GUI.color = Color.white;
			}
			GUILayout.EndVertical();

			GUILayout.Space(12);

			GUILayout.BeginVertical();

			int removeAt = -1;

			for (int i = 0; i < _testRows.Count; i++)
			{
				TestRow row = _testRows[i];

				GUILayout.BeginHorizontal();
				GUILayout.Label($"{i + 1}", GUILayout.Width(16));

				if (_ids.Length == 0)
				{
					row.id = EditorGUILayout.TextField(row.id, GUILayout.Width(180));
				}
				else
				{
					int index = System.Array.IndexOf(_ids, row.id);
					int next = EditorGUILayout.Popup(Mathf.Max(0, index), _ids, GUILayout.Width(180));
					row.id = _ids[Mathf.Clamp(next, 0, _ids.Length - 1)];
				}

				row.quantity = EditorGUILayout.IntField(row.quantity, GUILayout.Width(70));

				using (new EditorGUI.DisabledScope(_testRows.Count <= 1))
				{
					if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(24))) removeAt = i;
				}

				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			if (removeAt >= 0) _testRows.RemoveAt(removeAt);

			GUILayout.Space(2);

			if (GUILayout.Button("+ Thêm dòng", EditorStyles.miniButton, GUILayout.Width(100)))
			{
				_testRows.Add(new TestRow { id = _ids.Length > 0 ? _ids[0] : "" });
			}

			GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			SirenixEditorGUI.EndBox();

			if (Application.isPlaying) GUIHelper.RequestRepaint();
		}

		static RewardManager Live
		{
			get
			{
				if (!Application.isPlaying) return null;

				RewardManager manager = RewardManager.Instance;
				return manager != null ? manager : null;
			}
		}

		List<GameReward> BuildTestRewards()
		{
			var rewards = new List<GameReward>(_testRows.Count);

			foreach (TestRow row in _testRows)
			{
				if (!string.IsNullOrWhiteSpace(row.id)) rewards.Add(new GameReward(row.id, row.quantity));
			}

			return rewards;
		}

		void DrawCoverage()
		{
			RewardManager live = Live;

			if (live == null)
			{
				GUILayout.Label("Vào Play để xem id nào chưa được Bind.", EditorStyles.miniLabel);
				return;
			}

			var missing = new List<string>();

			foreach (string id in _ids)
			{
				if (!live.IsBound(id)) missing.Add(id);
			}

			if (missing.Count == 0) SirenixEditorGUI.InfoMessageBox($"{_ids.Length} id đều đã Bind.");
			else SirenixEditorGUI.WarningMessageBox($"Chưa Bind trong RewardBindings: {string.Join(", ", missing)}");
		}

		static void OpenScript(string path)
		{
			var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

			if (asset != null) AssetDatabase.OpenAsset(asset);
			else Debug.LogError($"[Reward] Chưa có file {path}.");
		}

		void PathRow(string label, string path)
		{
			bool exists = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), path));

			GUILayout.BeginHorizontal();
			GUILayout.Label(label, GUILayout.Width(170));

			GUI.color = exists ? new Color(0.55f, 0.9f, 0.6f) : new Color(0.75f, 0.75f, 0.75f);
			GUILayout.Label(exists ? "đã có" : "chưa có", EditorStyles.miniLabel, GUILayout.Width(55));
			GUI.color = Color.white;

			EditorGUILayout.SelectableLabel(path, EditorStyles.miniLabel, GUILayout.Height(16));
			GUILayout.EndHorizontal();
		}

		void SyncRows()
		{
			if (_catalog == null)
			{
				_rows = null;
				return;
			}

			if (!ReferenceEquals(_rows, _catalog.rewards)) _rows = _catalog.rewards;
		}

		void RebuildIds()
		{
			_ids = _rows == null
				? System.Array.Empty<string>()
				: _rows.Where(r => r != null && !string.IsNullOrWhiteSpace(r.id)).Select(r => r.id).ToArray();
		}

		static RewardCatalog Locate()
		{
			string guid = AssetDatabase.FindAssets("t:RewardCatalog").FirstOrDefault();

			return string.IsNullOrEmpty(guid)
				? null
				: AssetDatabase.LoadAssetAtPath<RewardCatalog>(AssetDatabase.GUIDToAssetPath(guid));
		}
	}
}
