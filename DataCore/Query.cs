using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Text;

namespace DataCore
{
    public interface IQuery
    {
        IQuery Build();

        bool Built { get; }
        StringBuilder SqlCommand { get; }
        Parameters Parameters { get; }
    }

    public class Query<T> : IQuery
    {
        private readonly ITranslator _translator;
        private TableDefinition _tableDefinition;

        public bool Built { get; private set; }

        public string SqlSelectFormat { get; set; }
        public string SqlColumns { get; set; }
        public string SqlFrom { get; set; }
        public string SqlWhere { get; set; }
        public string SqlOrderBy { get; set; }
        public string SqlHaving { get; set; }
        public string SqlGroupBy { get; set; }
        public string SqlEnd { get; set; }
        public StringBuilder SqlCommand { get; private set; }

        private readonly List<IQuery> _unionQueries;
        private readonly List<IQuery> _unionAllQueries;

        public Parameters Parameters { get; private set; }

        public Query(ITranslator translator)
        {
            Built = false;
            
            _translator = translator;
            _tableDefinition = new TableDefinition(typeof(T));

            SqlSelectFormat = "{0}";
            SqlWhere = string.Empty;
            SqlCommand = new StringBuilder();
            SqlColumns = "*";
            SqlGroupBy = "";
            SqlOrderBy = "";
            SqlEnd = "";
            SqlFrom = _translator.GetSelectTableName(_tableDefinition);

            Parameters = new Parameters();

            _unionQueries = new List<IQuery>();
            _unionAllQueries = new List<IQuery>();
        }

        public IQuery Build()
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

            if (!string.IsNullOrWhiteSpace(SqlHaving))
                SqlCommand.AppendFormat(" HAVING {0}", SqlHaving);

            if (!string.IsNullOrWhiteSpace(SqlOrderBy))
                SqlCommand.AppendFormat(" ORDER BY {0}", SqlOrderBy);

            if (!string.IsNullOrWhiteSpace(SqlEnd))
                SqlCommand.Append(" ").Append(SqlEnd);
            
            foreach (var unionQuery in _unionQueries)
            {
                SqlCommand.Append(" UNION ").Append(unionQuery.Build().SqlCommand);
            }
            foreach (var unionQuery in _unionAllQueries)
            {
                SqlCommand.Append(" UNION ALL ").Append(unionQuery.Build().SqlCommand);
            }

            Built = true;

            return this;
        }

        public Query<T> Select(Expression<Func<T, dynamic>> clause)
        {
            if (SqlColumns == "*")
                SqlColumns = string.Empty;

            SqlColumns = ExpressionHelper.FormatStringFromArguments(_translator, clause, SqlColumns, Parameters);

            return this;
        }

        public Query<T> Top(int count)
        {
            _translator.Top(this, count);

            return this;
        }

        public Query<T> Count()
        {
            _translator.Count(this);

            return this;
        }

        public Query<T> Where(Expression<Func<T, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") AND (", query, ")");

