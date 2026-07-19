namespace Application.Tests.Calculator;

using Application.Calculator;
using Domain.Calculator;
using Domain.Calculator.Values;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class ListVariablesUseCaseTests
{
    [Fact]
    public void Execute_NoVariables_ReturnsEmptyList()
    {
        var context = new VariableContext();
        var useCase = new ListVariablesUseCase(context);

        useCase.Execute().Should().BeEmpty();
    }

    [Fact]
    public void Execute_MixOfNormalAndConstantVariables_ListsNormalVariablesBeforeConstants()
    {
        var context = new VariableContext();
        context.Seed(new Dictionary<string, Value> { ["_pi"] = new NumberValue(Math.PI) });
        context.Set("b", new NumberValue(2));
        context.Set("a", new NumberValue(1));
        var useCase = new ListVariablesUseCase(context);

        var result = useCase.Execute();

        result.Select(v => v.Name).Should().Equal("a", "b", "_pi");
    }

    [Fact]
    public void Execute_ConstantVariable_IsFlaggedAsConstant()
    {
        var context = new VariableContext();
        context.Seed(new Dictionary<string, Value> { ["_pi"] = new NumberValue(Math.PI) });
        var useCase = new ListVariablesUseCase(context);

        var result = useCase.Execute();

        result.Single(v => v.Name == "_pi").IsConstant.Should().BeTrue();
    }

    [Fact]
    public void Execute_NormalVariable_IsNotFlaggedAsConstant()
    {
        var context = new VariableContext();
        context.Set("x", new NumberValue(1));
        var useCase = new ListVariablesUseCase(context);

        var result = useCase.Execute();

        result.Single(v => v.Name == "x").IsConstant.Should().BeFalse();
    }
}
