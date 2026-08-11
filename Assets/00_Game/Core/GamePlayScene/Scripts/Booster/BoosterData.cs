using System.Collections.Generic;

[System.Serializable]
public class BoosterRecord
{
    public int Amount;
    public bool TutorialDone;
}

public static class BoosterData
{
    private const string SaveKey = "boosters";

    private static List<BoosterRecord> records;

    private static List<BoosterRecord> Records
        => records ??= GamePrefs.Get(SaveKey, new List<BoosterRecord>());

    public static bool Has(int index) => index >= 0 && index < Records.Count;

    public static BoosterRecord Get(int index)
    {
        while (Records.Count <= index) Records.Add(new BoosterRecord());
        return Records[index];
    }

    public static void Save() => GamePrefs.Set(SaveKey, Records);
}
