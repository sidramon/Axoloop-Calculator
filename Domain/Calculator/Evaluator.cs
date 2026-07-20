namespace Domain.Calculator;

using Domain.Calculator.Ast;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Operations.SpecialForms;
using Domain.Calculator.Values;

public sealed class Evaluator
{
    private const int MaxCallDepth = 64;

    private readonly IReadOnlyDictionary<(string Name, int Arity), IFunction> _builtins;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<int>> _builtinArities;
    private readonly IReadOnlyDictionary<(string Name, int Arity), ISpecialForm> _specialForms;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<int>> _specialFormArities;
    private readonly FunctionContext _functions;
    private int _callDepth;

    public Evaluator(
        IEnumerable<IFunction> builtins,
        IEnumerable<ISpecialForm> specialForms,
        FunctionContext functions)
    {
        var builtinList = builtins.ToList();
        _builtins = builtinList.ToDictionary(f => (f.Name, f.Arity));
        _builtinArities = builtinList
            .GroupBy(f => f.Name)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<int>)g.Select(f => f.Arity).OrderBy(a => a).ToList());

        var specialFormList = specialForms.ToList();
        _specialForms = specialFormList.ToDictionary(f => (f.Name, f.Arity));
        _specialFormArities = specialFormList
            .GroupBy(f => f.Name)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<int>)g.Select(f => f.Arity).OrderBy(a => a).ToList());

        _functions = functions;
    }

    public Value Evaluate(IExpression expression, VariableContext context) => expression switch
    {
        NumberExpression number           => number.Value,
        IdentifierExpression id           => ResolveIdentifier(id.Name, context),
        AssignmentExpression assign       => Assign(assign, context),
        FunctionDefinitionExpression def  => DefineFunction(def),
        UnaryExpression unary             => unary.Operator.Apply(Evaluate(unary.Operand, context)),
        CallExpression call               => EvaluateCall(call, context),
        InvokeExpression invoke           => EvaluateInvoke(invoke, context),
        LogicalExpression logical         => EvaluateLogical(logical, context),
        NotExpression not                 => EvaluateNot(not, context),
        MatrixExpression matrix           => EvaluateMatrix(matrix, context),
        BinaryExpression binary           => binary.Operator.Apply(
            Evaluate(binary.Left, context),
            Evaluate(binary.Right, context)),
        _ => throw new NotSupportedException($"Unknown expression type: {expression.GetType().Name}")
    };

    private Value ResolveIdentifier(string name, VariableContext context)
    {
        if (context.IsDefined(name))
            return context.Get(name);

        if (_specialFormArities.ContainsKey(name))
            throw new InvalidOperationException(
                $"'{name}' is a special form and can only be used as a call, e.g. '{name}(...)'.");

        if (_functions.TryGet(name, out var userFunction))
        {
            var signature = $"{userFunction.Name}({string.Join(", ", userFunction.Parameters)})";
            return new FunctionValue(
                userFunction.Name,
                userFunction.Parameters.Count,
                signature,
                args => InvokeUserFunction(userFunction, args, context));
        }

        if (_builtinArities.TryGetValue(name, out var builtinArities))
        {
            if (builtinArities.Count > 1)
                throw new InvalidOperationException(
                    $"'{name}' has multiple overloads ({string.Join(", ", builtinArities)} argument(s)) " +
                    $"and cannot be used as a bare value; call it directly, e.g. '{name}(...)'.");

            var builtin = _builtins[(name, builtinArities[0])];
            return new FunctionValue(builtin.Name, builtin.Arity, builtin.Signature, builtin.Apply);
        }

        throw new InvalidOperationException($"Undefined variable '{name}'.");
    }

    private Value Assign(AssignmentExpression assign, VariableContext context)
    {
        var value = Evaluate(assign.Value, context);
        context.Set(assign.Name, value);
        return value;
    }

    private Value DefineFunction(FunctionDefinitionExpression definition)
    {
        if (_specialFormArities.ContainsKey(definition.Name) || _builtinArities.ContainsKey(definition.Name))
            throw new InvalidOperationException($"'{definition.Name}' is a built-in and cannot be redefined.");

        _functions.Define(new UserFunction(definition.Name, definition.Parameters, definition.Body));
        return new FunctionDefinedValue(definition.Name, definition.Parameters);
    }

    private Value EvaluateCall(CallExpression call, VariableContext context)
    {
        if (_specialFormArities.TryGetValue(call.Name, out var arities))
        {
            if (!_specialForms.TryGetValue((call.Name, call.Arguments.Count), out var specialForm))
                throw new InvalidOperationException(
                    $"'{call.Name}' expects {string.Join(" or ", arities)} argument(s) but got {call.Arguments.Count}.");
            return specialForm.Apply(call.Arguments, context, this);
        }

        if (_builtinArities.TryGetValue(call.Name, out var callArities))
        {
            if (!_builtins.TryGetValue((call.Name, call.Arguments.Count), out var builtin))
                throw new InvalidOperationException(
                    $"'{call.Name}' expects {string.Join(" or ", callArities)} argument(s) but got {call.Arguments.Count}.");

            var args = call.Arguments.Select(a => Evaluate(a, context)).ToList();
            return builtin.Apply(args);
        }

        if (_functions.TryGet(call.Name, out var userFunction))
        {
            RequireArity(userFunction.Name, userFunction.Parameters.Count, call.Arguments.Count);

            if (_callDepth >= MaxCallDepth)
                throw new InvalidOperationException("Maximum call depth exceeded.");

            var args = call.Arguments.Select(a => Evaluate(a, context)).ToList();
            return InvokeUserFunctionBody(userFunction, args, context);
        }

        throw new InvalidOperationException($"Undefined function '{call.Name}'.");
    }

    private Value EvaluateInvoke(InvokeExpression invoke, VariableContext context)
    {
        var target = Evaluate(invoke.Target, context);

        if (target is not FunctionValue function)
            throw new InvalidOperationException($"Cannot invoke a {TypeName(target)}.");

        var args = invoke.Arguments.Select(a => Evaluate(a, context)).ToList();

        // FunctionValue.Invoke checks arity itself. For a user function, the delegate it
        // wraps is InvokeUserFunction below, which already enforces the call-depth guard —
        // no separate bookkeeping is needed here to cover this path.
        return function.Invoke(args);
    }

    private static string TypeName(Value value) => value switch
    {
        NumberValue   => "number",
        BooleanValue  => "boolean",
        MatrixValue   => "matrix",
        SolutionValue => "solution",
        _ => "value"
    };

    private Value InvokeUserFunction(UserFunction userFunction, IReadOnlyList<Value> args, VariableContext context)
    {
        if (_callDepth >= MaxCallDepth)
            throw new InvalidOperationException("Maximum call depth exceeded.");

        return InvokeUserFunctionBody(userFunction, args, context);
    }

    private Value InvokeUserFunctionBody(UserFunction userFunction, IReadOnlyList<Value> args, VariableContext context)
    {
        var scope = context.CreateChild();
        for (var i = 0; i < userFunction.Parameters.Count; i++)
            scope.Bind(userFunction.Parameters[i], args[i]);

        _callDepth++;
        try
        {
            return Evaluate(userFunction.Body, scope);
        }
        finally
        {
            _callDepth--;
        }
    }

    private static void RequireArity(string name, int expected, int actual)
    {
        if (expected != actual)
            throw new InvalidOperationException($"'{name}' expects {expected} argument(s) but got {actual}.");
    }

    private Value EvaluateLogical(LogicalExpression logical, VariableContext context)
    {
        var symbol = logical.Operator == LogicalOperator.And ? "and" : "or";
        var left = RequireBoolean(Evaluate(logical.Left, context), symbol);

        if (logical.Operator == LogicalOperator.And && !left) return new BooleanValue(false);
        if (logical.Operator == LogicalOperator.Or && left) return new BooleanValue(true);

        var right = RequireBoolean(Evaluate(logical.Right, context), symbol);
        return new BooleanValue(right);
    }

    private Value EvaluateNot(NotExpression not, VariableContext context)
    {
        var operand = RequireBoolean(Evaluate(not.Operand, context), "not");
        return new BooleanValue(!operand);
    }

    private static bool RequireBoolean(Value value, string operatorSymbol) => value is BooleanValue b
        ? b.Boolean
        : throw new InvalidOperationException($"'{operatorSymbol}' requires booleans.");

    private Value EvaluateMatrix(MatrixExpression matrix, VariableContext context)
    {
        var rows = matrix.Rows.Count;
        var cols = matrix.Rows[0].Count;
        var data = new double[rows, cols];

        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                var value = Evaluate(matrix.Rows[r][c], context);
                if (value is not NumberValue n)
                    throw new InvalidOperationException("Matrix elements must be numbers.");
                data[r, c] = n.Number;
            }
        }

        return new MatrixValue(data);
    }
}
