namespace Domain.Tests.Calculator.Symbolic;

using Domain.Calculator.Symbolic;
using FluentAssertions;

public class CanonicalizerTests
{
    private static Number N(int value) => new(new Rational(value));
    private static Symbol X => new("x");
    private static Symbol Y => new("y");
    private static SymbolicExpression C(SymbolicExpression e) => Canonicalizer.Canonicalize(e);

    // --- Flattening ---

    [Fact]
    public void Flatten_NestedSum_BecomesOneNAryStar()
    {
        var expr = new Sum(new SymbolicExpression[] { X, new Sum(new SymbolicExpression[] { Y, N(1) }) });

        C(expr).Should().Be(new Sum(new SymbolicExpression[] { N(1), X, Y }));
    }

    [Fact]
    public void Flatten_NestedProduct_BecomesOneNAry()
    {
        var expr = new Product(new SymbolicExpression[] { X, new Product(new SymbolicExpression[] { Y, N(2) }) });

        C(expr).Should().Be(new Product(new SymbolicExpression[] { N(2), X, Y }));
    }

    // --- Constant folding ---

    [Fact]
    public void FoldConstants_SumOfNumbers_ReturnsSingleNumber()
    {
        var expr = new Sum(new SymbolicExpression[] { N(2), N(3), N(4) });

        C(expr).Should().Be(N(9));
    }

    [Fact]
    public void FoldConstants_ProductOfNumbers_ReturnsSingleNumber()
    {
        var expr = new Product(new SymbolicExpression[] { N(2), N(3), N(4) });

        C(expr).Should().Be(N(24));
    }

    // --- Identities / absorbing elements ---

    [Fact]
    public void Identity_AddZero_IsRemoved()
    {
        var expr = new Sum(new SymbolicExpression[] { X, N(0) });

        C(expr).Should().Be(X);
    }

    [Fact]
    public void Identity_MultiplyByOne_IsRemoved()
    {
        var expr = new Product(new SymbolicExpression[] { X, N(1) });

        C(expr).Should().Be(X);
    }

    [Fact]
    public void Absorb_MultiplyByZero_ReturnsZero()
    {
        var expr = new Product(new SymbolicExpression[] { X, Y, N(0) });

        C(expr).Should().Be(N(0));
    }

    [Fact]
    public void Identity_PowerOne_ReturnsBase()
    {
        C(new Power(X, N(1))).Should().Be(X);
    }

    [Fact]
    public void Identity_PowerZero_ReturnsOne()
    {
        C(new Power(X, N(0))).Should().Be(N(1));
    }

    [Fact]
    public void Convention_ZeroToTheZero_ReturnsOne()
    {
        C(new Power(N(0), N(0))).Should().Be(N(1));
    }

    [Fact]
    public void ZeroBase_PositiveExponent_ReturnsZero()
    {
        C(new Power(N(0), N(3))).Should().Be(N(0));
    }

