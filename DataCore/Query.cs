using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DataCore
{
    public class Query<T>
    {
        private readonly ITranslator _translator;

        public string TableName { get; private set; }

        public bool Built { get; private set; }

        public string SqlSelectFormat { get; set; }
        public string SqlColumns { get; set; }
        public string SqlFrom { get; set; }
        public string SqlWhere { get; set; }
        public string SqlOrderBy { get; set; }
        public string SqlGroupBy { get; set; }
        public StringBuilder SqlCommand { get; private set; }

        public Query(ITranslator translator)
        {
            Built = false;

            _translator = translator;
            SqlSelectFormat = "{0}";
            SqlWhere = string.Empty;
            SqlCommand = new StringBuilder();
            SqlColumns = "*";
            SqlGroupBy = "";
            SqlOrderBy = "";
            TableName = typeof(T).Name;
            SqlFrom = _translator.GetTableName(TableName);
        }

        public Query<T> Build()
        {
            // run the query
            SqlCommand.Append("SELECT ");
            SqlCommand.AppendFormat(SqlSelectFormat, SqlColumns);
            SqlCommand.Append(" FROM ");
            SqlCommand.Append(SqlFrom);

            if (!string.IsNullOrWhiteSpace(SqlWhere))
                SqlCommand.AppendFormat(" WHERE {0}", SqlWhere);

            if (!string.IsNullOrWhiteSpace(SqlGroupBy))
                SqlCommand.AppendFormat(" GROUP BY {0}", SqlGroupBy);

            if (!string.IsNullOrWhiteSpace(SqlOrderBy))
                SqlCommand.AppendFormat(" ORDER BY {0}", SqlOrderBy);

            Built = true;

            return this;
        }

        public Query<T> Select(Expression<Func<T, dynamic>> clause)
        {
            if (SqlColumns == "*")
                SqlColumns = string.Empty;

            SqlColumns = FormatStringFromArguments(clause, SqlColumns);

            return this;
        }

        public Query<T> Top(int count)
        {
            _translator.Top(this, count);

            return this;
        }

        public Query<T> Where(Expression<Func<T, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));

            var query = GetQueryFromExpression(newExpression.Body);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") AND (", query, ")");

            return this;
        }

        public Query<T> Or(Expression<Func<T, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));

            var query = GetQueryFromExpression(newExpression.Body);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") OR (", query, ")");

            return this;
        }

        public Query<T> Join<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = GetQueryFromExpression(newExpression.Body);

            var joinedTableName = _translator.GetTableName(typeof(TJoined).Name);
            SqlFrom = string.Concat(SqlFrom, " INNER JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> Join<TJoinedLeft, TJoinedRight>(Expression<Func<TJoinedLeft, TJoinedRight, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = GetQueryFromExpression(newExpression.Body);

            var joinedTableName = _translator.GetTableName(typeof(TJoinedRight).Name);
            SqlFrom = string.Concat(SqlFrom, " INNER JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> LeftJoin<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = GetQueryFromExpression(newExpression.Body);

            var joinedTableName = _translator.GetTableName(typeof(TJoined).Name);
            SqlFrom = string.Concat(SqlFrom, " LEFT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> LeftJoin<TJoinedLeft, TJoinedRight>(Expression<Func<TJoinedLeft, TJoinedRight, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = GetQueryFromExpression(newExpression.Body);

            var joinedTableName = _translator.GetTableName(typeof(TJoinedRight).Name);
            SqlFrom = string.Concat(SqlFrom, " LEFT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> RightJoin<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = GetQueryFromExpression(newExpression.Body);

            var joinedTableName = _translator.GetTableName(typeof(TJoined).Name);
            SqlFrom = string.Concat(SqlFrom, " RIGHT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> RightJoin<TJoinedLeft, TJoinedRight>(Expression<Func<TJoinedLeft, TJoinedRight, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = GetQueryFromExpression(newExpression.Body);

            var joinedTableName = _translator.GetTableName(typeof(TJoinedRight).Name);
            SqlFrom = string.Concat(SqlFrom, " RIGHT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> OrderBy(Expression<Func<T, dynamic>> clause)
        {
            SqlOrderBy = FormatStringFromArguments(clause, SqlOrderBy);

            return this;
        }

        public Query<T> GroupBy(Expression<Func<T, dynamic>> clause)
        {
            SqlGroupBy = FormatStringFromArguments(clause, SqlGroupBy);

            return this;
        }

        private static Expression[] GetExpressionsFromDynamic(Expression<Func<T, dynamic>> clause)
        {
            Expression[] arguments = null;

            var newBody = clause.Body as NewExpression;
            if (newBody != null)
                arguments = ((NewExpression)clause.Body).Arguments.ToArray();

            var unaryBody = clause.Body as UnaryExpression;
            if (unaryBody != null)
                arguments = new[] { unaryBody.Operand };
            return arguments;
        }

        private string GetQueryFromExpression(Expression clause)
        {
            var members = GetMemberExpressions(clause);
            if (!members.Any())
                return string.Empty;

            var enumerator = members.GetEnumerator();
            return GetString(enumerator);
        }

        private string GetString(IEnumerator<Expression> iterator)
        {
            if (!iterator.MoveNext())
                return string.Empty;

            var expression = iterator.Current;

            var binaryExpression = expression as BinaryExpression;
            if (binaryExpression != null)
                return BinaryExpressionString(iterator, binaryExpression);

            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
                return MemberExpressionString(memberExpression);

            var constantExpression = expression as ConstantExpression;
            if (constantExpression != null)
                return ConstantExpressionString(constantExpression);

            return string.Empty;
        }

        private static string MemberExpressionString(MemberExpression memberExpression)
        {
            return string.Concat(memberExpression.Member.DeclaringType.Name, ".", memberExpression.Member.Name);
        }

        private string BinaryExpressionString(IEnumerator<Expression> iterator, BinaryExpression binaryExpression)
        {
            var format = _translator.GetFormatFor(binaryExpression.NodeType);

            var left = string.Empty;
            var right = string.Empty;

            if (iterator.MoveNext())
                left = GetQueryFromExpression(iterator.Current);

            if (iterator.MoveNext())
                right = GetQueryFromExpression(iterator.Current);

            return string.Format(format, left, right);
        }

        private string ConstantExpressionString(ConstantExpression constantExpression)
        {
            switch (constantExpression.Type.Name)
            {
                case "Boolean":
                    return _translator.GetBooleanValue(constantExpression.Value);
                case "String":
                    return _translator.GetStringValue(constantExpression.Value);
                case "DateTime":
                    {
                        var date = Convert.ToDateTime(constantExpression.Value);
                        return _translator.GetDateTimeValue(date);
                    }
                default:
                    return Convert.ToString(constantExpression.Value);
            }
        }

        private static IEnumerable<Expression> GetMemberExpressions(Expression body)
        {
            // A Queue preserves left to right reading order of expressions in the tree
            var candidates = new Queue<Expression>(new[] { body });
            while (candidates.Count > 0)
            {
                var expr = candidates.Dequeue();
                if (expr is MemberExpression)
                {
                    //var member = ((MemberExpression)expr).Member;
                    //var property = member as PropertyInfo;
                    //if (property != null && property.PropertyType.Name == "Boolean")
                    //{
                    //    candidates.Enqueue(Expression.MakeBinary(ExpressionType.Equal, expr, Expression.Constant(true)));
                    //    continue;
                    //}

                    yield return expr;
                }
                else if (expr is ConstantExpression)
                {
                    yield return expr;
                }
                else if (expr is UnaryExpression)
                {
                    var unary = (UnaryExpression)expr;
                    if (unary.NodeType == ExpressionType.Not)
                    {
                        var binaryExpression = unary.Operand as BinaryExpression;
                        if (binaryExpression != null)
                        {
                            if (unary.Operand.NodeType == ExpressionType.Equal)
                                candidates.Enqueue(Expression.MakeBinary(ExpressionType.NotEqual, binaryExpression.Left, binaryExpression.Right));
                            else
                                candidates.Enqueue(Expression.MakeBinary(ExpressionType.Equal, binaryExpression.Left, binaryExpression.Right));

                            continue;
                        }
                    }

                    candidates.Enqueue(unary.Operand);
                }
                else if (expr is BinaryExpression)
                {
                    var binary = (BinaryExpression)expr;
                    candidates.Enqueue(binary.Left);
                    candidates.Enqueue(binary.Right);

                    yield return expr;
                }
                else if (expr is LambdaExpression)
                {
                    candidates.Enqueue(((LambdaExpression)expr).Body);
                }
            }
        }

        private static string FormatStringFromArguments(Expression<Func<T, dynamic>> clause, string startString)
        {
            var returnString = startString;

            var arguments = GetExpressionsFromDynamic(clause);

            if (arguments != null && arguments.Length > 0)
            {
                if (!string.IsNullOrEmpty(returnString))
                    returnString += ", ";

                returnString += string.Join(", ",
                    arguments.Select(
                        f =>
                            string.Concat(((MemberExpression)f).Member.DeclaringType.Name, ".",
                                ((MemberExpression)f).Member.Name))
                );
            }

            return returnString;
        }
    }
}
