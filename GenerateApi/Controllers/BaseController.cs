using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Http;

namespace GenerateApi.Controllers
{
    public class BaseController : ApiController
    {
        private string path = HttpContext.Current.Server.MapPath("~/File/connection.txt");

        protected SqlConnection SqlConn
        {
            get
            {
                string[] lineArr;
                using (var file = new StreamReader(path))
                {
                    string line = file.ReadLine();
                    lineArr = line.Split(';');
                }
                string connString = $@"Data Source={lineArr[0]};Initial Catalog={lineArr[1]}
                ;Persist Security Info=True;User ID={lineArr[2]};Password={lineArr[3]}";
                SqlConnection conn = new SqlConnection(connString);
                conn.Open();
                return conn;
            }
        }
    }
}