using Mvc.Models;
using Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace MVC.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
            return View(new mvcLoginModel());
        }

        [HttpPost]
        public ActionResult Login(mvcLoginModel user)
        {
            string manufacturerEmail = "manufacturer@gmail.com";
            string manufacturerPassword = "12345";

            if (user.Email == manufacturerEmail && user.Password == manufacturerPassword)
            {
                TempData["SuccessMessage"] = "Login Successful";
                Session["UserName"] = "CozyComfort";
                Session["UserType"] = "manufacturer";
                Session["UserId"] = 1;

                return RedirectToAction("Home", "Manufacturer");
            }

            try
            {
                // Fix 1: Check the API endpoint - it should be "User" not "Users"
                HttpResponseMessage response = GlobalVariables.WebApiClient.GetAsync("User").Result;

                // Fix 2: Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    var userList = response.Content.ReadAsAsync<IEnumerable<mvcLoginModel>>().Result;

                    // Check if the user exists in the list
                    var loggedInUser = userList.FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);
                    if (loggedInUser != null)
                    {
                        TempData["SuccessMessage"] = "Login Successful";

                        // Set session variables for the logged-in user
                        Session["UserName"] = loggedInUser.UserName;
                        Session["UserType"] = loggedInUser.UserType;
                        Session["UserId"] = loggedInUser.UserId;

                        if (loggedInUser.UserType == "Distributor")
                        {
                            return RedirectToAction("Home", "Distributor");
                        }
                        else if (loggedInUser.UserType == "Seller")
                        {
                            return RedirectToAction("Home", "Seller");
                        }
                        else
                        {
                            return RedirectToAction("Home", "Customer");
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid Username or Password";
                        return View(user);
                    }
                }
                else
                {
                    // Fix 3: Handle API error responses
                    string errorContent = response.Content.ReadAsStringAsync().Result;
                    TempData["ErrorMessage"] = "Server error. Please try again later.";
                    return View(user);
                }
            }
            catch (Exception ex)
            {
                // Fix 4: Handle exceptions gracefully
                TempData["ErrorMessage"] = "Connection error. Please check your internet connection and try again.";
                return View(user);
            }
        }


        public ActionResult AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View(new mvcLoginModel());
            }
            else
            {
                HttpResponseMessage response = GlobalVariables.WebApiClient.GetAsync("User/" + id.ToString()).Result;
                return View(response.Content.ReadAsAsync<mvcLoginModel>().Result);
            }
        }

        [HttpPost]
        public ActionResult AddOrEdit(mvcLoginModel use)
        {
            if (use.UserId == 0)
            {
                HttpResponseMessage response = GlobalVariables.WebApiClient.PostAsJsonAsync("User", use).Result;
                TempData["SuccessMessage"] = "Saved Successfully";
            }
            else
            {
                HttpResponseMessage response = GlobalVariables.WebApiClient.PutAsJsonAsync("User/" + use.UserId, use).Result;
                TempData["SuccessMessage"] = "Users Added Successfully";
            }
            return RedirectToAction("Login");
        }

        public ActionResult Delete(int id)
        {
            HttpResponseMessage response = GlobalVariables.WebApiClient.DeleteAsync("User/" + id.ToString()).Result;
            TempData["SuccessMessage"] = "Deleted Successfully";
            return RedirectToAction("Index");
        }



        // Logout method
        public ActionResult Logout()
        {
            Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out.";
            return RedirectToAction("Login");
        }
    }
}