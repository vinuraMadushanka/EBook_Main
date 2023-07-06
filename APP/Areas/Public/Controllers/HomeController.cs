using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using APP.Session;
using Utility;

namespace APP.Areas.Public.Controllers
{
    [Area("Public")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;
        public readonly IWebHostEnvironment _hostEnvironment;
        private readonly ISession _session;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment,IHttpContextAccessor httpContext)
        {
            _hostEnvironment = hostEnvironment;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _session = httpContext.HttpContext.Session;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<Books> productList = await _unitOfWork.Books.GetAllAsync(a=>a.CurStatus==SD.Active);
           
            if (productList !=null)
            {
                return View(productList);
            }
           
            return View();

        }
        [Authorize(Roles = SD.RoleCustomer)]
        public async Task<IActionResult> AddToCart(string Id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var books = await _unitOfWork.Books.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(Id));
            var cmnt = await _unitOfWork.Comments.GetAllAsync(a => a.BookId == books.Id,includeProperties:"User");
            var crtVM = new CartVM()
            {
                Books = books,
                Comments=cmnt
                
            };
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var crt = await _unitOfWork.Cart.GetFirstOrDefaultAsync(a => a.UserId == claim.Value && a.CurStatus == SD.Active);
                    if (crt == null)
                    {
                        var cart = new Carts()
                        {
                            CurStatus = SD.Active,
                            UserId = claim.Value,
                        };
                        await _unitOfWork.Cart.AddAsync(cart);
                        _unitOfWork.SaveAsync();
                        _unitOfWork.CommitAsync(transaction);
                        crtVM.Carts = cart;
                    }
                    else
                    {
                        crtVM.Carts = crt;
                    }

                }
                catch (Exception)
                {
                    _unitOfWork.RollbackAsync(transaction);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            return View(crtVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(CartVM cartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (cartVM.CartDetails.Qty>0)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        var cartDetails = new CartDetails()
                        {
                            CartId = cartVM.Carts.Id,
                            BookId = cartVM.Books.Id,
                            Qty = cartVM.CartDetails.Qty,
                        };
                        await _unitOfWork.CartDetails.AddAsync(cartDetails);
                        _unitOfWork.SaveAsync();
                        _unitOfWork.CommitAsync(transaction);

                        await SetSession(claim);
                        return RedirectToAction("ViewCart");
                    }
                    catch (Exception)
                    {
                        _unitOfWork.RollbackAsync(transaction);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
            }
            else
            {
                return RedirectToAction("AddToCart", new { Id = cartVM.Books.Id });
            }


        }
        

        [Authorize(Roles=SD.RoleCustomer)]
        public async Task<IActionResult> ViewCart()
        {
            var crtVM = new CartVM();

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var carts = await _unitOfWork.Cart.GetFirstOrDefaultAsync(a => a.UserId == claim.Value && a.CurStatus == SD.Active);
            if (carts != null)
            {
                var cartDt = await _unitOfWork.CartDetails.GetAllAsync(a => a.CartId == carts.Id, includeProperties: "Books");
                if (cartDt != null)
                {
                    crtVM.CartDetailList = cartDt;
                    crtVM.Carts = carts;
                    return View(crtVM);
                }
                
            }

            
                return View(crtVM);
            
          
        }

        public async Task<IActionResult> AddCmt(string cmt, string bookId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var cmnt = new Comments()
                    {
                        BookId = Convert.ToInt32(bookId),
                        UserId=claim.Value,
                        Comment=cmt
                    };
                    await _unitOfWork.Comments.AddAsync(cmnt);
                     _unitOfWork.SaveAsync();
                     _unitOfWork.CommitAsync(transaction);
                    return RedirectToAction("AddToCart", new { Id = bookId });
                }
                catch (Exception)
                {
                    _unitOfWork.RollbackAsync(transaction);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
        }
        [Authorize(Roles = SD.RoleCustomer)]
        public async Task<IActionResult> UserDetails()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(a => a.Id == claim.Value);
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserDetails(ApplicationUser usr)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(a => a.Id == claim.Value);
            if (ModelState.IsValid)
            {
                
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        user.Address = usr.Address;
                        user.FirstName = usr.FirstName;
                        user.LastName = usr.LastName;
                        user.UpdatedBy = user.UserName;
                        user.UpdatedOnUTC = DateTime.UtcNow;
                        _unitOfWork.SaveAsync();
                        _unitOfWork.CommitAsync(transaction);
                        return RedirectToAction("Index");
                    }
                    catch (Exception)
                    {
                        _unitOfWork.RollbackAsync(transaction);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
            }
            else
            {
                return View(usr);
            }


        }


        [Authorize(Roles=SD.RoleCustomer)]
        public async Task<IActionResult> Order()
        {
            var crtVM = new CartVM();

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var carts = await _unitOfWork.Cart.GetFirstOrDefaultAsync(a => a.UserId == claim.Value && a.CurStatus == SD.Active);
            if (carts != null)
            {
                var cartDt = await _unitOfWork.CartDetails.GetAllAsync(a => a.CartId == carts.Id, includeProperties: "Books");
                if (cartDt != null)
                {
                    crtVM.CartDetailList = cartDt;
                    crtVM.Carts = carts;
                    return View(crtVM);
                }
                
            }

            
                return View(crtVM);
            
          
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Order(CartVM cartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(a => a.Id == claim.Value);
            if (!string.IsNullOrEmpty(user.Address))
            {
                double totAmt = 0; 
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                    var cartDetails = await _unitOfWork.CartDetails.GetAllAsync(a => a.CartId == cartVM.Carts.Id, includeProperties: "Books");
                        var order = new Orders()
                        {
                            CurStatus = SD.Active,
                            OrderDate = DateTime.UtcNow,
                            OrderStatus = "PEN",
                            TotPaid = 0.00,
                            UserId = claim.Value
                        };
                        await _unitOfWork.Order.AddAsync(order);
                        _unitOfWork.SaveAsync();
                        foreach (var item in cartDetails)
                        {
                            var orderDetails = new OrderDetails()
                            {
                                BookId=item.BookId,
                                OrderId=order.Id,
                                Qty=item.Qty,
                               
                            };
                            totAmt = totAmt + item.Qty * item.Books.Price;
                            await _unitOfWork.OrderDetails.AddAsync(orderDetails);
                            var books = await _unitOfWork.Books.GetFirstOrDefaultAsync(a => a.Id == item.BookId);
                            books.Qty = books.Qty - item.Qty;
                           
                            _unitOfWork.SaveAsync();
                        }
                        var odr = await _unitOfWork.Order.GetFirstOrDefaultAsync(a => a.Id == order.Id);
                        odr.TotAmount = totAmt;
                        var cart = await _unitOfWork.Cart.GetFirstOrDefaultAsync(a => a.Id == cartVM.Carts.Id);
                        cart.CurStatus = SD.DeActive;
                        _unitOfWork.SaveAsync();
                        _unitOfWork.CommitAsync(transaction);

                    //await SetSession(claim);
                    _session.Remove<CartDetails>("Cart");
                    return RedirectToAction("Index");
                    }
                    catch (Exception)
                    {
                        _unitOfWork.RollbackAsync(transaction);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
            }
            else
            {
                return RedirectToAction("UserDetails");
            }


        }

        [Authorize(Roles = SD.RoleCustomer)]
        public async Task<IActionResult> MyOrders()
        {
            var orderVM = new OrderVM();

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var orders = await _unitOfWork.Order.GetAllAsync(a => a.UserId == claim.Value && a.CurStatus == SD.Active);
            if (orders != null)
            {
                orderVM.OrderList = orders;
                return View(orderVM);
            }
            return View();
        }
        public async Task<IActionResult> ViewOrder(string id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
          
                try
                {
                    var orderVM = new OrderVM();

                    var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(id));
                    var orderDetails = await _unitOfWork.OrderDetails.GetAllAsync(a => a.OrderId == Convert.ToInt32(id), includeProperties: "Books");
                    orderVM.Orders = order;
                    orderVM.OrderDetailList = orderDetails;
                    return View(orderVM);
                    
                }
                catch (Exception)
                {
                   
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
        }
       
        public async Task<IActionResult> CancelOrder(string id)
        {

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(id));
                    order.CurStatus = SD.DeActive;
                    _unitOfWork.SaveAsync();
                    var orderDetails = await _unitOfWork.OrderDetails.GetAllAsync(a => a.OrderId == order.Id);
                    foreach (var item in orderDetails)
                    {
                        var books = await _unitOfWork.Books.GetFirstOrDefaultAsync(a => a.Id == item.BookId);
                        books.Qty = books.Qty + item.Qty;
                        _unitOfWork.SaveAsync();
                    }
                    _unitOfWork.CommitAsync(transaction);
                    return RedirectToAction("MyOrders");
                }
                catch (Exception)
                {
                    _unitOfWork.RollbackAsync(transaction);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }

        }


        public async Task<IActionResult> plus(string Id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var cartDt = await _unitOfWork.CartDetails.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(Id), includeProperties: "Books");
                    cartDt.Qty = cartDt.Qty + 1;
                    _unitOfWork.SaveAsync();
                    _unitOfWork.CommitAsync(transaction);
                     await  SetSession(claim);
                    return RedirectToAction("ViewCart");
                }
                catch (Exception)
                {
                    _unitOfWork.RollbackAsync(transaction);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                
            }
         
        }

        public async Task<IActionResult> minus(string Id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var cartDt = await _unitOfWork.CartDetails.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(Id), includeProperties: "Books");
                    cartDt.Qty = cartDt.Qty - 1;
                    _unitOfWork.SaveAsync();
                    _unitOfWork.CommitAsync(transaction);
                    await SetSession(claim);
                    return RedirectToAction("ViewCart");
                }
                catch (Exception)
                {
                    _unitOfWork.RollbackAsync(transaction);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

            }
            
        }

        public async Task<IActionResult> remove(string Id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var cartDt = await _unitOfWork.CartDetails.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(Id), includeProperties: "Books");
                    await _unitOfWork.CartDetails.Remove(Convert.ToInt32(Id));
                    _unitOfWork.SaveAsync();
                    _unitOfWork.CommitAsync(transaction);
                    await SetSession(claim);
                    return RedirectToAction("ViewCart");
                }
                catch (Exception)
                {
                    _unitOfWork.RollbackAsync(transaction);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

            }

        }
        private async Task SetSession(Claim claim)
        {
            var carts = await _unitOfWork.Cart.GetFirstOrDefaultAsync(a => a.UserId == claim.Value && a.CurStatus == SD.Active);
            if (carts != null)
            {

                var cartDt = await _unitOfWork.CartDetails.GetAllAsync(a => a.CartId == carts.Id, includeProperties: "Books");
                if (cartDt != null)
                {
                    _session.SetSessionsNew<IEnumerable<CartDetails>>("Cart", cartDt);

                }
            }
        }
    }
}
