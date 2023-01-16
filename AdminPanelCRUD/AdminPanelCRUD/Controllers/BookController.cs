﻿
using AdminPanelCRUD.Models;
using Newtonsoft.Json;

namespace AdminPanelCRUD.Controllers
{
    public class BookController :Controller
    {
        private readonly PustokContext _pustokContext;
        public BookController(PustokContext pustokContext)
        {     
            _pustokContext= pustokContext;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Detail(int id)
        {

            Book book = _pustokContext.Books
                .Include(x => x.Author).Include(x=>x.Genre)
                .Include(x=>x.BookImages).FirstOrDefault(x=>x.Id==id);

            if (book == null) return View("Error");
            BookDetailViewModel bookVM = new BookDetailViewModel
            {
            Book = book,
            RelatedBooks = _pustokContext.Books
                .Include(x => x.BookImages)
                .Include(x => x.Author).Include(x => x.Genre)
                .Where(x => x.GenreId == book.GenreId).ToList(),
            };
            return View(bookVM);
        }

        public IActionResult AddToBasket(int bookId)
        {
            if (!_pustokContext.Books.Any(x => x.Id == bookId)) return NotFound(); //404
            List<BasketItemViewModel> basketItems = new List<BasketItemViewModel>();
            BasketItemViewModel basketItem = null;
            string basketItemsStr = HttpContext.Request.Cookies["BasketItems"];

            if (basketItemsStr != null)
            {
                basketItems = JsonConvert.DeserializeObject<List<BasketItemViewModel>>(basketItemsStr);
                basketItem = basketItems.FirstOrDefault(x => x.BookId == bookId);
                if (basketItem != null) basketItem.Count++;
                else
                {
                    basketItem = new BasketItemViewModel
                    {
                        BookId = bookId,
                        Count = 1
                    };
                    basketItems.Add(basketItem);
                }
            }
            else
            {
                basketItem = new BasketItemViewModel
                {
                    BookId = bookId,
                    Count = 1
                };
                basketItems.Add(basketItem);
            }

            basketItemsStr = JsonConvert.SerializeObject(basketItems);
            HttpContext.Response.Cookies.Append("BasketItems", basketItemsStr);

            return Ok(); //200
        }

        public IActionResult GetBasket()
        {
            List<BasketItemViewModel> basketItems = new List<BasketItemViewModel>();
            string basketItemsStr = HttpContext.Request.Cookies["BasketItems"];

            if (basketItemsStr != null)
            {
                basketItems = JsonConvert.DeserializeObject<List<BasketItemViewModel>>(basketItemsStr);
            }
            return Json(basketItems);
        }

        public IActionResult Checkout()
        {
            List<BasketItemViewModel> basketItems = new List<BasketItemViewModel>();
            List<CheckoutItemViewModel> checkoutItems = new List<CheckoutItemViewModel>();
            CheckoutItemViewModel checkoutItem = null;
            string basketItemStr = HttpContext.Request.Cookies["BasketItems"];

            if (basketItemStr!=null)
            {
                basketItems = JsonConvert.DeserializeObject <List<BasketItemViewModel>> (basketItemStr);

                foreach(var item in basketItems)
                {
                    checkoutItem = new CheckoutItemViewModel
                    {
                        Book=_pustokContext.Books.FirstOrDefault(x => x.Id == item.BookId),
                        Count=item.Count,
                };
                    checkoutItems.Add(checkoutItem);
                }
            }
            return View(checkoutItems);
        }

    }
}
