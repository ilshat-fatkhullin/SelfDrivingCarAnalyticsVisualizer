using UnityEngine;

public class Statistics : MonoBehaviour
{
    [SerializeField] private SingleValuePlotBuilder Speed;

    [SerializeField] private SingleValuePlotBuilder Acceleration;

    [SerializeField] private SingleValuePlotBuilder Perception;

    private void Start()
    {
        EventBus.Instance.OnDataLoad.AddListener(OnDataLoad);
    }

    private void OnDataLoad(DriveData driveData)
    {
        Speed.LoadData(driveData.SpeedFrames);
        Acceleration.LoadData(driveData.AccelerationFrames);
        Perception.LoadData(driveData.PerceptionFrames);
    }
}
