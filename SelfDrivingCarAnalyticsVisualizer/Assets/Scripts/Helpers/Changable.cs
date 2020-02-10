using UnityEngine.Events;

/// <summary>
/// Implements OnChange UnityEvent inside of it so there are no need for code duplication.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Changable<T>
{
    public UnityEvent OnChanged;

    public T Value
    {
        get
        {
            return value;
        }
        set
        {
            this.value = value;
            OnChanged.Invoke();
        }
    }

    T value;

    public Changable(T value)
    {
        this.value = value;
        OnChanged = new UnityEvent();
    }
}
