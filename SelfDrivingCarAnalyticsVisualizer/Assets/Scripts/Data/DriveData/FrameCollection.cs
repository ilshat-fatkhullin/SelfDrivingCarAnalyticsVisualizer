﻿using UnityEngine;
using UnityEngine.XR;

public class FrameCollection<T> where T: Frame
{
    public T[] Frames;

    public float GetDuration()
    {
        return Frames[Frames.Length - 1].Timestamp - Frames[0].Timestamp;
    }

    public T GetNormalizedFrameAtTime(float time)
    {
        time += Frames[0].Timestamp;
        return GetFrameAtTime(time);
    }

    public T GetFrameAtTime(float time)
    {
        if (Frames == null || Frames.Length == 0 || time < 0)
            return null;

        if (Frames.Length == 1)
            return Frames[0];

        int low = 0;
        int high = Frames.Length - 1;

        int mid;
        while (low < high)
        {
            mid = (high + low) / 2;
            if (Frames[mid].Timestamp == time)
            {
                break;
            }
            else if (Frames[mid].Timestamp > time)
            {
                high = mid - 1;
            }
            else
            {
                low = mid + 1;
            }
        }
        mid = (high + low) / 2;

        return Frames[mid];
    }
}
