using System;
using EventDispatcher;
using UnityEngine;
public enum CurrencyType
{
    Coin,
    Gem
}
public static class NumberFormatter
{
    public static string Format(double value)
    {
        double abs = System.Math.Abs(value);
        if (abs < 10000) return ((long)value).ToString();
        if (abs < 1_000_000) return $"{value / 1_000.0:0.0}K";
        if (abs < 1_000_000_000) return $"{value / 1_000_000.0:0.0}M";
        if (abs < 1_000_000_000_000) return $"{value / 1_000_000_000.0:0.0}B";
        return $"{value / 1_000_000_000_000.0:0.0}T";
    }
}

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    private ToastManager toastManager;

    public void Init(ToastManager toastManager)
    {
        Instance = this;
        this.toastManager = toastManager;
    }

    public int Get(CurrencyType type) => type switch
    {
        CurrencyType.Coin => UseProfile.Coin,
        _ => throw new ArgumentException($"Unknown currency: {type}")
    };

    private void Set(CurrencyType type, int value)
    {
        switch (type)
        {
            case CurrencyType.Coin:
                UseProfile.Coin = value;
                break;
            default:
                throw new ArgumentException($"Unknown currency: {type}");
        }
    }

    public bool TrySpend(CurrencyType type, int amount)
    {
        if (amount <= 0) return true;

        int current = Get(type);
        if (current < amount)
        {
            toastManager.ShowToast($"Not enough {type}");
            return false;
        }

        Set(type, current - amount);
        PostChangeEvent(type);
        return true;
    }

    public void Add(CurrencyType type, int amount)
    {
        if (amount <= 0) return;

        Set(type, Get(type) + amount);
        PostChangeEvent(type);
    }

    public bool CanAfford(CurrencyType type, int amount) => Get(type) >= amount;

    private void PostChangeEvent(CurrencyType type)
    {
        EventID eventId = type switch
        {
            CurrencyType.Coin => EventID.CHANGE_COIN,
            _ => EventID.CHANGE_COIN
        };
        this.PostEvent(eventId);
    }
}