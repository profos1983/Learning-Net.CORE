var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(configure =>
{
    configure.AddConsole();
    configure.AddDebug();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// 2. НАШ ЛОГГЕР — как можно выше в конвейере!
app.UseRequestLogging();

var env = app.Environment;

// Логирование всех запросов
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("New request to http://{Host}{Path}",
        context.Request.Host.Value,
        context.Request.Path);
    await next();
});

app.Map("/", async (context) =>
{
    // 1. Явно указываем кодировку UTF-8 для ответа
    context.Response.ContentType = "text/plain; charset=utf-8";

    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Map / сработал!");

    Console.WriteLine($"Launching project from: {env.ContentRootPath}");

    // 2. Отправляем текст
    await context.Response.WriteAsync(
        "Hello World!\n" +
        $"Наименование приложения: {env.ApplicationName}. Конфигурация: {env.EnvironmentName}"
    );
});

// 2. Правильный обработчик для всех остальных путей (404)
app.MapFallback(async (context) =>
{
    context.Response.StatusCode = 404;
    await context.Response.WriteAsync("Page not found");
});

app.Run();

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddLogging(configure =>
//{
//    configure.AddConsole();
//    configure.AddDebug();
//});

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}

//var env = app.Environment;

//// Включаем маршрутизацию – ЭТО ОБЯЗАТЕЛЬНО!
//app.UseRouting();

//// Логирование
//app.Use(async (context, next) =>
//{
//    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//    logger.LogInformation("New request to http://{Host}{Path} ",
//        context.Request.Host.Value,
//        context.Request.Path);
//    await next();
//});

//app.Map("/", async (context) =>
//{
//    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//    logger.LogInformation("Map / сработал!");   // <-- добавьте это
//    await context.Response.WriteAsync(
//        "Hello World!" +
//        $"\nНаименование приложения: {env.ApplicationName}. Приложение запущено в конфигурации: {env.EnvironmentName}"
//    );
//});

//// Обработчик для всех остальных путей (404)
//app.Run(async (context) =>
//{
//    context.Response.StatusCode = 404;
//    await context.Response.WriteAsync("Page not found");
//});

//app.Run();
