
using UnityEngine;

public class DataRepo : MonoBehaviour
{
    public static DataRepo Instance { get; private set; }

    public AvatarCatalogSO avatarData;
    public void Init()
    {
        Instance = this;
    }
}
