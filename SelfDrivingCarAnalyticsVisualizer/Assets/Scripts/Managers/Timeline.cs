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
        _duration = driveData.GetDuration();
        EventBus.Instance.OnTimelineChangeEvent.Invoke(_duration);
    }
}
