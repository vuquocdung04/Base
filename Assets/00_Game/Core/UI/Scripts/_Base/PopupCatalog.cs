using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PopupSetting
{
    public string typeName;

    public float animDuration = 0.3f;
    [Range(0f, 1f)] public float backdropAlpha = 0.95f;
    public bool closeOnBackdropClick;
    public bool peekOnBackdropHold;
    public float peekFadeDuration = 0.15f;

    public static readonly PopupSetting Fallback = new PopupSetting();
}

[CreateAssetMenu(menuName = "Base/UI/Popup Catalog", fileName = "PopupCatalog")]
public class PopupCatalog : ScriptableObject
{
    public List<PopupSetting> popups = new();

    public PopupSetting Find(string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return null;

        for (int i = 0; i < popups.Count; i++)
        {
            if (popups[i] != null && popups[i].typeName == typeName) return popups[i];
        }
        return null;
    }
}
