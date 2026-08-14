using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarDataBase", menuName = "Base/Avatar/Avatar Database")]
public class AvatarDataBase : ScriptableObject
{
    public List<Sprite> avatars;

    public Sprite GetSpriteById(int id)
    {
        if (id < 0 || id >= avatars.Count) return null;
        return avatars[id];
    }

    public int Count => avatars.Count;
}