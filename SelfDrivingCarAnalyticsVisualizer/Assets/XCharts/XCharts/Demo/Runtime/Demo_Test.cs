﻿/******************************************/
/*                                        */
/*     Copyright (c) 2018 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using UnityEngine;
using UnityEngine.UI;

namespace XCharts
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class Demo_Test : MonoBehaviour
    {
        private float updateTime = 0;
        BaseChart chart;
        void Awake()
        {
            chart = gameObject.GetComponent<BaseChart>();
            var btnTrans = transform.parent.Find("Button");
            if (btnTrans)
            {
                btnTrans.gameObject.GetComponent<Button>().onClick.AddListener(OnTestBtn);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AddData();
            }
        }

        void OnTestBtn()
        {
            chart.ClearData();
        }

        void AddData()
        {
            chart.ClearData();
            int count = Random.Range(5, 20);
            for (int i = 0; i < count; i++)
            {
                (chart as CoordinateChart).AddXAxisData("x" + i);
                if (Random.Range(1, 3) == 2)
                    chart.AddData(0, Random.Range(10, 200));
                else
                    chart.AddData(0, Random.Range(10, 100));
            }
        }
    }
}