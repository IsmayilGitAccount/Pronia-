using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaApplication.DAL;
using ProniaApplication.Models;
using ProniaApplication.ViewModels;

namespace ProniaApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly AppDBContext _context;

        public HomeController(AppDBContext context)
        {
            _context = context;
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
    }
}
