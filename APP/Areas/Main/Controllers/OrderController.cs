using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility;

namespace APP.Areas.Main.Controllers
{
    [Area("Main")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderController> _logger;
        public readonly IWebHostEnvironment _hostEnvironment;
        public OrderController(ILogger<OrderController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult PayOrder()
        {
            return View();
        }
        public async Task<IActionResult> ViewOrder(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(id),includeProperties:"User");
                
                return View(order);
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> PayOrderRcpt(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(id),includeProperties:"User");
                
                return View(order);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayOrderRcpt(Orders orders)
        {
            var ord = await _unitOfWork.Order.GetFirstOrDefaultAsync(a => a.Id == orders.Id, includeProperties: "User");
            if (ModelState.IsValid)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        ord.TotPaid = ord.TotAmount;
                        _unitOfWork.SaveAsync();
                        _unitOfWork.CommitAsync(transaction);
                        return RedirectToAction(nameof(PayOrder));
                    }
                    catch (Exception)
                    {
                        _unitOfWork.RollbackAsync(transaction);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }

                }
            }

            return View(ord);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ViewOrder(Orders orders)
        {
            var ord = await _unitOfWork.Order.GetFirstOrDefaultAsync(a => a.Id == orders.Id, includeProperties: "User");
            if (ModelState.IsValid)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {

                     
                        ord.OrderStatus = orders.OrderStatus;
                        _unitOfWork.SaveAsync();
                        _unitOfWork.CommitAsync(transaction);
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception)
                    {
                        _unitOfWork.RollbackAsync(transaction);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }

                }
            }

            return View(ord);
        }

        #region Data Loads
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var objList = from a in await _unitOfWork.Order.GetAllAsync(includeProperties: "User")
                          select new
                          {
                              user = a.User.UserName,
                              date = a.OrderDate,
                              totAmt= a.TotAmount,
                              paidAmt = a.TotPaid,
                              orderStatus= a.OrderStatus==SD.Delivered?"Delivered":(a.OrderStatus==SD.OnProcess?"On Process": "Pending"),
                              curStatus= a.CurStatus==SD.Active?"Active":"De-Activate",
                              a.Id
                          };
          

            var data = objList;
            return Json(new { data});
            
        }
        [HttpGet]
        public async Task<IActionResult> GetAllOrder()
        {
            var objList = from a in await _unitOfWork.Order.GetAllAsync(includeProperties: "User")
                          where a.CurStatus == SD.Active && a.TotAmount>a.TotPaid && a.OrderStatus==SD.Delivered
                          select new
                          {
                              user = a.User.UserName,
                              date = a.OrderDate,
                              totAmt = a.TotAmount,
                              paidAmt = a.TotPaid,
                              orderStatus = a.OrderStatus == SD.Delivered ? "Delivered" : (a.OrderStatus == SD.OnProcess ? "On Process" : "Pending"),
                              curStatus = a.CurStatus == SD.Active ? "Active" : "De-Activate",
                              a.Id
                          };


            var data = objList;
            return Json(new { data });

        }
        [HttpGet]
        public async Task<IActionResult> GetOrderDetailAll(string id)
        {
            var objList = from a in await _unitOfWork.OrderDetails.GetAllAsync(includeProperties: "Books")
                          where a.OrderId==Convert.ToInt32(id)
                          select new
                          {
                              url=a.Books.ImgUrl,
                              name = a.Books.Name,
                              isbn = a.Books.ISBN,
                              qty = a.Qty,
                          
                          };
            return Json(new { data = objList });
        }

        #endregion
    }
}
