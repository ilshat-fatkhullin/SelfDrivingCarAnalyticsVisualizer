using UnityEngine;
using XCharts;

public class SingleValuePlotBuilder : MonoBehaviour
{
    [SerializeField] private RectTransform _containerRectTransform;

    [SerializeField] private LineChart _lineChart;

    private void Awake()
    {
        _lineChart.SetSize(_containerRectTransform.rect.width, _containerRectTransform.rect.height);
        _lineChart.RemoveData();
        var serie = _lineChart.AddSerie(SerieType.Bar);
        serie.symbol.type = SerieSymbolType.None;
    }

    public void LoadData(FrameCollection<SingleValueFrame> data)
    {
        float time = 0;
        for (int i = 0; i < data.Frames.Length; i++)
        {            
            _lineChart.AddData(0, time, data.Frames[i].Value);
            _lineChart.AddXAxisData(DateTimeHelper.GetTimelineLabelFromTime(time));
            time = data.Frames[i].Timestamp;
        }
        _lineChart.RefreshChart();
    }
}
