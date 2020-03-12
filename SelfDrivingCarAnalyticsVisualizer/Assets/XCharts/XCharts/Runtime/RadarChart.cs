﻿/******************************************/
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
    [AddComponentMenu("XCharts/RadarChart", 16)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public partial class RadarChart : BaseChart
    {
        private const string INDICATOR_TEXT = "indicator";
        [SerializeField] private List<Radar> m_Radars = new List<Radar>();
        private bool m_IsEnterLegendButtom;
        private bool m_RadarsDirty;

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

        protected override void Awake()
        {
            base.Awake();
            InitIndicator();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void CheckComponent()
        {
            var anyDirty = IsAnyRadarDirty();
            if (m_RadarsDirty || anyDirty)
            {
                InitIndicator();
                OnRadarChanged();
                RefreshChart();
                m_Tooltip.UpdateToTop();
                if (anyDirty)
                {
                    foreach (var radar in m_Radars)
                    {
                        radar.ClearDirty();
                    }
                }
            }
            base.CheckComponent();
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            RemoveData();
            m_Radars.Add(Radar.defaultRadar);
            m_Title.text = "RadarChart";
            var serie = AddSerie(SerieType.Radar, "serie1");
            serie.symbol.type = SerieSymbolType.EmptyCircle;
            serie.symbol.size = 4;
            serie.symbol.selectedSize = 6;
            serie.showDataName = true;
            List<float> data = new List<float>();
            for (int i = 0; i < 5; i++)
            {
                data.Add(Random.Range(20, 90));
            }
            AddData(0, data, "legendName");
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            m_RadarsDirty = true;
        }
#endif

        private void InitIndicator()
        {
            ChartHelper.HideAllObject(transform, INDICATOR_TEXT);
            for (int n = 0; n < m_Radars.Count; n++)
            {
                Radar radar = m_Radars[n];
                radar.UpdateRadarCenter(chartWidth, chartHeight);
                int indicatorNum = radar.indicatorList.Count;
                float txtWid = 100;
                float txtHig = 20;
                for (int i = 0; i < indicatorNum; i++)
                {
                    var indicator = radar.indicatorList[i];
                    var pos = radar.GetIndicatorPosition(i);
                    TextAnchor anchor = TextAnchor.MiddleCenter;
                    var textStyle = indicator.textStyle;
                    var textColor = textStyle.color == Color.clear ? (Color)m_ThemeInfo.axisTextColor : textStyle.color;
                    var txt = ChartHelper.AddTextObject(INDICATOR_TEXT + "_" + n + "_" + i, transform, m_ThemeInfo.font,
                    textColor, anchor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(txtWid, txtHig), textStyle.fontSize, textStyle.rotate, textStyle.fontStyle, textStyle.lineSpacing);
                    txt.text = radar.indicatorList[i].name;
                    txt.gameObject.SetActive(radar.indicator);
                    var txtWidth = txt.preferredWidth;
                    var sizeDelta = new Vector2(txt.preferredWidth, txt.preferredHeight);
                    txt.GetComponent<RectTransform>().sizeDelta = sizeDelta;
                    var diff = pos.x - radar.runtimeCenterPos.x;
                    if (diff < -1f) //left
                    {
                        pos = new Vector3(pos.x - txtWidth / 2, pos.y);
                    }
                    else if (diff > 1f) //right
                    {
                        pos = new Vector3(pos.x + txtWidth / 2, pos.y);
                    }
                    else
                    {
                        float y = pos.y > radar.runtimeCenterPos.y ? pos.y + txtHig / 2 : pos.y - txtHig / 2;
                        pos = new Vector3(pos.x, y);
                    }
                    txt.transform.localPosition = pos + new Vector3(textStyle.offset.x, textStyle.offset.y);
                }
            }
        }

        private bool IsAnyRadarDirty()
        {
            foreach (var radar in m_Radars)
            {
                if (radar.anyDirty) return true;
            }
            return false;
        }

        protected virtual void OnRadarChanged()
        {
        }

        protected override void DrawChart(VertexHelper vh)
        {
            base.DrawChart(vh);
            foreach (var radar in m_Radars)
            {
                radar.UpdateRadarCenter(chartWidth, chartHeight);
                if (radar.shape == Radar.Shape.Circle)
                {
                    DrawCricleRadar(vh, radar);
                }
                else
                {
                    DrawRadar(vh, radar);
                }
            }
            DrawData(vh);
        }

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();
            foreach (var radar in m_Radars)
            {
                radar.splitArea.color.Clear();
                switch (m_ThemeInfo.theme)
                {
                    case Theme.Dark:
                        radar.splitArea.color.Add(ThemeInfo.GetColor("#6f6f6f"));
                        radar.splitArea.color.Add(ThemeInfo.GetColor("#606060"));
                        break;
                    case Theme.Default:
                        radar.splitArea.color.Add(ThemeInfo.GetColor("#f6f6f6"));
                        radar.splitArea.color.Add(ThemeInfo.GetColor("#e7e7e7"));
                        break;
                    case Theme.Light:
                        radar.splitArea.color.Add(ThemeInfo.GetColor("#f6f6f6"));
                        radar.splitArea.color.Add(ThemeInfo.GetColor("#e7e7e7"));
                        break;
                }
            }
            m_RadarsDirty = true;
        }

        Dictionary<string, int> serieNameSet = new Dictionary<string, int>();
        private void DrawData(VertexHelper vh)
        {
            Vector3 startPoint = Vector3.zero;
            Vector3 toPoint = Vector3.zero;
            Vector3 firstPoint = Vector3.zero;

            serieNameSet.Clear();
            int serieNameCount = -1;
            for (int i = 0; i < m_Series.Count; i++)
            {
                var serie = m_Series.list[i];
                var radar = m_Radars[serie.radarIndex];
                int indicatorNum = radar.indicatorList.Count;
                var angle = 2 * Mathf.PI / indicatorNum;
                Vector3 p = radar.runtimeCenterPos;
                serie.animation.InitProgress(1, 0, 1);
                if (!IsActive(i) || serie.animation.HasFadeOut())
                {
                    continue;
                }
                var rate = serie.animation.GetCurrRate();
                var dataChanging = false;
                var dataChangeDuration = serie.animation.GetUpdateAnimationDuration();
                for (int j = 0; j < serie.data.Count; j++)
                {
                    var serieData = serie.data[j];
                    int key = i * 100 + j;
                    if (!radar.runtimeDataPosList.ContainsKey(key))
                    {
                        radar.runtimeDataPosList.Add(i * 100 + j, new List<Vector3>(serieData.data.Count));
                    }
                    else
                    {
                        radar.runtimeDataPosList[key].Clear();
                    }
                    string dataName = serieData.name;
                    int serieIndex = 0;
                    if (string.IsNullOrEmpty(dataName))
                    {
                        serieNameCount++;
                        serieIndex = serieNameCount;
                    }
                    else if (!serieNameSet.ContainsKey(dataName))
                    {
                        serieNameSet.Add(dataName, serieNameCount);
                        serieNameCount++;
                        serieIndex = serieNameCount;
                    }
                    else
                    {
                        serieIndex = serieNameSet[dataName];
                    }
                    if (!serieData.show)
                    {
                        continue;
                    }
                    var isHighlight = serie.highlighted || serieData.highlighted ||
                        (m_Tooltip.show && m_Tooltip.runtimeDataIndex[0] == i && m_Tooltip.runtimeDataIndex[1] == j);
                    var areaColor = serie.GetAreaColor(m_ThemeInfo, serieIndex, isHighlight);
                    var lineColor = serie.GetLineColor(m_ThemeInfo, serieIndex, isHighlight);
                    int dataCount = radar.indicatorList.Count;
                    List<Vector3> pointList = radar.runtimeDataPosList[key];
                    for (int n = 0; n < dataCount; n++)
                    {
                        if (n >= serieData.data.Count) break;
                        float min = radar.GetIndicatorMin(n);
                        float max = radar.GetIndicatorMax(n);
                        float value = serieData.GetCurrData(n, dataChangeDuration);
                        if (serieData.IsDataChanged()) dataChanging = true;
                        if (max == 0)
                        {
                            serie.GetMinMaxData(n, out min, out max);
                            min = radar.GetIndicatorMin(n);
                        }
                        var radius = max < 0 ? radar.runtimeDataRadius - radar.runtimeDataRadius * value / max
                        : radar.runtimeDataRadius * value / max;
                        var currAngle = (n + (radar.positionType == Radar.PositionType.Between ? 0.5f : 0)) * angle;
                        radius *= rate;
                        if (n == 0)
                        {
                            startPoint = new Vector3(p.x + radius * Mathf.Sin(currAngle),
                                p.y + radius * Mathf.Cos(currAngle));
                            firstPoint = startPoint;
                        }
                        else
                        {
                            toPoint = new Vector3(p.x + radius * Mathf.Sin(currAngle),
                                p.y + radius * Mathf.Cos(currAngle));
                            if (serie.areaStyle.show)
                            {
                                ChartDrawer.DrawTriangle(vh, p, startPoint, toPoint, areaColor);
                            }
                            if (serie.lineStyle.show)
                            {
                                DrawLineStyle(vh, serie.lineStyle, startPoint, toPoint, lineColor);
                            }
                            startPoint = toPoint;
                        }
                        pointList.Add(startPoint);
                    }
                    if (serie.areaStyle.show)
                    {
                        ChartDrawer.DrawTriangle(vh, p, startPoint, firstPoint, areaColor);
                    }
                    if (serie.lineStyle.show)
                    {
                        DrawLineStyle(vh, serie.lineStyle, startPoint, firstPoint, lineColor);
                    }
                    if (serie.symbol.type != SerieSymbolType.None)
                    {
                        var symbolSize = (isHighlight ? serie.symbol.selectedSize : serie.symbol.size);
                        var symbolColor = serie.symbol.color != Color.clear ? serie.symbol.color : lineColor;
                        symbolColor.a *= serie.symbol.opacity;
                        foreach (var point in pointList)
                        {
                            DrawSymbol(vh, serie.symbol.type, symbolSize, serie.lineStyle.width, point, symbolColor,
                                serie.symbol.gap);
                        }
                    }
                }
                if (!serie.animation.IsFinish())
                {
                    serie.animation.CheckProgress(1);
                    RefreshChart();
                }
                if (dataChanging)
                {
                    RefreshChart();
                }
            }
        }

        private void DrawRadar(VertexHelper vh, Radar radar)
        {
            if (!radar.splitLine.show && !radar.splitArea.show)
            {
                return;
            }
            float insideRadius = 0, outsideRadius = 0;
            float block = radar.runtimeRadius / radar.splitNumber;
            int indicatorNum = radar.indicatorList.Count;
            Vector3 p1, p2, p3, p4;
            Vector3 p = radar.runtimeCenterPos;
            float angle = 2 * Mathf.PI / indicatorNum;
            var lineColor = GetLineColor(radar);
            for (int i = 0; i < radar.splitNumber; i++)
            {
                Color color = radar.splitArea.color[i % radar.splitArea.color.Count];
                outsideRadius = insideRadius + block;
                p1 = new Vector3(p.x + insideRadius * Mathf.Sin(0), p.y + insideRadius * Mathf.Cos(0));
                p2 = new Vector3(p.x + outsideRadius * Mathf.Sin(0), p.y + outsideRadius * Mathf.Cos(0));
                for (int j = 0; j <= indicatorNum; j++)
                {
                    float currAngle = j * angle;
                    p3 = new Vector3(p.x + outsideRadius * Mathf.Sin(currAngle),
                        p.y + outsideRadius * Mathf.Cos(currAngle));
                    p4 = new Vector3(p.x + insideRadius * Mathf.Sin(currAngle),
                        p.y + insideRadius * Mathf.Cos(currAngle));
                    if (radar.splitArea.show)
                    {
                        ChartDrawer.DrawPolygon(vh, p1, p2, p3, p4, color);
                    }
                    if (radar.splitLine.NeedShow(i))
                    {
                        DrawLineStyle(vh, radar.splitLine.lineStyle, p2, p3, lineColor);
                    }
                    p1 = p4;
                    p2 = p3;
                }
                insideRadius = outsideRadius;
            }
            for (int j = 0; j <= indicatorNum; j++)
            {
                float currAngle = j * angle;
                p3 = new Vector3(p.x + outsideRadius * Mathf.Sin(currAngle),
                    p.y + outsideRadius * Mathf.Cos(currAngle));
                if (radar.splitLine.show)
                {
                    DrawLineStyle(vh, radar.splitLine.lineStyle, p, p3, lineColor);
                }
            }
        }

        private void DrawCricleRadar(VertexHelper vh, Radar radar)
        {
            if (!radar.splitLine.show && !radar.splitArea.show)
            {
                return;
            }
            float insideRadius = 0, outsideRadius = 0;
            float block = radar.runtimeRadius / radar.splitNumber;
            int indicatorNum = radar.indicatorList.Count;
            Vector3 p = radar.runtimeCenterPos;
            Vector3 p1;
            float angle = 2 * Mathf.PI / indicatorNum;
            var lineColor = GetLineColor(radar);
            for (int i = 0; i < radar.splitNumber; i++)
            {
                Color color = radar.splitArea.color[i % radar.splitArea.color.Count];
                outsideRadius = insideRadius + block;
                if (radar.splitArea.show)
                {
                    ChartDrawer.DrawDoughnut(vh, p, insideRadius, outsideRadius, color, Color.clear,
                         m_Settings.cicleSmoothness, 0, 360);
                }
                if (radar.splitLine.show)
                {
                    ChartDrawer.DrawEmptyCricle(vh, p, outsideRadius, radar.splitLine.lineStyle.width, lineColor,
                        Color.clear, m_Settings.cicleSmoothness);
                }
                insideRadius = outsideRadius;
            }
            for (int j = 0; j <= indicatorNum; j++)
            {
                float currAngle = j * angle;
                p1 = new Vector3(p.x + outsideRadius * Mathf.Sin(currAngle),
                    p.y + outsideRadius * Mathf.Cos(currAngle));
                if (radar.splitLine.show)
                {
                    ChartDrawer.DrawLine(vh, p, p1, radar.splitLine.lineStyle.width / 2, lineColor);
                }
            }
        }

        private Color GetLineColor(Radar radar)
        {
            return radar.splitLine.GetColor(m_ThemeInfo);
        }

        protected override void CheckTootipArea(Vector2 local)
        {
            if (m_IsEnterLegendButtom) return;

            bool highlight = false;
            for (int i = 0; i < m_Series.Count; i++)
            {
                if (!IsActive(i)) continue;
                var serie = m_Series.GetSerie(i);
                var radar = m_Radars[serie.radarIndex];
                var dist = Vector2.Distance(radar.runtimeCenterPos, local);
                if (dist > radar.runtimeRadius + serie.symbol.size)
                {
                    continue;
                }
                for (int n = 0; n < serie.data.Count; n++)
                {
                    var posKey = i * 100 + n;
                    if (radar.runtimeDataPosList.ContainsKey(posKey))
                    {
                        var posList = radar.runtimeDataPosList[posKey];
                        foreach (var pos in posList)
                        {
                            if (Vector2.Distance(pos, local) <= serie.symbol.size * 1.2f)
                            {
                                m_Tooltip.runtimeDataIndex[0] = i;
                                m_Tooltip.runtimeDataIndex[1] = n;
                                highlight = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (!highlight)
            {
                if (m_Tooltip.IsActive())
                {
                    m_Tooltip.SetActive(false);
                    RefreshChart();
                }
            }
            else
            {
                m_Tooltip.UpdateContentPos(new Vector2(local.x + 18, local.y - 25));
                UpdateTooltip();
                RefreshChart();
            }
        }

        protected override void UpdateTooltip()
        {
            base.UpdateTooltip();
            int serieIndex = m_Tooltip.runtimeDataIndex[0];
            if (serieIndex < 0)
            {
                if (m_Tooltip.IsActive())
                {
                    m_Tooltip.SetActive(false);
                    RefreshChart();
                }
                return;
            }
            m_Tooltip.SetActive(true);
            var serie = m_Series.GetSerie(serieIndex);
            var radar = m_Radars[serie.radarIndex];
            var serieData = serie.GetSerieData(m_Tooltip.runtimeDataIndex[1]);
            StringBuilder sb = new StringBuilder(serieData.name);
            for (int i = 0; i < radar.indicatorList.Count; i++)
            {
                string key = radar.indicatorList[i].name;
                float value = serieData.GetData(i);
                if ((i == 0 && !string.IsNullOrEmpty(serieData.name)) || i > 0) sb.Append("\n");
                sb.AppendFormat("{0}: {1}", key, ChartCached.FloatToStr(value, 0, m_Tooltip.forceENotation));
            }
            m_Tooltip.UpdateContentText(sb.ToString());
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
    }
}
