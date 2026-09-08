var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// here connecting the BankingManagementAPI with the web application using HttpClientFactory to make HTTP requests to the API endpoints.
// The base address of the API is set to "https://localhost:7112/". This allows the web application to communicate with the
// BankingManagementAPI for various operations such as adding customers, retrieving customer information, and managing accounts.
builder.Services.AddHttpClient("BankingManagementAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7112/"); 
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
