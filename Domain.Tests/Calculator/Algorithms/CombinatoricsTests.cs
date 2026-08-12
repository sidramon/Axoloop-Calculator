namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using FluentAssertions;

public class CombinatoricsTests
{
    [Theory]
    [InlineData(5, 2, 10)]
    [InlineData(5, 0, 1)]
    [InlineData(5, 5, 1)]
    [InlineData(10, 3, 120)]
    public void Choose_SmallExactValues_MatchesKnownResult(int n, int k, double expected)
    {
        Combinatorics.Choose(n, k).Should().Be(expected);
    }

    [Fact]
    public void Choose_SymmetricAroundNMinusK_ReturnsSameValue()
    {
        Combinatorics.Choose(10, 3).Should().Be(Combinatorics.Choose(10, 7));
    }

    [Fact]
    public void Choose_KGreaterThanN_ReturnsZero()
    {
        Combinatorics.Choose(5, 10).Should().Be(0);
    }

    [Fact]
    public void Choose_LargeValues_ExactDespiteDoubleArithmetic()
    {
        // The canonical 5-card-poker-hand count; large enough to exercise the
        // multiplicative formula's accumulated rounding, small enough to still land
        // exactly on the true integer.
        Combinatorics.Choose(52, 5).Should().Be(2598960);
    }

    [Fact]
    public void Choose_NegativeN_Throws()
    {
        var act = () => Combinatorics.Choose(-1, 2);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Choose_NegativeK_Throws()
    {
        var act = () => Combinatorics.Choose(5, -1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(5, 2, 20)]
    [InlineData(5, 0, 1)]
    [InlineData(10, 3, 720)]
    public void Permutations_SmallExactValues_MatchesKnownResult(int n, int k, double expected)
    {
        Combinatorics.Permutations(n, k).Should().Be(expected);
    }

    [Fact]
    public void Permutations_KGreaterThanN_ReturnsZero()
    {
        Combinatorics.Permutations(5, 10).Should().Be(0);
    }

    [Fact]
    public void Permutations_NegativeN_Throws()
    {
        var act = () => Combinatorics.Permutations(-1, 2);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Permutations_NegativeK_Throws()
    {
        var act = () => Combinatorics.Permutations(5, -1);

        act.Should().Throw<InvalidOperationException>();
    }
}
