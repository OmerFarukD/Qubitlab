using System.Linq.Expressions;

namespace Qubitlab.Persistence.EFCore.Specifications;

public class OrSpecification<T> : BaseSpecification<T>
{
    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        if (left.Criteria != null && right.Criteria != null)
        {
            // Parametreleri tek bir parametreye eşleştirerek OrElse ile birleştiriyoruz
            var parameter = Expression.Parameter(typeof(T), "x");

            var leftBody  = ReplaceParameter(left.Criteria.Body,  left.Criteria.Parameters[0],  parameter);
            var rightBody = ReplaceParameter(right.Criteria.Body, right.Criteria.Parameters[0], parameter);

            Criteria = Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(leftBody, rightBody), parameter);
        }
        else if (left.Criteria != null)
        {
            Criteria = left.Criteria;
        }
        else if (right.Criteria != null)
        {
            Criteria = right.Criteria;
        }

        Includes.AddRange(left.Includes);
        Includes.AddRange(right.Includes);
        IncludeStrings.AddRange(left.IncludeStrings);
        IncludeStrings.AddRange(right.IncludeStrings);
    }

    private static Expression ReplaceParameter(Expression body, ParameterExpression oldParam, ParameterExpression newParam)
    {
        return new ParameterReplacer(oldParam, newParam).Visit(body)!;
    }

    private sealed class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}