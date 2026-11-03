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
