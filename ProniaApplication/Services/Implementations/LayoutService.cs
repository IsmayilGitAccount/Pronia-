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
        
        public LayoutService(AppDBContext context, IHttpContextAccessor http)
        {
            _context = context;
            
        }

       

        

        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
            return settings;
        }
    }
}
