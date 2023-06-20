using Microsoft.AspNetCore.Mvc;
using EPM.Mouser.Interview.Data;
using System.Text.Json;
using System.Net;
using System.Collections.Generic;
using EPM.Mouser.Interview.Models;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using Newtonsoft.Json;
using System.Web.Http.Results;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EPM.Mouser.Interview.Web.Controllers
{
    
    public class IndexModel : PageModel
    {
        private List<Product>? products;
        private WarehouseApi whAPI = new WarehouseApi();

        public void OnGet()
        {
            
            JsonResult jsonResult = whAPI.GetPublicInStockProducts();
            this.products = JsonConvert.DeserializeObject<List<Product>>(jsonResult.ToString());
        }
        //public List<Product> Products { get
        //    {
        //        JsonResult jsonResult = whAPI.GetPublicInStockProducts();
        //        return products = JsonConvert.DeserializeObject<List<Product>>(jsonResult.ToString());
        //    }
        //    set => products = value; }
    }
    public class WarehouseApi : Controller
    {

        public ActionResult Index()
        {
           
            return View();
        }

        
        /*
*  Action: GET
*  Url: api/warehouse/id
*  This action should return a single product for an Id
*/
        /// <summary>
        /// Return Product
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetProduct(long id)
        {
            var product = new Data.WarehouseRepository();
            
            // If implemented in real world long should be avoided for id, instead using GUID
            return Json(product.Get(id));
        }

        /*
         *  Action: GET
         *  Url: api/warehouse
         *  This action should return a collection of products in stock
         *  In stock means In Stock Quantity is greater than zero and In Stock Quantity is greater than the Reserved Quantity
         */
        /// <summary>
        /// Get in stock products
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetPublicInStockProducts()
        {
            var warehouseAPI = new Data.WarehouseRepository();
            Task<List<Models.Product>> products = warehouseAPI.List();
            return Json(products);
        }


        /*
         *  Action: GET
         *  Url: api/warehouse/order
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *  This action should increase the Reserved Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would increase the Reserved Quantity to be greater than the In Stock Quantity.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        /// <summary>
        /// Order Item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public JsonResult OrderItem(long id, int quantity)
        {
            var updateResp = new Models.UpdateQuantityRequest();
            
            updateResp.Id = id;
            updateResp.Quantity = quantity;

            try
            {
                return Json(updateResp);
            }
            catch
            {
                var resp = new Models.UpdateResponse();
                //if (resp.Success == false)
                //{
                return Json(resp.ErrorReason);
                //}
            }
            
            
        }

        /*
         *  Url: api/warehouse/ship
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *
         *  This action should:
         *     - decrease the Reserved Quantity for the product requested by the amount requested to a minimum of zero.
         *     - decrease the In Stock Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would cause the In Stock Quantity to go below zero.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        /// <summary>
        /// Ship Item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public JsonResult ShipItem(long id, int quantity)
        {
            var updateResp = new Models.UpdateQuantityRequest();
            var data = new Models.Product();

            data.Id = id;
            if (data.ReservedQuantity - quantity <= 0)
            {
                data.ReservedQuantity = 0;
            }
            else
            {
                data.ReservedQuantity = data.ReservedQuantity - quantity;
            }
            
            data.InStockQuantity = quantity;

            updateResp.Id = id;
            updateResp.Quantity = quantity;

            try
            {
                return Json(updateResp);
            }
            catch
            {
                var resp = new Models.UpdateResponse();
                //if (resp.Success == false)
                //{
                return Json(resp.ErrorReason);
                //}
            }
        }

        /*
        *  Url: api/warehouse/restock
        *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "quantity": 1
        *       }
        *
        *
        *  This action should:
        *     - increase the In Stock Quantity for the product requested by the amount requested
        *
        *  This action should return failure (success = false) when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested
        *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        /// <summary>
        /// Restock Item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public JsonResult RestockItem(long id, int quantity)
        {
            _ = new Data.WarehouseRepository();
            var data = new Models.Product();
            
            data.Id = id;
            data.InStockQuantity = quantity;

            try
            {
                return Json(data);
            }
            catch
            {
                var resp = new Models.UpdateResponse();
                //if (resp.Success == false)
                //{
                return Json(resp.ErrorReason);
                //}
            }
        }

        /*
        *  Url: api/warehouse/add
        *  This action should return a EPM.Mouser.Interview.Models.CreateResponse<EPM.Mouser.Interview.Models.Product>
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.Product in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "inStockQuantity": 1,
        *           "reservedQuantity": 1,
        *           "name": "product name"
        *       }
        *
        *
        *  This action should:
        *     - create a new product with:
        *          - The requested name - But forced to be unique - see below
        *          - The requested In Stock Quantity
        *          - The Reserved Quantity should be zero
        *
        *       UNIQUE Name requirements
        *          - No two products can have the same name
        *          - Names should have no leading or trailing whitespace before checking for uniqueness
        *          - If a new name is not unique then append "(x)" to the name [like windows file system does, where x is the next avaiable number]
        *
        *
        *  This action should return failure (success = false) and an empty Model property when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested for the In Stock Quantity
        *     - ErrorReason.InvalidRequest when: A blank or empty name is requested
        */
        /// <summary>
        /// Add new product
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddNewProductAsync(string productName)
        {
            var product = new Data.WarehouseRepository();
            List<Models.Product> products = await Task.Run(function: () => product.List());
            
            var query = products.GroupBy(p => p.Name)
                .Where(w => w.Count() > 1)
                .Select(s => s.Key)
                .ToList();

            // Check for duplicates
            if (query.Count > 0)
            {
                //Duplicate found
                return Json(null);
            }
            else
            {
                // Add new
                var data = new Models.Product();

                data.Id = 1; 
                data.Name = productName;
                data.InStockQuantity = 1;
                data.ReservedQuantity = 1;

                await product.Insert(data);
                var resp = new Models.UpdateResponse();

                if (resp.Success == false)
                {
                    return Json(resp.ErrorReason);
                }
                else { return Json(resp.Success); }
            }

        }
    }
}
