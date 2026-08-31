using API_HorarioProfessor.Controllers;
using API_HorarioProfessor.Data;
using API_HorarioProfessor.Models;
using API_HorarioProfessor.Services;
using API_HorarioProfessor.Services.CoreServices;
using API_HorarioProfessor.Services.CoreServices.MetodosAuxiliares;
using API_HorarioProfessor.Services.ServicesCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Adicionar suporte para logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug(); // Adiciona suporte para debug logging

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Determine the connection string based on the configuration parameter
var useProduction = builder.Configuration.GetValue<bool>("DatabaseConfig:ProductionConnection");
var connectionString = builder.Configuration.GetConnectionString(useProduction ? "ProductionConnection" : "DesenvConnection");

// Configuração do contexto do banco de dados
builder.Services.AddDbContext<HorarioProfessorDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)), mysqlOptions =>
    {
        mysqlOptions.EnableRetryOnFailure();
        mysqlOptions.CommandTimeout(60); // Ajuste o tempo limite conforme necessário
    }));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)), mysqlOptions =>
    {
        mysqlOptions.EnableRetryOnFailure();
        mysqlOptions.CommandTimeout(60); // Ajuste o tempo limite conforme necessário
    }));

// Registrar os serviços - Ajustar para Transient onde aplicável
builder.Services.AddTransient<IParametroDiarioOficialService, ParametroDiarioOficialService>();

// Scoped garante que a mesma instância da variável será usada durante toda a requisição
builder.Services.AddScoped<AppState>();
// Registro dos serviços incluindo o EscolaService
builder.Services.AddTransient<IEscolaService, EscolaService>();
builder.Services.AddTransient<IUsuarioAcessoService, UsuarioAcessoService>();

// Registrar o serviço IProfessorService e sua implementação
builder.Services.AddTransient<IProfessorService, ProfessorService>();

// Registro do serviço de distribuição do horário do professor
builder.Services.AddTransient<PrincipalService>();

// Registrar o LogService como Transient
builder.Services.AddTransient<LogService>();
builder.Services.AddTransient<IProfessorNewCadastroService, ProfessorNewCadastroService>();
builder.Services.AddTransient<ConfiguracaoAcaoController>();
builder.Services.AddTransient<IOpcoesProfessorService, OpcoesProfessorService>();

// Registro do TurmaPadraoEscolaService
builder.Services.AddScoped<TurmaPadraoEscolaService>();
builder.Services.AddScoped<GetMaterialAulasPorAno>();

// No arquivo Program.cs ou Startup.cs
builder.Services.AddScoped<HorarioService>();
builder.Services.AddScoped<HorarioDistribuicaoService>();



// Configurar o logging
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddConsole();
    logging.AddDebug();
});

// Lendo a configuração do ambiente
//var environment = builder.Configuration.GetValue<string>("Environment");
//string pdfFilePath = builder.Configuration.GetSection("PdfFilePaths")[environment];

var app = builder.Build();

// A distribuição é iniciada pelos endpoints, não durante a inicialização da API.

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}
else if (app.Environment.IsProduction())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "v1.0");
        options.RoutePrefix = "api/swagger";
    });
}

app.UseSwagger();
app.UseSwaggerUI();

// Não usar HTTPS, pois ainda não está configurado
// app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();



//**********************************************
//antes de mudar para transiente.
//using API_HorarioProfessor.Controllers;
//using API_HorarioProfessor.Data;
//using API_HorarioProfessor.Models;
//using API_HorarioProfessor.Services;
//using API_HorarioProfessor.Services.CoreServices;
//using API_HorarioProfessor.Services.ServicesCore;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//var builder = WebApplication.CreateBuilder(args);

//// Adicionar suporte para logging
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
//builder.Logging.AddDebug(); // Adiciona suporte para debug logging

//// Add services to the container.
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// Determine the connection string based on the configuration parameter
//var useProduction = builder.Configuration.GetValue<bool>("DatabaseConfig:ProductionConnection");
//var connectionString = builder.Configuration.GetConnectionString(useProduction ? "ProductionConnection" : "DesenvConnection");

//builder.Services.AddDbContext<HorarioProfessorDbContext>(options =>
//    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)), mysqlOptions =>
//    {
//        mysqlOptions.EnableRetryOnFailure();
//        mysqlOptions.CommandTimeout(60); // Ajuste o tempo limite conforme necessário
//    }));

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)), mysqlOptions =>
//    {
//        mysqlOptions.EnableRetryOnFailure();
//        mysqlOptions.CommandTimeout(60); // Ajuste o tempo limite conforme necessário
//    }));

