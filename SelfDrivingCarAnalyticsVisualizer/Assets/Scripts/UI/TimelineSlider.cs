using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TimelineSlider))]
public class TimelineSlider : MonoBehaviour
{
    private Slider _slider;

    private void Start()
    {
        EventBus.Instance.OnDataLoad.AddListener(OnDataLoaded);
        EventBus.Instance.OnTimelineChangeEvent.AddListener(OnTimelineChange);

        _slider = GetComponent<Slider>();
        _slider.interactable = false;
        _slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        EventBus.Instance.OnTimelineValueChange.Invoke(value);
    }

    private void OnDataLoaded(DriveData driveData)
    {
        _slider.interactable = true;
    }

    private void OnTimelineChange(float duration)
    {
        _slider.minValue = 0;
        _slider.maxValue = duration;
        _slider.value = 0;
    }
}
