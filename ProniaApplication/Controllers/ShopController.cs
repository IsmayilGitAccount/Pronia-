using System.Security.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaApplication.DAL;
using ProniaApplication.Models;
using ProniaApplication.Utilities.Enums;
using ProniaApplication.Utilities.Exceptions;
using ProniaApplication.ViewModels;

namespace ProniaApplication.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDBContext _context;

        public ShopController(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string? search, int? categoryId, int key=1, int page=1)
        {
            IQueryable<Product> query = _context.Products.Include(p => p.productsImages.Where(pi => pi.IsPrimary != null));

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            if (categoryId != null && categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            switch (key)
            {
                case (int)SortType.Name:
                    query = query.OrderBy(p => p.Name);
                    break;
                case (int)SortType.Price:
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case (int)SortType.Date:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }


            int count = query.Count();
            double totalPage = Math.Ceiling((double)count / 3);

            query = query.Skip((page - 1) * 3).Take(3);


            ShopVM shopVM = new ShopVM()
            {
                Products = query.Select(p=>new GetProductVM
                {
                    Id= p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Image = p.productsImages.FirstOrDefault(pi=>pi.IsPrimary==true).ImageURL,
                    SecondaryImage = p.productsImages.FirstOrDefault(pi => pi.IsPrimary == false).ImageURL,
                }).ToList(),

                Categories = await _context.Categories.Select(c=>new GetCategoryVM
                {
                    Id= c.Id,
                    Name = c.Name,
                    Count = c.Products.Count
                }).ToListAsync(),
                Search = search,
                CategoryId = categoryId,
                Key = key,
                TotalPage = totalPage,
                CurrentPage = page
            };

            return View(shopVM);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0) throw new BadRequestException($"There are no any product with {id}");

            Product? product = await _context.Products
                .Include(p => p.productsImages
                .OrderByDescending(pi => pi.IsPrimary))
                .Include(p => p.category)
                .Include(p => p.ProductColors)
                .ThenInclude(pc => pc.Color)
                .Include(p => p.ProductSizes)
                .ThenInclude(pc => pc.Size)
                .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) throw new NotFoundException($"Not Found product with {id}!");

            DetailsVM detailsVM = new DetailsVM
            {
                Product = product,
                RelatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Include(p => p.productsImages.Where(pi => pi.IsPrimary != null))
                .Take(8)
                .ToListAsync()
            };
            return View(detailsVM);
        }
    }
}
