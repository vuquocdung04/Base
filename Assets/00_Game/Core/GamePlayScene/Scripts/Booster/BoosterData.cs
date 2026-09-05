using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoosterRecord
{
    public int Amount;
    public bool TutorialDone;
    public bool Seeded;
}

public static class BoosterData
{
    private const string SaveKey = "booster.records";

    private static Dictionary<string, BoosterRecord> records;
    private static bool memoryOnly;

    public static event Action<string> OnAmountChanged;

    private static Dictionary<string, BoosterRecord> Records
        => records ??= GamePrefs.Get(SaveKey, new Dictionary<string, BoosterRecord>());

    public static void UseMemoryOnly()
    {
        memoryOnly = true;
        records = new Dictionary<string, BoosterRecord>();
    }

    public static BoosterRecord Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return new BoosterRecord();

        if (!Records.TryGetValue(id, out BoosterRecord record))
        {
            record = new BoosterRecord();
            Records[id] = record;
        }
        return record;
    }

    public static int GetAmount(string id) => Get(id).Amount;

    public static void SetAmount(string id, int amount)
    {
        if (string.IsNullOrEmpty(id)) return;

        BoosterRecord record = Get(id);
        int value = Mathf.Max(0, amount);

        if (record.Amount == value) return;

        record.Amount = value;
        Save();

        OnAmountChanged?.Invoke(id);
    }

    public static void Add(string id, int amount) => SetAmount(id, GetAmount(id) + amount);

    public static bool IsTutorialDone(string id) => Get(id).TutorialDone;

    public static void SetTutorialDone(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        BoosterRecord record = Get(id);
        if (record.TutorialDone) return;

        record.TutorialDone = true;
        Save();
    }

    public static void Save()
    {
        if (memoryOnly) return;

        GamePrefs.Set(SaveKey, Records);
    }
}
