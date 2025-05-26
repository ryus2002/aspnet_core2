# 微服務電商平台日誌系統文檔

本文檔詳細說明了微服務電商平台的日誌系統設計與使用方法，包括基本日誌系統設置、集中式日誌文件配置、結構化日誌格式實現，以及簡單的日誌查看工具。

## 1. 日誌系統概述

我們的日誌系統基於 Serilog 構建，提供了以下功能：

- **統一的日誌格式**：所有微服務使用相同的日誌格式，便於分析和查詢
- **多目標輸出**：同時輸出到控制台、文件和 Seq 服務器（可選）
- **結構化日誌**：使用 JSON 格式存儲日誌，保留完整的上下文信息
- **請求追蹤**：自動記錄 HTTP 請求和響應的詳細信息
- **全局異常處理**：捕獲並記錄未處理的異常
- **日誌查詢 API**：提供 REST API 用於查詢和分析日誌
- **日誌查看工具**：內置簡單的 Web 界面用於查看和分析日誌

## 2. 設置基本日誌系統

### 2.1 配置文件設置

在每個微服務的 `appsettings.json` 文件中添加以下配置：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "MinimumLevel": "Information",
    "LogDirectory": "logs",
    "FilePath": "logs/{ServiceName}/log-.json",
    "SeqServerUrl": "http://seq:5341"
  }
}
```

配置說明：

- `LogLevel`：設置不同命名空間的日誌級別
- `MinimumLevel`：設置最小日誌級別，可選值：Verbose、Debug、Information、Warning、Error、Fatal
- `LogDirectory`：日誌文件存儲的根目錄
- `FilePath`：日誌文件路徑模板，{ServiceName} 會被替換為服務名稱
- `SeqServerUrl`：Seq 日誌服務器的 URL（可選）

### 2.2 在 Program.cs 中註冊日誌系統

在每個微服務的 `Program.cs` 文件中添加以下代碼：

```csharp
// 添加統一日誌系統
builder.Services.AddUnifiedLogging(builder.Configuration, "服務名稱");

// 添加日誌查看工具
builder.Services.AddLogViewer();

// 在中間件管道中添加全局異常處理和請求日誌中間件
app.UseGlobalExceptionHandling(app.Environment.IsDevelopment());
app.UseRequestLogging();

// 啟用日誌查看工具
app.UseLogViewer();
```

### 2.3 引用必要的 NuGet 包

在每個微服務的項目文件中添加對 Shared.Logging 項目的引用：

```xml
<ItemGroup>
  <ProjectReference Include="..\..\shared\Shared.Logging\Shared.Logging.csproj" />
</ItemGroup>
```

## 3. 配置集中式日誌文件

### 3.1 日誌文件存儲結構

日誌文件按照以下結構存儲：

```
logs/
  ├── product-service/
  │     ├── log-20250523.json
  │     └── log-20250524.json
  ├── order-service/
  │     ├── log-20250523.json
  │     └── log-20250524.json
  └── ...
```

每個服務的日誌存儲在獨立的子目錄中，按天滾動生成新文件。

### 3.2 Docker 環境中的日誌持久化

在 Docker 環境中，可以通過以下方式實現日誌的持久化：

1. 在 `docker-compose.yml` 中添加卷映射：

```yaml
services:
  product-service:
    # ...其他配置
    volumes:
      - ./logs/product-service:/app/logs/product-service
```

2. 確保日誌目錄具有適當的權限：

```bash
mkdir -p logs/product-service
chmod -R 777 logs
```

## 4. 實現結構化日誌格式

### 4.1 日誌事件結構

我們使用 JSON 格式存儲日誌事件，每個日誌事件包含以下字段：

```json
{
  "timestamp": "2025-05-23T02:16:48.344Z",
  "level": "Information",
  "messageTemplate": "處理請求 {RequestMethod} {RequestPath}",
  "renderedMessage": "處理請求 GET /api/products",
  "exception": null,
  "properties": {
    "RequestMethod": "GET",
    "RequestPath": "/api/products",
    "RequestId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "ServiceName": "product-service",
    "MachineName": "web-server-01",
    "ProcessId": 1234,
    "ThreadId": 5678
  }
}
```

### 4.2 在代碼中使用結構化日誌

在代碼中使用 ILogger 接口記錄結構化日誌：

```csharp
// 注入 ILogger
private readonly ILogger<ProductController> _logger;

public ProductController(ILogger<ProductController> logger)
{
    _logger = logger;
}

// 記錄簡單消息
_logger.LogInformation("獲取產品列表");

// 記錄結構化數據
_logger.LogInformation("獲取產品 {ProductId}", id);

// 記錄多個屬性
_logger.LogInformation("創建產品 {ProductName} 在分類 {CategoryId} 中，價格為 {Price}", 
    product.Name, product.CategoryId, product.Price);

// 記錄異常
try
{
    // 業務邏輯
}
catch (Exception ex)
{
    _logger.LogError(ex, "處理產品 {ProductId} 時發生錯誤", id);
    throw;
}
```

### 4.3 使用日誌上下文

使用 ILogContextAccessor 在請求生命週期中共享上下文信息：

```csharp
private readonly ILogContextAccessor _logContext;

