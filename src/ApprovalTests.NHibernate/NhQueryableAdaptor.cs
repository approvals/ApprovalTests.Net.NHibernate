﻿using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using ApprovalUtilities.Persistence.Database;
using ApprovalUtilities.Reflection;
using NHibernate;
using NHibernate.AdoNet.Util;
using NHibernate.Engine;
using NHibernate.Hql.Ast.ANTLR;
using NHibernate.Linq;
using NHibernate.Linq.Visitors;

namespace ApprovalTests.NHibernate
{
    public class NhQueryableAdaptor<T> : IDatabaseToExecuteableQueryAdaptor
    {
        private readonly NhQueryable<T> queryable;

        public NhQueryableAdaptor(NhQueryable<T> queryable)
        {
            this.queryable = queryable;
        }

        public virtual string GetQuery()
        {
            return GetGeneratedSql(queryable, GetSession(queryable));
        }

        public virtual string GetGeneratedSql(IQueryable queryable, ISession session)
        {
            var sessionImp = (ISessionImplementor)session;
            var nhLinqExpression = new NhLinqExpression(queryable.Expression, sessionImp.Factory);
            var translatorFactory = new ASTQueryTranslatorFactory();
            var translators = translatorFactory.CreateQueryTranslators(nhLinqExpression, null, false,
                                                                       sessionImp.EnabledFilters, sessionImp.Factory);

            var sql = translators.First().SQLString;
            var formattedSql = FormatStyle.Basic.Formatter.Format(sql);
            var i = 0;
            var map = ExpressionParameterVisitor.Visit(queryable.Expression, sessionImp.Factory).ToArray();
            formattedSql = Regex.Replace(formattedSql, @"\?", m => map[i++].Key.ToString().Replace('"', '\''));

            return formattedSql;
        }

        static ISession GetSession<TResult>(NhQueryable<TResult> nhQueryable)
        {
            var queryProvider = nhQueryable.Provider;
            return ReflectionUtilities.GetValueForProperty<ISession>(queryProvider, "Session");
        }

        public virtual DbConnection GetConnection()
        {
            return (DbConnection)GetSession(queryable).Connection;
        }
    }
}