using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XCharts.Runtime;

public class LevelsChart : MonoBehaviour
{
    [SerializeField] private BarChart chart;

    [Header("Area to display")]
    [SerializeField, Range(1, 5)] private int areaNumber = 1;

    private void Start()
    {
        RefreshChart();
    }

    public void RefreshChart()
    {
        if (chart == null)
            return;

        var title = chart.GetChartComponent<Title>();

        if (title != null)
            title.text = "EREDMÉNYEK";

        if (GameProgressManager.Instance == null)
            return;

        chart.RemoveData();

        var levels = new List<string>();
        var times = new List<double>();
        var targetTimes = new List<double>();
        var correct = new List<double>();
        var required = new List<double>();

        int maxQuestions = 1;

        for (int level = 1; level <= 5; level++)
        {
            string levelID = $"{areaNumber}-{level}";

            GameProgressManager.LevelResult result =
                GameProgressManager.Instance.GetLevelResult(levelID);

            if (result == null || !result.completed)
                continue;

            levels.Add(levelID);
            times.Add(result.completionTime);
            correct.Add(result.correctAnswers);
            required.Add(result.requiredCorrectAnswers);

            targetTimes.Add(120);

            maxQuestions = Mathf.Max(
                maxQuestions,
                result.totalQuestions
            );
        }

        foreach (string levelID in levels)
            chart.AddYAxisData(levelID);

        AddSeries("IDÕ", times, false,
            new Color32(52, 120, 210, 255));

        AddSeries("IDÕKORLÁT", targetTimes, false,
            new Color32(240, 159, 67, 255));

        AddSeries("HELYES VÁLASZOK", correct, true,
            new Color32(35, 160, 115, 255));

        AddSeries("KÉRDÉSEK SZÁMA", required, true,
            new Color32(153, 102, 204, 255));

        ConfigureQuizAxis(maxQuestions);
    }

    private void AddSeries(
        string name,
        List<double> values,
        bool useQuizAxis,
        Color color)
    {
        Bar serie = chart.AddSerie<Bar>(name);

        if (useQuizAxis)
            serie.xAxisIndex = 1;

        serie.itemStyle.color = color;

        var label = serie.EnsureComponent<LabelStyle>();
        label.show = true;
        label.position = LabelStyle.Position.Inside;
        label.textStyle.color = Color.white;
        label.numericFormatter = "0.0";

        foreach (double value in values)
            serie.AddData(value);
    }

    private void ConfigureQuizAxis(int maxQuestions)
    {
        var secondAxis = chart.AddChartComponent<XAxis>();

        secondAxis.type = Axis.AxisType.Value;
        secondAxis.position = Axis.AxisPosition.Top;
        secondAxis.offset = 30;

        secondAxis.minMaxType = Axis.AxisMinMaxType.Custom;
        secondAxis.min = 0;
        secondAxis.max = maxQuestions;

        secondAxis.axisLabel.show = true;
        secondAxis.axisTick.show = true;
        secondAxis.axisLine.show = true;

        var legend = chart.AddChartComponent<Legend>();
        legend.show = true;
        legend.orient = Orient.Horizonal;
        legend.location.top = 60;
    }
}
