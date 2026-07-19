namespace Application.Calculator.Plotting;

using Domain.Calculator.Plotting;

public interface IPlotRenderer
{
    PlotFormat Format { get; }
    string Render(PlotSeries series, PlotOptions options);
}
