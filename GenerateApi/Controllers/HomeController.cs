using GenerateApi.Models;
using System.IO;
using System.Web.Mvc;

namespace GenerateApi.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            string path = HttpContext.Server.MapPath("~/File/connection.txt");

            string[] lineArr;
            using (var file = new StreamReader(path))
            {
                string line = file.ReadLine();
                lineArr = line.Split(';');
            }
            SetConnectionModel model = new SetConnectionModel()
            {
                Server = lineArr[0],
                Db=lineArr[1],
                User=lineArr[2],
                Pwd=lineArr[3]
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(SetConnectionModel param)
        {
            if (ModelState.IsValid)
            {
                var connString = $"{param.Server};{param.Db};{param.User};{param.Pwd}";
                OverrideConnection(connString);
                return RedirectToAction("Table");
            }
            return View(param);
        }

        public ActionResult Table()
        {
            return View();
        }

        public ActionResult Sp()
        {
            return View();
        }

        private void OverrideConnection(string connString)
        {
            string path = HttpContext.Server.MapPath("~/File/connection.txt");
            System.IO.File.WriteAllText(path, connString);
        }
    }
}