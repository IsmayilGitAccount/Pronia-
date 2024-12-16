using System.Security.Claims;
using Azure.Core;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaApplication.DAL;
using ProniaApplication.Models;
using ProniaApplication.Services.Interfaces;
using ProniaApplication.ViewModels;

namespace ProniaApplication.Services.Impelementations
{
    public class LayoutService:ILayoutService
    {
        private readonly AppDBContext _context;
        public readonly IHttpContextAccessor _http;
        public readonly ClaimsPrincipal _user;
        public LayoutService(AppDBContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
            _user = _http.HttpContext.User;
        }

       

        public async Task<List<BasketItemVM>> GetBasketAsync()
        {
            List<BasketItemVM> basketVM = new();

            if (_user.Identity.IsAuthenticated)
            {
                basketVM = await _context.BasketItems
                    .Where(bi => bi.AppUserID == _user.FindFirstValue(ClaimTypes.NameIdentifier))
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
                string cookie = _http.HttpContext.Request.Cookies["basket"];

                if (cookie == null)
                {
                    return basketVM;
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

            return basketVM;
        }

        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
            return settings;
        }
    }
}
