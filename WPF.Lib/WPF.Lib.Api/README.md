# WPF.Lib.Api

`WPF.Lib.Api`는 WPF에 종속되지 않는 .NET 8용 JSON HTTP 클라이언트입니다. GET, POST, PUT, DELETE 요청과 DI 등록, 오류 상태 코드 및 기본 로깅을 공통으로 처리합니다.

실행 프로젝트에서 참조한 뒤 API 주소와 제한 시간을 등록합니다.

```csharp
services.AddApiClient(options =>
{
    options.BaseAddress = new Uri("https://api.example.com/");
    options.Timeout = TimeSpan.FromSeconds(30);
});
```

제품별 DTO와 API 서비스는 이 프로젝트가 아니라 해당 제품의 Application 또는 Infrastructure 프로젝트에 둡니다. 기능별 서비스는 `IApiClient`를 생성자로 주입받아 사용합니다.
