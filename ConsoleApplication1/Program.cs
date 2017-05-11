using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {

            // here I create a new instance of class
            A a = new A();
            A b = new A ();

            b.X = a.X;

            a.X.y =  10;


            Console.WriteLine("a.X is " + a.X.y + " b.X.y is" + b.X.y);
            Console.ReadKey();

            string connectionString =
        @"Data Source = (localdb)\MSSQLLocalDB;Integrated Security = True; Persist Security Info=False;Initial Catalog = nservicebus";

            var t = new Context();
            var t2 = new Context2 {DbConnection = t.DbConnection};
            t.DbConnection = new SqlConnection(connectionString);

            Console.WriteLine(t2.DbConnection.ConnectionString);
            Console.ReadKey();
        }
    }

    class A
    {
        public NestedProperty X = new NestedProperty();

        public void Set(ref NestedProperty x)
        {
            X = x;
        }
    }

    class NestedProperty
    {
        public int y = 0;
    }

    class Context
    {

        public IDbConnection DbConnection { get; set; }
    }

    class Context2
    {
        public IDbConnection DbConnection { get; set; }
    }

}
