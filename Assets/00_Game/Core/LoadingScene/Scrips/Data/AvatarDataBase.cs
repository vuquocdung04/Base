using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarDataBase", menuName = "Base/Avatar/Avatar Database")]
public class AvatarDataBase : ScriptableObject
{
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)]
    [PreviewField(64, ObjectFieldAlignment.Left)]
    public List<Sprite> avatars;

    public Sprite GetSpriteById(int id)
    {
        if (id < 0 || id >= avatars.Count) return null;
        return avatars[id];
    }

    public int Count => avatars.Count;
}