            return this;
        }

        public Query<T> Where<TJoined>(Expression<Func<TJoined, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") AND (", query, ")");

            return this;
        }

        public Query<T> Where<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") AND (", query, ")");

            return this;
        }

        public Query<T> Where<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") AND (", query, ")");

            return this;
        }

        public Query<T> Where(string clause)
        {
            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? clause : string.Concat("(", SqlWhere, ") AND (", clause, ")");

            return this;
        }

        public Query<T> And(Expression<Func<T, bool>> clause)
        {
            return Where(clause);
        }

        public Query<T> And<TJoined>(Expression<Func<TJoined, bool>> clause)
        {
            return Where(clause);
        }

        public Query<T> And<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            return Where<T, TJoined>(clause);
        }

        public Query<T> And<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> clause)
        {
            return Where(clause);
        }

        public Query<T> And(string clause)
        {
            return Where(clause);
        }

        public Query<T> Or(Expression<Func<T, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") OR (", query, ")");

            return this;
        }

        public Query<T> Or<TJoined>(Expression<Func<TJoined, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") OR (", query, ")");

            return this;
        }

        public Query<T> Or<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") OR (", query, ")");

            return this;
        }

        public Query<T> Or<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") OR (", query, ")");

            return this;
        }

        public Query<T> Or(string clause)
        {
            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? clause : string.Concat("(", SqlWhere, ") OR (", clause, ")");

            return this;
        }

        public Query<T> Join<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            var tableDefinition = new TableDefinition(typeof(TJoined));

            var joinedTableName = _translator.GetSelectTableName(tableDefinition);
            SqlFrom = string.Concat(SqlFrom, " INNER JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> Join<TJoinedLeft, TJoinedRight>(Expression<Func<TJoinedLeft, TJoinedRight, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            var tableDefinition = new TableDefinition(typeof(TJoinedRight));

            var joinedTableName = _translator.GetSelectTableName(tableDefinition);
            SqlFrom = string.Concat(SqlFrom, " INNER JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> LeftJoin<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            var tableDefinition = new TableDefinition(typeof(TJoined));

            var joinedTableName = _translator.GetSelectTableName(tableDefinition);
            SqlFrom = string.Concat(SqlFrom, " LEFT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> LeftJoin<TJoinedLeft, TJoinedRight>(Expression<Func<TJoinedLeft, TJoinedRight, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            var tableDefinition = new TableDefinition(typeof(TJoinedRight));

            var joinedTableName = _translator.GetSelectTableName(tableDefinition);
            SqlFrom = string.Concat(SqlFrom, " LEFT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> RightJoin<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            var tableDefinition = new TableDefinition(typeof(TJoined));

            var joinedTableName = _translator.GetSelectTableName(tableDefinition);
            SqlFrom = string.Concat(SqlFrom, " RIGHT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> RightJoin<TJoinedLeft, TJoinedRight>(Expression<Func<TJoinedLeft, TJoinedRight, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            var tableDefinition = new TableDefinition(typeof(TJoinedRight));

            var joinedTableName = _translator.GetSelectTableName(tableDefinition);
            SqlFrom = string.Concat(SqlFrom, " RIGHT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> Union<TOther>(Query<TOther> other)
        {
            _unionQueries.Add(other);

            return this;
        }

        public Query<T> UnionAll<TOther>(Query<TOther> other)
        {
            _unionAllQueries.Add(other);

            return this;
        }

        public Query<T> OrderBy(Expression<Func<T, dynamic>> clause)
        {
            SqlOrderBy = ExpressionHelper.FormatStringFromArguments(_translator, clause, SqlOrderBy, Parameters);

            return this;
        }

        public Query<T> OrderByDescending(Expression<Func<T, dynamic>> clause)
        {
            SqlOrderBy = ExpressionHelper.FormatStringFromArguments(_translator, clause, SqlOrderBy, Parameters, _translator.GetOrderByDescendingFormat());

            return this;
        }

        public Query<T> Having(Expression<Func<T, bool>> clause)
        {
            var query = GetQueryFromClause(clause);

            SqlHaving = string.IsNullOrEmpty(SqlHaving) ? query : string.Concat("(", SqlHaving, ") AND (", query, ")");

            return this;
        }

        public Query<T> GroupBy(Expression<Func<T, dynamic>> clause)
        {
            SqlGroupBy = ExpressionHelper.FormatStringFromArguments(_translator, clause, SqlGroupBy, Parameters);

            return this;
        }

        public Query<T> Paginate(int recordsPerPage, int currentPage)
        {
            _translator.Paginate(this, recordsPerPage, currentPage);

            return this;
        }

        private string GetQueryFromClause<T2, T3>(Expression<Func<T2, T3, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor(new Parameters()).Visit(clause));
            var query = ExpressionHelper.GetQueryFromExpression(_translator, newExpression.Body, Parameters);
            return query;
        }

        private string GetQueryFromClause<T2>(Expression<Func<T2, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor(new Parameters()).Visit(clause));
            var query = ExpressionHelper.GetQueryFromExpression(_translator, newExpression.Body, Parameters);
            return query;
        }
    }
}
