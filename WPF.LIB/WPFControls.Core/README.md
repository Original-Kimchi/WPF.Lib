# WPFControls.Core

WPF 애플리케이션에서 공통으로 사용할 UI 독립 계약과 모델을 제공합니다.

- `Abstractions`: 화면 이동, 대화상자, 파일 선택, 테마 서비스 계약
- `Models`: 서비스 계약에서 사용하는 UI 독립 데이터 형식

이 프로젝트는 WPF 구체 타입을 참조하지 않습니다. 실제 WPF 구현과 DI 등록은 실행 프로젝트 또는 별도의 Infrastructure 프로젝트에서 담당합니다.
