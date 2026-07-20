namespace Domain.Tests.Calculator.Operations.Functions;

using Domain.Tests.Calculator.TestHelpers;
using FluentAssertions;

public class FunctionDocumentationCompletenessTests
{
    [Fact]
    public void AllRegisteredBuiltinFunctions_HaveNonEmptySignatureDescriptionAndExamples()
    {
        foreach (var function in EvaluatorFactory.Builtins())
        {
            function.Signature.Should().NotBeNullOrWhiteSpace(
                $"'{function.Name}' must document its signature");
            function.Description.Should().NotBeNullOrWhiteSpace(
                $"'{function.Name}' must document its behavior");
            function.Examples.Should().NotBeEmpty(
                $"'{function.Name}' must have at least one example");
            function.Examples.Should().OnlyContain(
                example => !string.IsNullOrWhiteSpace(example),
                $"'{function.Name}' examples must not be blank");
        }
    }

    [Fact]
    public void AllRegisteredSpecialForms_HaveNonEmptySignatureDescriptionAndExamples()
    {
        foreach (var specialForm in EvaluatorFactory.SpecialForms())
        {
            specialForm.Signature.Should().NotBeNullOrWhiteSpace(
                $"'{specialForm.Name}' must document its signature");
            specialForm.Description.Should().NotBeNullOrWhiteSpace(
                $"'{specialForm.Name}' must document its behavior");
            specialForm.Examples.Should().NotBeEmpty(
                $"'{specialForm.Name}' must have at least one example");
        }
    }

    [Fact]
    public void RemovedFSolveFunction_LeavesNoTraceInAnyRegisteredDocumentation()
    {
        // fsolve was removed as a duplicate of solve(equation, unknown[, xMin, xMax]).
        // /functions, /help, and the generated web page all render from exactly these two
        // registries (mirroring Program.cs), so checking here covers all of them.
        foreach (var function in EvaluatorFactory.Builtins())
        {
            function.Name.Should().NotBe("fsolve");
            function.Signature.Should().NotContainEquivalentOf("fsolve");
            function.Description.Should().NotContainEquivalentOf("fsolve");
            function.Examples.Should().OnlyContain(example => !example.ToLowerInvariant().Contains("fsolve"));
        }

        foreach (var specialForm in EvaluatorFactory.SpecialForms())
        {
            specialForm.Description.Should().NotContainEquivalentOf("fsolve");
        }
    }
}
