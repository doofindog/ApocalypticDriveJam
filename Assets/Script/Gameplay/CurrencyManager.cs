using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public event Action<float> OnCurrencyChanged;

    [SerializeField] private float startingAmount = 0f;

    private float currentAmount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentAmount = Mathf.Max(0f, startingAmount);
    }

    public float GetAmount()
    {
        return currentAmount;
    }

    public void Add(float amount)
    {
        if (amount <= 0f) return;

        currentAmount += amount;
        OnCurrencyChanged?.Invoke(currentAmount);
    }

    /// <summary>
    /// Removes currency. Returns true if fully removed, false if not enough.
    /// </summary>
    public bool Remove(float amount)
    {
        if (amount <= 0f) return true;

        if (currentAmount < amount)
            return false;

        currentAmount -= amount;
        OnCurrencyChanged?.Invoke(currentAmount);
        return true;
    }

    /// <summary>
    /// Sets the currency directly (useful for save/load or cheats)
    /// </summary>
    public void Set(float amount)
    {
        currentAmount = Mathf.Max(0f, amount);
        OnCurrencyChanged?.Invoke(currentAmount);
    }

    public bool HasEnough(float amount)
    {
        return currentAmount >= amount;
    }
}