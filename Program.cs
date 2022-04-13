using Blog;
using Blog.Data;
using Blog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
ConfigureAuthentication(builder);
ConfigureMvc(builder);
ConfigureServices(builder);

// ADICIONANDO SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
LoadConfiguration(app);

// Redireciona para Https msm o request sendo chamado em Http
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Para Utilizar o compression necessário a App reconhecer
app.UseResponseCompression();

// Utilizado para poder fazer upload de arquivos dentro do projeto
app.UseStaticFiles();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    // ADICIONANDO SWAGGER
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();


void LoadConfiguration(WebApplication app)
{
    app.Configuration.GetValue<string>("JwtKey");
    app.Configuration.GetValue<string>("ApiKeyName");
    app.Configuration.GetValue<string>("ApiKey");

    var smtp = new Configuration.SmtpConfiguration();
    // bind popula o objeto smtp com os atributos do app.settings.json em caso de tiverem o mesmo nome de atributos
    app.Configuration.GetSection("Smtp").Bind(smtp);
    Configuration.Smtp = smtp;
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);

    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    }).AddJwtBearer(x =>
    {
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,

        };
    });
}

void ConfigureMvc(WebApplicationBuilder builder)
{
    builder.Services.AddMemoryCache();

    // Utilizando Compression para a response voltar muito mais leve(zipada), o front entende e descompacta para utilizacao
    builder.Services.AddResponseCompression(options =>
    {
        options.Providers.Add<GzipCompressionProvider>();
    });

    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Optimal;
    });

    builder
    .Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    })
    .AddJsonOptions(x =>
    {
        //Adicionado para ele não se perder na serialização de objetos que possuem a mesma relação ex: Post e Category
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    });
}

void ConfigureServices(WebApplicationBuilder builder)
{

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<BlogDataContext>(options => options.UseSqlServer(connectionString));
    builder.Services.AddTransient<TokenService>(); // injeção de dependencia sem a necessidade da Interface
    builder.Services.AddTransient<EmailService>(); // injeção de dependencia sem a necessidade da Interface
}
