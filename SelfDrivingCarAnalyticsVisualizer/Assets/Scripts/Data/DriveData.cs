public class DriveData
{
    public FrameCollection<PositionFrame> PositionFrames;

    public FrameCollection<SpeedFrame> SpeedFrames;

    public FrameCollection<AccelerationFrame> AccelerationFrames;

    public float GetDuration()
    {
        return UnityEngine.Mathf.Max(PositionFrames.GetDuration(), SpeedFrames.GetDuration(), AccelerationFrames.GetDuration());
    }
}
