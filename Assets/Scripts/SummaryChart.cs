using System.Collections.Generic;
using UnityEngine;
using XCharts.Runtime;

public class SummaryChart : MonoBehaviour
{
    [SerializeField] private LineChart chart;

    private static readonly Color color = new Color32(52, 160, 130, 255);

    public void RefreshChart()
    {
        if (chart == null)
            return;

        chart.RemoveData();

        if (GameProgressManager.Instance == null)
            return;

        var title = chart.GetChartComponent<Title>();
        if (title != null)
            title.text = "ÖSSZESÍTETT EREDMÉNY";

        var xAxis = chart.GetChartComponent<XAxis>();
        if (xAxis != null)
            xAxis.type = Axis.AxisType.Category;

        var yAxis = chart.GetChartComponent<YAxis>();
        if (yAxis != null)
        {
            yAxis.type = Axis.AxisType.Value;
            yAxis.minMaxType = Axis.AxisMinMaxType.Custom;
            yAxis.min = 0f;
            yAxis.max = 100f;
        }

        var areas = new List<int>();
        var scores = new List<double>();

        for (int areaNumber = 1; areaNumber <= 5; areaNumber++)
        {
            GameProgressManager.AreaQuizResult result = GameProgressManager.Instance.GetAreaQuizResult(areaNumber);

            if (result == null || result.totalQuestions <= 0)
                continue;

            areas.Add(areaNumber);
            scores.Add(result.accuracyPercent);
        }

        if (areas.Count == 0)
            return;

        foreach (int areaNumber in areas)
            chart.AddXAxisData("TERÜLET " + areaNumber);

        Line line = chart.AddSerie<Line>("PONTOSSÁGOD");
        line.itemStyle.color = color;

        LabelStyle label = line.EnsureComponent<LabelStyle>();
        label.show = true;
        label.numericFormatter = "0'%'";

        foreach (double score in scores)
            line.AddData(score);
    }
}
