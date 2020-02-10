using UnityEngine;

public class Timeline : MonoBehaviour
{
    private float _duration;

    private void Start()
    {
        EventBus.Instance.OnDataLoad.AddListener(OnDataLoaded);
    }

    private void OnDataLoaded(DriveData driveData)
    {
        _duration = driveData.SecondsPerFrame * (driveData.Frames.Length - 1);
        EventBus.Instance.OnTimelineChangeEvent.Invoke(_duration);
    }
}
