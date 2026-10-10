using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NovelProject;
using NovelProject.BackGround;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



builder.Services.AddSignalR();
// 读取 CORS 配置
var corsSettings = builder.Configuration.GetSection("CorsSettings").Get<CorsSettings>();

// 注册 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsSettings.PolicyName, policy =>
    {
        policy.WithOrigins(corsSettings.AllowOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var scheme = new OpenApiSecurityScheme()
    {
        Description = "Authorization header. \r\nExample: 'Bearer 12345abcdef'",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Authorization" },
        Scheme = "oauth2",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header
    };

    c.AddSecurityDefinition("Authorization", scheme);

    var requirement = new OpenApiSecurityRequirement();
    requirement[scheme] = new List<string>();

    c.AddSecurityRequirement(requirement);
});


builder.Services.AddDbContext<MyDbContext>(opt =>
{
    string constr = builder.Configuration.GetConnectionString("Default");
    opt.UseSqlServer(constr);
});
builder.Services.AddDataProtection();





builder.Services.AddIdentityCore<User>(option =>
{
    option.Password.RequiredLength = 6;
    option.Password.RequireNonAlphanumeric = false;
    option.Password.RequireDigit = false;
    option.Password.RequireLowercase = false;
    option.Password.RequireUppercase = false;

    option.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
    option.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;

    option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 锁定5分钟
    option.Lockout.MaxFailedAccessAttempts = 5; // 最多5次失败
    option.Lockout.AllowedForNewUsers = true; // 新用户允许锁定
});
var ibuilder = new IdentityBuilder(typeof(User), typeof(Role), builder.Services);
ibuilder.AddEntityFrameworkStores<MyDbContext>()
    .AddDefaultTokenProviders().AddUserManager<UserManager<User>>()
    .AddRoleManager<RoleManager<Role>>();


builder.Services.AddScoped<IJwtService, JwtService>();
//配置绑定，把appsettings.json配置文件里的JWT节点，绑定到自定义的JWTSettings配置类上
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));
//注册认证服务
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        //读取配置 + 生成签名密钥
        var jwtsettings = builder.Configuration.GetSection("JWT").Get<JwtSettings>();
        // 把字符串密钥转成字节数组
        byte[] keyBytes = Encoding.UTF8.GetBytes(jwtsettings.SecKey);
        // 生成JWT用的对称安全密钥
        var seckey = new SymmetricSecurityKey(keyBytes);
        opt.TokenValidationParameters = new()
        {
            //JWT 校验规则配置
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = seckey,
            ClockSkew = TimeSpan.Zero//令牌过期后，立即失效
        };
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                //websocket不支持自定义报文头
                //所有我们需要把JWT通过url中的QueryString传递
                //然后在服务器的OnMessageReceived中，把QueryString的jwt读出来,然后赋值给context.token
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/Hubs/ChatRoomHub")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddHostedService<ReadingRecordCleanUpService>();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseStaticFiles(); // 启用默认的wwwroot文件夹静态文件访问

// 配置Picture文件夹的静态文件访问
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Picture")),
    RequestPath = "/Picture"
});


// 启用 CORS（必须在 认证 之前）
app.UseCors(corsSettings.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatRoomHub>("/Hubs/ChatRoomHub");

app.MapControllers();

app.Run();

// CORS 配置实体
public class CorsSettings
{
    public string PolicyName { get; set; }
    public string[] AllowOrigins { get; set; }
}