using System;
using System.Collections.Generic;

public class Observable<T>
{
    private T _value;

    public T Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value))
            {
                return;
            }

            _value = value;
            OnValueChanged?.Invoke(_value);
        }
    }

    public event Action<T> OnValueChanged;

    public Observable(T initial = default) => _value = initial;
}