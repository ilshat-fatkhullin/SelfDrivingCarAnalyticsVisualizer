using UnityEngine;

public class FrameCollection<T> where T: Frame
{
    public float Frequency;

    public T[] Frames;

    public float GetDuration()
    {
        return Frequency * (Frames.Length - 1);
    }

    public T GetFrameAtTime(float time)
    {
        if (Frames == null || Frames.Length == 0)
            return null;

        return Frames[Mathf.Min(Mathf.RoundToInt(time / Frequency), Frames.Length - 1)];
    }
}
