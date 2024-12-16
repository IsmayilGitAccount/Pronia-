using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.ContentModel;
using ProniaApplication.DAL;
using ProniaApplication.Models;
using ProniaApplication.ViewModels;

namespace ProniaApplication.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BasketController(AppDBContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            List<BasketItemVM> basketVM = new();

            if (User.Identity.IsAuthenticated)
            {
                basketVM = await _context.BasketItems
                    .Where(bi => bi.AppUserID == User.FindFirstValue(ClaimTypes.NameIdentifier))
                    .Select(bi => new BasketItemVM()
                    {
                        Id = bi.ProductId,
                        Price = bi.Product.Price,
                        Count = bi.Count,
                        Image = bi.Product.productsImages.FirstOrDefault(pi => pi.IsPrimary == true).ImageURL,
                        Name = bi.Product.Name,
                        Subtotal = bi.Count * bi.Product.Price
                    }).ToListAsync();

            }
            else
            {
                List<BasketCookieItemVM> cookiesVM;
                string cookie = Request.Cookies["basket"];

                if (cookie == null)
                {
                    return View(basketVM);
                }

                cookiesVM = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookie);



                foreach (BasketCookieItemVM item in cookiesVM)
                {
                    Product product = await _context.Products.Include(p => p.productsImages.Where(pi => pi.IsPrimary == true)).FirstOrDefaultAsync(p => p.Id == item.Id);
                    if (product != null)
                    {
                        basketVM.Add(new BasketItemVM
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Image = product.productsImages[0].ImageURL,
                            Count = item.Count,
                            Subtotal = product.Price * item.Count,
                            Price = product.Price
                        });

                    }
                }
            }

            return View(basketVM);
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

            return RedirectToAction("Index", "Home");
        }

        public IActionResult GetBasket()
        {
            return Content(Request.Cookies["basket"]);
        }

        public async Task<IActionResult> Remove(int? id)
        {
            
            List<BasketCookieItemVM> basket;

            string cookies = Request.Cookies["basket"];

            if (cookies != null) 
            {
                basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookies);

                BasketCookieItemVM existed = basket.FirstOrDefault(p => p.Id == id);

                basket.Remove(existed);

                string json = JsonConvert.SerializeObject(basket);

                Response.Cookies.Append("basket", json);
            }

            return RedirectToAction("Index");
        }


    }
}
