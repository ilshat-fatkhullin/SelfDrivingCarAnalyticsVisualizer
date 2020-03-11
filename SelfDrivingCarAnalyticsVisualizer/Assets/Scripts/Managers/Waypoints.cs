using UnityEngine;

public class Waypoints : MonoBehaviour
{
    private FrameCollection<PositionFrame> _frames;

    private void Start()
    {
        EventBus.Instance.OnDataLoad.AddListener(OnDataLoaded);
        EventBus.Instance.OnTimelineValueChange.AddListener(OnTimelineValueChanged);
    }

    private void OnDataLoaded(DriveData driveData)
    {
        _frames = driveData.PositionFrames;

        Vector3[] positions = new Vector3[driveData.PositionFrames.Frames.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = driveData.PositionFrames.Frames[i].ToVector();
        }

        EventBus.Instance.OnWaypointsUpdate.Invoke(positions);
        EventBus.Instance.OnCurrentWaypointChange.Invoke(positions[0]);
    }

    private void OnTimelineValueChanged(float value)
    {
        EventBus.Instance.OnCurrentWaypointChange.Invoke(_frames.GetFrameAtTime(value).ToVector());
    }
}
