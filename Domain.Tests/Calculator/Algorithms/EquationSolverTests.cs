namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using FluentAssertions;

public class EquationSolverTests
{
    [Fact]
    public void Solve_LinearEquation_FindsExpectedRoot()
    {
        double f(double x) => 3 * x + 5;

        var root = EquationSolver.Solve(f, 10, 0, 1e-9);

        root.Should().BeApproximately(5.0 / 3.0, 1e-6);
    }

    [Fact]
    public void Solve_SineEquation_FindsExpectedRoot()
    {
        var root = EquationSolver.Solve(Math.Sin, 0.5, 1, 1e-9);

        root.Should().BeApproximately(Math.Asin(0.5), 1e-6);
    }

    [Fact]
    public void Solve_NewtonCyclesForever_FallsBackToBisectionAndFindsRoot()
    {
        // Textbook Newton cycle: from x0 = 0 the iterates alternate 0 -> 1 -> 0 -> 1 ...
        // forever without ever converging, even though a real root exists near x = -1.769.
        double f(double x) => x * x * x - 2 * x + 2;

        var root = EquationSolver.Solve(f, 0, 0, 1e-9);

        f(root).Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void Solve_ConstantFunctionWithUnreachableTarget_Throws()
    {
        double f(double x) => 5;

        var act = () => EquationSolver.Solve(f, 0, 0, 1e-9);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Solve_NoRealRoot_ThrowsClearError()
    {
        double f(double x) => x * x + 1;

        var act = () => EquationSolver.Solve(f, 0, 0, 1e-9);

        act.Should().Throw<InvalidOperationException>().WithMessage("*No root found*");
    }

    [Fact]
    public void Solve_FunctionThrowingAtTestedPoint_WrapsWithClearMessage()
    {
        double f(double x) => x < 0 ? throw new InvalidOperationException("domain error") : Math.Sqrt(x) - 2;

        var act = () => EquationSolver.Solve(f, 0, -5, 1e-9);

        act.Should().Throw<InvalidOperationException>();
    }
}
