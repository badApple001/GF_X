using GameFramework;
using GameFramework.Event;

public class EnterGameEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(EnterGameEventArgs).GetHashCode();
    public override int Id => EventId;
    public override void Clear()
    {
    }
    public static EnterGameEventArgs Create()
    {
        var instance = ReferencePool.Acquire<EnterGameEventArgs>();
        return instance;
    }
}
