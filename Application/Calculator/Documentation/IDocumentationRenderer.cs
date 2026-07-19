namespace Application.Calculator.Documentation;

public interface IDocumentationRenderer
{
    string Render(IReadOnlyList<FunctionDoc> functions, IReadOnlyList<OperatorDoc> operators);
}
