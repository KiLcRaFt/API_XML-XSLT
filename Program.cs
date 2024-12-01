using Microsoft.EntityFrameworkCore;
using API_XML_XSLT.Models;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры
builder.Services.AddControllers();

// Добавляем распределённую память для сессий
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Время действия сессии
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Подключаем DbContext
builder.Services.AddDbContext<TootajaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настраиваем CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Добавляем авторизацию (если требуется для вашей логики)
builder.Services.AddAuthorization();

// Подключаем Swagger для документации API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
});

var app = builder.Build();

// Подключаем Swagger в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Настраиваем middleware
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseSession(); // Подключаем поддержку сессий
app.UseAuthorization();

// Подключаем контроллеры
app.MapControllers();

app.Run();
