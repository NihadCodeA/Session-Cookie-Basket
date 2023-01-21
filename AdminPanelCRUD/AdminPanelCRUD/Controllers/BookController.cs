using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace AdminPanelCRUD.Controllers
{
    public class BookController :Controller
    {
        private readonly PustokContext _pustokContext;
        private readonly UserManager<AppUser> _userManager;
        public BookController(PustokContext pustokContext,UserManager<AppUser> userManager)
        {     
            _pustokContext = pustokContext;
            _userManager = userManager;
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

        public async Task<IActionResult> AddToBasket(int bookId)
        {
            if (!_pustokContext.Books.Any(x => x.Id == bookId)) return NotFound(); //404
            List<BasketItemViewModel> basketItems = new List<BasketItemViewModel>();
            BasketItemViewModel basketItem = null;
            string basketItemsStr = HttpContext.Request.Cookies["BasketItems"];
            //AppUser member = null;
            //if (User.Identity.IsAuthenticated)
            //{
            //    member = await _userManager.FindByNameAsync(User.Identity.Name);
            //}
            //if (member == null)
            //{
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
            //}
            //else
            //{
            //    BasketItem memberBasketItem = _pustokContext.BasketItems.Include(x=>x.Book).FirstOrDefault(x=>x.AppUserId==member.Id && x.Id==bookId);
            //    if (memberBasketItem != null) memberBasketItem.Count++;
            //    else
            //    {
            //        memberBasketItem = new BasketItem
            //        {
            //            Id = bookId,
            //            AppUserId = member.Id,
            //            Count = 1
            //        };
            //        _pustokContext.BasketItems.Add(memberBasketItem);
            //    }
            //_pustokContext.SaveChanges();
            //}
            //return PartialView("_BasketItemPartial", basketItems);
            return Ok();
        }

        public IActionResult GetBasket()
        {
            List<BasketItemViewModel> basketItems = new List<BasketItemViewModel>();
            List<CheckoutItemViewModel> checkoutItems = new List<CheckoutItemViewModel>();
            CheckoutItemViewModel checkoutItem = null;
            string basketItemStr = HttpContext.Request.Cookies["BasketItems"];

            if (basketItemStr != null)
            {
                basketItems = JsonConvert.DeserializeObject<List<BasketItemViewModel>>(basketItemStr);

                foreach (var item in basketItems)
                {
                    checkoutItem = new CheckoutItemViewModel
                    {
                        Book = _pustokContext.Books.FirstOrDefault(x => x.Id == item.BookId),
                        Count = item.Count,
                    };
                    checkoutItems.Add(checkoutItem);
                }
            }
            return Json(checkoutItems);
        }

        public async Task<IActionResult> Checkout()
        {
            List<BasketItemViewModel> basketItems = new List<BasketItemViewModel>();
            List<CheckoutItemViewModel> checkoutItems = new List<CheckoutItemViewModel>();
            CheckoutItemViewModel checkoutItem = null;
            List<BasketItem> memberBasketItems= new List<BasketItem>();
            

            string basketItemStr = HttpContext.Request.Cookies["BasketItems"];
            AppUser member = null;

            if (User.Identity.IsAuthenticated)
            {
                member = await _userManager.FindByNameAsync(User.Identity.Name);
            }
            if(member == null)
            {
                if (basketItemStr != null)
                {
                    basketItems = JsonConvert.DeserializeObject<List<BasketItemViewModel>>(basketItemStr);

                    foreach (var item in basketItems)
                    {
                        checkoutItem = new CheckoutItemViewModel
                        {
                            Book = _pustokContext.Books.FirstOrDefault(x => x.Id == item.BookId),
                            Count = item.Count,
                        };
                        checkoutItems.Add(checkoutItem);
                    }
                }
            }
            else
            {
                memberBasketItems = _pustokContext.BasketItems.Include(x=>x.Book).Where(x => x.AppUserId == member.Id).ToList();
                foreach(var item in memberBasketItems)
                {
                    checkoutItem = new CheckoutItemViewModel
                    {
                        Book = item.Book,
                        Count = item.Count
                    };
                    checkoutItems.Add(checkoutItem);
                }
            }
            return View(checkoutItems);
        }

    }
}
