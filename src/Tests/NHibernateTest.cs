using System.Linq;
using System.Reflection;
using ApprovalTests.NHibernate;
using ApprovalTests.NHibernate.Tests;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Linq;
using Xunit;

public class NHibernateTest
{
    public static ISessionFactory SessionFactory;

    public static ISession OpenSession()
    {
        if (SessionFactory == null) //not threadsafe
        {
            //SessionFactories are expensive, create only once
            var configuration = new Configuration();
            configuration.AddAssembly(Assembly.GetCallingAssembly());
            SessionFactory = configuration.BuildSessionFactory();
        }

        return SessionFactory.OpenSession();
    }

    [Fact]
    public void TestSimpleQuery()
    {
        using (var session = OpenSession())
        {
            var query =
                from a in session.Query<Company>()
                where a.Name.StartsWith("Mic")
                select a;
            NHibernateApprovals.Verify((NhQueryable<Company>) query);
        }
    }
}