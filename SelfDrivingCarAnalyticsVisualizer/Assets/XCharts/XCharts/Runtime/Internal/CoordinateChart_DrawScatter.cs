/******************************************/
/*                                        */
/*     Copyright (c) 2018 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using UnityEngine;
using UnityEngine.UI;

namespace XCharts
{
    public partial class CoordinateChart
    {
        protected void DrawScatterSerie(VertexHelper vh, int colorIndex, Serie serie)
        {
            if (serie.animation.HasFadeOut()) return;
            var yAxis = m_YAxises[serie.axisIndex];
            var xAxis = m_XAxises[serie.axisIndex];
            var color = serie.symbol.color != Color.clear ? serie.symbol.color : (Color)m_ThemeInfo.GetColor(colorIndex);
            color.a *= serie.symbol.opacity;
            int maxCount = serie.maxShow > 0 ?
                (serie.maxShow > serie.dataCount ? serie.dataCount : serie.maxShow)
                : serie.dataCount;
            serie.animation.InitProgress(1, 0, 1);
            var rate = serie.animation.GetCurrRate();
            var dataChangeDuration = serie.animation.GetUpdateAnimationDuration();
            var dataChanging = false;
            for (int n = serie.minShow; n < maxCount; n++)
            {
                var serieData = serie.GetDataList(m_DataZoom)[n];
                float xValue = serieData.GetCurrData(0, dataChangeDuration);
                float yValue = serieData.GetCurrData(1, dataChangeDuration);
                if (serieData.IsDataChanged()) dataChanging = true;
                float pX = coordinateX + xAxis.axisLine.width;
                float pY = coordinateY + yAxis.axisLine.width;
                float xDataHig = (xValue - xAxis.runtimeMinValue) / (xAxis.runtimeMaxValue - xAxis.runtimeMinValue) * coordinateWidth;
                float yDataHig = (yValue - yAxis.runtimeMinValue) / (yAxis.runtimeMaxValue - yAxis.runtimeMinValue) * coordinateHeight;
                var pos = new Vector3(pX + xDataHig, pY + yDataHig);

                var datas = serie.data[n].data;
                float symbolSize = 0;
                if (serie.highlighted || serieData.highlighted)
                {
                    symbolSize = serie.symbol.GetSelectedSize(datas);
                }
                else
                {
                    symbolSize = serie.symbol.GetSize(datas);
                }
                symbolSize *= rate;
                if (symbolSize > 100) symbolSize = 100;
                if (serie.type == SerieType.EffectScatter)
                {
                    for (int count = 0; count < serie.symbol.animationSize.Count; count++)
                    {
                        var nowSize = serie.symbol.animationSize[count];
                        color.a = (symbolSize - nowSize) / symbolSize;
                        DrawSymbol(vh, serie.symbol.type, nowSize, 3, pos, color, serie.symbol.gap);
                    }
                    RefreshChart();
                }
                else
                {
                    DrawSymbol(vh, serie.symbol.type, symbolSize, 3, pos, color, serie.symbol.gap);
                }
            }
            if (!serie.animation.IsFinish())
            {
                serie.animation.CheckProgress(1);
                m_IsPlayingAnimation = true;
                RefreshChart();
            }
            if (dataChanging)
            {
                RefreshChart();
            }
        }
    }
}