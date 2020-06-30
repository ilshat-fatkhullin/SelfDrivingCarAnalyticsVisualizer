using UnityEngine;

public class Waypoints : MonoBehaviour
{
    [SerializeField]
    private float splineStep;

    private FrameCollection<PositionFrame> _frames;    

    private SplineBuilder splineBuilder;

    private void Start()
    {
        EventBus.Instance.OnDataLoad.AddListener(OnDataLoaded);
        EventBus.Instance.OnTimelineValueChange.AddListener(OnTimelineValueChanged);
    }

    private void OnDataLoaded(DriveData driveData)
    {
        if (driveData.PositionFrames == null)
        {
            return;
        }

        _frames = driveData.PositionFrames;

        Vector3[] positions = new Vector3[driveData.PositionFrames.Frames.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = driveData.PositionFrames.Frames[i].Point.ToVector();
        }

        splineBuilder = new SplineBuilder(positions);

        EventBus.Instance.OnWaypointsUpdate.Invoke(splineBuilder.GetSplinePoints(splineStep));
        EventBus.Instance.OnCurrentWaypointChange.Invoke(positions[0]);
    }

    private void OnTimelineValueChanged(float value)
    {
        float splineTime = _frames.GetSplineTimeAtNormalizedTime(value);
        EventBus.Instance.OnCurrentWaypointChange.Invoke(splineBuilder.GetSplineAtTime(splineTime));
    }
}
