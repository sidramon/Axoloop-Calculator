namespace Domain.Tests.Calculator.Operations.Functions;

using Domain.Calculator.Operations.Functions.Matrix;
using Domain.Calculator.Values;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class MatrixFunctionTests
{
    private static MatrixValue M(double[,] data) => new(data);

    [Fact]
    public void Transpose_Matrix_ReturnsTransposedMatrix()
    {
        var m = M(new double[,] { { 1, 2 }, { 3, 4 } });

        var result = (MatrixValue)new TransposeFunction().Apply(new Value[] { m });

        result[0, 1].Should().BeApproximately(3, 1e-10);
    }

    [Fact]
    public void Transpose_NonMatrixArgument_Throws()
    {
        var act = () => new TransposeFunction().Apply(new Value[] { new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Determinant_Matrix_ReturnsScalar()
    {
        var m = M(new double[,] { { 1, 2 }, { 3, 4 } });

        var result = (NumberValue)new DeterminantFunction().Apply(new Value[] { m });

        result.Number.Should().BeApproximately(-2, 1e-10);
    }

    [Fact]
    public void Determinant_NonMatrixArgument_Throws()
    {
        var act = () => new DeterminantFunction().Apply(new Value[] { new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Inverse_Matrix_ReturnsInvertedMatrix()
    {
        var m = M(new double[,] { { 1, 0 }, { 0, 2 } });

        var result = (MatrixValue)new InverseFunction().Apply(new Value[] { m });

        result[1, 1].Should().BeApproximately(0.5, 1e-10);
    }

    [Fact]
    public void Inverse_NonMatrixArgument_Throws()
    {
        var act = () => new InverseFunction().Apply(new Value[] { new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Trace_Matrix_ReturnsSumOfDiagonal()
    {
        var m = M(new double[,] { { 1, 2 }, { 3, 4 } });

        var result = (NumberValue)new TraceFunction().Apply(new Value[] { m });

        result.Number.Should().BeApproximately(5, 1e-10);
    }

    [Fact]
    public void Trace_NonMatrixArgument_Throws()
    {
        var act = () => new TraceFunction().Apply(new Value[] { new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Rank_Matrix_ReturnsRank()
    {
        var m = M(new double[,] { { 1, 2 }, { 2, 4 } });

        var result = (NumberValue)new RankFunction().Apply(new Value[] { m });

        result.Number.Should().Be(1);
    }

    [Fact]
    public void Rank_NonMatrixArgument_Throws()
    {
        var act = () => new RankFunction().Apply(new Value[] { new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Identity_GivenSize_ReturnsIdentityMatrix()
    {
        var result = (MatrixValue)new IdentityFunction().Apply(new Value[] { new NumberValue(2) });

        result[0, 0].Should().Be(1);
        result[0, 1].Should().Be(0);
        result[1, 1].Should().Be(1);
    }

    [Fact]
    public void Identity_NonIntegerSize_Throws()
    {
        var act = () => new IdentityFunction().Apply(new Value[] { new NumberValue(2.5) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Identity_NonNumericArgument_Throws()
    {
        var act = () => new IdentityFunction().Apply(new Value[] { new BooleanValue(true) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DotProduct_TwoVectors_ReturnsScalar()
    {
        var a = M(new double[,] { { 1, 2, 3 } });
        var b = M(new double[,] { { 4, 5, 6 } });

        var result = (NumberValue)new DotProductFunction().Apply(new Value[] { a, b });

        result.Number.Should().BeApproximately(32, 1e-10);
    }

    [Fact]
    public void DotProduct_NonMatrixArgument_Throws()
    {
        var a = M(new double[,] { { 1, 2 } });

        var act = () => new DotProductFunction().Apply(new Value[] { a, new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CrossProduct_TwoThreeDimensionalVectors_ReturnsVector()
    {
        var a = M(new double[,] { { 1, 0, 0 } });
        var b = M(new double[,] { { 0, 1, 0 } });

        var result = (MatrixValue)new CrossProductFunction().Apply(new Value[] { a, b });

        result[0, 2].Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void CrossProduct_NonMatrixArgument_Throws()
    {
        var a = M(new double[,] { { 1, 0, 0 } });

        var act = () => new CrossProductFunction().Apply(new Value[] { a, new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Zeros_GivenDimensions_ReturnsMatrixOfZeros()
    {
        var result = (MatrixValue)new ZerosFunction().Apply(new Value[] { new NumberValue(2), new NumberValue(3) });

        result.Rows.Should().Be(2);
        result.Columns.Should().Be(3);
        result[0, 0].Should().Be(0);
    }

    [Fact]
    public void Zeros_NonNumericDimension_Throws()
    {
        var act = () => new ZerosFunction().Apply(new Value[] { new BooleanValue(true), new NumberValue(2) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ones_GivenDimensions_ReturnsMatrixOfOnes()
    {
        var result = (MatrixValue)new OnesFunction().Apply(new Value[] { new NumberValue(2), new NumberValue(2) });

        result[0, 0].Should().Be(1);
        result[1, 1].Should().Be(1);
    }

    [Fact]
    public void Ones_NonIntegerDimension_Throws()
    {
        var act = () => new OnesFunction().Apply(new Value[] { new NumberValue(2.5), new NumberValue(2) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reshape_MatrixWithMatchingElementCount_ReturnsReshapedMatrix()
    {
        var m = M(new double[,] { { 1, 2, 3, 4, 5, 6 } });

        var result = (MatrixValue)new ReshapeFunction().Apply(new Value[] { m, new NumberValue(2), new NumberValue(3) });

        result.Rows.Should().Be(2);
        result.Columns.Should().Be(3);
    }

    [Fact]
    public void Reshape_NonMatrixArgument_Throws()
    {
        var act = () => new ReshapeFunction().Apply(new Value[] { new NumberValue(1), new NumberValue(1), new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LinSolve_UniqueSystem_ReturnsColumnVector()
    {
        var a = M(new double[,] { { 1, 1 }, { 1, -1 } });
        var b = M(new double[,] { { 3 }, { 1 } });

        var result = (MatrixValue)new LinSolveFunction().Apply(new Value[] { a, b });

        result[0, 0].Should().BeApproximately(2, 1e-9);
        result[1, 0].Should().BeApproximately(1, 1e-9);
    }

    [Fact]
    public void LinSolve_NoSolution_Throws()
    {
        var a = M(new double[,] { { 1, 1 }, { 1, 1 } });
        var b = M(new double[,] { { 1 }, { 2 } });

        var act = () => new LinSolveFunction().Apply(new Value[] { a, b });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LinSolve_InfiniteSolutions_Throws()
    {
        var a = M(new double[,] { { 1, 1 } });
        var b = M(new double[,] { { 2 } });

        var act = () => new LinSolveFunction().Apply(new Value[] { a, b });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LinSolve_NonMatrixArgument_Throws()
    {
        var act = () => new LinSolveFunction().Apply(new Value[] { new NumberValue(1), new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LinSolveGeneral_UniqueSystem_ReturnsSingleColumn()
    {
        var a = M(new double[,] { { 1, 1 }, { 1, -1 } });
        var b = M(new double[,] { { 3 }, { 1 } });

        var result = (MatrixValue)new LinSolveGeneralFunction().Apply(new Value[] { a, b });

        result.Columns.Should().Be(1);
        result[0, 0].Should().BeApproximately(2, 1e-9);
        result[1, 0].Should().BeApproximately(1, 1e-9);
    }

    [Fact]
    public void LinSolveGeneral_InfiniteSolutions_ReturnsParticularAndNullSpaceColumns()
    {
        var a = M(new double[,] { { 1, 1 } });
        var b = M(new double[,] { { 2 } });

        var result = (MatrixValue)new LinSolveGeneralFunction().Apply(new Value[] { a, b });

        result.Columns.Should().Be(2);
        var particular = M(new double[,] { { result[0, 0] }, { result[1, 0] } });
        a.Multiply(particular)[0, 0].Should().BeApproximately(2, 1e-9);
        var basis = M(new double[,] { { result[0, 1] }, { result[1, 1] } });
        a.Multiply(basis)[0, 0].Should().BeApproximately(0, 1e-9);
    }

    [Fact]
    public void LinSolveGeneral_NoSolution_Throws()
    {
        var a = M(new double[,] { { 1, 1 }, { 1, 1 } });
        var b = M(new double[,] { { 1 }, { 2 } });

        var act = () => new LinSolveGeneralFunction().Apply(new Value[] { a, b });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Rref_RankDeficientMatrix_ReturnsNormalizedEchelonForm()
    {
        var m = M(new double[,] { { 1, 2 }, { 2, 4 } });

        var result = (MatrixValue)new RrefFunction().Apply(new Value[] { m });

        result[0, 0].Should().BeApproximately(1, 1e-9);
        result[0, 1].Should().BeApproximately(2, 1e-9);
        result[1, 0].Should().BeApproximately(0, 1e-9);
        result[1, 1].Should().BeApproximately(0, 1e-9);
    }

    [Fact]
    public void Rref_NonMatrixArgument_Throws()
    {
        var act = () => new RrefFunction().Apply(new Value[] { new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NullSpace_RankDeficientMatrix_ReturnsBasisVectorInTheKernel()
    {
        var m = M(new double[,] { { 1, 1 }, { 1, 1 } });

        var result = (MatrixValue)new NullSpaceFunction().Apply(new Value[] { m });

        result.Columns.Should().Be(1);
        m.Multiply(result)[0, 0].Should().BeApproximately(0, 1e-9);
        m.Multiply(result)[1, 0].Should().BeApproximately(0, 1e-9);
    }

    [Fact]
    public void NullSpace_FullColumnRankMatrix_Throws()
    {
        var m = M(new double[,] { { 1, 0 }, { 0, 1 } });

        var act = () => new NullSpaceFunction().Apply(new Value[] { m });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NullSpace_NonMatrixArgument_Throws()
    {
        var act = () => new NullSpaceFunction().Apply(new Value[] { new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }
}
