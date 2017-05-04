using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reciever
{
    public class ContextHelper 
    {
        public IDbTransaction dbTransaction { get; set; }
        public IDbConnection dbConnection { get; set; }
    }
}
