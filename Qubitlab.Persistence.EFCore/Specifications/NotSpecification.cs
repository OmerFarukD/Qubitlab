using System.Linq.Expressions;

namespace Qubitlab.Persistence.EFCore.Specifications;

public class NotSpecification<T> : BaseSpecification<T>
{
    public NotSpecification(ISpecification<T> specification)
    {
        if (specification.Criteria != null)
        {
            var original = specification.Criteria;
            // Expression.Not ile orijinal lambda'nın gövdesini tersine çeviriyoruz
            Criteria = Expression.Lambda<Func<T, bool>>(
                Expression.Not(original.Body),
                original.Parameters);
        }

        Includes.AddRange(specification.Includes);
        IncludeStrings.AddRange(specification.IncludeStrings);
    }
}