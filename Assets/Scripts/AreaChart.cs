using System.Collections.Generic;
using UnityEngine;
using XCharts.Runtime;

public class AreaChart : MonoBehaviour
{
    [SerializeField] private BarChart timeChart;
    [SerializeField] private BarChart quizChart;

    private const float targetTime = 120f;
    private const float targetQuizPercent = 80f;

    private static readonly Color actualColor = new Color32(52, 120, 210, 255);
    private static readonly Color targetColor = new Color32(240, 159, 67, 255);

    public void RefreshChart(int areaNumber)
    {
        if (GameProgressManager.Instance == null)
            return;

        float averageTime =
            GameProgressManager.Instance.GetAreaAverageTime(areaNumber);

        GameProgressManager.AreaQuizResult quizResult =
            GameProgressManager.Instance.GetAreaQuizResult(areaNumber);

        RefreshTimeChart(averageTime);
        RefreshQuizChart(quizResult);
    }

    private void RefreshTimeChart(float averageTime)
    {
        if (timeChart == null)
            return;

        var title = timeChart.GetChartComponent<Title>();

        if (title != null)
            title.text = "IDÕ EREDMÉNY";

        timeChart.RemoveData();

        if (averageTime < 0f)
            return;

        timeChart.AddYAxisData("IDÕ (másodperc)");

        AddSeries(timeChart, "ÁTLAG IDÕ",
            new List<double> { averageTime }, actualColor);

        AddSeries(timeChart, "IDÕKORLÁT",
            new List<double> { targetTime }, targetColor);
    }

    private void RefreshQuizChart(
        GameProgressManager.AreaQuizResult result)
    {
        if (quizChart == null)
            return;

        var title = quizChart.GetChartComponent<Title>();

        if (title != null)
            title.text = "KVÍZ EREDMÉNY";

        quizChart.RemoveData();

        if (result == null || result.totalQuestions <= 0)
            return;

        quizChart.AddYAxisData("PONTOSSÁG (%)");

        AddSeries(quizChart, "PONTOSSÁGOD",
            new List<double> { result.accuracyPercent }, actualColor);

        AddSeries(quizChart, "ELVÁRT PONTOSSÁG",
            new List<double> { targetQuizPercent }, targetColor);
    }

    private void AddSeries(
        BarChart chart,
        string seriesName,
        List<double> values,
        Color color)
    {
        Bar series = chart.AddSerie<Bar>(seriesName);

        series.itemStyle.color = color;

        LabelStyle label = series.EnsureComponent<LabelStyle>();
        label.show = true;
        label.position = LabelStyle.Position.Inside;
        label.textStyle.color = Color.white;
        label.numericFormatter = "0.0";

        foreach (double value in values)
            series.AddData(value);
    }
}
