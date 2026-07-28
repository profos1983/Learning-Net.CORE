var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Проверяем, в какой конфигурации запущена программа
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

var env = app.Environment;



app.MapGet("/", () => "Hello World!");
app.MapGet("/about", () => $"Окружение: {env.EnvironmentName},\nНазвание приложения: {env.ApplicationName}");

app.MapGet("/error", () =>
{
    throw new Exception("Тестовая ошибка!");
});

app.Run();
