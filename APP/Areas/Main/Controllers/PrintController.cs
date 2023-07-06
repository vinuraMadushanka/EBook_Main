using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility;

namespace APP.Areas.Main.Controllers
{
    [Area("Main")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class PrintController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PrintController> _logger;
        public readonly IWebHostEnvironment _hostEnvironment;
        public PrintController(ILogger<PrintController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var objList = from a in await _unitOfWork.Order.GetAllAsync(includeProperties: "User")
                          select new
                          {
                              user = a.User.UserName,
                              date = a.OrderDate,
                              totAmt = a.TotAmount,
                              paidAmt = a.TotPaid,
                              orderStatus = a.OrderStatus == SD.Delivered ? "Delivered" : (a.OrderStatus == SD.OnProcess ? "On Process" : "Pending"),
                              curStatus = a.CurStatus == SD.Active ? "Active" : "De-Activate"
                          };


            var data = objList;
            return Json(new { data });

        }
        public IActionResult Books()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBook()
        {
            var objList = await _unitOfWork.Books.GetAllAsync();
            return Json(new { data = objList });
        }


        public IActionResult ReciveAmount()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllRecipts()
        {
            var objList = from a in await _unitOfWork.Order.GetAllAsync(includeProperties: "User")
                          where a.TotAmount==a.TotPaid
                          select new
                          {
                              user = a.User.UserName,
                              date = a.OrderDate,
                              totAmt = a.TotAmount,
                              paidAmt = a.TotPaid
                             
                          };


            var data = objList;
            return Json(new { data });

        }

        public IActionResult OnProcess()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllOnProcess()
        {
            var objList = from a in await _unitOfWork.Order.GetAllAsync(includeProperties: "User")
                          where a.TotAmount == a.TotPaid
                          select new
                          {
                              user = a.User.UserName,
                              date = a.OrderDate,
                              totAmt = a.TotAmount,
                              paidAmt = a.TotPaid

                          };


            var data = objList;
            return Json(new { data });

        }
    }
}
