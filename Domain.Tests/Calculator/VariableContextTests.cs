namespace Domain.Tests.Calculator;

using Domain.Calculator;
using Domain.Calculator.Values;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class VariableContextTests
{
    [Fact]
    public void SetThenGet_NormalVariable_ReturnsStoredValue()
    {
        var context = new VariableContext();

        context.Set("x", new NumberValue(5));

        context.Get("x").Should().Be(new NumberValue(5));
    }

    [Fact]
    public void Set_ReassigningNormalVariable_IsAllowed()
    {
        var context = new VariableContext();
        context.Set("x", new NumberValue(5));

        context.Set("x", new NumberValue(10));

        context.Get("x").Should().Be(new NumberValue(10));
    }

    [Fact]
    public void Set_ReassigningConstantAfterSeed_ThrowsInvalidOperationException()
    {
        var context = new VariableContext();
        context.Seed(new Dictionary<string, Value> { ["_pi"] = new NumberValue(Math.PI) });

        var act = () => context.Set("_pi", new NumberValue(0));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*_pi*");
    }

    [Fact]
    public void Get_UndefinedName_ThrowsInvalidOperationException()
    {
        var context = new VariableContext();

        var act = () => context.Get("missing");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IsDefined_AfterSet_ReturnsTrue()
    {
        var context = new VariableContext();
        context.Set("x", new NumberValue(1));

        context.IsDefined("x").Should().BeTrue();
    }

    [Fact]
    public void IsDefined_UnknownName_ReturnsFalse()
    {
        var context = new VariableContext();

        context.IsDefined("unknown").Should().BeFalse();
    }

    [Fact]
    public void IsProtected_AfterSeed_ReturnsTrue()
    {
        var context = new VariableContext();
        context.Seed(new Dictionary<string, Value> { ["_pi"] = new NumberValue(Math.PI) });

        context.IsProtected("_pi").Should().BeTrue();
    }

    [Fact]
    public void IsProtected_NormalVariable_ReturnsFalse()
    {
        var context = new VariableContext();
        context.Set("x", new NumberValue(1));

        context.IsProtected("x").Should().BeFalse();
    }

    // ---- Scope chaining ----

    [Fact]
    public void Get_NameOnlyInParent_ResolvesThroughChild()
    {
        var parent = new VariableContext();
        parent.Set("x", new NumberValue(5));
        var child = parent.CreateChild();

        child.Get("x").Should().Be(new NumberValue(5));
    }

    [Fact]
    public void Bind_ParameterInChild_MasksParentVariableWithoutOverwritingIt()
    {
        var parent = new VariableContext();
        parent.Set("x", new NumberValue(1));
        var child = parent.CreateChild();

        child.Bind("x", new NumberValue(99));

        child.Get("x").Should().Be(new NumberValue(99));
        parent.Get("x").Should().Be(new NumberValue(1));
    }

    [Fact]
    public void Get_UndefinedInWholeChain_ThrowsInvalidOperationException()
    {
        var parent = new VariableContext();
        var child = parent.CreateChild();

        var act = () => child.Get("missing");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IsDefined_NameOnlyInParent_ReturnsTrueForChild()
    {
        var parent = new VariableContext();
        parent.Set("x", new NumberValue(1));
        var child = parent.CreateChild();

        child.IsDefined("x").Should().BeTrue();
    }

    [Fact]
    public void IsProtected_ConstantInParent_ReturnsTrueForChild()
    {
        var parent = new VariableContext();
        parent.Seed(new Dictionary<string, Value> { ["_pi"] = new NumberValue(Math.PI) });
        var child = parent.CreateChild();

        child.IsProtected("_pi").Should().BeTrue();
    }

    [Fact]
    public void Set_NameProtectedInParent_ThrowsInvalidOperationExceptionFromChild()
    {
        var parent = new VariableContext();
        parent.Seed(new Dictionary<string, Value> { ["_pi"] = new NumberValue(Math.PI) });
        var child = parent.CreateChild();

        var act = () => child.Set("_pi", new NumberValue(0));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Set_InChild_DoesNotAffectParentScope()
    {
        var parent = new VariableContext();
        var child = parent.CreateChild();

        child.Set("y", new NumberValue(7));

        child.Get("y").Should().Be(new NumberValue(7));
        parent.IsDefined("y").Should().BeFalse();
    }
}
