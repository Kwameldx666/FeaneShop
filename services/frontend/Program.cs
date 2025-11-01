var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowAll");

var routeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["/"] = "/Pages/Home/Menu.html",
    ["/home"] = "/Pages/Home/Menu.html",
    ["/home/index"] = "/Pages/Home/Menu.html",
    ["/home/menu"] = "/Pages/Home/Menu.html",
    ["/home/about"] = "/Pages/Home/About.html",
    ["/home/privacy"] = "/Pages/Home/Privacy.html",
    ["/menu"] = "/Pages/Home/Menu.html",
    ["/about"] = "/Pages/Home/About.html",
    ["/privacy"] = "/Pages/Home/Privacy.html",

    ["/account"] = "/Pages/Account/ReservationHistory.html",
    ["/account/index"] = "/Pages/Account/ReservationHistory.html",
    ["/account/addresses"] = "/Pages/Account/Addresses.html",
    ["/account/authentication"] = "/Pages/Account/Authentication.html",
    ["/account/contacts"] = "/Pages/Account/Contacts.html",
    ["/account/reservationhistory"] = "/Pages/Account/ReservationHistory.html",
    ["/account/reservation-history"] = "/Pages/Account/ReservationHistory.html",
    ["/account/reservations"] = "/Pages/Account/ReservationHistory.html",
    ["/account/history"] = "/Pages/Account/ReservationHistory.html",
    ["/account/login"] = "/Pages/Account/Authentication.html",
    ["/account/signin"] = "/Pages/Account/Authentication.html",
    ["/account/register"] = "/Pages/Account/Authentication.html",
    ["/account/profile"] = "/Pages/Account/ReservationHistory.html",
    ["/account/resetpassword"] = "/Pages/Account/ResetPassword.html",
    ["/account/reset-password"] = "/Pages/Account/ResetPassword.html",
    ["/account/logout"] = "/Pages/Account/Authentication.html",

    ["/addresses"] = "/Pages/Account/Addresses.html",
    ["/contacts"] = "/Pages/Account/Contacts.html",
    ["/profile"] = "/Pages/Account/ReservationHistory.html",

    ["/analytics"] = "/Pages/Analytics/Index.html",
    ["/analytics/index"] = "/Pages/Analytics/Index.html",

    ["/jwt-test"] = "/Pages/jwt-test.html",
    ["/test"] = "/Pages/jwt-test.html",
    ["/auth-test"] = "/Pages/auth-test.html",
    ["/diagnostic"] = "/Pages/diagnostic.html",
    ["/diag"] = "/Pages/diagnostic.html",
    ["/instructions"] = "/Pages/instructions.html",
    ["/help"] = "/Pages/instructions.html",

    ["/cart"] = "/Pages/Cart/Cart.html",
    ["/cart/cart"] = "/Pages/Cart/Cart.html",

    ["/order"] = "/Pages/Orders/Index.html",
    ["/order/index"] = "/Pages/Orders/Index.html",
    ["/order/checkout"] = "/Pages/Orders/Checkout.html",
    ["/order/payment"] = "/Pages/Orders/Payment.html",
    ["/order/details"] = "/Pages/Orders/Details.html",

    ["/orders"] = "/Pages/Orders/Index.html",
    ["/orders/index"] = "/Pages/Orders/Index.html",
    ["/orders/checkout"] = "/Pages/Orders/Checkout.html",
    ["/orders/payment"] = "/Pages/Orders/Payment.html",
    ["/orders/details"] = "/Pages/Orders/Details.html",

    ["/dish"] = "/Pages/Dish/Index.html",
    ["/dish/index"] = "/Pages/Dish/Index.html",
    ["/dish/adddish"] = "/Pages/Dish/AddDish.html",
    ["/dish/editdish"] = "/Pages/Dish/EditDish.html",

    ["/notifications"] = "/Pages/Notifications/Index.html",
    ["/notifications/index"] = "/Pages/Notifications/Index.html",
    ["/notifications/filters"] = "/Pages/Notifications/Filters.html",

    ["/payment/checkout"] = "/Pages/Payment/Checkout.html",
    ["/payment/confirmation"] = "/Pages/Payment/Confirmation.html",

    ["/reservation"] = "/Pages/Reservation/Book.html",
    ["/reservation/index"] = "/Pages/Reservation/Book.html",
    ["/reservation/book"] = "/Pages/Reservation/Book.html",
    ["/book"] = "/Pages/Reservation/Book.html",

    ["/user"] = "/Pages/User/Index.html",
    ["/user/index"] = "/Pages/User/Index.html",

    ["/weather"] = "/Pages/Weather/Index.html",
    ["/weather/index"] = "/Pages/Weather/Index.html",

    ["/error/404"] = "/Pages/Error/Error404.html",
    ["/404"] = "/Pages/Error/Error404.html"
};

app.Use(async (context, next) =>
{
    var requestPath = context.Request.Path.Value ?? "/";

    if (string.IsNullOrWhiteSpace(requestPath) || requestPath == "/")
    {
        requestPath = "/";
    }
    else
    {
        requestPath = requestPath.TrimEnd('/');
        if (requestPath.Length == 0) requestPath = "/";
    }

    if (!requestPath.Contains('.') && routeMap.TryGetValue(requestPath, out var mappedPath))
        context.Request.Path = mappedPath;

    await next();
});

app.UseStaticFiles();

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == StatusCodes.Status404NotFound &&
        !context.Response.HasStarted)
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync("wwwroot/Pages/Error/Error404.html");
    }
});

app.Run();