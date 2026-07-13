using Microsoft.AspNetCore.Http;

namespace Nadixa.Web.Helpers
{
    public static class CouponSessionHelper
    {
        private const string SessionKey = "AppliedCouponCode";

        public static string? Get(HttpContext httpContext) =>
            httpContext.Session.GetString(SessionKey);

        public static void Set(HttpContext httpContext, string couponCode) =>
            httpContext.Session.SetString(SessionKey, couponCode);

        public static void Remove(HttpContext httpContext) =>
            httpContext.Session.Remove(SessionKey);
    }
}