﻿
/******************************************/
/*                                        */
/*     Copyright (c) 2018 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace XCharts
{
    [AddComponentMenu("XCharts/RingChart", 20)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public partial class RingChart : BaseChart
    {
        private bool m_UpdateTitleText = false;
        private bool m_UpdateLabelText = false;
        private bool m_IsEnterLegendButtom;

        protected override void Update()
        {
            base.Update();
            if (m_UpdateTitleText)
            {
                m_UpdateTitleText = false;
                TitleStyleHelper.UpdateTitleText(m_Series);
            }
            if (m_UpdateLabelText)
            {
                m_UpdateLabelText = false;
                SerieLabelHelper.UpdateLabelText(m_Series, m_ThemeInfo);
            }
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            m_Title.text = "RingChart";
            m_Tooltip.type = Tooltip.Type.Line;
            RemoveData();
            var serie = AddSerie(SerieType.Ring, "serie1");
            serie.roundCap = true;
            serie.radius = new float[] { 0.3f, 0.35f };
            serie.titleStyle.show = false;
            serie.titleStyle.textStyle.offset = new Vector2(0, 30);
            serie.label.show = true;
            serie.label.position = SerieLabel.Position.Center;
            serie.label.formatter = "{d:f0}%";
            serie.label.fontSize = 28;
            var value = Random.Range(30, 90);
            var max = 100;
            AddData(0, value, max, "data1");
        }
#endif

        protected override void DrawChart(VertexHelper vh)
        {
            base.DrawChart(vh);
            for (int i = 0; i < m_Series.list.Count; i++)
            {
                var serie = m_Series.list[i];
                var data = serie.data;
                serie.index = i;
                if (!serie.show || serie.type != SerieType.Ring || serie.animation.HasFadeOut())
                {
                    continue;
                }
                serie.animation.InitProgress(data.Count, serie.startAngle, serie.startAngle + 360);
                serie.UpdateCenter(chartWidth, chartHeight);
                TitleStyleHelper.CheckTitle(serie, ref m_ReinitTitle, ref m_UpdateTitleText);
                SerieLabelHelper.CheckLabel(serie, ref m_ReinitLabel, ref m_UpdateLabelText);
                var dataChangeDuration = serie.animation.GetUpdateAnimationDuration();
                var ringWidth = serie.runtimeOutsideRadius - serie.runtimeInsideRadius;
                var dataChanging = false;
                for (int j = 0; j < data.Count; j++)
                {
                    var serieData = data[j];
                    if (!serieData.show) continue;
                    if (serieData.IsDataChanged()) dataChanging = true;
                    var value = serieData.GetFirstData(dataChangeDuration);
                    var max = serieData.GetLastData();
                    var degree = 360 * value / max;
                    var startDegree = GetStartAngle(serie);
                    var toDegree = GetToAngle(serie, degree);
                    var itemColor = SerieHelper.GetItemColor(serie, m_ThemeInfo, j, serieData.highlighted);
                    var outsideRadius = serie.runtimeOutsideRadius - j * (ringWidth + serie.ringGap);
                    var insideRadius = outsideRadius - ringWidth;
                    var centerRadius = (outsideRadius + insideRadius) / 2;

                    serieData.runtimePieStartAngle = serie.clockwise ? startDegree : toDegree;
                    serieData.runtimePieToAngle = serie.clockwise ? toDegree : startDegree;
                    serieData.runtimePieInsideRadius = insideRadius;
                    serieData.runtimePieOutsideRadius = outsideRadius;

                    DrawBackground(vh, serie, j, insideRadius, outsideRadius);
                    DrawRoundCap(vh, serie, serie.runtimeCenterPos, itemColor, insideRadius, outsideRadius,
                        ref startDegree, ref toDegree);
                    ChartDrawer.DrawDoughnut(vh, serie.runtimeCenterPos, insideRadius,
                        outsideRadius, itemColor, Color.clear, m_Settings.cicleSmoothness,
                        startDegree, toDegree);
                    DrawBorder(vh, serie, insideRadius, outsideRadius);
                    DrawCenter(vh, serie, insideRadius, j == data.Count - 1);
                    UpateLabelPosition(serie, serieData, j, startDegree, toDegree, centerRadius);
                }
                if (!serie.animation.IsFinish())
                {
                    serie.animation.CheckProgress(360);
                    serie.animation.CheckSymbol(serie.symbol.size);
                    RefreshChart();
                }
                if (dataChanging)
                {
                    RefreshChart();
                }
            }
        }

        private float GetStartAngle(Serie serie)
        {
            return serie.clockwise ? serie.startAngle : 360 - serie.startAngle;
        }

        private float GetToAngle(Serie serie, float angle)
        {
            var toAngle = angle + serie.startAngle;
            if (!serie.clockwise)
            {
                toAngle = 360 - toAngle - serie.startAngle;
            }
            if (!serie.animation.IsFinish())
            {
                var currAngle = serie.animation.GetCurrDetail();
                if (serie.clockwise)
                {
                    toAngle = toAngle > currAngle ? currAngle : toAngle;
                }
                else
                {
                    toAngle = toAngle < 360 - currAngle ? 360 - currAngle : toAngle;
                }
            }
            return toAngle;
        }

        private void DrawCenter(VertexHelper vh, Serie serie, float insideRadius, bool last)
        {
            if (serie.itemStyle.centerColor != Color.clear && last)
            {
                var radius = insideRadius - serie.itemStyle.centerGap;
                ChartDrawer.DrawCricle(vh, serie.runtimeCenterPos, radius, serie.itemStyle.centerColor);
            }
        }

        private void UpateLabelPosition(Serie serie, SerieData serieData, int index, float startAngle,
            float toAngle, float centerRadius)
        {
            if (!serie.label.show) return;
            switch (serie.label.position)
            {
                case SerieLabel.Position.Center:
                    serieData.labelPosition = serie.runtimeCenterPos + serie.label.offset;
                    break;
                case SerieLabel.Position.Bottom:
                    var px1 = Mathf.Sin(startAngle * Mathf.Deg2Rad) * centerRadius;
                    var py1 = Mathf.Cos(startAngle * Mathf.Deg2Rad) * centerRadius;
                    var xDiff = serie.clockwise ? -serie.label.margin : serie.label.margin;
                    serieData.labelPosition = serie.runtimeCenterPos + new Vector3(px1 + xDiff, py1);
                    break;
                case SerieLabel.Position.Top:
                    startAngle += serie.clockwise ? -serie.label.margin : serie.label.margin;
                    toAngle += serie.clockwise ? serie.label.margin : -serie.label.margin;
                    var px2 = Mathf.Sin(toAngle * Mathf.Deg2Rad) * centerRadius;
                    var py2 = Mathf.Cos(toAngle * Mathf.Deg2Rad) * centerRadius;
                    serieData.labelPosition = serie.runtimeCenterPos + new Vector3(px2, py2);
                    break;
            }
        }

        private void DrawBackground(VertexHelper vh, Serie serie, int index, float insideRadius, float outsideRadius)
        {
            var backgroundColor = SerieHelper.GetItemBackgroundColor(serie, m_ThemeInfo, index, false);
            if (serie.itemStyle.backgroundWidth != 0)
            {
                var centerRadius = (outsideRadius + insideRadius) / 2;
                var inradius = centerRadius - serie.itemStyle.backgroundWidth / 2;
                var outradius = centerRadius + serie.itemStyle.backgroundWidth / 2;
                ChartDrawer.DrawDoughnut(vh, serie.runtimeCenterPos, inradius,
                    outradius, backgroundColor, Color.clear, m_Settings.cicleSmoothness);
            }
            else
            {
                ChartDrawer.DrawDoughnut(vh, serie.runtimeCenterPos, insideRadius,
                    outsideRadius, backgroundColor, Color.clear, m_Settings.cicleSmoothness);
            }
        }

        private void DrawBorder(VertexHelper vh, Serie serie, float insideRadius, float outsideRadius)
        {
            if (serie.itemStyle.show && serie.itemStyle.borderWidth > 0 && serie.itemStyle.borderColor != Color.clear)
            {
                ChartDrawer.DrawDoughnut(vh, serie.runtimeCenterPos, outsideRadius,
                outsideRadius + serie.itemStyle.borderWidth, serie.itemStyle.borderColor,
                Color.clear, m_Settings.cicleSmoothness);
                ChartDrawer.DrawDoughnut(vh, serie.runtimeCenterPos, insideRadius,
                insideRadius + serie.itemStyle.borderWidth, serie.itemStyle.borderColor,
                Color.clear, m_Settings.cicleSmoothness);
            }
        }


        private void DrawRoundCap(VertexHelper vh, Serie serie, Vector3 centerPos, Color color,
            float insideRadius, float outsideRadius, ref float drawStartDegree, ref float drawEndDegree)
        {
            if (serie.roundCap && insideRadius > 0 && drawStartDegree != drawEndDegree)
            {
                var width = (outsideRadius - insideRadius) / 2;
                var radius = insideRadius + width;

                var diffDegree = Mathf.Asin(width / radius) * Mathf.Rad2Deg;
                drawStartDegree += serie.clockwise ? diffDegree : -diffDegree;
                drawEndDegree -= serie.clockwise ? diffDegree : -diffDegree;
                ChartDrawer.DrawRoundCap(vh, centerPos, width, radius, drawStartDegree, serie.clockwise, color, false);
                ChartDrawer.DrawRoundCap(vh, centerPos, width, radius, drawEndDegree, serie.clockwise, color, true);
            }
        }

        protected override void OnLegendButtonClick(int index, string legendName, bool show)
        {
            CheckDataShow(legendName, show);
            UpdateLegendColor(legendName, show);
            RefreshChart();
        }

        protected override void OnLegendButtonEnter(int index, string legendName)
        {
            m_IsEnterLegendButtom = true;
            CheckDataHighlighted(legendName, true);
            RefreshChart();
        }

        protected override void OnLegendButtonExit(int index, string legendName)
        {
            m_IsEnterLegendButtom = false;
            CheckDataHighlighted(legendName, false);
            RefreshChart();
        }

        protected override void CheckTootipArea(Vector2 local)
        {
            if (m_IsEnterLegendButtom) return;
            m_Tooltip.runtimeDataIndex.Clear();
            bool selected = false;
            foreach (var serie in m_Series.list)
            {
                int index = GetRingIndex(serie, local);
                m_Tooltip.runtimeDataIndex.Add(index);
                if (serie.type != SerieType.Ring) continue;
                bool refresh = false;
                for (int j = 0; j < serie.data.Count; j++)
                {
                    var serieData = serie.data[j];
                    if (serieData.highlighted != (j == index)) refresh = true;
                    serieData.highlighted = j == index;
                }
                if (index >= 0) selected = true;
                if (refresh) RefreshChart();
            }
            if (selected)
            {
                m_Tooltip.UpdateContentPos(new Vector2(local.x + 18, local.y - 25));
                UpdateTooltip();
            }
            else if (m_Tooltip.IsActive())
            {
                m_Tooltip.SetActive(false);
                RefreshChart();
            }
        }

        private int GetRingIndex(Serie serie, Vector2 local)
        {
            if (serie.type != SerieType.Ring) return -1;
            var dist = Vector2.Distance(local, serie.runtimeCenterPos);
            if (dist > serie.runtimeOutsideRadius) return -1;
            Vector2 dir = local - new Vector2(serie.runtimeCenterPos.x, serie.runtimeCenterPos.y);
            float angle = VectorAngle(Vector2.up, dir);
            for (int i = 0; i < serie.data.Count; i++)
            {
                var serieData = serie.data[i];
                if (dist >= serieData.runtimePieInsideRadius &&
                    dist <= serieData.runtimePieOutsideRadius &&
                    angle >= serieData.runtimePieStartAngle &&
                    angle <= serieData.runtimePieToAngle)
                {
                    return i;
                }
            }
            return -1;
        }

        float VectorAngle(Vector2 from, Vector2 to)
        {
            float angle;

            Vector3 cross = Vector3.Cross(from, to);
            angle = Vector2.Angle(from, to);
            angle = cross.z > 0 ? -angle : angle;
            angle = (angle + 360) % 360;
            return angle;
        }

        StringBuilder sb = new StringBuilder();
        protected override void UpdateTooltip()
        {
            base.UpdateTooltip();
            bool showTooltip = false;
            foreach (var serie in m_Series.list)
            {
                int index = m_Tooltip.runtimeDataIndex[serie.index];
                if (index < 0) continue;
                showTooltip = true;
                if (tooltip.IsNoFormatter())
                {
                    var serieData = serie.GetSerieData(index);
                    float value = serieData.GetFirstData();
                    sb.Length = 0;
                    if (!string.IsNullOrEmpty(serieData.name))
                    {
                        sb.Append("<color=#").Append(m_ThemeInfo.GetColorStr(index)).Append(">● </color>")
                        .Append(serieData.name).Append(": ").Append(ChartCached.FloatToStr(value, 0, m_Tooltip.forceENotation));
                    }
                    else
                    {
                        sb.Append(ChartCached.FloatToStr(value, 0, m_Tooltip.forceENotation));
                    }
                    m_Tooltip.UpdateContentText(sb.ToString());
                }
                else
                {
                    m_Tooltip.UpdateContentText(m_Tooltip.GetFormatterContent(index, m_Series, null, m_ThemeInfo));
                }

                var pos = m_Tooltip.GetContentPos();
                if (pos.x + m_Tooltip.runtimeWidth > chartWidth)
                {
                    pos.x = chartWidth - m_Tooltip.runtimeWidth;
                }
                if (pos.y - m_Tooltip.runtimeHeight < 0)
                {
                    pos.y = m_Tooltip.runtimeHeight;
                }
                m_Tooltip.UpdateContentPos(pos);
            }
            m_Tooltip.SetActive(showTooltip);
        }
    }
}
