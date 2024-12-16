using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.ContentModel;
using ProniaApplication.DAL;
using ProniaApplication.Models;
using ProniaApplication.Services.Interfaces;
using ProniaApplication.ViewModels;

namespace ProniaApplication.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<AppUser> _userManager;

        public IBasketService _basketService { get; }

        public BasketController(AppDBContext context, UserManager<AppUser> userManager, IBasketService basketService)
        {
            _context = context;
            _userManager = userManager;
            _basketService = basketService;
        }
        public async Task<IActionResult> Index()
        {


            return View(await _basketService.GetBasketAsync());
        }

        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id is null || id < 1) return BadRequest();

            bool result = await _context.Products.AnyAsync(p => p.Id == id);

            if (!result) return NotFound();

            if (User.Identity.IsAuthenticated)
            {
                AppUser? user = await _userManager.Users.Include(u => u.BasketItems).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                BasketItem item = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);

                if (item is null)
                {
                    user.BasketItems.Add(new BasketItem()
                    {
                        ProductId = id.Value,
                        Count = 1
                    });
                }
                else
                {
                    item.Count++;
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                List<BasketCookieItemVM> basket;

                string cookies = Request.Cookies["basket"];

                if (cookies is not null)
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookies);

                    BasketCookieItemVM existed = basket.FirstOrDefault(p => p.Id == id);

                    if (existed is not null)
                    {
                        existed.Count++;
                    }
                    else
                    {
                        basket.Add(new BasketCookieItemVM()
                        {
                            Id = id.Value,
                            Count = 1
                        });
                    }

                }
                else
                {
                    basket = new();
                    basket.Add(new BasketCookieItemVM()
                    {
                        Id = id.Value,
                        Count = 1
                    });
                }


                string json = JsonConvert.SerializeObject(basket);

                Response.Cookies.Append("basket", json);
            }

            return RedirectToAction(nameof(GetBasket));

        }

        public async Task<IActionResult> GetBasket()
        {
            return PartialView("basketPartialView", await _basketService.GetBasketAsync());
        }

        //public async Task<IActionResult> GetBasket()
        //{
        //    return Content(Request.Cookies["basket"]);
        //}

        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Checkout()
        {
            OrderVM orderVM = new OrderVM()
            {
                BasketInOrderVms = await _context
                .BasketItems
                .Where(bi => bi.AppUserID == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Select(bi => new BasketInOrderVM()
                {
                    Name = bi.Product.Name,
                    Price = bi.Product.Price,
                    Count = bi.Count,
                    SubTotal = bi.Count * bi.Product.Price
                })
                .ToListAsync()
            };
            return View(orderVM);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(OrderVM orderVM)
        {
            List<BasketItem> basketItems = await _context
                .BasketItems
                .Where(bi => bi.AppUserID == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Include(bi => bi.Product)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                orderVM.BasketInOrderVms = basketItems.Select(bi => new BasketInOrderVM()
                {
                    Name = bi.Product.Name,
                    Price = bi.Product.Price,
                    Count = bi.Count,
                    SubTotal = bi.Count * bi.Product.Price
                }).ToList();

                return View(orderVM);
            }

            Order order = new Order()
            {
                Address = orderVM.Address,
                AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                OrderItems = basketItems.Select(bi => new OrderItem
                {
                    AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Count = bi.Count,
                    Price = bi.Product.Price,
                    ProductId = bi.ProductId,

                }).ToList(),
                TotalPrice = basketItems.Sum(bi => bi.Product.Price * bi.Count)
            };

            await _context.Orders.AddAsync(order);

            _context.BasketItems.RemoveRange(basketItems);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");

        }


        public async Task<IActionResult> Remove(int? id)
        {
            List<BasketCookieItemVM> basket;

            string cookies = Request.Cookies["basket"];

            if (User.Identity.IsAuthenticated)
            {
                AppUser? user = await _userManager.Users.Include(u => u.BasketItems).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
                BasketItem item = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);
                if (item != null)
                {
                    _context.BasketItems.Remove(item);
                    
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Plus(int? id)
        {
            if (id is null || id < 1) return BadRequest();

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.Users.Include(u => u.BasketItems).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                BasketItem item = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);

                if (item != null) {
                    item.Count++;
                }
                
                await _context.SaveChangesAsync();
            }
            else
            {
                List<BasketCookieItemVM> basket;

                string cookies = Request.Cookies["basket"];

                if (cookies is not null)
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookies);

                    BasketCookieItemVM existed = basket.FirstOrDefault(p => p.Id == id);

                    if (existed is not null)
                    {
                        existed.Count++;
                    }

                    string json = JsonConvert.SerializeObject(basket);

                    Response.Cookies.Append("basket", json);

                }
            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Minus(int? id)
        {
            if (id is null || id < 1) return BadRequest();

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.Users.Include(u => u.BasketItems).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                BasketItem item = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);

                if (item != null)
                {
                    item.Count--;

                    if (item.Count < 0)
                    {
                        _context.Remove(item);
                    }
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                List<BasketCookieItemVM> basket;

                string cookies = Request.Cookies["basket"];

                if (cookies is not null)
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookies);

                    var existed = basket.FirstOrDefault(p => p.Id == id);

                    if (existed is not null)
                    {
                        existed.Count--;
                        if(existed.Count < 1)
                        {
                            basket.Remove(existed);
                            
                        }
                    }
                    
                    string json = JsonConvert.SerializeObject(basket);

                    Response.Cookies.Append("basket", json);

                }
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
