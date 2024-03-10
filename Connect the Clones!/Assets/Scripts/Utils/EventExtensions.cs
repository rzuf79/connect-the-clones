public delegate void EventDelegate();
public delegate void EventDelegate<T>(T param);
public delegate void EventDelegate<T, U>(T param1, U param2);
public delegate void EventDelegate<T, U, V>(T param1, U param2, V param3);

public static class EventExtensions 
{
    public static void Fire(this EventDelegate eventDelegate)
    {
        eventDelegate?.Invoke();
    }
    
    public static void Fire<T>(this EventDelegate<T> eventDelegate, T param)
    {
        eventDelegate?.Invoke(param);
    }
    
    public static void Fire<T, U> (this EventDelegate<T, U>  eventDelegate, T param1, U param2)
    {
        eventDelegate?.Invoke(param1, param2);
    }
    
    public static void Fire<T, U, W> (this EventDelegate<T, U, W>  eventDelegate, T param1, U param2, W param3)
    {
        eventDelegate?.Invoke(param1, param2, param3);
    }
}
