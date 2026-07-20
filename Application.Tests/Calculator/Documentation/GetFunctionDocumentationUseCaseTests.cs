namespace Application.Tests.Calculator.Documentation;

using Application.Calculator.Documentation;
using Domain.Calculator;
using Domain.Calculator.Ast;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Operations.SpecialForms;
using Domain.Calculator.Values;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class GetFunctionDocumentationUseCaseTests
{
    private sealed class FakeFunction : IFunction
    {
        public required string Name { get; init; }
        public int Arity { get; init; } = 1;
        public required FunctionCategory Category { get; init; }
        public string Signature { get; init; } = "fake(x)";
        public string Description { get; init; } = "Fake function used for testing.";
        public IReadOnlyList<string> Examples { get; init; } = new[] { "fake(1) → 1" };

        public Value Apply(IReadOnlyList<Value> arguments) => arguments[0];
    }

    private sealed class FakeSpecialForm : ISpecialForm
    {
        public required string Name { get; init; }
        public int Arity { get; init; } = 1;
        public required FunctionCategory Category { get; init; }
        public string Signature { get; init; } = "fakeform(x)";
        public string Description { get; init; } = "Fake special form used for testing.";
        public IReadOnlyList<string> Examples { get; init; } = new[] { "fakeform(1) → 1" };

        public Value Apply(IReadOnlyList<IExpression> arguments, VariableContext context, Evaluator evaluator) =>
            new NumberValue(0);
    }

    [Fact]
    public void Execute_ByName_ProjectsAllDocumentationFields()
    {
        var function = new FakeFunction { Name = "widget", Category = FunctionCategory.Arithmetic };
        var useCase = new GetFunctionDocumentationUseCase(new[] { function }, Array.Empty<ISpecialForm>());

        var doc = useCase.Execute("widget");

        doc.Should().NotBeNull();
        doc!.Name.Should().Be("widget");
        doc.Category.Should().Be(FunctionCategory.Arithmetic);
        doc.Signature.Should().Be("fake(x)");
        doc.Arity.Should().Be(1);
        doc.Description.Should().Be("Fake function used for testing.");
        doc.Examples.Should().Equal("fake(1) → 1");
    }

    [Fact]
    public void Execute_ByName_IsCaseInsensitive()
    {
        var function = new FakeFunction { Name = "widget", Category = FunctionCategory.Arithmetic };
        var useCase = new GetFunctionDocumentationUseCase(new[] { function }, Array.Empty<ISpecialForm>());

        useCase.Execute("WIDGET").Should().NotBeNull();
    }

    [Fact]
    public void Execute_ByName_UnknownName_ReturnsNull()
    {
        var useCase = new GetFunctionDocumentationUseCase(Array.Empty<IFunction>(), Array.Empty<ISpecialForm>());

        useCase.Execute("nope").Should().BeNull();
    }

    [Fact]
    public void Execute_ByName_IncludesSpecialForms()
    {
        var form = new FakeSpecialForm { Name = "iffy", Category = FunctionCategory.Logic };
        var useCase = new GetFunctionDocumentationUseCase(Array.Empty<IFunction>(), new[] { form });

        useCase.Execute("iffy").Should().NotBeNull();
    }

    [Fact]
    public void Execute_Grouped_GroupsByCategoryAndSortsNamesWithinGroup()
    {
        var functions = new[]
        {
            new FakeFunction { Name = "zeta", Category = FunctionCategory.Matrix },
            new FakeFunction { Name = "alpha", Category = FunctionCategory.Matrix },
            new FakeFunction { Name = "sin", Category = FunctionCategory.Trigonometry },
        };
        var useCase = new GetFunctionDocumentationUseCase(functions, Array.Empty<ISpecialForm>());

        var groups = useCase.Execute();

        var matrixGroup = groups.Single(g => g.Category == FunctionCategory.Matrix);
        matrixGroup.Functions.Select(f => f.Name).Should().Equal("alpha", "zeta");
    }

    [Fact]
    public void SuggestSimilarNames_UnknownNameWithCommonPrefix_SuggestsClosestMatches()
    {
        var functions = new[]
        {
            new FakeFunction { Name = "sqrt", Category = FunctionCategory.Arithmetic },
            new FakeFunction { Name = "square", Category = FunctionCategory.Arithmetic },
            new FakeFunction { Name = "cos", Category = FunctionCategory.Trigonometry },
        };
        var useCase = new GetFunctionDocumentationUseCase(functions, Array.Empty<ISpecialForm>());

        var suggestions = useCase.SuggestSimilarNames("sqr");

        suggestions.Should().Contain("sqrt");
    }

    [Fact]
    public void SuggestSimilarNames_NoSharedPrefixWithAnyName_ReturnsEmpty()
    {
        var functions = new[] { new FakeFunction { Name = "sqrt", Category = FunctionCategory.Arithmetic } };
        var useCase = new GetFunctionDocumentationUseCase(functions, Array.Empty<ISpecialForm>());

        useCase.SuggestSimilarNames("zzz").Should().BeEmpty();
    }

    [Fact]
    public void Execute_SpecialFormRegisteredUnderMultipleArities_CollapsesToASingleEntry()
    {
        var arity2 = new FakeSpecialForm { Name = "solve", Arity = 2, Category = FunctionCategory.Arithmetic };
        var arity4 = new FakeSpecialForm { Name = "solve", Arity = 4, Category = FunctionCategory.Arithmetic };
        var useCase = new GetFunctionDocumentationUseCase(Array.Empty<IFunction>(), new[] { arity2, arity4 });

        var groups = useCase.Execute();

        groups.SelectMany(g => g.Functions)
            .Count(f => string.Equals(f.Name, "solve", StringComparison.OrdinalIgnoreCase))
            .Should().Be(1);
        useCase.Execute("solve").Should().NotBeNull();
    }
}