//// Register the services classe principal que faz DISTRIBUICAO DE HORARIO
//builder.Services.AddScoped<IParametroDiarioOficialService, ParametroDiarioOficialService>();

//// Register AppState as Scoped
//builder.Services.AddTransient<AppState>();

//// **** Register the services ****
//builder.Services.AddScoped<IParametroDiarioOficialService, ParametroDiarioOficialService>();
//// Registro dos serviços incluindo o EscolaService
//builder.Services.AddScoped<IEscolaService, EscolaService>();
//builder.Services.AddScoped<IUsuarioAcessoService, UsuarioAcessoService>();

//// Registrar o serviço IProfessorService e sua implementação
//builder.Services.AddScoped<IProfessorService, ProfessorService>();

//// Registro do serviço distribuicao do horario professor
//builder.Services.AddScoped<PrincipalService>();

//// Registre o LogService como Scoped
//builder.Services.AddScoped<LogService>();
//builder.Services.AddScoped<IProfessorNewCadastroService, ProfessorNewCadastroService>();
//builder.Services.AddScoped<ConfiguracaoAcaoController>();
//builder.Services.AddScoped<IOpcoesProfessorService, OpcoesProfessorService>(); // Assegure-se de que está correto
//builder.Services.AddScoped<IProfessorService, ProfessorService>();

//builder.Host.ConfigureLogging(logging =>
//{
//    logging.ClearProviders();
//    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
//    logging.AddConsole();
//    logging.AddDebug();
//    // logging.AddFile(builder.Configuration.GetSection("Logging:File"));
//});

//// Lendo a configuração do ambiente
//var environment = builder.Configuration.GetValue<string>("Environment");
//string pdfFilePath = builder.Configuration.GetSection("PdfFilePaths")[environment];

//var app = builder.Build();

//// Inicialize AppState usando escopo manual
//using (var scope = app.Services.CreateScope())
//{
//    var scopedAppState = scope.ServiceProvider.GetRequiredService<AppState>();
//    await scopedAppState.InicializarProfessores(0, "", 0, 0);
//}

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwaggerUI();
//}
//else if (app.Environment.IsProduction())
//{
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "v1.0");
//        options.RoutePrefix = "api/swagger";
//    });
//}

//app.UseSwagger();
//app.UseSwaggerUI();

//// nao usar pois ainda nao temos https
////app.UseHttpsRedirection();

//app.UseAuthorization();

//app.UseRouting();

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//});

//app.Run();










//using API_HorarioProfessor.Controllers;
//using API_HorarioProfessor.Data;
//using API_HorarioProfessor.Models;
//using API_HorarioProfessor.Services;
//using API_HorarioProfessor.Services.CoreServices;
//using API_HorarioProfessor.Services.ServicesCore;

////using API_HorarioProfessor.Services.ServicesCore;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//var builder = WebApplication.CreateBuilder(args);

//// Adicionar suporte para logging
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
//builder.Logging.AddDebug(); // Adiciona suporte para debug logging

//// Add services to the container.
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// Determine the connection string based on the configuration parameter
//var useProduction = builder.Configuration.GetValue<bool>("DatabaseConfig:ProductionConnection");
//var connectionString = builder.Configuration.GetConnectionString(useProduction ? "ProductionConnection" : "DesenvConnection");

//builder.Services.AddDbContext<HorarioProfessorDbContext>(options =>
//    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)), mysqlOptions =>
//    {
//        mysqlOptions.EnableRetryOnFailure();
//        mysqlOptions.CommandTimeout(60); // Ajuste o tempo limite conforme necessário
//    }));

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)), mysqlOptions =>
//    {
//        mysqlOptions.EnableRetryOnFailure();
//        mysqlOptions.CommandTimeout(60); // Ajuste o tempo limite conforme necessário
//    }));


//// Register the services classe principal que faz DISTRIBUICAO DE HORARIO
//builder.Services.AddScoped<IParametroDiarioOficialService, ParametroDiarioOficialService>();

//// Register AppState as a singleton
////builder.Services.AddSingleton<AppState>();
//builder.Services.AddScoped<AppState>();

//// **** Register the services ****
//builder.Services.AddScoped<IParametroDiarioOficialService, ParametroDiarioOficialService>();
//// Registro dos serviços incluindo o EscolaService
//builder.Services.AddScoped<IEscolaService, EscolaService>();
//builder.Services.AddScoped<IUsuarioAcessoService, UsuarioAcessoService>();

