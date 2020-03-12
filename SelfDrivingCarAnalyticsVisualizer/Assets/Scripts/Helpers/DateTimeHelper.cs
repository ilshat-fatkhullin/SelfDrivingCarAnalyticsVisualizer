using UnityEngine;

public static class DateTimeHelper
{
    public static string GetTimelineLabelFromTime(float time)
    {
        int seconds = Mathf.FloorToInt(time % 60);
        int minutes = Mathf.FloorToInt(time / 60);
        return string.Format("{0:D2}", minutes) + ":" + string.Format("{0:D2}", seconds);
    }
}
