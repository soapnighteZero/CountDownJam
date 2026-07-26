using System;
using UnityEngine;

public class SharedSegmentInventory : MonoBehaviour
{
    [SerializeField, Min(0)] private int storedSegments;
    [SerializeField, Min(1)] private int capacity = 14;

    public event Action<int> CountChanged;
    public event Action<int> CapacityChanged;

    public int StoredSegments => storedSegments;
    public int Capacity => Mathf.Max(1, capacity);
    public bool IsFull => storedSegments >= Capacity;
    public int RemainingCapacity =>
        Mathf.Max(0, Capacity - storedSegments);

    public void SetCount(int amount)
    {
        int newCount = Mathf.Clamp(amount, 0, Capacity);

        if (newCount == storedSegments)
        {
            return;
        }

        storedSegments = newCount;
        CountChanged?.Invoke(storedSegments);
    }

    public void SetCapacity(int amount)
    {
        int newCapacity = Mathf.Max(1, amount);

        if (newCapacity == Capacity)
        {
            return;
        }

        capacity = newCapacity;
        bool countClamped = storedSegments > newCapacity;

        if (countClamped)
        {
            storedSegments = newCapacity;
        }

        CapacityChanged?.Invoke(newCapacity);

        if (countClamped)
        {
            CountChanged?.Invoke(storedSegments);
        }
    }

    public bool TryAdd(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        if (amount == 0)
        {
            return true;
        }

        if (amount > RemainingCapacity ||
            storedSegments > int.MaxValue - amount)
        {
            return false;
        }

        storedSegments += amount;
        CountChanged?.Invoke(storedSegments);
        return true;
    }

    public void Add(int amount)
    {
        TryAdd(amount);
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0 || amount > storedSegments)
        {
            return false;
        }

        if (amount == 0)
        {
            return true;
        }

        storedSegments -= amount;
        CountChanged?.Invoke(storedSegments);
        return true;
    }
}
