public class DriveData
{
    public FrameCollection<PositionFrame> PositionFrames;

    public FrameCollection<SingleValueFrame> SpeedFrames;

    public FrameCollection<SingleValueFrame> AccelerationFrames;

    public FrameCollection<SingleValueFrame> PerceptionFrames;

    public FrameCollection<PointCloudFrame> PointCloudFrames;

    public float GetDuration()
    {
        float position = 0, speed = 0, acceleration = 0, perception = 0;

        if (PositionFrames != null)
        {
            position = PositionFrames.GetDuration();
        }
        if (SpeedFrames != null)
        {
            speed = SpeedFrames.GetDuration();
        }
        if (AccelerationFrames != null)
        {
            acceleration = AccelerationFrames.GetDuration();
        }
        if (PerceptionFrames != null)
        {
            perception = PerceptionFrames.GetDuration();
        }

        return UnityEngine.Mathf.Max(
            position, 
            speed, 
            acceleration,
            perception);
    }
}
