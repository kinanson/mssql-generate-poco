using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace GenerateApi.Service
{
    public class TableNativeService : ITableService
    {
        public StringBuilder GenerateInsert(string tableName, SqlConnection sqlConn)
        {
            throw new NotImplementedException();
        }

        public StringBuilder GenerateUpdate(string tableName, SqlConnection sqlConn)
        {
            throw new NotImplementedException();
        }
    }
}