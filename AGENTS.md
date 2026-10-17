# Geuneda.WebView.Gree - AI 에이전트 가이드

> **관련 파일**: `CLAUDE.md`는 이 파일을 감쌉니다 — `CLAUDE.md`가 아니라 `AGENTS.md`를 편집하세요. `README.md`는 사용자 대상 문서입니다.

## 1. 패키지 개요
- **패키지**: `com.geuneda.webview.gree`
- **Unity**: 2021.3+
- **의존성**: `com.geuneda.webview`. gree unity-webview(`net.gree.unity-webview`, 어셈블리 `unity-webview`, 네임스페이스 `Gree.UnityWebView`)는 태그가 없는 git 패키지라 의존성으로 넣지 않고 versionDefines 로 켭니다(ads.max 와 같은 방식). gree 는 uGUI 가 필요합니다
- **역할**: `IWebViewFactory`/`IWebViewNative` 를 gree `WebViewObject` 로 구현합니다. 판정·보안 규칙은 `com.geuneda.webview` 에 있고 여기에는 넣지 않습니다(D3)
- **확인한 gree 커밋**: `20f14061ca4b45e8490c1e89e00684a5d3ed6be0`(2026-10-15)

## 2. 구조

| 파일 | 역할 |
|------|------|
| `Runtime/GreeWebViewFactory.cs` | 공개 어댑터 |
| `Runtime/GreeWebView.cs` | internal `MonoBehaviour` + `IWebViewNative`. 준비 대기, 콜백 넘기기 |
| `Tests/PlayMode/GreeWebViewInteropTest.cs` | 실제 웹뷰 상호 운용(명시 실행) |

## 3. 규칙

- **준비 전 호출**: gree 는 `Init` 뒤 `IsInitialized` 가 될 때까지 기다려야 합니다(샘플이 그렇게 함). 준비 전 값은 기억했다가 `Update` 에서 이동 제한(`SetURLPattern("", deny, "")`) → 여백 → 보이기 → 로드 순서로 적용합니다. 이동 제한은 첫 로드 전이어야 하고, 걸 수 없으면(`false`) 로드하지 않고 `OnLoadError` 로 열기를 실패시킵니다(허용 출처 경계를 지킬 수 없음, 리뷰 1회차).
- **데스크톱 크기**: gree macOS·Windows `SetMargins` 는 여백 값이 같으면 크기 계산 전에 돌아갑니다. 화면 크기만 바뀐 경우 왼쪽 여백 +1 을 한 번 넘긴 뒤 원래 값을 넘깁니다(`#if` 데스크톱만). Android·iOS 는 네이티브 레이아웃(마진 유지, autoresizing)이 크기를 따라가므로 하지 않습니다.
- **게임 오브젝트 이름**: gree 는 이름으로 네이티브 알림을 보냅니다. 버린 오브젝트는 프레임 끝까지 남으므로 이름에 GUID 를 넣습니다.
- **여백은 비율**(`relative: true`): gree Android 의 픽셀 여백은 Unity 렌더 해상도와 기기 해상도가 다르면 어긋납니다. macOS 는 비율 인자를 무시하고 Unity 픽셀을 씁니다.
- **hook 이 아니라 deny**: hook 은 iOS 에서 iframe 이동에도 걸려 페이지를 열 때 외부 브라우저가 뜰 수 있습니다. 다른 출처 링크는 JS 파일이 `openUrl` 로 엽니다.
- **메시지 디코딩**: gree 의 `CallFromJS` 는 Android 가 아니면 `UnEscapeURL` 을 합니다. JS 파일이 그 경로(`webkit.messageHandlers`)에서 URL 인코딩해 보내므로 어댑터는 받은 글을 그대로 넘깁니다. gree 를 올릴 때 이 동작이 바뀌었는지 상호 운용 테스트로 확인합니다.
- **시작 알림을 넘김**: `started` 를 `OnPageStarted` 로 넘깁니다. gree Android 의 이동 제한은 `shouldOverrideUrlLoading` 에만 있어 POST 이동은 막지 못하고, 그 이동은 `onPageStarted` 로만 알 수 있습니다(서비스가 닫음, 리뷰 2회차).
- **버린 뒤 콜백 무시**: `Dispose` 뒤에 온 gree 콜백은 넘기지 않습니다(서비스도 세션으로 거름).
- **static 가변 상태 금지**(D2). 이 패키지에는 static 필드가 없습니다.

## 4. 테스트

- PlayMode `Geuneda.WebView.Gree.Tests.PlayMode`(gree 가 있을 때만 컴파일). `GreeWebViewInteropTest` 는 명시 실행이고 macOS 에디터의 그래픽 장치가 필요합니다(`-nographics` 불가). `com.geuneda.webview` 의 `Web~` 를 정적 서버로 띄우고 `GEUNEDA_WEBVIEW_INTEROP_URL=http://127.0.0.1:5700/tests/interop.html`.
- 호스트 레포에는 gree 가 없어 이 패키지가 컴파일되지 않습니다. gree 를 넣은 별도 프로젝트에서 확인합니다.

## 5. 문서

- 동작을 바꾸면 `README.md`, `CHANGELOG.md`, `package.json` 버전, 호스트 레포의 `claude-skills/geuneda-webview/` 를 함께 고칩니다.
- gree 커밋을 올리면 README·이 파일의 확인한 커밋과 상호 운용 테스트 결과를 함께 갱신합니다.
