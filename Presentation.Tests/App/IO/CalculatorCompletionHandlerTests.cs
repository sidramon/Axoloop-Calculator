namespace Presentation.Tests.App.IO;

using Application.Calculator;
using Domain.Calculator;
using Domain.Calculator.Values;
using FluentAssertions;
using Presentation.App.IO;

public class CalculatorCompletionHandlerTests
{
    private static CalculatorCompletionHandler CreateHandler(
        VariableContext? context = null,
        FunctionContext? functionContext = null)
    {
        context ??= new VariableContext();
        functionContext ??= new FunctionContext();
        var functionNames = new[] { "sqrt", "sin", "cos" };
        var metaCommands = new[] { "/help", "/vars", "/funcs", "/clear" };
        return new CalculatorCompletionHandler(
            functionNames,
            metaCommands,
            new ListVariablesUseCase(context),
            new ListFunctionsUseCase(functionContext));
    }

    [Fact]
    public void GetSuggestions_FunctionPrefix_ReturnsMatchingFunctionNames()
    {
        var handler = CreateHandler();

        var suggestions = handler.GetSuggestions("sq", 0);

        suggestions.Should().Equal("sqrt");
    }

    [Fact]
    public void GetSuggestions_MetaCommandPrefix_ReturnsMatchingMetaCommands()
    {
        var handler = CreateHandler();

        var suggestions = handler.GetSuggestions("/h", 0);

        suggestions.Should().Equal("/help");
    }

    [Fact]
    public void GetSuggestions_VariableNamePrefix_ReturnsMatchingVariableName()
    {
        var context = new VariableContext();
        context.Set("xylophone", new NumberValue(1));
        var handler = CreateHandler(context);

        var suggestions = handler.GetSuggestions("xy", 0);

        suggestions.Should().Equal("xylophone");
    }

    [Fact]
    public void GetSuggestions_UserFunctionPrefix_ReturnsMatchingUserFunctionName()
    {
        var functionContext = new FunctionContext();
        functionContext.Define(new UserFunction("yield", new[] { "x" }, new Domain.Calculator.Ast.IdentifierExpression("x")));
        var handler = CreateHandler(functionContext: functionContext);

        var suggestions = handler.GetSuggestions("yi", 0);

        suggestions.Should().Equal("yield");
    }

    [Fact]
    public void GetSuggestions_EmptyPrefix_ReturnsNoSuggestions()
    {
        var handler = CreateHandler();

        var suggestions = handler.GetSuggestions("sqrt ", 5);

        suggestions.Should().BeEmpty();
    }

    [Fact]
    public void GetSuggestions_NoMatch_ReturnsEmptyArray()
    {
        var handler = CreateHandler();

        var suggestions = handler.GetSuggestions("zzz", 0);

        suggestions.Should().BeEmpty();
    }
}