public ProductService(ILogContextAccessor logContext)
{
    _logContext = logContext;
}

public async Task ProcessProduct(string productId)
{
    // 設置上下文屬性
    _logContext.SetProperty("ProductId", productId);
    
    // 業務邏輯
    
    // 清除上下文屬性
    _logContext.RemoveProperty("ProductId");
}
```

## 5. 建立簡單的日誌查看工具

### 5.1 訪問日誌查看工具

日誌查看工具可通過以下 URL 訪問：

```
http://localhost:5000/logs
```

### 5.2 日誌查看工具功能

日誌查看工具提供以下功能：

- **搜索日誌**：按服務名稱、日誌級別、時間範圍和關鍵字搜索日誌
- **瀏覽日誌文件**：直接瀏覽日誌文件內容
- **統計分析**：查看日誌的統計信息，包括按級別、服務和時間的分佈
- **查看詳情**：查看日誌事件的完整詳情，包括異常堆棧和屬性

### 5.3 日誌 API

日誌查看工具提供以下 API：

- `GET /api/logs/files`：獲取日誌文件列表
- `GET /api/logs/file`：從指定文件讀取日誌事件
- `GET /api/logs/search`：搜索日誌事件
- `GET /api/logs/statistics`：獲取日誌統計信息

## 6. 最佳實踐

### 6.1 日誌級別使用指南

- **Verbose**：詳細的調試信息，僅在深度排查問題時使用
- **Debug**：調試信息，包括變量值和執行流程
- **Information**：應用程序正常運行的信息，如請求處理、業務操作完成
- **Warning**：可能的問題或異常情況，但不影響主要功能
- **Error**：發生錯誤，影響功能但不導致應用崩潰
- **Fatal**：嚴重錯誤，導致應用崩潰或需要立即關注

### 6.2 結構化日誌最佳實踐

- 使用命名參數而非位置參數：`{ProductId}` 而非 `{0}`
- 使用具有描述性的參數名稱：`{ProductName}` 而非 `{p}`
- 不要在消息模板中包含敏感信息
- 不要記錄過大的對象，可能導致性能問題
- 使用適當的日誌級別，避免記錄過多的信息

### 6.3 性能考慮

- 在記錄大量數據前檢查日誌級別是否啟用：
  ```csharp
  if (_logger.IsEnabled(LogLevel.Debug))
  {
      var expensiveData = GenerateExpensiveData();
      _logger.LogDebug("詳細數據: {Data}", expensiveData);
  }
  ```
- 避免在熱路徑中記錄過多日誌
- 定期清理舊的日誌文件
- 考慮使用非同步日誌寫入（Serilog 默認支持）

## 7. 故障排除

### 7.1 常見問題

- **日誌文件未創建**：檢查日誌目錄權限和配置
- **日誌查看工具無法訪問**：確保已註冊 `AddLogViewer()` 和 `UseLogViewer()`
- **日誌級別過濾不生效**：檢查 `MinimumLevel` 配置
- **日誌查詢性能差**：考慮使用 Seq 等專業日誌管理工具

### 7.2 診斷步驟

1. 檢查 `appsettings.json` 中的日誌配置
2. 確認日誌目錄存在且具有寫入權限
3. 檢查服務啟動日誌中是否有日誌相關錯誤
4. 嘗試直接訪問日誌文件查看內容

## 8. 進階主題

### 8.1 集成 Seq 日誌服務器

Seq 是一個專業的日誌管理工具，提供更強大的查詢和分析功能。

1. 在 Docker Compose 中添加 Seq 服務：

```yaml
services:
  seq:
    image: datalust/seq:latest
    ports:
      - "5341:80"
    environment:
      - ACCEPT_EULA=Y
    volumes:
      - seq-data:/data
```

2. 在 `appsettings.json` 中配置 Seq 服務器 URL：

```json
{
  "Logging": {
    "SeqServerUrl": "http://seq:5341"
  }
}
```

### 8.2 自定義日誌豐富器

可以創建自定義的日誌豐富器，添加更多上下文信息：

```csharp
public class CustomEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
            "Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"));
    }
}

// 在 LoggerExtensions.cs 中註冊
loggerConfig.Enrich.With<CustomEnricher>();
```

### 8.3 日誌聚合與分析

對於大型系統，可以考慮使用 ELK (Elasticsearch, Logstash, Kibana) 或 Grafana Loki 等工具進行日誌聚合和分析。

## 9. 總結

本文檔詳細介紹了微服務電商平台的日誌系統設計與使用方法。通過實施本文檔中的建議，您可以建立一個統一、可靠且易於使用的日誌系統，幫助開發團隊更有效地監控和排查問題。

日誌系統是應用程序可觀測性的重要組成部分，與指標監控和分佈式追蹤一起，構成了完整的可觀測性解決方案。未來可以考慮進一步集成 OpenTelemetry 等標準，實現更全面的可觀測性。