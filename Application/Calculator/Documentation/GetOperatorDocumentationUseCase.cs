namespace Application.Calculator.Documentation;

using Domain.Calculator.Operations;

public sealed class GetOperatorDocumentationUseCase
{
    private readonly IReadOnlyList<OperatorDoc> _all;

    public GetOperatorDocumentationUseCase(IEnumerable<OperatorDocumentationEntry> operators)
    {
        _all = operators.Select(OperatorDoc.From).ToList();
    }

    public IReadOnlyList<OperatorDoc> Execute() => _all;
}
