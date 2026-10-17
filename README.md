# Geuneda WebView gree

`com.geuneda.webview` 의 [gree unity-webview](https://github.com/gree/unity-webview) 어댑터입니다. `GreeWebViewFactory` 를 `WebViewServiceOptions.Factory` 에 넣습니다.

| 기능 | 타입 | 네임스페이스 |
|------|------|-------------|
| 어댑터 | `GreeWebViewFactory` | `Geuneda.WebView.Gree` |

## 설치

gree unity-webview 는 태그가 없는 git 패키지라 이 패키지의 의존성으로 넣지 않습니다. 게임이 커밋을 고정해 함께 넣습니다(아래 커밋으로 확인함). gree 는 uGUI(`com.unity.ugui`)가 필요합니다. gree 가 없으면 이 패키지는 아무것도 컴파일하지 않습니다(versionDefines `net.gree.unity-webview` → `GEUNEDA_WEBVIEW_GREE`).

```json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.geuneda.core": "https://github.com/geuneda/geuneda-core.git",
    "com.geuneda.webview": "https://github.com/geuneda/geuneda-webview.git",
    "com.geuneda.webview.gree": "https://github.com/geuneda/geuneda-webview-gree.git",
    "com.unity.ugui": "2.0.0",
    "net.gree.unity-webview": "https://github.com/gree/unity-webview.git?path=/dist/package-nofragment#20f14061ca4b45e8490c1e89e00684a5d3ed6be0"
  }
}
```

- **`dist/package-nofragment` 를 씁니다.** 기본 `dist/package` 는 Android 에 미디어 읽기 권한(`READ_MEDIA_IMAGES`·`READ_MEDIA_VIDEO`·`READ_MEDIA_AUDIO`), FileProvider, androidx core 를 넣어 Google Play 권한 정책에 걸릴 수 있습니다. nofragment 는 이것들을 넣지 않고, 같은 커밋으로 Android 에뮬레이터(상호 운용·POST·뒤로가기·영역), iOS 18.6 시뮬레이터(열기·글자 왕복·허용 밖 이동 차단·영역), macOS 에디터(상호 운용)에서 기본 package 와 같은 결과로 통과했습니다(2026-10-17).
- 페이지에서 파일 입력(`<input type="file">`, 사진 고르기)이 필요하면 기본 `dist/package` 를 씁니다(그 권한과 FileProvider 가 파일 선택에 쓰임).
- 시험용 http 페이지(`http://127.0.0.1` 등)를 Android 에서 열려면 Scripting Define Symbols 에 `UNITYWEBVIEW_ANDROID_USES_CLEARTEXT_TRAFFIC` 를 넣습니다. Player Settings 의 `insecureHttpOption` 만으로는 gree 웹뷰가 http 를 열지 않습니다. 운영 페이지는 https 를 씁니다.

gree unity-webview 의 라이선스는 zlib 입니다. 플랫폼별 빌드 설정(Android 하드웨어 가속, iOS WebKit 등)은 gree 의 README 와 빌드 후처리를 따릅니다.

## 사용법

```csharp
using Geuneda.WebView;
using Geuneda.WebView.Gree;

var webView = new WebViewService(new WebViewServiceOptions
{
    Factory = new GreeWebViewFactory(),
    AllowedOrigins = new[] { "https://notice.example.com" }
});
```

- 열 때마다 `WebViewObject` 를 가진 게임 오브젝트를 하나 만들고(`DontDestroyOnLoad`, 이름에 GUID), 닫으면 버립니다. 쿠키·캐시는 네이티브 웹뷰가 앱 단위로 유지합니다.
- 네이티브가 준비(`IsInitialized`)되기 전에 받은 값은 기억했다가 이동 제한 → 여백 → 보이기 → 로드 순서로 적용합니다. 준비 전 `EvaluateJavaScript` 는 버립니다(페이지가 없음).
- 여백은 화면 비율(`SetMargins(..., relative: true)`)로 넘겨 Android 렌더 해상도 조정에도 같은 자리에 둡니다.
- Android 강제 다크 모드는 끕니다(`androidForceDarkMode: 1`). 페이지가 `prefers-color-scheme` 으로 다크 모드를 처리합니다.
- Android 기기에 System WebView 가 없으면(`IsWebViewAvailable`) 열기가 `WebViewResultCodes.LoadFailed` 입니다.
- 이동 제한(`SetURLPattern`)을 걸 수 없는 플랫폼(WebGL, Linux 에디터)에서는 허용 출처 경계를 지킬 수 없으므로 열기가 `WebViewResultCodes.LoadFailed` 입니다.

## gree 와 맞춘 것

- **페이지 → Unity**: Android 는 JavaScript 인터페이스 `window.Unity.call`, iOS·macOS 는 `window.webkit.messageHandlers.unityControl` 입니다. gree 는 iOS·macOS 에서 받은 글에 `UnEscapeURL` 을 하므로(`+` 가 공백이 됨) `geuneda-webview.js` 가 그 경로에서는 URL 인코딩해 보냅니다.
- **오류**: gree Android 는 이미지 같은 리소스의 HTTP 오류도 `httpErr` 로 알리고 메인 프레임 연결 실패는 `err` 로 알립니다(그 뒤 `about:blank` 로 이동). 서비스가 브리지 연결로 열기를 판정하므로 리소스 오류로 실패하지 않습니다.
- **시작 알림**: gree 의 시작 알림(`started`)을 넘깁니다. iOS·macOS 는 이동 제한을 통과한 뒤에만 보내고, Android 는 `onPageStarted`(메인 프레임, POST 포함)에서도 보냅니다. Android 는 POST 이동에 이동 제한 콜백을 부르지 않으므로, 서비스가 허용 밖 시작 알림을 받으면 웹뷰를 닫습니다.
- **iframe**: gree 는 메시지를 보낸 프레임을 알려 주지 않습니다(Android 는 모든 프레임에 `window.Unity`, iOS 는 `frameInfo` 를 넘기지 않음). 서비스가 메인 프레임에만 넘기는 키로 가립니다. gree iOS 는 iframe 이동에도 이동 제한을 걸고 시작 알림을 보내므로 서비스는 시작 알림을 쓰지 않고 메인 프레임 로드 끝만 씁니다.
- **데스크톱 크기**: gree macOS·Windows 는 뷰 크기를 화면 크기 - 여백으로 정하고 여백이 같으면 다시 계산하지 않습니다. 화면 크기만 바뀌면 한 번 다른 여백을 넘긴 뒤 원래 여백을 넘겨 다시 계산하게 합니다.

## 테스트

`GreeWebViewInteropTest`(PlayMode, 명시 실행)는 실제 gree 웹뷰로 페이지와 왕복합니다: 열기(키 연결), `+`·`%`·한글·U+2028·따옴표 왕복, 모르는 명령, Unity 가 보낸 결과·이벤트의 짝 없는 서로게이트·이모지·U+2028(코드 단위 그대로 오는지, webview 0.2.0 의 ASCII 스크립트), 이벤트, 허용 밖 이동 차단, 키를 모르는 sandbox iframe 의 명령 차단. 2026-10-17 macOS 에디터(gree `dist/package-nofragment` 20f14061)에서 통과했고, webview 0.1.0 에서는 짝 없는 서로게이트 결과가 `ArgumentException`(Illegal byte sequence)으로 사라져 실패합니다.

```bash
# com.geuneda.webview 패키지 루트에서
python3 -m http.server 5700 --bind 127.0.0.1 --directory "Web~"
# Unity(macOS 에디터, 그래픽 장치 필요): GEUNEDA_WEBVIEW_INTEROP_URL=http://127.0.0.1:5700/tests/interop.html 로 이 테스트를 이름으로 골라 실행
```
