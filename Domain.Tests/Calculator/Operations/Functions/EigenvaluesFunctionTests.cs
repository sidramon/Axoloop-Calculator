namespace Domain.Tests.Calculator.Operations.Functions;

using Domain.Calculator.Operations.Functions.Matrix.Eigen;
using Domain.Calculator.Values;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class EigenvaluesFunctionTests
{
    private static MatrixValue M(double[,] data) => new(data);

    [Fact]
    public void Eigvals_RealMatrix_StillReturnsMatrixValueSortedDescending()
    {
        var m = M(new double[,] { { 2, 0 }, { 0, 3 } });

        var result = new EigenvaluesFunction().Apply(new Value[] { m });

        result.Should().BeOfType<MatrixValue>();
        var matrix = (MatrixValue)result;
        matrix[0, 0].Should().BeApproximately(3, 1e-9);
        matrix[0, 1].Should().BeApproximately(2, 1e-9);
    }

    [Fact]
    public void Eigvals_TwoByTwoRotation_ReturnsComplexConjugatePairAsValueList()
    {
        var m = M(new double[,] { { 0, -1 }, { 1, 0 } });

        var result = new EigenvaluesFunction().Apply(new Value[] { m });

        result.Should().BeOfType<ValueListValue>();
        var list = (ValueListValue)result;
        list.Values.Should().HaveCount(2);

        var first = (ComplexValue)list.Values[0];
        var second = (ComplexValue)list.Values[1];
        first.Real.Should().BeApproximately(0, 1e-9);
        first.Imaginary.Should().BeApproximately(1, 1e-9);
        second.Real.Should().BeApproximately(0, 1e-9);
        second.Imaginary.Should().BeApproximately(-1, 1e-9);
    }

    [Fact]
    public void Eigvals_NonMatrixArgument_Throws()
    {
        var act = () => new EigenvaluesFunction().Apply(new Value[] { new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Eigvals_LargerMatrixWithComplexEigenvalues_StillThrows()
    {
        // Larger-than-2x2 complex eigenvalue extraction remains unsupported (documented
        // limitation), unlike the 2x2 case handled directly above.
        var m = M(new double[,]
        {
            { 0, -1, 0 },
            { 1, 0, 0 },
            { 0, 0, 5 },
        });

        var act = () => new EigenvaluesFunction().Apply(new Value[] { m });

        act.Should().Throw<InvalidOperationException>();
    }
}
