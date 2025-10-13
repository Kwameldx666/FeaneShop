using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FeaneMVC.Attributes
{
    public class ModeratorModeAttribute : ActionFilterAttribute
    {
        private readonly ISessionService _session;

        // Constructor to initialize the ISession instance
        public ModeratorModeAttribute(ISessionService session)
        {
            _session = session;
        }

        // This method is executed before the action method is called
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Retrieve the cookie named "X-KEY" from the HTTP request
            var apiCookie = context.HttpContext.Request.Cookies["X-KEY"];

            if (apiCookie != null)
            {
                // Call the asynchronous method to get the user profile by cookie
                var profile = _session.GetUserByCookie(apiCookie);

                // Check if the profile is not null and if the user role is Moderator
                if (profile != null && profile.Roles == Role.Moderator)
                {
                    // Set the user profile in the current HttpContext
                    context.HttpContext.Items["UserProfile"] = profile;
                }
                else
                {
                    // Redirect to the error page if the user is not a Moderator
                    context.Result = new RedirectToActionResult("Error404", "Error", null);
                }
            }
            else
            {
                // Redirect to the error page if the cookie is not found
                context.Result = new RedirectToActionResult("Error404", "Error", null);
            }

            // Call the base method to ensure the filter executes correctly
            base.OnActionExecuting(context);
        }
    }
}
