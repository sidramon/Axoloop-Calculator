namespace Domain.Tests.Calculator.Operations.Functions;

using Domain.Calculator;
using Domain.Calculator.Operations.Functions.Random;
using Domain.Calculator.Random;
using Domain.Calculator.Values;
using Domain.Tests.Calculator.TestHelpers;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class RandomFunctionTests
{
    private static double[] Vector(MatrixValue m)
    {
        var values = new double[m.Columns];
        for (var i = 0; i < m.Columns; i++) values[i] = m[0, i];
        return values;
    }

    // ---- rand: wiring via a scripted source ----

    [Fact]
    public void Rand_NoArguments_ReturnsExactlyWhatTheSourceProduces()
    {
        var source = new ScriptedRandomSource(doubles: new[] { 0.3141592 });

        var result = (NumberValue)new RandFunction(source).Apply(Array.Empty<Value>());

        result.Number.Should().Be(0.3141592);
    }

    [Fact]
    public void Rand_WithCount_ReturnsAVectorConsumingOneDrawPerElementInOrder()
    {
        var source = new ScriptedRandomSource(doubles: new[] { 0.1, 0.2, 0.3 });

        var result = (MatrixValue)new RandFunction(source, hasCount: true).Apply(new Value[] { new NumberValue(3) });

        result.Rows.Should().Be(1);
        result.Columns.Should().Be(3);
        Vector(result).Should().Equal(0.1, 0.2, 0.3);
    }

    [Fact]
    public void Rand_NonPositiveCount_Throws()
    {
        var source = new ScriptedRandomSource();

        var act = () => new RandFunction(source, hasCount: true).Apply(new Value[] { new NumberValue(0) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- randint: bounds forwarded inclusive, min > max rejected ----

    [Fact]
    public void RandInt_ForwardsMinAndMaxToSource_AndReturnsWhateverItProduces()
    {
        // The source is scripted to return exactly the lower bound, then exactly the
        // upper bound -- proving randint doesn't shift or clamp what the source gives it
        // (no off-by-one from treating an inclusive source as exclusive or vice versa).
        var source = new ScriptedRandomSource(ints: new[] { 1, 6 });
        var fn = new RandIntFunction(source);

        ((NumberValue)fn.Apply(new Value[] { new NumberValue(1), new NumberValue(6) })).Number.Should().Be(1);
        ((NumberValue)fn.Apply(new Value[] { new NumberValue(1), new NumberValue(6) })).Number.Should().Be(6);
    }

    [Fact]
    public void RandInt_WithCount_ReturnsVectorOfSourceValues()
    {
        var source = new ScriptedRandomSource(ints: new[] { 4, 2, 6 });

        var result = (MatrixValue)new RandIntFunction(source, hasCount: true)
            .Apply(new Value[] { new NumberValue(1), new NumberValue(6), new NumberValue(3) });

        Vector(result).Should().Equal(4, 2, 6);
    }

    [Fact]
    public void RandInt_MinGreaterThanMax_Throws()
    {
        var source = new ScriptedRandomSource();

        var act = () => new RandIntFunction(source).Apply(new Value[] { new NumberValue(6), new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RandInt_DegenerateEqualBounds_Allowed()
    {
        var source = new ScriptedRandomSource(ints: new[] { 5 });

        var result = (NumberValue)new RandIntFunction(source).Apply(new Value[] { new NumberValue(5), new NumberValue(5) });

        result.Number.Should().Be(5);
    }

    [Fact]
    public void RandInt_WithCountNonPositive_Throws()
    {
        var source = new ScriptedRandomSource();

        var act = () => new RandIntFunction(source, hasCount: true).Apply(
            new Value[] { new NumberValue(1), new NumberValue(6), new NumberValue(-1) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- randnorm: mu/sigma forwarded, sigma validated ----

    [Fact]
    public void RandNorm_ForwardsMeanAndStdDevToSource()
    {
        var source = new ScriptedRandomSource(gaussians: new[] { 7.5 });

        var result = (NumberValue)new RandNormFunction(source)
            .Apply(new Value[] { new NumberValue(5), new NumberValue(2) });

        result.Number.Should().Be(7.5);
    }

    [Fact]
    public void RandNorm_WithCount_ReturnsVectorOfSourceValues()
    {
        var source = new ScriptedRandomSource(gaussians: new[] { 1.0, 2.0, 3.0 });

        var result = (MatrixValue)new RandNormFunction(source, hasCount: true)
            .Apply(new Value[] { new NumberValue(0), new NumberValue(1), new NumberValue(3) });

        Vector(result).Should().Equal(1.0, 2.0, 3.0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RandNorm_NonPositiveSigma_Throws(double sigma)
    {
        var source = new ScriptedRandomSource();

        var act = () => new RandNormFunction(source).Apply(new Value[] { new NumberValue(0), new NumberValue(sigma) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- randbin: counts successes against p via NextDouble ----

    [Fact]
    public void RandBin_CountsDrawsBelowP_AsSuccesses()
    {
        // p = 0.5: draws < 0.5 succeed. Scripted: succeed, fail, succeed, fail, succeed -> 3.
        var source = new ScriptedRandomSource(doubles: new[] { 0.1, 0.9, 0.2, 0.8, 0.3 });

        var result = (NumberValue)new RandBinFunction(source)
            .Apply(new Value[] { new NumberValue(5), new NumberValue(0.5) });

        result.Number.Should().Be(3);
    }

    [Fact]
    public void RandBin_NonPositiveN_Throws()
    {
        var source = new ScriptedRandomSource();

        var act = () => new RandBinFunction(source).Apply(new Value[] { new NumberValue(0), new NumberValue(0.5) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void RandBin_POutOfRange_Throws(double p)
    {
        var source = new ScriptedRandomSource();

        var act = () => new RandBinFunction(source).Apply(new Value[] { new NumberValue(5), new NumberValue(p) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- randsamp: without replacement (default) ----

    [Fact]
    public void RandSamp_WithoutReplacement_NoDuplicatesAndAllFromSourceVector()
    {
        var source = new SystemRandomSource();
        source.Seed(11);
        var v = new MatrixValue(new double[,] { { 10, 20, 30, 40, 50 } });

        var result = (MatrixValue)new RandSampFunction(source).Apply(new Value[] { v, new NumberValue(3) });

        var drawn = Vector(result);
        drawn.Should().HaveCount(3);
        drawn.Should().OnlyHaveUniqueItems();
        drawn.Should().OnlyContain(x => new[] { 10.0, 20, 30, 40, 50 }.Contains(x));
    }

    [Fact]
    public void RandSamp_WithoutReplacement_KEqualsVectorLength_ReturnsAPermutationOfTheWholeVector()
    {
        var source = new SystemRandomSource();
        source.Seed(12);
        var v = new MatrixValue(new double[,] { { 1, 2, 3, 4 } });

        var result = (MatrixValue)new RandSampFunction(source).Apply(new Value[] { v, new NumberValue(4) });

        Vector(result).Should().BeEquivalentTo(new double[] { 1, 2, 3, 4 });
    }

    [Fact]
    public void RandSamp_WithoutReplacement_KGreaterThanVectorLength_Throws()
    {
        var source = new SystemRandomSource();
        var v = new MatrixValue(new double[,] { { 1, 2, 3 } });

        var act = () => new RandSampFunction(source).Apply(new Value[] { v, new NumberValue(5) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RandSamp_WithReplacement_KCanExceedVectorLength()
    {
        var source = new SystemRandomSource();
        source.Seed(13);
        var v = new MatrixValue(new double[,] { { 1, 2, 3 } });

        var result = (MatrixValue)new RandSampFunction(source, hasReplacementArgument: true)
            .Apply(new Value[] { v, new NumberValue(10), new NumberValue(1) });

        Vector(result).Should().HaveCount(10);
        Vector(result).Should().OnlyContain(x => x == 1 || x == 2 || x == 3);
    }

    [Fact]
    public void RandSamp_ReplacementFlagZero_StillSamplesWithoutDuplicates()
    {
        var source = new SystemRandomSource();
        source.Seed(14);
        var v = new MatrixValue(new double[,] { { 1, 2, 3 } });

        var result = (MatrixValue)new RandSampFunction(source, hasReplacementArgument: true)
            .Apply(new Value[] { v, new NumberValue(3), new NumberValue(0) });

        Vector(result).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void RandSamp_ReplacementFlagZero_StillRejectsKGreaterThanVectorLength()
    {
        // k=5 > length=3 is only ever rejected under without-replacement rules -- proves
        // a 0 flag activates that mode rather than "with replacement" (which would allow it).
        var source = new SystemRandomSource();
        var v = new MatrixValue(new double[,] { { 1, 2, 3 } });

        var act = () => new RandSampFunction(source, hasReplacementArgument: true)
            .Apply(new Value[] { v, new NumberValue(5), new NumberValue(0) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RandSamp_NonPositiveK_Throws()
    {
        var source = new SystemRandomSource();
        var v = new MatrixValue(new double[,] { { 1, 2, 3 } });

        var act = () => new RandSampFunction(source).Apply(new Value[] { v, new NumberValue(0) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- seed: return value and effect on the shared source ----

    [Fact]
    public void Seed_ReturnsTheSeedItself()
    {
        var source = new ScriptedRandomSource();

        var result = (NumberValue)new SeedFunction(source).Apply(new Value[] { new NumberValue(42) });

        result.Number.Should().Be(42);
    }

    [Fact]
    public void Seed_CallsSeedOnTheInjectedSource()
    {
        var source = new ScriptedRandomSource();

        new SeedFunction(source).Apply(new Value[] { new NumberValue(42) });

        source.LastSeed.Should().Be(42);
        source.SeedCallCount.Should().Be(1);
    }

    [Fact]
    public void Seed_NonIntegerArgument_Throws()
    {
        var source = new ScriptedRandomSource();

        var act = () => new SeedFunction(source).Apply(new Value[] { new NumberValue(4.5) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Shared source: the property that makes seed() meaningful at all ----

    [Fact]
    public void SharedSource_SeedThenRand_IsReproducibleAcrossTwoIndependentFunctionInstances()
    {
        // Two SEPARATE RandFunction/SeedFunction objects, but constructed over the SAME
        // IRandomSource instance -- exactly how Program.cs wires the composition root.
        // If seed(k) only affected its own function instance rather than the shared
        // source, this would fail.
        var source = new SystemRandomSource();
        var seed = new SeedFunction(source);
        var rand = new RandFunction(source);

        seed.Apply(new Value[] { new NumberValue(2024) });
        var first = ((NumberValue)rand.Apply(Array.Empty<Value>())).Number;

        seed.Apply(new Value[] { new NumberValue(2024) });
        var second = ((NumberValue)rand.Apply(Array.Empty<Value>())).Number;

        first.Should().Be(second);
    }

    [Fact]
    public void SharedSource_SeedAffectsRandintAndRandnormToo_NotJustRand()
    {
        var source = new SystemRandomSource();
        var seed = new SeedFunction(source);
        var randInt = new RandIntFunction(source);
        var randNorm = new RandNormFunction(source);

        seed.Apply(new Value[] { new NumberValue(555) });
        var int1 = ((NumberValue)randInt.Apply(new Value[] { new NumberValue(1), new NumberValue(1000) })).Number;
        var norm1 = ((NumberValue)randNorm.Apply(new Value[] { new NumberValue(0), new NumberValue(1) })).Number;

        seed.Apply(new Value[] { new NumberValue(555) });
        var int2 = ((NumberValue)randInt.Apply(new Value[] { new NumberValue(1), new NumberValue(1000) })).Number;
        var norm2 = ((NumberValue)randNorm.Apply(new Value[] { new NumberValue(0), new NumberValue(1) })).Number;

        int1.Should().Be(int2);
        norm1.Should().Be(norm2);
    }

    // ---- Distribution tests through the real evaluator, seeded for determinism ----

    [Fact]
    public void Distribution_MeanOfManyRandDraws_IsCloseToOneHalf()
    {
        var parser = ParserFactory.CreateDefault();
        var source = new SystemRandomSource();
        var evaluator = EvaluatorFactory.CreateDefault(new FunctionContext(), source);
        var context = new VariableContext();
        context.Seed(Constants.All);

        evaluator.Evaluate(parser.Parse("seed(2026)"), context);
        var sample = evaluator.Evaluate(parser.Parse("rand(10000)"), context);
        var mean = evaluator.Evaluate(parser.Parse("mean(x)"), Bind(context, "x", sample));

        ((NumberValue)mean).Number.Should().BeApproximately(0.5, 0.02);
    }

    [Fact]
    public void Distribution_MeanAndStdOfRandNormDraws_MatchRequestedParameters()
    {
        var parser = ParserFactory.CreateDefault();
        var source = new SystemRandomSource();
        var evaluator = EvaluatorFactory.CreateDefault(new FunctionContext(), source);
        var context = new VariableContext();
        context.Seed(Constants.All);

        evaluator.Evaluate(parser.Parse("seed(2026)"), context);
        var sample = evaluator.Evaluate(parser.Parse("randnorm(5, 2, 10000)"), context);
        var withSample = Bind(context, "x", sample);
        var mean = ((NumberValue)evaluator.Evaluate(parser.Parse("mean(x)"), withSample)).Number;
        var stdDev = ((NumberValue)evaluator.Evaluate(parser.Parse("std(x, 1)"), withSample)).Number;

        mean.Should().BeApproximately(5, 0.1);
        stdDev.Should().BeApproximately(2, 0.1);
    }

    private static VariableContext Bind(VariableContext context, string name, Value value)
    {
        var child = context.CreateChild();
        child.Bind(name, value);
        return child;
    }
}
