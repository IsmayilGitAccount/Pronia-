using ProniaApplication.ViewModels;

namespace ProniaApplication.Services.Interfaces
{
    public interface ILayoutService
    {
        Task<Dictionary<string, string>> GetSettingsAsync();
        
    }
}
