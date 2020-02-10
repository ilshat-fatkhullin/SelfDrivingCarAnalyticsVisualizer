using UnityEngine;
using UnityEngine.Events;

public class EventBus: MonoBehaviour
{
    public static EventBus Instance { get { return Singleton<EventBus>.Instance; } }

    public DataLoadingEvent OnDataLoad;

    public TimelineChangeEvent OnTimelineChangeEvent;

    public TimelineValueChangeEvent OnTimelineValueChange;

    public WaypointsUpdateEvent OnWaypointsUpdate;

    public CurrentWaypointChangeEvent OnCurrentWaypointChange;

    void Awake()
    {
        OnDataLoad = new DataLoadingEvent();
        OnTimelineChangeEvent = new TimelineChangeEvent();
        OnTimelineValueChange = new TimelineValueChangeEvent();
        OnWaypointsUpdate = new WaypointsUpdateEvent();
        OnCurrentWaypointChange = new CurrentWaypointChangeEvent();
    }
}

public class DataLoadingEvent: UnityEvent<DriveData> { }

public class TimelineChangeEvent : UnityEvent<float> { }

public class TimelineValueChangeEvent : UnityEvent<float> { }

public class WaypointsUpdateEvent : UnityEvent<Vector3[]> { }

public class CurrentWaypointChangeEvent : UnityEvent<Vector3> { }