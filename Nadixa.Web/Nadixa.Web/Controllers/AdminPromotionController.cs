using Nadixa.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Web.Models.ViewModels;
using System.Threading.Tasks;
using Nadixa.Web.Helpers;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Common;

namespace Nadixa.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPromotionController : Controller
    {
        private readonly IPromotionService _promotionService;
        private readonly NadixaDbContext _context; // لتعبئة الدروب داون بتاعة الأقسام/المنتجات

        public AdminPromotionController(IPromotionService promotionService, NadixaDbContext context)
        {
            _promotionService = promotionService;
            _context = context;
        }

        // GET: /AdminPromotion
        public async Task<IActionResult> Index()
        {
            var promotions = await _promotionService.GetAllAsync();
            return View(promotions);
        }

        // GET: /AdminPromotion/Create
        public async Task<IActionResult> Create()
        {
            var vm = new PromotionFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View("Form", vm);
            }

            var promotion = MapToEntity(vm, new Promotion());
            await _promotionService.CreateAsync(promotion);

            TempData["Success"] = AppMessages.PromotionCreated;
            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminPromotion/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var promotion = await _promotionService.GetByIdAsync(id);
            if (promotion == null) return NotFound();

            var vm = MapToViewModel(promotion);
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PromotionFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View("Form", vm);
            }

            var promotion = await _promotionService.GetByIdAsync(vm.Id);
            if (promotion == null) return NotFound();

            MapToEntity(vm, promotion);
            await _promotionService.UpdateAsync(promotion);

            TempData["Success"] = AppMessages.PromotionUpdated;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _promotionService.DeleteAsync(id);
            TempData["Success"] = AppMessages.PromotionDeleted;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            await _promotionService.ToggleActiveAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync(PromotionFormViewModel vm)
        {
            vm.Categories = await _context.ProductCategories.ToListAsync();
            vm.SubCategories = await _context.ProductSubCategories.ToListAsync();
            vm.Products = await _context.Products.ToListAsync();
        }

        private static Promotion MapToEntity(PromotionFormViewModel vm, Promotion entity)
        {
            entity.Name = vm.Name;
            entity.Description = vm.Description;
            entity.BadgeText = vm.BadgeText;
            entity.BadgeColorHex = vm.BadgeColorHex;
            entity.Type = vm.Type;
            entity.Scope = vm.Scope;
            entity.Value = vm.Value;
            entity.BuyQuantity = vm.BuyQuantity;
            entity.FreeQuantity = vm.FreeQuantity;
            entity.ProductCategoryId = vm.Scope == PromotionScope.Category ? vm.ProductCategoryId : null;
            entity.ProductSubCategoryId = vm.Scope == PromotionScope.SubCategory ? vm.ProductSubCategoryId : null;
            entity.ProductId = vm.Scope == PromotionScope.SpecificProduct ? vm.ProductId : null;
            entity.StartDate = vm.StartDate;
            entity.EndDate = vm.EndDate;
            entity.IsActive = vm.IsActive;
            entity.Priority = vm.Priority;
            entity.IsFirstPurchaseOnly = vm.IsFirstPurchaseOnly;
            return entity;
        }

        private static PromotionFormViewModel MapToViewModel(Promotion p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            BadgeText = p.BadgeText,
            BadgeColorHex = p.BadgeColorHex ?? "#FF3B30",
            Type = p.Type,
            Scope = p.Scope,
            Value = p.Value,
            BuyQuantity = p.BuyQuantity,
            FreeQuantity = p.FreeQuantity,
            ProductCategoryId = p.ProductCategoryId,
            ProductSubCategoryId = p.ProductSubCategoryId,
            ProductId = p.ProductId,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = p.IsActive,
            Priority = p.Priority,
            IsFirstPurchaseOnly = p.IsFirstPurchaseOnly
        };
    }
}
