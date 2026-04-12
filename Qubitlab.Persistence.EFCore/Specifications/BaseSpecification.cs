using System.Linq.Expressions;

namespace Qubitlab.Persistence.EFCore.Specifications;

public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification()
    {
    }

    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public int Take { get; protected set; }
    public int Skip { get; protected set; }
    public bool IsPagingEnabled { get; protected set; }


    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }


    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

 
    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }


    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }


    protected virtual void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        if (Criteria == null)
        {
            Criteria = criteria;
        }
        else
        {
            var parameter = Expression.Parameter(typeof(T));
            
            var leftVisitor = new ReplaceExpressionVisitor(Criteria.Parameters[0], parameter);
            var left = leftVisitor.Visit(Criteria.Body);

            var rightVisitor = new ReplaceExpressionVisitor(criteria.Parameters[0], parameter);
            var right = rightVisitor.Visit(criteria.Body);

            Criteria = Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(left!, right!), parameter);
        }
    }

    private class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression? Visit(Expression? node)
        {
            return node == _oldValue ? _newValue : base.Visit(node);
        }
    }
}