using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Repositories;
using WorkoutFitnessTrackerAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Microsoft.AspNetCore.Hosting;
using WorkoutFitnessTrackerAPI.Mappings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WorkoutFitnessTrackerAPI", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the text input below.\n\nExample: \"abcdef12345\"",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

DotNetEnv.Env.Load();
var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        RequireExpirationTime = true,
        ValidateLifetime = true,
    };
});
string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!;
builder.Services.AddDbContext<WFTDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = true;
})
.AddRoles<IdentityRole<Guid>>()
.AddEntityFrameworkStores<WFTDbContext>() 
.AddSignInManager<SignInManager<User>>(); 

builder.Services.AddScoped<ITokenService,TokenService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
builder.Services.AddAutoMapper(typeof(WorkoutMappingProfile));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WorkoutFitnessTrackerAPI v1"));
}

app.UseHttpsRedirection();

app.UseAuthentication();  
app.UseAuthorization();   

app.MapControllers();

app.Run();
