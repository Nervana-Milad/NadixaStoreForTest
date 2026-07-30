using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Web.Helpers;
using Nadixa.Web.Models.ViewModels;
using System.Threading.Tasks;

namespace Nadixa.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCouponController : Controller
    {
        private readonly ICouponService _couponService;

        public AdminCouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        // GET: /AdminCoupon
        public async Task<IActionResult> Index()
        {
            var coupons = await _couponService.GetAllAsync();
            return View(coupons);
        }

        // GET: /AdminCoupon/Create
        public IActionResult Create()
        {
            return View("Form", new CouponFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("Form", vm);

            var coupon = MapToEntity(vm, new Coupon());
            coupon.Code = coupon.Code.Trim().ToUpper();

            await _couponService.CreateAsync(coupon);

            TempData["Success"] = AppMessages.CouponCreated;
            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminCoupon/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _couponService.GetByIdAsync(id);
            if (coupon == null) return NotFound();

            return View("Form", MapToViewModel(coupon));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CouponFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("Form", vm);

            var coupon = await _couponService.GetByIdAsync(vm.Id);
            if (coupon == null) return NotFound();

            MapToEntity(vm, coupon);
            coupon.Code = coupon.Code.Trim().ToUpper();

            await _couponService.UpdateAsync(coupon);

            TempData["Success"] = AppMessages.CouponUpdated;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _couponService.DeleteAsync(id);
            TempData["Success"] = AppMessages.CouponDeleted;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            await _couponService.ToggleActiveAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private static Coupon MapToEntity(CouponFormViewModel vm, Coupon entity)
        {
            entity.Code = vm.Code;
            entity.DiscountType = vm.DiscountType;
            entity.Value = vm.Value;
            entity.MinOrderAmount = vm.MinOrderAmount;
            entity.MaxDiscountAmount = vm.MaxDiscountAmount;
            entity.MaxTotalUsage = vm.MaxTotalUsage;
            entity.MaxUsagePerUser = vm.MaxUsagePerUser;
            entity.FirstOrderOnly = vm.FirstOrderOnly;
            entity.StartDate = vm.StartDate;
            entity.EndDate = vm.EndDate;
            entity.IsActive = vm.IsActive;
            return entity;
        }

        private static CouponFormViewModel MapToViewModel(Coupon c) => new()
        {
            Id = c.Id,
            Code = c.Code,
            DiscountType = c.DiscountType,
            Value = c.Value,
            MinOrderAmount = c.MinOrderAmount,
            MaxDiscountAmount = c.MaxDiscountAmount,
            MaxTotalUsage = c.MaxTotalUsage,
            MaxUsagePerUser = c.MaxUsagePerUser,
            FirstOrderOnly = c.FirstOrderOnly,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            IsActive = c.IsActive
        };
    }
}
