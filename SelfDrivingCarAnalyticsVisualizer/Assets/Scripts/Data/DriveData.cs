public class DriveData
{
    public FrameCollection<PositionFrame> PositionFrames;

    public FrameCollection<SingleValueFrame> SpeedFrames;

    public FrameCollection<SingleValueFrame> AccelerationFrames;

    public FrameCollection<SingleValueFrame> PerceptionFrames;

    public float GetDuration()
    {
        return UnityEngine.Mathf.Max(
            PositionFrames.GetDuration(), 
            SpeedFrames.GetDuration(), 
            AccelerationFrames.GetDuration(),
            PerceptionFrames.GetDuration());
    }
}
