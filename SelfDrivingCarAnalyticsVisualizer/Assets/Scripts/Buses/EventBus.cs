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

    public SplineBuilderInitializationEvent OnSplineBuilderInitialized;

    public FileLoadingEvent OnFileLoad;

    void Awake()
    {
        OnDataLoad = new DataLoadingEvent();
        OnTimelineChangeEvent = new TimelineChangeEvent();
        OnTimelineValueChange = new TimelineValueChangeEvent();
        OnWaypointsUpdate = new WaypointsUpdateEvent();
        OnCurrentWaypointChange = new CurrentWaypointChangeEvent();
        OnFileLoad = new FileLoadingEvent();
        OnSplineBuilderInitialized = new SplineBuilderInitializationEvent();
    }
}

public class DataLoadingEvent: UnityEvent<DriveData> { }

public class TimelineChangeEvent : UnityEvent<float> { }

public class TimelineValueChangeEvent : UnityEvent<float> { }

public class WaypointsUpdateEvent : UnityEvent<Vector3[]> { }

public class CurrentWaypointChangeEvent : UnityEvent<Vector3> { }

public class FileLoadingEvent: UnityEvent<string> { }

public class SplineBuilderInitializationEvent : UnityEvent<SplineBuilder> { }