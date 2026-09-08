# WPF.Lib.Api

`WPF.Lib.Api`는 WPF에 종속되지 않는 .NET 8용 JSON HTTP 클라이언트입니다. GET, POST, PUT, DELETE 요청과 DI 등록, 오류 상태 코드 및 기본 로깅을 공통으로 처리합니다.

실행 프로젝트에서 참조한 뒤 API 주소와 제한 시간을 등록합니다.

```csharp
services.AddApiClient(options =>
{
    options.BaseAddress = new Uri("https://api.example.com/");
    options.Timeout = TimeSpan.FromSeconds(30);
    options.MaximumResponseContentBytes = 10 * 1024 * 1024;
});
```

`MaximumResponseContentBytes`의 기본값은 10 MiB입니다. `Content-Length`와 실제 읽은 바이트를 모두 검사하며, 상한을 넘으면 `ApiResponseTooLargeException`이 발생합니다.

제품별 DTO와 API 서비스는 이 프로젝트가 아니라 해당 제품의 Application 또는 Infrastructure 프로젝트에 둡니다. 기능별 서비스는 `IApiClient`를 생성자로 주입받아 사용합니다.

```csharp
public sealed class CustomerApi(IApiClient apiClient)
{
    public Task<Customer?> GetAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return apiClient.GetAsync<Customer>(
            $"customers/{id}",
            cancellationToken);
    }
}
```

## 요청 작성 기준

- 호출 경로에는 절대 URI가 아니라 `BaseAddress` 기준의 상대 경로만 전달합니다.
- 외부 입력을 경로에 넣을 때는 경로 세그먼트나 쿼리 값을 적절히 이스케이프하고, 허용할 호스트와 경로를 제품 계층에서 검증합니다.
- 토큰, 비밀번호, 개인 정보는 쿼리 문자열에 넣지 않습니다. 현재 오류 로그에는 `RequestUri`가 포함되므로 쿼리 값도 기록될 수 있습니다.
- 모든 공개 작업에 호출자의 `CancellationToken`을 전달합니다.
- `HttpClient`나 handler를 직접 장기 보관하지 않고 DI가 구성한 `IApiClient`를 사용합니다.

## 응답 메모리 제한

`ApiClient`는 성공 및 오류 응답을 설정된 상한까지만 메모리 버퍼에 읽습니다. 성공 응답은 제한된 버퍼에서 비동기 JSON 역직렬화를 수행하고, 오류 응답도 같은 제한 안에서만 `ApiException.ResponseContent`에 보관합니다. 제한값은 API가 반환하는 정상 응답 크기에 맞춰 가능한 작게 설정합니다.

## 현재 구현에서 제공하지 않는 보호

- 요청 URI의 절대 URI 여부 검사와 허용 호스트 검증
- 로그에 기록하기 전 쿼리 문자열과 민감 헤더 마스킹

따라서 사용자 입력에서 만들어진 절대 URI를 전달하지 말고, 제품 계층에서 호스트와 경로를 검증해야 합니다. 공통 안전 기준은 [라이브러리 수명·메모리·안전 가이드](../../docs/library-lifecycle-and-safety.md)를 참고합니다.
