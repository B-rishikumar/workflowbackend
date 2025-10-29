public class WorkflowParameterReplacer : System.Linq.Expressions.ExpressionVisitor
{
    private readonly System.Linq.Expressions.ParameterExpression _from, _to;

    public WorkflowParameterReplacer(System.Linq.Expressions.ParameterExpression from, System.Linq.Expressions.ParameterExpression to)
    {
        _from = from;
        _to = to;
    }

    protected override System.Linq.Expressions.Expression VisitParameter(System.Linq.Expressions.ParameterExpression node)
    {
        return node == _from ? _to : base.VisitParameter(node);
    }
}