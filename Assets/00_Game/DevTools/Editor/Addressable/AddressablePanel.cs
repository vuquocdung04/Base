using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace DevTools.Addressable
{
	[System.Serializable]
	public class AddressablePanel : IDevToolPanel
	{
		public string Title => "Addressable";
		public int Order => 40;
		public SdfIconType Icon => SdfIconType.Box;

		const string PrefKey = "AutoTool_PrefabPath";
		const string PopupCatalogFolder = "Assets/00_Game/Resources";

		[Title("Cấu hình", bold: true)]
		[FolderPath(RequireExistingPath = true)]
		[LabelText("Folder"), LabelWidth(80)]
		[OnValueChanged("SavePrefs")]
		public string prefabFolderPath;

		public AddressablePanel()
		{
			prefabFolderPath = EditorPrefs.GetString(PrefKey, "");
			popupCatalog = LocatePopupCatalog();
			SyncPopup();
		}

		void SavePrefs()
		{
			EditorPrefs.SetString(PrefKey, prefabFolderPath);
		}

		[HorizontalGroup("Actions"), PropertyOrder(5)]
		[Button("QUÉT", ButtonSizes.Large), GUIColor(0.4f, 0.9f, 0.5f)]
		void Scan()
		{
			entries = new List<PrefabEntry>();

			if (string.IsNullOrEmpty(prefabFolderPath) || !AssetDatabase.IsValidFolder(prefabFolderPath))
			{
				EditorUtility.DisplayDialog("Lỗi", "Folder không hợp lệ!", "OK");
				return;
			}

			var settings = AddressableAssetSettingsDefaultObject.Settings;
			string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolderPath });

			foreach (string guid in prefabGuids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				string prefabName = Path.GetFileNameWithoutExtension(assetPath);
				string constName = Regex.Replace(prefabName, "([a-z])([A-Z])", "$1_$2").ToUpper();
				bool isAddressable = settings != null && settings.FindAssetEntry(guid) != null;

				entries.Add(new PrefabEntry
				{
					guid = guid,
					asset = AssetDatabase.LoadMainAssetAtPath(assetPath),
					name = prefabName,
					address = prefabName,
					constant = constName,
					addressable = isAddressable
				});
			}

			Debug.Log($"<color=cyan>[Addressable]</color> Quét thấy <b>{entries.Count}</b> prefab trong {prefabFolderPath}.");
		}

		[HorizontalGroup("Actions"), PropertyOrder(5)]
		[Button("Tích & Sinh Script", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
		[ShowIf("@entries != null && entries.Count > 0")]
		void Apply()
		{
			var settings = AddressableAssetSettingsDefaultObject.Settings;
			if (settings == null)
			{
				Debug.LogError("Không tìm thấy Addressable Settings!");
				return;
			}

			AddressableAssetGroup group = settings.DefaultGroup;
			List<string> generatedConstants = new List<string>();

			foreach (var e in entries)
			{
				AddressableAssetEntry entry = settings.CreateOrMoveEntry(e.guid, group, readOnly: false, postEvent: false);
				entry.address = e.name;
				generatedConstants.Add($"    public const string {e.constant} = \"{e.name}\";");
			}

			settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
			AssetDatabase.SaveAssets();

			string scriptPath = AutoDetectScriptPath();
			GenerateScript(scriptPath, generatedConstants);

			Debug.Log($"<color=green>[Thành Công]</color> Đã update {entries.Count} prefabs. File lưu tại: {scriptPath}");
			Scan();
		}

		[HorizontalGroup("Actions", MaxWidth = 160), PropertyOrder(5)]
		[Button("Untick", ButtonSizes.Large), GUIColor(1f, 0.5f, 0.4f)]
		[ShowIf("@entries != null && entries.Count > 0")]
		void RemoveAddressables()
		{
			var settings = AddressableAssetSettingsDefaultObject.Settings;
			if (settings == null)
			{
				Debug.LogError("Không tìm thấy Addressable Settings!");
				return;
			}

			int removed = 0;
			foreach (var e in entries)
			{
				if (settings.RemoveAssetEntry(e.guid, postEvent: false))
					removed++;
			}

			settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, null, true);
			AssetDatabase.SaveAssets();

			Debug.Log($"<color=orange>[Addressable]</color> Đã untick {removed}/{entries.Count} prefab (gồm cả sub-folder).");
			Scan();
		}

		[Title("Kết quả", bold: true), PropertyOrder(6)]
		[ShowInInspector, HideLabel, Searchable]
		[TableList(IsReadOnly = true, ShowPaging = true, ShowIndexLabels = true, NumberOfItemsPerPage = 20)]
		public List<PrefabEntry> entries = new List<PrefabEntry>();

		// ===================== POPUP =====================

		[Title("Popup Catalog", bold: true), PropertyOrder(10)]
		[LabelText("Catalog"), LabelWidth(80)]
		[OnValueChanged("SyncPopup")]
		public PopupCatalog popupCatalog;

		[PropertyOrder(11)]
		[InfoBox("Chưa có PopupCatalog. Bấm 'Tạo Catalog' để tạo trong Resources.", InfoMessageType.Warning,
			VisibleIf = "@popupCatalog == null")]
		[Button("Tạo Catalog", ButtonSizes.Large), GUIColor(0.4f, 0.9f, 0.5f)]
		[ShowIf("@popupCatalog == null")]
		void CreatePopupCatalog()
		{
			Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), PopupCatalogFolder));
			AssetDatabase.Refresh();

			var asset = ScriptableObject.CreateInstance<PopupCatalog>();
			string path = AssetDatabase.GenerateUniqueAssetPath($"{PopupCatalogFolder}/PopupCatalog.asset");
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();

			popupCatalog = asset;
			SyncPopup();
		}

		[PropertyOrder(11)]
		[Button("Đồng bộ popup từ code", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
		[ShowIf("@popupCatalog != null")]
		void SyncPopup()
		{
			if (popupCatalog == null) return;

			List<string> names = TypeCache.GetTypesDerivedFrom<BaseBox>()
				.Where(t => !t.IsAbstract)
				.Select(t => t.Name)
				.ToList();

			popupCatalog.popups.RemoveAll(p => p == null || !names.Contains(p.typeName));

			foreach (string name in names)
			{
				if (popupCatalog.Find(name) == null)
					popupCatalog.popups.Add(new PopupSetting { typeName = name });
			}

			popupCatalog.popups.Sort((a, b) => string.CompareOrdinal(a.typeName, b.typeName));
			popupRows = popupCatalog.popups;

			EditorUtility.SetDirty(popupCatalog);
		}

		[PropertyOrder(12)]
		[ShowInInspector, HideLabel, Searchable]
		[TableList(ShowPaging = true, NumberOfItemsPerPage = 25)]
		[ShowIf("@popupCatalog != null")]
		List<PopupSetting> popupRows;

		[OnInspectorGUI, PropertyOrder(99)]
		void MarkPopupDirty()
		{
			if (popupCatalog != null && GUI.changed) EditorUtility.SetDirty(popupCatalog);
		}

		static PopupCatalog LocatePopupCatalog()
		{
			string guid = AssetDatabase.FindAssets("t:PopupCatalog").FirstOrDefault();
			return string.IsNullOrEmpty(guid)
				? null
				: AssetDatabase.LoadAssetAtPath<PopupCatalog>(AssetDatabase.GUIDToAssetPath(guid));
		}

		// ===================== HELPERS =====================

		string AutoDetectScriptPath()
		{
			string[] foundGuids = AssetDatabase.FindAssets("PathPrefabs t:MonoScript");

			if (foundGuids.Length > 0)
				return AssetDatabase.GUIDToAssetPath(foundGuids[0]);

			string defaultFolder = "Assets/Scripts/Data";

			if (!AssetDatabase.IsValidFolder("Assets/Scripts"))
				AssetDatabase.CreateFolder("Assets", "Scripts");

			if (!AssetDatabase.IsValidFolder(defaultFolder))
				AssetDatabase.CreateFolder("Assets/Scripts", "Data");

			return $"{defaultFolder}/PathPrefabs.cs";
		}

		void GenerateScript(string path, List<string> constants)
		{
			using (StreamWriter writer = new StreamWriter(path))
			{
				writer.WriteLine("// AUTO-GENERATED CODE. DO NOT EDIT MANUALLY.");
				writer.WriteLine("public class PathPrefabs");
				writer.WriteLine("{");

				foreach (string constLine in constants)
					writer.WriteLine(constLine);

				writer.WriteLine("}");
			}

			AssetDatabase.Refresh();
		}

		[System.Serializable]
		public class PrefabEntry
		{
			[HideInTables] public string guid;
			[HideInTables] public Object asset;

			[TableColumnWidth(60, false)]
			[ReadOnly, HideLabel]
			public bool addressable;

			[TableColumnWidth(180)]
			[ReadOnly, LabelText("Prefab")]
			public string name;

			[ReadOnly, LabelText("Address")]
			public string address;

			[ReadOnly, LabelText("Const")]
			public string constant;

			[Button("Ping"), TableColumnWidth(55, false)]
			void Ping()
			{
				if (asset != null)
				{
					EditorUtility.FocusProjectWindow();
					Selection.activeObject = asset;
					EditorGUIUtility.PingObject(asset);
				}
			}
		}
	}
}
