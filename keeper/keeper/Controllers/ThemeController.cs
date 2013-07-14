using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace keeper.Controllers
{
    public class ThemeController : Controller
    {
        public ActionResult ThemeSelect(string theme)
        {
            return View();
        }
    }
}
