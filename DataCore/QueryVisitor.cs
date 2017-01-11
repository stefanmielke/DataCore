using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataCore
{
    public class QueryVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var expression = Visit(memberExpression.Expression);
            if (expression is ConstantExpression)
            {
                object container = ((ConstantExpression)expression).Value;
                var member = memberExpression.Member;
                if (member is FieldInfo)
                {
                    object value = ((FieldInfo)member).GetValue(container);
                    return Expression.Constant(value);
                }
                if (member is PropertyInfo)
                {
                    object value = ((PropertyInfo)member).GetValue(container, null);
                    return Expression.Constant(value);
                }
            }

            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var args = node.Arguments.Select(a => ((ConstantExpression)a).Value).ToArray();

            var newObject = node.Constructor.Invoke(args);

            return Expression.Constant(newObject);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var obj = node.Object == null ? null : ((ConstantExpression)node.Object).Value;
            var args = node.Arguments.Select(
                a =>
                {
                    ConstantExpression constExpr;

                    var memberExpr = a as MemberExpression;
                    if (memberExpr != null)
                        constExpr = VisitMember(memberExpr) as ConstantExpression;
                    else
                        constExpr = a as ConstantExpression;
                    
                    return constExpr.Value;
                }
            ).ToArray();

            var newObject = node.Method.Invoke(obj, args);

            return Expression.Constant(newObject);
        }
    }
}
