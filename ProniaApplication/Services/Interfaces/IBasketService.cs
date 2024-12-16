using ProniaApplication.ViewModels;

namespace ProniaApplication.Services.Interfaces
{
    public interface IBasketService
    {
        Task<List<BasketItemVM>> GetBasketAsync();
    }
}
