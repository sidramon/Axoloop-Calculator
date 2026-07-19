namespace Presentation.Tests.App.Plotting;

using Application.Calculator.Plotting;
using Infrastructure.Plotting;
using Presentation.App.Plotting;
using FluentAssertions;

public class PlotRendererRegistryTests
{
    [Fact]
    public void RegistryBuiltFromRenderers_ResolvesEachByItsOwnFormat()
    {
        var renderers = new IPlotRenderer[] { new AsciiPlotRenderer(), new HtmlPlotRenderer() };

        var registry = renderers.ToDictionary(r => r.Format);

        registry[PlotFormat.Ascii].Should().BeOfType<AsciiPlotRenderer>();
        registry[PlotFormat.Html].Should().BeOfType<HtmlPlotRenderer>();
    }
}
