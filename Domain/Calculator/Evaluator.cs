namespace Domain.Calculator;

using Domain.Calculator.Ast;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Operations.SpecialForms;
using Domain.Calculator.Values;

public sealed class Evaluator
{
    private const int MaxCallDepth = 64;

    private readonly IReadOnlyDictionary<string, IFunction> _builtins;
    private readonly IReadOnlyDictionary<string, ISpecialForm> _specialForms;
    private readonly FunctionContext _functions;
    private int _callDepth;

    public Evaluator(
        IEnumerable<IFunction> builtins,
        IEnumerable<ISpecialForm> specialForms,
        FunctionContext functions)
    {
        _builtins = builtins.ToDictionary(f => f.Name);
        _specialForms = specialForms.ToDictionary(f => f.Name);
        _functions = functions;
    }

    public Value Evaluate(IExpression expression, VariableContext context) => expression switch
    {
        NumberExpression number           => number.Value,
        IdentifierExpression id           => context.Get(id.Name),
        AssignmentExpression assign       => Assign(assign, context),
        FunctionDefinitionExpression def  => DefineFunction(def),
        UnaryExpression unary             => unary.Operator.Apply(Evaluate(unary.Operand, context)),
        CallExpression call               => EvaluateCall(call, context),
        LogicalExpression logical         => EvaluateLogical(logical, context),
        NotExpression not                 => EvaluateNot(not, context),
        MatrixExpression matrix           => EvaluateMatrix(matrix, context),
        BinaryExpression binary           => binary.Operator.Apply(
            Evaluate(binary.Left, context),
            Evaluate(binary.Right, context)),
        _ => throw new NotSupportedException($"Unknown expression type: {expression.GetType().Name}")
    };

    private Value Assign(AssignmentExpression assign, VariableContext context)
    {
        var value = Evaluate(assign.Value, context);
        context.Set(assign.Name, value);
        return value;
    }

    private Value DefineFunction(FunctionDefinitionExpression definition)
    {
        if (_specialForms.ContainsKey(definition.Name) || _builtins.ContainsKey(definition.Name))
            throw new InvalidOperationException($"'{definition.Name}' is a built-in and cannot be redefined.");

        _functions.Define(new UserFunction(definition.Name, definition.Parameters, definition.Body));
        return new FunctionDefinedValue(definition.Name, definition.Parameters);
    }

    private Value EvaluateCall(CallExpression call, VariableContext context)
    {
        if (_specialForms.TryGetValue(call.Name, out var specialForm))
        {
            RequireArity(specialForm.Name, specialForm.Arity, call.Arguments.Count);
            return specialForm.Apply(call.Arguments, context, this);
        }

        if (_builtins.TryGetValue(call.Name, out var builtin))
        {
            RequireArity(builtin.Name, builtin.Arity, call.Arguments.Count);
            var args = call.Arguments.Select(a => Evaluate(a, context)).ToList();
            return builtin.Apply(args);
        }

        if (_functions.TryGet(call.Name, out var userFunction))
        {
            RequireArity(userFunction.Name, userFunction.Parameters.Count, call.Arguments.Count);

            if (_callDepth >= MaxCallDepth)
                throw new InvalidOperationException("Maximum call depth exceeded.");

            var args = call.Arguments.Select(a => Evaluate(a, context)).ToList();
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

        throw new InvalidOperationException($"Undefined function '{call.Name}'.");
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
