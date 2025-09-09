using Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace Mvc.Controllers
{
    public class ManufacturerController : Controller
    {
        // GET: Manufacturer
        public ActionResult Home()
        {
            return View();
        }

        public ActionResult Index()
        {
            IEnumerable<mvcBlanketsModel> blanketList;
            HttpResponseMessage response = GlobalVariables.WebApiClient.GetAsync("Blanket").Result;
            blanketList = response.Content.ReadAsAsync<IEnumerable<mvcBlanketsModel>>().Result;
            return View(blanketList);
        }

        public ActionResult AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View(new mvcBlanketsModel());
            }
            else
            {
                HttpResponseMessage response = GlobalVariables.WebApiClient.GetAsync("Blanket/" + id.ToString()).Result;
                return View(response.Content.ReadAsAsync<mvcBlanketsModel>().Result);
            }
        }

        [HttpPost]
        public ActionResult AddOrEdit(mvcBlanketsModel sup)
        {
            // Set the supplier's username from the session

            if (sup.BlanketId == 0)
            {
                HttpResponseMessage response = GlobalVariables.WebApiClient.PostAsJsonAsync("Blanket", sup).Result;
                TempData["SuccessMessage"] = "Saved Successfully";
            }
            else
            {
                HttpResponseMessage response = GlobalVariables.WebApiClient.PutAsJsonAsync("Blanket/" + sup.BlanketId, sup).Result;
                TempData["SuccessMessage"] = "Product Updated Successfully";
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            HttpResponseMessage response = GlobalVariables.WebApiClient.DeleteAsync("Blanket/" + id.ToString()).Result;
            TempData["SuccessMessage"] = "Deleted Successfully";
            return RedirectToAction("Index");
        }


    }
}