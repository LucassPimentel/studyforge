using Microsoft.EntityFrameworkCore;
using StudyForge.Api.Data;
using StudyForge.Api.Services;
using StudyForge.Domain;

var builder = WebApplication.CreateBuilder(args);

const string AngularCorsPolicy = "AngularDevCors";

// Controllers (thin) — a regra de negócio vive nos services.
builder.Services.AddControllers();

// Swagger/OpenAPI para desenvolvimento.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core + SQLite (studyforge.db).
var connectionString = builder.Configuration.GetConnectionString("StudyForge")
    ?? "Data Source=studyforge.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Algoritmo de agendamento SM-2 (spec spaced-repetition). Serviço puro e sem estado.
builder.Services.AddSingleton<ISpacedRepetitionService, SM2SchedulingService>();

// Services de regra de negócio (controllers finos delegam a eles).
builder.Services.AddScoped<IDeckService, DeckService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// CORS para o frontend Angular em desenvolvimento.
var angularOrigins = builder.Configuration
    .GetSection("Cors:AngularDevOrigins")
    .Get<string[]>() ?? ["http://localhost:4200"];
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
        policy.WithOrigins(angularOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// Aplica as migrations pendentes na inicialização, criando/atualizando o SQLite.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Em Development a API roda apenas em HTTP (perfil "http", porta 5200) e o Angular
// a consome via HTTP; aplicar o redirect para HTTPS quebraria essas chamadas.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(AngularCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
