using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public static class GamePrefs
{
    private static Dictionary<string, object> cache = new();

    private static bool isDirty = false;
    public static bool isSaving = false;
    public static string SavePath => Application.persistentDataPath + "/game_data.json";

    public static void Init()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                cache = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }
            catch
            {
                Debug.LogError($"File not read");
                cache = new();
            }
        }
    }

    public static void Set<T>(string key, T value)
    {
        if (cache.TryGetValue(key, out object existingValue))
            if (existingValue != null && existingValue.Equals(value)) return;

        cache[key] = value;
        isDirty = true;
    }

    public static T Get<T>(string key, T defaultValue = default)
    {
        if (cache.TryGetValue(key, out object value))
        {
            try
            {
                if (value is Newtonsoft.Json.Linq.JToken token)
                    return token.ToObject<T>();
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    public static bool HasKey(string key) => cache.ContainsKey(key);

    public static void DeleteKey(string key)
    {
        if (cache.Remove(key)) isDirty = true;
    }

    public static void SaveNow()
    {
        if (!isDirty || isSaving) return;
        _ = ExecuteSaveAsync();
    }

    private static async Task ExecuteSaveAsync()
    {
        isSaving = true;
        isDirty = false;

        try
        {
            string jsonToSave = JsonConvert.SerializeObject(cache);

            await Awaitable.BackgroundThreadAsync();
            File.WriteAllText(SavePath, jsonToSave);
        }
        catch (Exception e)
        {
            Debug.LogError("Lỗi khi ghi file: " + e.Message);
            isDirty = true;
        }
        finally
        {
            await Awaitable.MainThreadAsync();
            isSaving = false;
        }
    }

    public static async void StartAutoSaveLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Awaitable.WaitForSecondsAsync(10f, token);

                if (isDirty && !isSaving)
                {
                    await ExecuteSaveAsync();
                }
            }
        }
        catch (OperationCanceledException)
        {
            if (isDirty) SaveNow();
        }
    }
}


public class PrefVar<T>
{
    private string key;
    private T defaultValue;

    public PrefVar(string newKey, T newDefaultValue = default)
    {
        key = newKey;
        defaultValue = newDefaultValue;
    }

    public T Value
    {
        get => GamePrefs.Get<T>(key, defaultValue);
        set => GamePrefs.Set<T>(key, value);
    }

    public static implicit operator T(PrefVar<T> prefVar) => prefVar.Value;
}
