﻿/******************************************/
/*                                        */
/*     Copyright (c) 2018 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace XCharts
{
    /// <summary>
    /// A data item of serie.
    /// 系列中的一个数据项。可存储数据名和1-n维的数据。
    /// </summary>
    [System.Serializable]
    public class SerieData : SubComponent
    {
        [SerializeField] private string m_Name;
        [SerializeField] private bool m_Selected;
        [SerializeField] private float m_Radius;
        [SerializeField] private IconStyle m_IconStyle = new IconStyle();
        [SerializeField] private bool m_EnableLabel = false;
        [SerializeField] private SerieLabel m_Label = new SerieLabel();
        [SerializeField] private List<float> m_Data = new List<float>();

        private bool m_Show = true;
        private bool m_LabelAutoSize;
        private float m_LabelPaddingLeftRight;
        private float m_LabelPaddingTopBottom;
        private float m_RtPieOutsideRadius;

        public int index { get; set; }
        /// <summary>
        /// the name of data item.
        /// 数据项名称。
        /// </summary>
        public string name { get { return m_Name; } set { m_Name = value; } }
        /// <summary>
        /// 数据项图例名称。当数据项名称不为空时，图例名称即为系列名称；反之则为索引index。
        /// </summary>
        /// <value></value>
        public string legendName { get { return string.IsNullOrEmpty(name) ? ChartCached.IntToStr(index) : name; } }
        /// <summary>
        /// 自定义半径。可用在饼图中自定义某个数据项的半径。
        /// </summary>
        public float radius { get { return m_Radius; } set { m_Radius = value; } }
        /// <summary>
        /// Whether the data item is selected.
        /// 该数据项是否被选中。
        /// </summary>
        public bool selected { get { return m_Selected; } set { m_Selected = value; } }
        /// <summary>
        /// the icon of data.
        /// 数据项图标样式。
        /// </summary>
        public IconStyle iconStyle { get { return m_IconStyle; } set { m_IconStyle = value; } }
        /// <summary>
        /// 是否启用单个数据项的标签设置。
        /// </summary>
        public bool enableLabel { get { return m_EnableLabel; } set { m_EnableLabel = value; } }
        /// <summary>
        /// 单个数据项的标签设置。
        /// </summary>
        public SerieLabel label { get { return m_Label; } set { m_Label = value; } }
        /// <summary>
        /// An arbitrary dimension data list of data item.
        /// 可指定任意维数的数值列表。
        /// </summary>
        public List<float> data { get { return m_Data; } set { m_Data = value; } }
        /// <summary>
        /// [default:true] Whether the data item is showed.
        /// 该数据项是否要显示。
        /// </summary>
        public bool show { get { return m_Show; } set { m_Show = value; } }
        /// <summary>
        /// Whether the data item is highlighted.
        /// 该数据项是否被高亮，一般由鼠标悬停或图例悬停触发高亮。
        /// </summary>
        public bool highlighted { get; set; }
        /// <summary>
        /// the label of data item.
        /// 该数据项的文本标签。
        /// </summary>
        public Text labelText { get; private set; }
        public RectTransform labelRect { get; private set; }
        /// <summary>
        /// 标志位置。
        /// </summary>
        public Vector3 labelPosition { get; set; }
        private bool m_CanShowLabel = true;
        /// <summary>
        /// 是否可以显示Label
        /// </summary>
        public bool canShowLabel { get { return m_CanShowLabel; } set { m_CanShowLabel = value; } }
        /// <summary>
        /// the maxinum value.
        /// 最大值。
        /// </summary>
        public float max { get { return m_Data.Max(); } }
        /// <summary>
        /// the mininum value.
        /// 最小值。
        /// </summary>
        public float min { get { return m_Data.Min(); } }

        /// <summary>
        /// 关联的gameObject
        /// </summary>
        public GameObject gameObject { get; private set; }
        /// <summary>
        /// 饼图数据项的开始角度（运行时自动计算）
        /// </summary>
        public float runtimePieStartAngle { get; internal set; }
        /// <summary>
        /// 饼图数据项的结束角度（运行时自动计算）
        /// </summary>
        public float runtimePieToAngle { get; internal set; }
        /// <summary>
        /// 饼图数据项的一半时的角度（运行时自动计算）
        /// </summary>
        public float runtimePieHalfAngle { get; internal set; }
        /// <summary>
        /// 饼图数据项的当前角度（运行时自动计算）
        /// </summary>
        public float runtimePieCurrAngle { get; internal set; }
        /// <summary>
        /// 饼图数据项的内半径
        /// </summary>
        public float runtimePieInsideRadius { get; internal set; }
        /// <summary>
        /// 饼图数据项的外半径
        /// </summary>
        public float runtimePieOutsideRadius
        {
            get
            {
                if (radius > 0) return radius;
                else return m_RtPieOutsideRadius;
            }
            internal set
            {
                m_RtPieOutsideRadius = value;
            }
        }
        /// <summary>
        /// 饼图数据项的偏移半径
        /// </summary>
        public float runtimePieOffsetRadius { get; internal set; }
        public Vector3 runtiemPieOffsetCenter { get; internal set; }

        private List<float> m_PreviousData = new List<float>();
        private List<float> m_DataUpdateTime = new List<float>();
        private List<bool> m_DataUpdateFlag = new List<bool>();

        public float GetData(int index)
        {
            if (index >= 0 && index < m_Data.Count)
            {
                return m_Data[index];
            }
            else return 0;
        }

        public float GetPreviousData(int index)
        {
            if (index >= 0 && index < m_PreviousData.Count)
            {
                return m_PreviousData[index];
            }
            else return 0;
        }

        public float GetFirstData(float animationDuration = 500f)
        {
            if (m_Data.Count > 0) return GetCurrData(0, animationDuration);
            return 0;
        }

        public float GetLastData()
        {
            if (m_Data.Count > 0) return m_Data[m_Data.Count - 1];
            return 0;
        }

        public float GetCurrData(int index, float animationDuration = 500f)
        {
            if (index < m_DataUpdateFlag.Count && m_DataUpdateFlag[index] && animationDuration > 0)
            {
                var time = Time.time - m_DataUpdateTime[index];
                var total = animationDuration / 1000;
                if (time <= total)
                {
                    CheckLastData();
                    var curr = Mathf.Lerp(GetPreviousData(index), GetData(index), time / total);
                    return curr;
                }
                else
                {
                    m_DataUpdateFlag[index] = false;
                    return GetData(index);
                }
            }
            else
            {
                return GetData(index);
            }
        }

        public bool UpdateData(int dimension, float value)
        {
            if (dimension >= 0 && dimension < data.Count)
            {
                CheckLastData();
                m_PreviousData[dimension] = data[dimension];
                m_DataUpdateTime[dimension] = Time.time;
                m_DataUpdateFlag[dimension] = true;
                data[dimension] = value;
                return true;
            }
            return false;
        }

        private void CheckLastData()
        {
            if (m_PreviousData.Count != m_Data.Count)
            {
                m_PreviousData.Clear();
                m_DataUpdateTime.Clear();
                m_DataUpdateFlag.Clear();
                for (int i = 0; i < m_Data.Count; i++)
                {
                    m_PreviousData.Add(m_Data[i]);
                    m_DataUpdateTime.Add(Time.time);
                    m_DataUpdateFlag.Add(false);
                }
            }
        }

        public bool IsDataChanged()
        {
            foreach (var b in m_DataUpdateFlag)
                if (b) return true;
            return false;
        }

        public void InitLabel(GameObject labelObj, bool autoSize, float paddingLeftRight, float paddingTopBottom)
        {
            gameObject = labelObj;
            m_LabelAutoSize = autoSize;
            m_LabelPaddingLeftRight = paddingLeftRight;
            m_LabelPaddingTopBottom = paddingTopBottom;
            labelText = labelObj.GetComponentInChildren<Text>();
            labelRect = labelText.GetComponent<RectTransform>();
        }

        public void SetLabelActive(bool active)
        {
            if (labelRect)
            {
                ChartHelper.SetActive(labelRect, active);
            }
        }

        public bool SetLabelText(string text)
        {
            if (labelText && !labelText.text.Equals(text))
            {
                labelText.text = text;
                if (m_LabelAutoSize)
                {
                    var newSize = string.IsNullOrEmpty(text) ? Vector2.zero :
                        new Vector2(labelText.preferredWidth + m_LabelPaddingLeftRight * 2,
                                        labelText.preferredHeight + m_LabelPaddingTopBottom * 2);
                    var sizeChange = newSize.x != labelRect.sizeDelta.x || newSize.y != labelRect.sizeDelta.y;
                    if (sizeChange) labelRect.sizeDelta = newSize;
                    return sizeChange;
                }
            }
            return false;
        }

        public void SetLabelColor(Color color)
        {
            if (labelText)
            {
                labelText.color = color;
            }
        }

        public float GetLabelWidth()
        {
            if (labelRect) return labelRect.sizeDelta.x;
            else return 0;
        }

        public float GetLabelHeight()
        {
            if (labelRect) return labelRect.sizeDelta.y;
            return 0;
        }

        public void SetGameObjectPosition(Vector3 position)
        {
            if (gameObject)
            {
                gameObject.transform.localPosition = position;
            }
        }

        public void SetLabelPosition(Vector3 position)
        {
            if (labelRect) labelRect.localPosition = position;
        }

        [Obsolete("Use SerieData.SetIconImage() instead.", true)]
        public void SetIconObj(GameObject iconObj) { }

        public void SetIconImage(Image image)
        {
            if (iconStyle == null) return;
            iconStyle.SetImage(image);
        }

        public void UpdateIcon()
        {
            if (iconStyle == null) return;
            iconStyle.UpdateIcon();
        }

        public bool IsInitLabel()
        {
            return labelText != null;
        }

        public SerieLabel GetSerieLabel(SerieLabel parentLabel)
        {
            if (enableLabel) return label;
            else return parentLabel;
        }
    }
}
