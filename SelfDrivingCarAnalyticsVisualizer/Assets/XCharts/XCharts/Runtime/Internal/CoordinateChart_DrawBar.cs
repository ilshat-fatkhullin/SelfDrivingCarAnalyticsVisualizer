/******************************************/
/*                                        */
/*     Copyright (c) 2018 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XCharts
{
    public partial class CoordinateChart
    {
        protected float m_BarLastOffset = 0;

        protected void DrawYBarSerie(VertexHelper vh, Serie serie, int colorIndex, ref List<float> seriesHig)
        {
            if (!IsActive(serie.name)) return;
            if (serie.animation.HasFadeOut()) return;
            var xAxis = m_XAxises[serie.axisIndex];
            var yAxis = m_YAxises[serie.axisIndex];
            if (!yAxis.show) yAxis = m_YAxises[(serie.axisIndex + 1) % m_YAxises.Count];

            var showData = serie.GetDataList(m_DataZoom);
            float categoryWidth = yAxis.GetDataWidth(coordinateHeight, showData.Count, m_DataZoom);
            float barGap = GetBarGap();
            float totalBarWidth = GetBarTotalWidth(categoryWidth, barGap);
            float barWidth = serie.GetBarWidth(categoryWidth);
            float offset = (categoryWidth - totalBarWidth) / 2;
            float barGapWidth = barWidth + barWidth * barGap;
            float space = serie.barGap == -1 ? offset : offset + m_BarLastOffset;


            int maxCount = serie.maxShow > 0 ?
                (serie.maxShow > showData.Count ? showData.Count : serie.maxShow)
                : showData.Count;
            if (seriesHig.Count < serie.minShow)
            {
                for (int i = 0; i < serie.minShow; i++)
                {
                    seriesHig.Add(0);
                }
            }
            var isPercentStack = m_Series.IsPercentStack(serie.stack, SerieType.Bar);
            bool dataChanging = false;
            float dataChangeDuration = serie.animation.GetUpdateAnimationDuration();
            float xMinValue = xAxis.GetCurrMinValue(dataChangeDuration);
            float xMaxValue = xAxis.GetCurrMaxValue(dataChangeDuration);
            float borderWidth = serie.itemStyle.runtimeBorderWidth;
            for (int i = serie.minShow; i < maxCount; i++)
            {
                if (i >= seriesHig.Count)
                {
                    seriesHig.Add(0);
                }
                var serieData = showData[i];
                serieData.canShowLabel = true;
                float value = showData[i].GetCurrData(1, dataChangeDuration);
                if (showData[i].IsDataChanged()) dataChanging = true;
                float pX = seriesHig[i] + coordinateX + xAxis.runtimeZeroXOffset + yAxis.axisLine.width;
                float pY = coordinateY + +i * categoryWidth;
                if (!yAxis.boundaryGap) pY -= categoryWidth / 2;

                var barHig = 0f;
                var valueTotal = 0f;
                if (isPercentStack)
                {
                    valueTotal = GetSameStackTotalValue(serie.stack, i);
                    barHig = valueTotal != 0 ? (value / valueTotal * coordinateWidth) : 0;
                    seriesHig[i] += barHig;
                }
                else
                {
                    valueTotal = xMaxValue - xMinValue;
                    if (valueTotal != 0)
                        barHig = (xMinValue > 0 ? value - xMinValue : value)
                            / valueTotal * coordinateWidth;
                    seriesHig[i] += barHig;
                }

                float currHig = CheckAnimation(serie, i, barHig);

                Vector3 p1 = new Vector3(pX + borderWidth, pY + space + barWidth - borderWidth);
                Vector3 p2 = new Vector3(pX + currHig - 2 * borderWidth, pY + space + barWidth - borderWidth);
                Vector3 p3 = new Vector3(pX + currHig - 2 * borderWidth, pY + space + borderWidth);
                Vector3 p4 = new Vector3(pX + borderWidth, pY + space + borderWidth);
                Vector3 top = new Vector3(pX + currHig - borderWidth, pY + space + barWidth / 2);
                p1 = ClampInCoordinate(p1);
                p2 = ClampInCoordinate(p2);
                p3 = ClampInCoordinate(p3);
                p4 = ClampInCoordinate(p4);
                top = ClampInCoordinate(top);
                serie.dataPoints.Add(top);
                var highlight = (m_Tooltip.show && m_Tooltip.IsSelected(i))
                    || serie.data[i].highlighted
                    || serie.highlighted;
                if (serie.show)
                {
                    Color areaColor = serie.GetAreaColor(m_ThemeInfo, colorIndex, highlight);
                    Color areaToColor = serie.GetAreaToColor(m_ThemeInfo, colorIndex, highlight);
                    if (serie.barType == BarType.Zebra)
                    {
                        p1 = (p4 + p1) / 2;
                        p2 = (p2 + p3) / 2;
                        CheckClipAndDrawZebraLine(vh, p1, p2, barWidth / 2, serie.barZebraWidth, serie.barZebraGap,
                            areaColor, serie.clip);
                    }
                    else
                    {
                        CheckClipAndDrawPolygon(vh, p4, p1, p2, p3, areaColor, areaToColor, serie.clip);
                        if (borderWidth > 0)
                        {
                            var borderColor = serie.itemStyle.borderColor;
                            var itemWidth = Mathf.Abs(p3.x - p1.x);
                            var itemHeight = Mathf.Abs(p2.y - p4.y);
                            var center = new Vector3((p1.x + p3.x) / 2, (p2.y + p4.y) / 2);
                            ChartDrawer.DrawBorder(vh, center, itemWidth, itemHeight, borderWidth, borderColor);
                        }
                    }
                }
            }
            if (!m_Series.IsStack(serie.stack, SerieType.Bar))
            {
                m_BarLastOffset += barGapWidth;
            }
            if (dataChanging)
            {
                RefreshChart();
            }
        }

        private float CheckAnimation(Serie serie, int dataIndex, float barHig)
        {
            float currHig = serie.animation.CheckBarProgress(dataIndex, barHig);
            if (!serie.animation.IsFinish())
            {
                RefreshChart();
                m_IsPlayingAnimation = true;
            }
            return currHig;
        }

        protected void DrawXBarSerie(VertexHelper vh, Serie serie, int colorIndex, ref List<float> seriesHig)
        {
            if (!IsActive(serie.name)) return;
            if (serie.animation.HasFadeOut()) return;
            var showData = serie.GetDataList(m_DataZoom);
            var yAxis = m_YAxises[serie.axisIndex];
            var xAxis = m_XAxises[serie.axisIndex];
            if (!xAxis.show) xAxis = m_XAxises[(serie.axisIndex + 1) % m_XAxises.Count];

            float categoryWidth = xAxis.GetDataWidth(coordinateWidth, showData.Count, m_DataZoom);
            float barGap = GetBarGap();
            float totalBarWidth = GetBarTotalWidth(categoryWidth, barGap);
            float barWidth = serie.GetBarWidth(categoryWidth);
            float offset = (categoryWidth - totalBarWidth) / 2;
            float barGapWidth = barWidth + barWidth * barGap;
            float space = serie.barGap == -1 ? offset : offset + m_BarLastOffset;
            int maxCount = serie.maxShow > 0 ?
                (serie.maxShow > showData.Count ? showData.Count : serie.maxShow)
                : showData.Count;

            if (seriesHig.Count < serie.minShow)
            {
                for (int i = 0; i < serie.minShow; i++)
                {
                    seriesHig.Add(0);
                }
            }

            var isPercentStack = m_Series.IsPercentStack(serie.stack, SerieType.Bar);
            bool dataChanging = false;
            float dataChangeDuration = serie.animation.GetUpdateAnimationDuration();
            float yMinValue = yAxis.GetCurrMinValue(dataChangeDuration);
            float yMaxValue = yAxis.GetCurrMaxValue(dataChangeDuration);
            float borderWidth = serie.itemStyle.runtimeBorderWidth;
            for (int i = serie.minShow; i < maxCount; i++)
            {
                if (i >= seriesHig.Count)
                {
                    seriesHig.Add(0);
                }
                float value = showData[i].GetCurrData(1, dataChangeDuration);
                if (showData[i].IsDataChanged()) dataChanging = true;
                float pX = coordinateX + i * categoryWidth;
                float zeroY = coordinateY + yAxis.runtimeZeroYOffset;
                if (!xAxis.boundaryGap) pX -= categoryWidth / 2;
                float pY = seriesHig[i] + zeroY + xAxis.axisLine.width;

                var barHig = 0f;
                var valueTotal = 0f;
                if (isPercentStack)
                {
                    valueTotal = GetSameStackTotalValue(serie.stack, i);
                    barHig = valueTotal != 0 ? (value / valueTotal * coordinateHeight) : 0;
                    seriesHig[i] += barHig;
                }
                else
                {
                    valueTotal = yMaxValue - yMinValue;
                    if (valueTotal != 0)
                        barHig = (yMinValue > 0 ? value - yMinValue : value)
                            / valueTotal * coordinateHeight;
                    seriesHig[i] += barHig;
                }

                float currHig = CheckAnimation(serie, i, barHig);
                Vector3 p1 = new Vector3(pX + space + borderWidth, pY + borderWidth);
                Vector3 p2 = new Vector3(pX + space + borderWidth, pY + currHig - 2 * borderWidth);
                Vector3 p3 = new Vector3(pX + space + barWidth, pY + currHig - 2 * borderWidth);
                Vector3 p4 = new Vector3(pX + space + barWidth, pY + borderWidth);
                Vector3 top = new Vector3(pX + space + barWidth / 2, pY + currHig - borderWidth);
                p1 = ClampInCoordinate(p1);
                p2 = ClampInCoordinate(p2);
                p3 = ClampInCoordinate(p3);
                p4 = ClampInCoordinate(p4);
                top = ClampInCoordinate(top);
                serie.dataPoints.Add(top);
                var highlight = (m_Tooltip.show && m_Tooltip.IsSelected(i))
                    || serie.data[i].highlighted
                    || serie.highlighted;
                if (serie.show)
                {
                    Color areaColor = serie.GetAreaColor(m_ThemeInfo, colorIndex, highlight);
                    Color areaToColor = serie.GetAreaToColor(m_ThemeInfo, colorIndex, highlight);
                    if (serie.barType == BarType.Zebra)
                    {
                        p1 = (p4 + p1) / 2;
                        p2 = (p2 + p3) / 2;
                        CheckClipAndDrawZebraLine(vh, p1, p2, barWidth / 2, serie.barZebraWidth, serie.barZebraGap,
                            areaColor, serie.clip);
                    }
                    else
                    {
                        CheckClipAndDrawPolygon(vh, ref p4, ref p1, ref p2, ref p3, areaColor, areaToColor, serie.clip);
                        if (borderWidth > 0)
                        {
                            var borderColor = serie.itemStyle.borderColor;
                            var itemWidth = Mathf.Abs(p3.x - p1.x);
                            var itemHeight = Mathf.Abs(p2.y - p4.y);
                            var center = new Vector3((p1.x + p3.x) / 2, (p2.y + p4.y) / 2);
                            ChartDrawer.DrawBorder(vh, center, itemWidth, itemHeight, borderWidth, borderColor);
                        }
                    }
                }
            }
            if (dataChanging)
            {
                RefreshChart();
            }
            if (!m_Series.IsStack(serie.stack, SerieType.Bar))
            {
                m_BarLastOffset += barGapWidth;
            }
        }

        private float GetBarGap()
        {
            float gap = 0.3f;
            for (int i = 0; i < m_Series.Count; i++)
            {
                var serie = m_Series.list[i];
                if (serie.type == SerieType.Bar)
                {
                    if (serie.barGap != 0)
                    {
                        gap = serie.barGap;
                    }
                }
            }
            return gap;
        }

        private float GetSameStackTotalValue(string stack, int dataIndex)
        {
            if (string.IsNullOrEmpty(stack)) return 0;
            float total = 0;
            foreach (var serie in m_Series.list)
            {
                if (serie.type == SerieType.Bar)
                {
                    if (stack.Equals(serie.stack))
                    {
                        total += serie.data[dataIndex].data[1];
                    }
                }
            }
            return total;
        }


        private HashSet<string> barStackSet = new HashSet<string>();
        private float GetBarTotalWidth(float categoryWidth, float gap)
        {
            float total = 0;
            float lastGap = 0;
            barStackSet.Clear();
            for (int i = 0; i < m_Series.Count; i++)
            {
                var serie = m_Series.list[i];
                if (serie.type == SerieType.Bar && serie.show)
                {
                    if (!string.IsNullOrEmpty(serie.stack))
                    {
                        if (barStackSet.Contains(serie.stack)) continue;
                        barStackSet.Add(serie.stack);
                    }
                    var width = GetStackBarWidth(categoryWidth, serie);
                    if (gap == -1)
                    {
                        if (width > total) total = width;
                    }
                    else
                    {
                        lastGap = width * gap;
                        total += width;
                        total += lastGap;
                    }
                }
            }
            if (total > 0 && gap != -1) total -= lastGap;
            return total;
        }

        private float GetStackBarWidth(float categoryWidth, Serie now)
        {
            if (string.IsNullOrEmpty(now.stack)) return now.GetBarWidth(categoryWidth);
            float barWidth = 0;
            for (int i = 0; i < m_Series.Count; i++)
            {
                var serie = m_Series.list[i];
                if (serie.type == SerieType.Bar && serie.show && now.stack.Equals(serie.stack))
                {
                    if (serie.barWidth > barWidth) barWidth = serie.barWidth;
                }
            }
            if (barWidth > 1) return barWidth;
            else return barWidth * categoryWidth;
        }
    }
}