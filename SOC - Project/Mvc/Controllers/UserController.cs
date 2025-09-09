using Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace Mvc.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            IEnumerable<mvcUserModel> userList;
            HttpResponseMessage response = GlobalVariables.WebApiClient.GetAsync("User").Result;
            userList = response.Content.ReadAsAsync<IEnumerable<mvcUserModel>>().Result;
            return View(userList);
        }
    }
}