    [Fact]
    public void ZeroBase_NegativeExponent_Throws()
    {
        var act = () => C(new Power(N(0), N(-2)));

        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void ZeroBase_SymbolicExponent_StaysUnevaluated()
    {
        C(new Power(N(0), X)).Should().Be(new Power(N(0), X));
    }

    // --- Collecting like terms ---

    [Fact]
    public void Collect_XPlusX_ReturnsTwoX()
    {
        var expr = new Sum(new SymbolicExpression[] { X, X });

        C(expr).Should().Be(new Product(new SymbolicExpression[] { N(2), X }));
    }

    [Fact]
    public void Collect_XMinusX_ReturnsZero()
    {
        // x - x, i.e. Sum(x, Product(-1, x))
        var expr = new Sum(new SymbolicExpression[]
        {
            X,
            new Product(new SymbolicExpression[] { new Number(new Rational(-1)), X }),
        });

        C(expr).Should().Be(N(0));
    }

    [Fact]
    public void Collect_TwoXPlusThreeX_ReturnsFiveX()
    {
        var expr = new Sum(new SymbolicExpression[]
        {
            new Product(new SymbolicExpression[] { N(2), X }),
            new Product(new SymbolicExpression[] { N(3), X }),
        });

        C(expr).Should().Be(new Product(new SymbolicExpression[] { N(5), X }));
    }

    [Fact]
    public void Collect_DifferentSymbols_DoNotMerge()
    {
        var expr = new Sum(new SymbolicExpression[] { X, Y });

        var result = C(expr);
        result.Should().BeOfType<Sum>();
        ((Sum)result).Terms.Should().HaveCount(2);
    }

    // --- Merging powers ---

    [Fact]
    public void MergePowers_XTimesX_ReturnsXSquared()
    {
        var expr = new Product(new SymbolicExpression[] { X, X });

        C(expr).Should().Be(new Power(X, N(2)));
    }

    [Fact]
    public void MergePowers_XDividedByX_ReturnsOne()
    {
        // x / x, i.e. Product(x, Power(x,-1))
        var expr = new Product(new SymbolicExpression[] { X, new Power(X, new Number(new Rational(-1))) });

        C(expr).Should().Be(N(1));
    }

    [Fact]
    public void MergePowers_XSquaredTimesXCubed_ReturnsXFifth()
    {
        var expr = new Product(new SymbolicExpression[] { new Power(X, N(2)), new Power(X, N(3)) });

        C(expr).Should().Be(new Power(X, N(5)));
    }

    [Fact]
    public void MergePowers_PowerOfPower_MultipliesExponents()
    {
        var expr = new Power(new Power(X, N(2)), N(3));

        C(expr).Should().Be(new Power(X, N(6)));
    }

    [Fact]
    public void MergePowers_SymbolicExponents_DoNotMerge()
    {
        var expr = new Product(new SymbolicExpression[] { new Power(X, Y), new Power(X, N(3)) });

        var result = C(expr);
        result.Should().BeOfType<Product>();
        ((Product)result).Factors.Should().HaveCount(2);
    }

    // --- Distributing an integer power over a product's factors ---

    [Fact]
    public void Distribute_ReciprocalOfCompoundBase_MatchesChainedDivision()
    {
        // The test that motivates the whole rule: a/(b*c) and a/b/c must canonicalize to
        // the structurally identical tree, not merely an equal value.
        var reciprocalOfProduct = new Product(new SymbolicExpression[]
        {
            X,
            new Power(new Product(new SymbolicExpression[] { Y, new Symbol("z") }), new Number(new Rational(-1))),
        });
        var chainedDivision = new Product(new SymbolicExpression[]
        {
            X,
            new Power(Y, new Number(new Rational(-1))),
            new Power(new Symbol("z"), new Number(new Rational(-1))),
        });

        C(reciprocalOfProduct).Should().Be(C(chainedDivision));
    }

    [Fact]
    public void Distribute_SquareOfProduct_DistributesToEachFactor()
    {
        var expr = new Power(new Product(new SymbolicExpression[] { X, Y }), N(2));

        C(expr).Should().Be(new Product(new SymbolicExpression[] { new Power(X, N(2)), new Power(Y, N(2)) }));
    }

    [Fact]
    public void Distribute_FractionalExponentOfProduct_IsNotDistributed()
    {
        // (x*y)^(1/2) must NOT become x^(1/2) * y^(1/2): with negative factors that would
        // change the value (branch-cut issue), so it is never attempted, even for factors
        // that happen to be positive.
        var expr = new Power(new Product(new SymbolicExpression[] { X, Y }), new Number(new Rational(1, 2)));

        var result = C(expr);
        result.Should().BeOfType<Power>();
        ((Power)result).Base.Should().Be(new Product(new SymbolicExpression[] { X, Y }));
    }

    [Fact]
    public void Distribute_SymbolicExponentOfProduct_IsNotDistributed()
    {
        var n = new Symbol("n");
        var expr = new Power(new Product(new SymbolicExpression[] { X, Y }), n);

        var result = C(expr);
        result.Should().BeOfType<Power>();
        ((Power)result).Base.Should().Be(new Product(new SymbolicExpression[] { X, Y }));
        ((Power)result).Exponent.Should().Be(n);
    }

    [Fact]
    public void Distribute_SquareOfXTimesX_DistributesThenMergesToXFourth()
    {
        // Distribution alone gives x^2 * x^2; merging must then collapse that to x^4 rather
        // than leaving the intermediate two-factor form as the fixed point.
        var expr = new Power(new Product(new SymbolicExpression[] { X, X }), N(2));

        C(expr).Should().Be(new Power(X, N(4)));
    }

    [Fact]
    public void Distribute_ReciprocalOfThreeFactorProduct_GivesThreeSeparatePowers()
    {
        var expr = new Power(new Product(new SymbolicExpression[] { X, Y, new Symbol("z") }), new Number(new Rational(-1)));

        var result = C(expr);
        result.Should().BeOfType<Product>();
        var factors = ((Product)result).Factors;
        factors.Should().HaveCount(3);
        foreach (var factor in factors)
        {
            factor.Should().BeOfType<Power>();
            ((Power)factor).Exponent.Should().Be(new Number(new Rational(-1)));
        }
    }

    [Fact]
    public void Distribute_NegativeIntegerExponentOtherThanMinusOne_DistributesToEachFactor()
    {
        var expr = new Power(new Product(new SymbolicExpression[] { X, Y }), new Number(new Rational(-3)));

        C(expr).Should().Be(new Product(new SymbolicExpression[]
        {
            new Power(X, new Number(new Rational(-3))),
            new Power(Y, new Number(new Rational(-3))),
        }));
    }

    [Fact]
    public void Distribute_NestedPowerOfDistributedProduct_FullyMergesExponents()
    {
        // ((x*y)^2)^3 -> x^6 * y^6.
        var expr = new Power(new Power(new Product(new SymbolicExpression[] { X, Y }), N(2)), N(3));

        C(expr).Should().Be(new Product(new SymbolicExpression[] { new Power(X, N(6)), new Power(Y, N(6)) }));
    }

    [Fact]
    public void Distribute_Idempotence_CanonicalizingTwiceGivesSameResult()
    {
        var expr = new Power(new Product(new SymbolicExpression[] { X, Y }), new Number(new Rational(-1)));

        var once = C(expr);
        var twice = Canonicalizer.Canonicalize(once);

        twice.Should().Be(once);
    }

    [Fact]
    public void Distribute_DeeplyNestedPowerOfProduct_DoesNotTriggerIterationGuard()
    {
        SymbolicExpression expr = new Product(new SymbolicExpression[] { X, Y });
        for (var i = 0; i < 30; i++)
            expr = new Power(expr, N(2));

        var act = () => C(expr);

        act.Should().NotThrow();
    }

    // --- Sorting / commutative equality ---

    [Fact]
    public void Sort_XPlusYAndYPlusX_ProduceIdenticalTrees()
    {
        var a = C(new Sum(new SymbolicExpression[] { X, Y }));
        var b = C(new Sum(new SymbolicExpression[] { Y, X }));

        a.Should().Be(b);
    }

    [Fact]
    public void Sort_XTimesYAndYTimesX_ProduceIdenticalTrees()
    {
        var a = C(new Product(new SymbolicExpression[] { X, Y }));
        var b = C(new Product(new SymbolicExpression[] { Y, X }));

        a.Should().Be(b);
    }

    [Fact]
    public void Sort_NumbersFirstThenSymbolsAlphabetically()
    {
        var expr = new Sum(new SymbolicExpression[] { Y, N(1), X });

        C(expr).Should().Be(new Sum(new SymbolicExpression[] { N(1), X, Y }));
    }

    // --- Arity reduction ---

    [Fact]
    public void ArityReduction_SingleTermSum_ReturnsTheTerm()
    {
        C(new Sum(new SymbolicExpression[] { X })).Should().Be(X);
    }

    [Fact]
    public void ArityReduction_EmptySum_ReturnsZero()
    {
        C(new Sum(Array.Empty<SymbolicExpression>())).Should().Be(N(0));
    }

    [Fact]
    public void ArityReduction_EmptyProduct_ReturnsOne()
    {
        C(new Product(Array.Empty<SymbolicExpression>())).Should().Be(N(1));
    }

    // --- Combination ---

    [Fact]
    public void Combination_ThreeXPlusFiveXPlusTwo_ReturnsEightXPlusTwo()
    {
        var expr = new Sum(new SymbolicExpression[]
        {
            new Product(new SymbolicExpression[] { N(3), X }),
            new Product(new SymbolicExpression[] { N(5), X }),
            N(2),
        });

        C(expr).Should().Be(new Sum(new SymbolicExpression[]
        {
            N(2),
            new Product(new SymbolicExpression[] { N(8), X }),
        }));
    }

    [Fact]
    public void Combination_XTimesXTimesX_ReturnsXCubed()
    {
        var expr = new Product(new SymbolicExpression[] { X, X, X });

        C(expr).Should().Be(new Power(X, N(3)));
    }

    // --- Idempotence ---

    [Fact]
    public void Idempotence_CanonicalizingTwice_GivesSameResult()
    {
        var expr = new Sum(new SymbolicExpression[]
        {
            new Product(new SymbolicExpression[] { N(2), X }),
            new Product(new SymbolicExpression[] { N(3), X }),
            Y,
            N(5),
        });

        var once = C(expr);
        var twice = Canonicalizer.Canonicalize(once);

        twice.Should().Be(once);
    }

    // --- Termination ---

    [Fact]
    public void Termination_DeeplyNestedSum_DoesNotLoop()
    {
        SymbolicExpression expr = X;
        for (var i = 0; i < 200; i++)
            expr = new Sum(new SymbolicExpression[] { expr, N(1) });

        var act = () => C(expr);

        act.Should().NotThrow();
    }

    [Fact]
    public void Termination_DeeplyNestedPowerOfPower_DoesNotLoop()
    {
        SymbolicExpression expr = X;
        for (var i = 0; i < 50; i++)
            expr = new Power(expr, N(2));

        var act = () => C(expr);

        act.Should().NotThrow();
    }

    [Fact]
    public void Termination_DeeplyNestedProduct_DoesNotLoop()
    {
        SymbolicExpression expr = X;
        for (var i = 0; i < 200; i++)
            expr = new Product(new SymbolicExpression[] { expr, Y });

        var act = () => C(expr);

        act.Should().NotThrow();
    }

    // --- Unexpanded products of sums (no "expand" in scope) ---

    [Fact]
    public void ProductOfSum_IsNotExpanded()
    {
        var expr = new Product(new SymbolicExpression[] { new Sum(new SymbolicExpression[] { X, Y }), N(2) });

        var result = C(expr);
        result.Should().BeOfType<Product>();
        ((Product)result).Factors.Should().Contain(f => f is Sum);
    }
}
