using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace GenerateApi.Service
{
    public interface ITableService
    {
        StringBuilder GenerateInsert(string tableName, SqlConnection sqlConn);
        StringBuilder GenerateUpdate(string tableName, SqlConnection sqlConn);
    }
}