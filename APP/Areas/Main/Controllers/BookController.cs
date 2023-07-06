using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utility;

namespace APP.Areas.Main.Controllers
{

    [Area("Main")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class BookController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookController> _logger;
        public readonly IWebHostEnvironment _hostEnvironment;
        public BookController(ILogger<BookController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult SetQty()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Books books)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        string webRootPath = _hostEnvironment.WebRootPath;
                        var files = HttpContext.Request.Form.Files;
                        if (files.Count > 0)
                        {
                            string fileName = Guid.NewGuid().ToString();
                            var uploads = Path.Combine(webRootPath, @"img\book");
                            var extenstion = Path.GetExtension(files[0].FileName);

                            //if (books.ImgUrl != null)
                            //{
                            //    //this is an edit and we need to remove old image
                            //    var imagePath = Path.Combine(webRootPath, books.ImgUrl.TrimStart('\\'));
                            //    if (System.IO.File.Exists(imagePath))
                            //    {
                            //        System.IO.File.Delete(imagePath);
                            //    }
                            //}

                            using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                            {
                                files[0].CopyTo(filesStreams);
                            }
                            books.ImgUrl = @"\img\book\" + fileName + extenstion;
                        }
                        var book = new Books()
                        {
                            ImgUrl = books.ImgUrl,
                            Author = books.Author,
                            ISBN = books.ISBN,
                            Name = books.Name,
                            Price = books.Price,
                            Description=books.Description

                        };
                         await _unitOfWork.Books.AddAsync(book);
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

            return View(books);
        }
    
        public async Task<IActionResult>AddQty (string id)
        {
            var book= await _unitOfWork.Books.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(id));
            if (book == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQty(Books books)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        var book = await _unitOfWork.Books.GetFirstOrDefaultAsync(a => a.Id == books.Id);


                        book.Qty = books.Qty;
                        _unitOfWork.SaveAsync();
                        _unitOfWork.CommitAsync(transaction);
                        return RedirectToAction(nameof(SetQty));
                    }
                    catch (Exception)
                    {
                        _unitOfWork.RollbackAsync(transaction);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }

                }
            }

            return View(books);
        }
        public async Task<IActionResult> EditBook(string id)
        {
            var book = await _unitOfWork.Books.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(id));
            if (book == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(Books books)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        string webRootPath = _hostEnvironment.WebRootPath;
                        var files = HttpContext.Request.Form.Files;
                        if (files.Count > 0)
                        {
                            string fileName = Guid.NewGuid().ToString();
                            var uploads = Path.Combine(webRootPath, @"img\book");
                            var extenstion = Path.GetExtension(files[0].FileName);

                            if (books.ImgUrl != null)
                            {
                                
                                var imagePath = Path.Combine(webRootPath, books.ImgUrl.TrimStart('\\'));
                                if (System.IO.File.Exists(imagePath))
                                {
                                    System.IO.File.Delete(imagePath);
                                }
                            }
                            using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                            {
                                files[0].CopyTo(filesStreams);
                            }
                            books.ImgUrl = @"\img\book\" + fileName + extenstion;
                        }
                        var book = await _unitOfWork.Books.GetFirstOrDefaultAsync(a => a.Id == books.Id);

                        book.ImgUrl = books.ImgUrl;
                        book.Author = books.Author;
                        book.ISBN = books.ISBN;
                        book.Name = books.Name;
                        book.Price = books.Price;
                        book.Description = books.Description;
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

            return View(books);
        }

        public async Task<IActionResult> ViewBook(string id)
        {
            var book = await _unitOfWork.Books.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(id));
            if (book == null)
            {
                return RedirectToAction(nameof(Index));
            }
           
            return View(book);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ViewBook(Books books)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        
                        var book = await _unitOfWork.Books.GetFirstOrDefaultAsync(a => a.Id == books.Id);

                      
                        book.CurStatus = book.CurStatus==SD.Active?SD.DeActive:SD.Active;
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

            return View(books);
        }

        #region Data Loads
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var objList = await _unitOfWork.Books.GetAllAsync();
            return Json(new { data = objList });
        }
       
        #endregion

    }
}
