using Microsoft.EntityFrameworkCore;
using API_XML_XSLT.Models;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ��������� �����������
builder.Services.AddControllers();

// ��������� ������������� ������ ��� ������
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ����� �������� ������
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ���������� DbContext
builder.Services.AddDbContext<TootajaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ����������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ��������� ����������� (���� ��������� ��� ����� ������)
builder.Services.AddAuthorization();

// ���������� Swagger ��� ������������ API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
});

var app = builder.Build();

// ���������� Swagger � ������ ����������
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ����������� middleware
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseSession(); // ���������� ��������� ������
app.UseAuthorization();

// ���������� �����������
app.MapControllers();

app.Run();
