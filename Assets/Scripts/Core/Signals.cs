using System;
using UnityEngine;

public class BaseSignal { }

public class Signal : BaseSignal
{
    private Action listeners;
    
    public void AddListener(Action callback)
    {
        listeners -=  callback;
        listeners += callback;
    }

    public void RemoveListener(Action callback)
    {
        listeners -=  callback;
    }
    
    /// <summary>
    /// Phát signal không tham số tới tất cả listener đang đăng ký. Không có listener thì không làm gì.
    /// </summary>
    public void Dispatch()
    {
        listeners?.Invoke();
    }

    public void RemoveAllListeners()
    {
        listeners = null;
    }
}

public class Signal<T> : BaseSignal
{
    private Action<T> listeners;

    public void AddListener(Action<T> callback)
    {
        listeners -=  callback;
        listeners += callback;
    }

    public void RemoveListener(Action<T> callback)
    {
        listeners -= callback;
    }
    /// <summary>
    /// Phát signal một tham số tới tất cả listener đang đăng ký. Không có listener thì không làm gì.
    /// </summary>
    /// <param name="value">Giá trị gửi tới listener.</param>
    public void Dispatch(T value) {
        listeners?.Invoke(value);
    }

    public void RemoveAllListeners() {
        listeners = null;
    }
}

public class Signal<T, U> : BaseSignal
{
    private Action<T, U> listeners;

    public void AddListener(Action<T, U> callback)
    {
        listeners -= callback;
        listeners += callback;
    }

    public void RemoveListener(Action<T, U> callback)
    {
        listeners -= callback;
    }

    public void RemoveAllListeners()
    {
        listeners = null;
    }
}
