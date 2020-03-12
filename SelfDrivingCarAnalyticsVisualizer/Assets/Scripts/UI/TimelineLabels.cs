using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelineLabels : MonoBehaviour
{
    [SerializeField]
    private int _labelsCount;

    [SerializeField]
    private GameObject _timelineLabel;

    private List<GameObject> _timelineLabels;

    private RectTransform _rectTransform;

    private void Start()
    {
        EventBus.Instance.OnTimelineChangeEvent.AddListener(OnTimelineChange);

        _timelineLabels = new List<GameObject>();
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnTimelineChange(float duration)
    {
        while (_timelineLabels.Count > 0)
        {
            Destroy(_timelineLabels[0]);
            _timelineLabels.RemoveAt(0);
        }

        float step = duration / (_labelsCount - 1);
        float time = 0;

        for (int i = 0; i < _labelsCount; i++)
        {
            GameObject label = Instantiate(_timelineLabel, transform);
            label.GetComponent<Text>().text = DateTimeHelper.GetTimelineLabelFromTime(time);
            label.GetComponent<RectTransform>().anchoredPosition = 
                new Vector2(Mathf.Lerp(0, _rectTransform.rect.width, time / duration), 0);
            _timelineLabels.Add(label);
            time += step;
        }
    }
}
