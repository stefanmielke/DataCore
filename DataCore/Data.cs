using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DataCore
{
    public class Data<T>
    {
        public string SqlSelectFormat { get; private set; }
        public StringBuilder SqlColumns { get; private set; }
        public StringBuilder SqlWhere { get; private set; }
        public StringBuilder SqlCommand { get; private set; }

        public Data()
        {
            SqlSelectFormat = "{0}";
            SqlWhere = new StringBuilder();
            SqlCommand = new StringBuilder();
            SqlColumns = new StringBuilder("*");
        }

        public Data<T> Select()
        {
            // run the query
            SqlCommand.Append("SELECT ");
            SqlCommand.AppendFormat(SqlSelectFormat, SqlColumns);
            SqlCommand.Append(" FROM ");
            SqlCommand.Append(typeof(T).Name);

            var where = SqlWhere.ToString();
            if (!string.IsNullOrWhiteSpace(where))
                SqlCommand.AppendFormat(" WHERE {0}", where);

            return this;
        }

        public Data<T> Top(int count)
        {
            SqlSelectFormat = "TOP (" + count + ") {0}";

            return this;
        }

        public Data<T> Where(Expression<Func<T, bool>> clause)
        {
            var members = GetMemberExpressions(clause.Body);

            string nextFormat = "{0}";
            foreach (var member in members)
            {
                var binaryExpression = member as BinaryExpression;
                if (binaryExpression != null)
                {
                    switch (binaryExpression.NodeType)
                    {
                        case ExpressionType.Equal:
                            nextFormat = "{0} = ";
                            break;
                        case ExpressionType.GreaterThan:
                            nextFormat = "{0} > ";
                            break;
                        case ExpressionType.GreaterThanOrEqual:
                            nextFormat = "{0} >= ";
                            break;
                        case ExpressionType.LessThan:
                            nextFormat = "{0} < ";
                            break;
                        case ExpressionType.LessThanOrEqual:
                            nextFormat = "{0} <= ";
                            break;
                        default:
                            nextFormat = "{0}";
                            break;
                    }
                }
                var memberExpression = member as MemberExpression;
                if (memberExpression != null)
                {
                    SqlWhere.AppendFormat(nextFormat, memberExpression.Member.Name);
                    nextFormat = "{0}";
                }
                var constantExpression = member as ConstantExpression;
                if (constantExpression != null)
                {
                    SqlWhere.AppendFormat(nextFormat, constantExpression.Value);
                    nextFormat = "{0}";
                }
            }

            return this;
        }

        private static IEnumerable<Expression> GetMemberExpressions(Expression body)
        {
            // A Queue preserves left to right reading order of expressions in the tree
            var candidates = new Queue<Expression>(new[] { body });
            while (candidates.Count > 0)
            {
                var expr = candidates.Dequeue();
                if (expr is MemberExpression || expr is ConstantExpression)
                {
                    yield return expr;
                }
                else if (expr is UnaryExpression)
                {
                    candidates.Enqueue(((UnaryExpression)expr).Operand);
                }
                else if (expr is BinaryExpression)
                {
                    var binary = expr as BinaryExpression;
                    candidates.Enqueue(binary.Left);
                    candidates.Enqueue(binary.Right);

                    yield return expr;
                }
                else if (expr is MethodCallExpression)
                {
                    var method = expr as MethodCallExpression;
                    foreach (var argument in method.Arguments)
                    {
                        candidates.Enqueue(argument);
                    }
                }
                else if (expr is LambdaExpression)
                {
                    candidates.Enqueue(((LambdaExpression)expr).Body);
                }
            }
        }
    }
}
