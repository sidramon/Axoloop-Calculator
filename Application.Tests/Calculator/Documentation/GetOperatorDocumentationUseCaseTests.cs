namespace Application.Tests.Calculator.Documentation;

using Application.Calculator.Documentation;
using Domain.Calculator.Operations;
using FluentAssertions;

public class GetOperatorDocumentationUseCaseTests
{
    [Fact]
    public void Execute_ProjectsEachEntryField()
    {
        var entries = new[]
        {
            new OperatorDocumentationEntry("+", OperatorKind.Binary, 4, Associativity.Left, "Addition.", "2+3 → 5"),
        };
        var useCase = new GetOperatorDocumentationUseCase(entries);

        var docs = useCase.Execute();

        docs.Should().ContainSingle();
        var doc = docs[0];
        doc.Symbol.Should().Be("+");
        doc.Kind.Should().Be(OperatorKind.Binary);
        doc.Precedence.Should().Be(4);
        doc.Associativity.Should().Be(Associativity.Left);
        doc.Description.Should().Be("Addition.");
        doc.Example.Should().Be("2+3 → 5");
    }

    [Fact]
    public void Execute_EntryWithNoPrecedenceOrAssociativity_ProjectsAsNull()
    {
        var entries = new[]
        {
            new OperatorDocumentationEntry(":=", OperatorKind.Assignment, null, null, "Assignment.", "x := 5"),
        };
        var useCase = new GetOperatorDocumentationUseCase(entries);

        var doc = useCase.Execute().Single();

        doc.Precedence.Should().BeNull();
        doc.Associativity.Should().BeNull();
    }

    [Fact]
    public void Execute_PreservesDeclaredOrder()
    {
        var entries = new[]
        {
            new OperatorDocumentationEntry("or", OperatorKind.Logical, 1, Associativity.Left, "Or.", "a or b"),
            new OperatorDocumentationEntry("and", OperatorKind.Logical, 2, Associativity.Left, "And.", "a and b"),
        };
        var useCase = new GetOperatorDocumentationUseCase(entries);

        useCase.Execute().Select(d => d.Symbol).Should().Equal("or", "and");
    }
}
