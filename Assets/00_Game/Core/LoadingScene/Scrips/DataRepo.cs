
using UnityEngine;

public class DataRepo : MonoBehaviour
{
    public static DataRepo Instance { get; private set; }

    public AvatarDataBase avatarData;
    public void Init()
    {
        Instance = this;
    }
}
