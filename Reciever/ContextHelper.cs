using System.Data;

namespace Reciever
{
    public class ContextHelper 
    {
        public IDbTransaction dbTransaction { get; set; }
        public IDbConnection dbConnection { get; set; }
    }
}
