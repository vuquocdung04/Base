
using UnityEngine;

public class DataRepo : MonoBehaviour
{
    public static DataRepo Instance { get; private set;}
    
    public LocalizationDataBase localizationDataBase;
    public AudioDataBase audioData;
    public void Init()
    {
        Instance = this;
    }
}
