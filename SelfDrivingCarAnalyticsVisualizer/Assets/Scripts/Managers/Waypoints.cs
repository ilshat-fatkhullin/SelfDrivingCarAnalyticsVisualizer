using UnityEngine;

public class Waypoints : MonoBehaviour
{
    [Range(0.25f, 2f)]
    [SerializeField]
    private float _step;

    private float _secondsPerFrame;

    private SplineBuilder _splineBuilder;

    private void Start()
    {
        EventBus.Instance.OnDataLoad.AddListener(OnDataLoaded);
        EventBus.Instance.OnTimelineValueChange.AddListener(OnTimelineValueChanged);
    }

    private void OnDataLoaded(DriveData driveData)
    {
        _secondsPerFrame = driveData.SecondsPerFrame;

        Vector3[] positions = new Vector3[driveData.Frames.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            Frame frame = driveData.Frames[i];
            positions[i] = new Vector3(frame.X, frame.Y, frame.Z);
        }

        SetWaypoints(positions);

        EventBus.Instance.OnCurrentWaypointChange.Invoke(_splineBuilder.GetSplineAtTime(0));
    }

    private void SetWaypoints(Vector3[] positions)
    {
        _splineBuilder = new SplineBuilder(positions);
        EventBus.Instance.OnWaypointsUpdate.Invoke(_splineBuilder.GetSplinePoints(positions, _step));
    }

    private void OnTimelineValueChanged(float value)
    {
        EventBus.Instance.OnCurrentWaypointChange.Invoke(_splineBuilder.GetSplineAtTime(value / _secondsPerFrame));
    }
}
