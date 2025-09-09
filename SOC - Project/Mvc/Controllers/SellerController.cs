using Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mvc.Controllers
{
    public class SellerController : Controller
    {
        // GET: Seller
        public ActionResult Home()
        {
            return View();
        }

        // GET: Available Products for Purchase
        public async Task<ActionResult> AvailableProducts()
        {
            try
            {
                IEnumerable<mvcBlanketsModel> blanketList;
                HttpResponseMessage response = await GlobalVariables.WebApiClient.GetAsync("DistributorProducts");

                if (response.IsSuccessStatusCode)
                {
                    blanketList = await response.Content.ReadAsAsync<IEnumerable<mvcBlanketsModel>>();
                    return View(blanketList);
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to load products. Please try again later.";
                    return View(new List<mvcBlanketsModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading products: " + ex.Message;
                return View(new List<mvcBlanketsModel>());
            }
        }

        // GET: Purchase Product
        public async Task<ActionResult> PurchaseProduct(int id)
        {
            try
            {
                HttpResponseMessage response = await GlobalVariables.WebApiClient.GetAsync("DistributorProducts/" + id.ToString());

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Product not found";
                    return RedirectToAction("AvailableProducts");
                }

                var blanket = await response.Content.ReadAsAsync<mvcBlanketsModel>();

                if (blanket == null)
                {
                    TempData["ErrorMessage"] = "Product not found";
                    return RedirectToAction("AvailableProducts");
                }

                var purchaseModel = new mvcPurchaseModel
                {
                    Productname = blanket.Name,
                    UserName = Session["UserName"]?.ToString() ?? "Unknown"
                };

                ViewBag.BlanketPrice = blanket.Price;
                ViewBag.BlanketName = blanket.Name;
                ViewBag.AvailableQuantity = blanket.Quantity;
                ViewBag.BlanketId = blanket.BlanketId;

                return View(purchaseModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return RedirectToAction("AvailableProducts");
            }
        }

        // POST: Purchase Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PurchaseProduct(mvcPurchaseModel purchase, int blanketPrice, int blanketId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validate quantity is positive
                    if (purchase.PurchasedQuantity <= 0)
                    {
                        ModelState.AddModelError("PurchasedQuantity", "Quantity must be greater than 0");
                        return await ReloadPurchaseForm(purchase, blanketPrice, blanketId);
                    }

                    // Calculate total amount
                    purchase.TotalAmount = purchase.PurchasedQuantity * blanketPrice;
                    purchase.UserName = Session["UserName"]?.ToString() ?? "Unknown";

                    // Ensure product name is set
                    if (string.IsNullOrEmpty(purchase.Productname))
                    {
                        var blanketResponse = await GlobalVariables.WebApiClient.GetAsync("DistributorProducts/" + blanketId.ToString());
                        if (blanketResponse.IsSuccessStatusCode)
                        {
                            var blanket = await blanketResponse.Content.ReadAsAsync<mvcBlanketsModel>();
                            purchase.Productname = blanket?.Name ?? "Unknown Product";
                        }
                    }

                    // Save purchase to database
                    HttpResponseMessage response = await GlobalVariables.WebApiClient.PostAsJsonAsync("Purchases", purchase);

                    if (response.IsSuccessStatusCode)
                    {
                        // Optionally update the blanket quantity
                        await UpdateBlanketQuantity(blanketId, purchase.PurchasedQuantity);

                        TempData["SuccessMessage"] = "Purchase completed successfully!";
                        return RedirectToAction("PurchaseHistory");
                    }
                    else
                    {
                        // Log the error response for debugging
                        string errorContent = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = "Failed to complete purchase. Server response: " + response.StatusCode + " - " + errorContent;
                    }
                }
                else
                {
                    // Add validation errors to TempData for debugging
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    TempData["ErrorMessage"] = "Validation failed: " + string.Join(", ", errors);
                }

                return await ReloadPurchaseForm(purchase, blanketPrice, blanketId);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while processing your purchase: " + ex.Message;
                return await ReloadPurchaseForm(purchase, blanketPrice, blanketId);
            }
        }

        // Helper method to reload purchase form with product data
        private async Task<ActionResult> ReloadPurchaseForm(mvcPurchaseModel purchase, int blanketPrice, int blanketId)
        {
            try
            {
                HttpResponseMessage blanketResponse = await GlobalVariables.WebApiClient.GetAsync("DistributorProducts/" + blanketId.ToString());
                if (blanketResponse.IsSuccessStatusCode)
                {
                    var blanket = await blanketResponse.Content.ReadAsAsync<mvcBlanketsModel>();
                    if (blanket != null)
                    {
                        ViewBag.BlanketPrice = blanket.Price;
                        ViewBag.BlanketName = blanket.Name;
                        ViewBag.AvailableQuantity = blanket.Quantity;
                        ViewBag.BlanketId = blanket.BlanketId;

                        // Ensure the purchase model has the correct product name
                        if (string.IsNullOrEmpty(purchase.Productname))
                        {
                            purchase.Productname = blanket.Name;
                        }
                    }
                }
                else
                {
                    ViewBag.BlanketPrice = blanketPrice;
                    ViewBag.BlanketId = blanketId;
                }
            }
            catch (Exception)
            {
                ViewBag.BlanketPrice = blanketPrice;
                ViewBag.BlanketId = blanketId;
            }

            return View(purchase);
        }

        // Helper method to update blanket quantity after purchase
        private async Task UpdateBlanketQuantity(int blanketId, int purchasedQuantity)
        {
            try
            {
                // Get current blanket data
                HttpResponseMessage response = await GlobalVariables.WebApiClient.GetAsync("DistributorProducts/" + blanketId.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var blanket = await response.Content.ReadAsAsync<mvcBlanketsModel>();
                    if (blanket != null)
                    {
                        // Update quantity
                        blanket.Quantity -= purchasedQuantity;
                        if (blanket.Quantity < 0) blanket.Quantity = 0;

                        // Update blanket in database
                        await GlobalVariables.WebApiClient.PutAsJsonAsync("DistributorProducts/" + blanketId.ToString(), blanket);
                    }
                }
            }
            catch (Exception)
            {
                // Log error but don't fail the purchase
                // You might want to implement proper logging here
            }
        }

        // GET: Purchase History
        public async Task<ActionResult> PurchaseHistory()
        {
            try
            {
                IEnumerable<mvcPurchaseModel> purchaseList = new List<mvcPurchaseModel>();

                // Try different endpoint names that might exist
                string[] possibleEndpoints = { "Purchase", "Purchases", "PurchaseHistory" };

                foreach (string endpoint in possibleEndpoints)
                {
                    try
                    {
                        HttpResponseMessage response = await GlobalVariables.WebApiClient.GetAsync(endpoint);

                        if (response.IsSuccessStatusCode)
                        {
                            purchaseList = await response.Content.ReadAsAsync<IEnumerable<mvcPurchaseModel>>();

                            // Filter by current user
                            string currentUser = Session["UserName"]?.ToString();
                            if (!string.IsNullOrEmpty(currentUser))
                            {
                                purchaseList = purchaseList.Where(p => p.UserName == currentUser);
                            }

                            return View(purchaseList);
                        }
                    }
                    catch (Exception)
                    {
                        // Continue to next endpoint
                        continue;
                    }
                }

                // If no endpoint worked
                TempData["ErrorMessage"] = "Unable to load purchase history. Please contact support.";
                return View(purchaseList);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading purchase history: " + ex.Message;
                return View(new List<mvcPurchaseModel>());
            }
        }

        // GET: Purchase Details
        public async Task<ActionResult> PurchaseDetails(int id)
        {
            try
            {
                // Try different endpoint patterns
                string[] possibleEndpoints = {
                    $"Purchase/{id}",
                    $"Purchases/{id}",
                    $"PurchaseHistory/{id}"
                };

                foreach (string endpoint in possibleEndpoints)
                {
                    try
                    {
                        HttpResponseMessage response = await GlobalVariables.WebApiClient.GetAsync(endpoint);

                        if (response.IsSuccessStatusCode)
                        {
                            var purchase = await response.Content.ReadAsAsync<mvcPurchaseModel>();

                            if (purchase == null)
                            {
                                TempData["ErrorMessage"] = "Purchase record not found";
                                return RedirectToAction("PurchaseHistory");
                            }

                            // Verify this purchase belongs to current user
                            string currentUser = Session["UserName"]?.ToString();
                            if (!string.IsNullOrEmpty(currentUser) && purchase.UserName != currentUser)
                            {
                                TempData["ErrorMessage"] = "Access denied";
                                return RedirectToAction("PurchaseHistory");
                            }

                            return View(purchase);
                        }
                    }
                    catch (Exception)
                    {
                        // Continue to next endpoint
                        continue;
                    }
                }

                TempData["ErrorMessage"] = "Unable to load purchase details";
                return RedirectToAction("PurchaseHistory");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return RedirectToAction("PurchaseHistory");
            }
        }

        public ActionResult Index()
        {
            IEnumerable<mvcSellerBlankets> blanketList;
            HttpResponseMessage response = GlobalVariables.WebApiClient.GetAsync("SellerBlankets").Result;
            blanketList = response.Content.ReadAsAsync<IEnumerable<mvcSellerBlankets>>().Result;
            return View(blanketList);
        }

        public ActionResult AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View(new mvcSellerBlankets());
            }
            else
            {
                HttpResponseMessage response = GlobalVariables.WebApiClient.GetAsync("SellerBlankets/" + id.ToString()).Result;
                return View(response.Content.ReadAsAsync<mvcSellerBlankets>().Result);
            }
        }

        [HttpPost]
        public ActionResult AddOrEdit(mvcSellerBlankets sup)
        {
            // Set the supplier's username from the session

            if (sup.BlanketId == 0)
            {
                HttpResponseMessage response = GlobalVariables.WebApiClient.PostAsJsonAsync("SellerBlankets", sup).Result;
                TempData["SuccessMessage"] = "Saved Successfully";
            }
            else
            {
                HttpResponseMessage response = GlobalVariables.WebApiClient.PutAsJsonAsync("SellerBlankets/" + sup.BlanketId, sup).Result;
                TempData["SuccessMessage"] = "Product Updated Successfully";
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            HttpResponseMessage response = GlobalVariables.WebApiClient.DeleteAsync("SellerBlankets/" + id.ToString()).Result;
            TempData["SuccessMessage"] = "Deleted Successfully";
            return RedirectToAction("Index");
        }
    }
}