//// Registrar o serviço IProfessorService e sua implementação
//builder.Services.AddScoped<IProfessorService, ProfessorService>();

//// Registro do servico distribuicao do horario professor
//builder.Services.AddScoped<PrincipalService>();

//// Registre o LogService como Scoped
//builder.Services.AddScoped<LogService>();
//builder.Services.AddScoped<IProfessorNewCadastroService, ProfessorNewCadastroService>();
//builder.Services.AddScoped<ConfiguracaoAcaoController>();
//builder.Services.AddScoped<IOpcoesProfessorService, OpcoesProfessorService>(); // Assegure-se de que está correto
//builder.Services.AddScoped<IProfessorService, ProfessorService>();




//builder.Host.ConfigureLogging(logging =>
//{
//    logging.ClearProviders();
//    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
//    logging.AddConsole();
//    logging.AddDebug();
//    // logging.AddFile(builder.Configuration.GetSection("Logging:File"));
//});

//// Lendo a configuração do ambiente
//var environment = builder.Configuration.GetValue<string>("Environment");
//string pdfFilePath = builder.Configuration.GetSection("PdfFilePaths")[environment];

//var app = builder.Build();

//// Inicialize AppState
//var appState = app.Services.GetRequiredService<AppState>();
//await appState.InicializarProfessores(0,"",0,0);

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwaggerUI();
//}
//else if (app.Environment.IsProduction())
//{
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "v1.0");
//        options.RoutePrefix = "api/swagger";
//    });
//}

//app.UseSwagger();
//app.UseSwaggerUI();

//// nao usar pois ainda nao temos https
////app.UseHttpsRedirection();

//app.UseAuthorization();

//app.UseRouting();

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//});

//app.Run();







//using API_HorarioProfessor.Data;
//using API_HorarioProfessor.Models;
//using API_HorarioProfessor.Services;
//using API_HorarioProfessor.Services.CoreServices;
//using API_HorarioProfessor.Services.ServicesCore;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//var builder = WebApplication.CreateBuilder(args);

//// Adicionar suporte para logging
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
//builder.Logging.AddDebug(); // Adiciona suporte para debug logging

//// Add services to the container.
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();



//// Determine the connection string based on the configuration parameter
//var useProduction = builder.Configuration.GetValue<bool>("DatabaseConfig:ProductionConnection");
//var connectionString = builder.Configuration.GetConnectionString(useProduction ? "ProductionConnection" : "DesenvConnection");

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)), mysqlOptions =>
//    {
//        mysqlOptions.EnableRetryOnFailure();
//        mysqlOptions.CommandTimeout(60); // Ajuste o tempo limite conforme necessário
//    }));

//// Register the services classe principal que faz DISTRIBUICAO DE HORARIO
//builder.Services.AddScoped<IParametroDiarioOficialService, ParametroDiarioOficialService>();
//// Register AppState as a singleton
//builder.Services.AddSingleton<AppState>();

//// **** Register the services ****
//builder.Services.AddScoped<IParametroDiarioOficialService, ParametroDiarioOficialService>();
//// Registro dos serviços incluindo o EscolaService
//builder.Services.AddScoped<IEscolaService, EscolaService>();
//builder.Services.AddScoped<IUsuarioAcessoService, UsuarioAcessoService>();

//// Registrar o serviço IProfessorService e sua implementação
//builder.Services.AddScoped<IProfessorService, ProfessorService>();

////**** registro do servico distribuicao do horario professor */
//builder.Services.AddSingleton<AppState>();
//builder.Services.AddScoped<PrincipalService>();


//builder.Host.ConfigureLogging(logging =>
//{
//    logging.ClearProviders();
//    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
//    logging.AddConsole();
//    logging.AddDebug();
//     // logging.AddFile(builder.Configuration.GetSection("Logging:File"));
//});

//// Lendo a configuração do ambiente
//var environment = builder.Configuration.GetValue<string>("Environment");
//string pdfFilePath = builder.Configuration.GetSection("PdfFilePaths")[environment];


//var app = builder.Build();


//if (app.Environment.IsDevelopment())
//{
//    app.UseSwaggerUI();
//}
//else if (app.Environment.IsProduction())
//{
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "v1.0");
//        options.RoutePrefix = "api/swagger";
//    });
//}



//app.UseSwagger();
//app.UseSwaggerUI();
////nao usar poise ainda nao temos https
////app.UseHttpsRedirection();


//app.UseAuthorization();

//app.UseRouting();

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//});

//app.Run();
