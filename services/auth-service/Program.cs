// 註冊權限服務
builder.Services.AddScoped<IPermissionService, PermissionService>();

// 在管道中添加授權中間件
app.UseMiddleware<AuthorizationMiddleware>();