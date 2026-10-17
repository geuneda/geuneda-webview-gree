# Changelog
이 패키지의 모든 주요 변경사항은 이 파일에 기록됩니다.

이 형식은 [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)를 기반으로 하며,
이 프로젝트는 [Semantic Versioning](http://semver.org/spec/v2.0.0.html)을 따릅니다.

## [0.2.0] - 2026-10-17

**Changed**:
- 의존성을 `com.geuneda.webview` 0.2.0 으로 올림. 상호 운용 테스트가 0.2.0 의 ASCII 스크립트(짝 없는 서로게이트·이모지·U+2028 이 코드 단위 그대로 페이지에 감)를 확인합니다. 어댑터 코드는 바뀌지 않았습니다

**Docs**:
- 설치 예시를 gree `?path=/dist/package-nofragment` 로 바꿈. 기본 `dist/package` 는 Android 에 미디어 읽기 권한·FileProvider·androidx core 를 넣어 Google Play 권한 정책에 걸릴 수 있음. 같은 커밋(20f14061)의 nofragment 로 Android 에뮬레이터·iOS 시뮬레이터·macOS 에디터 확인. 파일 입력이 필요하면 기본 package 를 쓰라는 안내
- 시험용 http 페이지는 Android 에서 `UNITYWEBVIEW_ANDROID_USES_CLEARTEXT_TRAFFIC` define 이 필요함(`insecureHttpOption` 만으로는 안 됨)

**Tests**:
- `GreeWebViewInteropTest`: Unity 가 보낸 결과·이벤트의 글(짝 없는 서로게이트·이모지·U+2028·한글)이 코드 단위 그대로 페이지에 가는지 확인(`interop.html` 이 16진수로 돌려줌). webview 0.1.0 에서는 macOS 에디터의 문자열 변환이 `ArgumentException` 을 던져 실패함

## [0.1.0] - 2026-10-17

**New**:
- `GreeWebViewFactory`: gree unity-webview(`net.gree.unity-webview`, 커밋 `20f14061` 으로 확인)로 `IWebViewNative` 를 만듦. 열 때마다 이름이 겹치지 않는 게임 오브젝트(`DontDestroyOnLoad`), 시작 알림(이동 제한을 비껴간 Android POST 이동 감지)을 넘김, 네이티브 준비 전 값은 준비 뒤 이동 제한 → 여백 → 보이기 → 로드 순서로 적용, 여백은 화면 비율, Android 강제 다크 모드 끔, System WebView 가 없으면 로드 오류로 알림, 이동 제한을 걸 수 없는 플랫폼은 열기 실패, 데스크톱에서 화면 크기만 바뀌어도 뷰 크기를 다시 계산
- gree 가 있을 때만 컴파일(versionDefines `net.gree.unity-webview` → `GEUNEDA_WEBVIEW_GREE`)
- `GreeWebViewInteropTest`(PlayMode, 명시 실행): 실제 웹뷰로 열기·특수 문자 왕복·모르는 명령·이벤트·허용 밖 이동 차단·키 없는 iframe 명령 차단
