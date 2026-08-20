using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace DevTools.Shop
{
	[System.Serializable]
	public class ShopPanel : IDevToolPanel
	{
		public string Title => "Shop";
		public int Order => 37;
		public SdfIconType Icon => SdfIconType.CartFill;

		const string CatalogFolder = "Assets/00_Game/Configs/Shop/Resources";
		const float ButtonHeight = 24f;
		const float NameWidth = 190f;
		const float AddWidth = 190f;

		const double ValidateInterval = 0.5d;

		PackCatalog _catalog;
		bool _located;
		List<PackConfig> _rows;
		readonly List<string> _errors = new();
		readonly List<string> _warnings = new();
		double _nextValidate;
		bool _validateDirty = true;

		static PackManager Live
		{
			get
			{
				if (!Application.isPlaying) return null;

				PackManager manager = PackManager.Instance;
				return manager != null ? manager : null;
			}
		}

		[Title("Catalog", bold: true), PropertyOrder(0)]
		[ShowInInspector, LabelText("Asset"), LabelWidth(70)]
		[OnValueChanged(nameof(SyncRows))]
		PackCatalog Catalog
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
		[InfoBox("Chưa có PackCatalog. Bấm 'Tạo Catalog' để tạo trong Configs/Shop/Resources.",
			InfoMessageType.Warning, VisibleIf = "@Catalog == null")]
		[Button("Tạo Catalog", ButtonSizes.Large), GUIColor(0.4f, 0.9f, 0.5f)]
		[ShowIf("@Catalog == null")]
		void CreateCatalog()
		{
			Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), CatalogFolder));
			AssetDatabase.Refresh();

			PackCatalog created = ScriptableObject.CreateInstance<PackCatalog>();
			string path = AssetDatabase.GenerateUniqueAssetPath($"{CatalogFolder}/{PackCatalog.RESOURCE_PATH}.asset");

			AssetDatabase.CreateAsset(created, path);
			AssetDatabase.SaveAssets();

			Catalog = created;
			SyncRows();
			EditorGUIUtility.PingObject(created);
		}

		[Title("Danh sách Pack", bold: true), PropertyOrder(10)]
		[ShowInInspector, HideLabel]
		[ShowIf("@Catalog != null")]
		[ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowPaging = false,
			ListElementLabelName = nameof(PackConfig.packId))]
		List<PackConfig> Rows
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

			if (GUILayout.Button("＋ Thêm pack", GUILayout.Width(AddWidth), GUILayout.Height(ButtonHeight + 2f)))
			{
				Undo.RecordObject(Catalog, "Add Pack");
				_rows.Add(new PackConfig());
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

			if (Event.current.type == EventType.Layout &&
				(_validateDirty || EditorApplication.timeSinceStartup >= _nextValidate))
			{
				_validateDirty = false;
				_nextValidate = EditorApplication.timeSinceStartup + ValidateInterval;
				Revalidate();
			}

			GUILayout.Space(4);

			foreach (string error in _errors) SirenixEditorGUI.ErrorMessageBox(error);
			foreach (string warning in _warnings) SirenixEditorGUI.WarningMessageBox(warning);

			if (_errors.Count == 0 && _warnings.Count == 0 && _rows.Count > 0)
				SirenixEditorGUI.InfoMessageBox($"{_rows.Count} pack, không có lỗi.");

			if (GUI.changed)
			{
				EditorUtility.SetDirty(Catalog);
				_validateDirty = true;
			}
		}

		void Revalidate()
		{
			_errors.Clear();
			_warnings.Clear();

			var errors = _errors;
			var warnings = _warnings;
			var seen = new HashSet<string>();

			for (int i = 0; i < _rows.Count; i++)
			{
				PackConfig pack = _rows[i];
				if (pack == null) continue;

				string id = pack.packId ?? "";
				string label = string.IsNullOrWhiteSpace(id) ? $"Pack {i + 1}" : $"'{id}'";

				if (string.IsNullOrWhiteSpace(id)) errors.Add($"Pack {i + 1}: chưa đặt packId.");
				else if (!seen.Add(id)) errors.Add($"Pack {i + 1}: packId '{id}' bị trùng.");

				if (pack.RewardCount == 0) warnings.Add($"{label}: chưa có phần thưởng.");

				if (pack.cost.type == PackCostType.Iap && string.IsNullOrWhiteSpace(pack.cost.productId))
					errors.Add($"{label}: cost là Iap nhưng chưa có productId.");

				if (pack.cost.type == PackCostType.Coin && pack.cost.amount <= 0)
					warnings.Add($"{label}: cost là Coin nhưng amount = 0, thành pack miễn phí.");
			}
		}

		[OnInspectorGUI, PropertyOrder(30)]
		void DrawLive()
		{
			if (Catalog == null) return;

			PackManager live = Live;

			GUILayout.Space(6);
			SirenixEditorGUI.BeginBox();
			SirenixEditorGUI.BeginBoxHeader();
			GUILayout.Label("Thử mua", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			GUILayout.Label(live != null ? $"Play Mode · level {live.CurrentLevel}" : "vào Play để dùng",
				EditorStyles.miniLabel);
			SirenixEditorGUI.EndBoxHeader();

			DrawFakeWarning();

			if (live == null)
			{
				SirenixEditorGUI.InfoMessageBox("Vào Play để mua thử và xem pack nào đang hiện.");
				SirenixEditorGUI.EndBox();
				return;
			}

			for (int i = 0; i < _rows.Count; i++)
			{
				PackConfig pack = _rows[i];
				if (pack == null || string.IsNullOrWhiteSpace(pack.packId)) continue;

				bool purchased = live.IsPurchased(pack.packId);
				bool visible = live.CanShow(pack);

				GUILayout.BeginHorizontal();

				GUI.color = visible ? Color.white : new Color(1f, 1f, 1f, 0.45f);
				GUILayout.Label(pack.packId, GUILayout.Width(NameWidth));
				GUI.color = Color.white;

				GUILayout.Label($"{pack.cost.type}", EditorStyles.miniLabel, GUILayout.Width(50));
				GUILayout.Label(live.GetPriceText(pack), EditorStyles.boldLabel, GUILayout.Width(80));
				GUILayout.Label(purchased ? "đã mua" : visible ? "đang hiện" : "đang ẩn",
					EditorStyles.miniLabel, GUILayout.Width(70));

				if (GUILayout.Button("Mua", GUILayout.Width(60), GUILayout.Height(ButtonHeight)))
					live.TryPurchase(pack).Forget();

				using (new EditorGUI.DisabledScope(!purchased))
				{
					if (GUILayout.Button("Xóa đã mua", GUILayout.Width(90), GUILayout.Height(ButtonHeight)))
						live.ClearPurchased(pack.packId);
				}

				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			SirenixEditorGUI.EndBox();

			GUIHelper.RequestRepaint();
		}

		void DrawFakeWarning()
		{
			if (!Application.isPlaying || !PackPayers.UsingFakePayer) return;

			SirenixEditorGUI.WarningMessageBox(
				"Pack loại Ads đang dùng payer giả — bấm Mua là nhận thưởng miễn phí.");
		}

		void SyncRows()
		{
			if (_catalog == null)
			{
				_rows = null;
				return;
			}

			if (!ReferenceEquals(_rows, _catalog.packs)) _rows = _catalog.packs;
		}

		static PackCatalog Locate()
		{
			string guid = AssetDatabase.FindAssets("t:PackCatalog").FirstOrDefault();

			return string.IsNullOrEmpty(guid)
				? null
				: AssetDatabase.LoadAssetAtPath<PackCatalog>(AssetDatabase.GUIDToAssetPath(guid));
		}
	}
}
