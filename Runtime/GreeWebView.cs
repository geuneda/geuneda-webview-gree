using System;
using System.Globalization;
using Gree.UnityWebView;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace Geuneda.WebView.Gree
{
	/// <summary>
	/// gree <c>WebViewObject</c> 하나를 감쌉니다. 네이티브 준비(<c>IsInitialized</c>) 전에 받은 값은 기억했다가 준비되면 이동 제한 → 여백 → 보이기 → 로드 순서로 적용합니다.
	/// </summary>
	internal sealed class GreeWebView : MonoBehaviour, IWebViewNative
	{
		private WebViewObject _webView;
		private IWebViewNativeListener _listener;
		private string _denyPattern;
		private bool _available;
		private bool _unavailableReported;
		private bool _initialized;
		private bool _disposed;
		private string _pendingUrl;
		private bool _hasMargins;
		private WebViewMargins _margins;
		private bool _hasApplied;
		private WebViewMargins _appliedMargins;
		private Vector2Int _appliedScreen;
		private bool _visible;

		public bool CanGoBack => _initialized && _webView.CanGoBack();

		public static GreeWebView Create(WebViewNativeSettings settings, IWebViewNativeListener listener)
		{
			// gree 는 게임 오브젝트 이름으로 네이티브 알림을 보내므로 이름이 겹치면 안 된다(버린 웹뷰는 프레임 끝까지 남음)
			var gameObject = new GameObject("GeunedaWebView-" + Guid.NewGuid().ToString("N"));

			if (Application.isPlaying)
			{
				DontDestroyOnLoad(gameObject);
			}

			var view = gameObject.AddComponent<GreeWebView>();
			view._listener = listener;
			view._denyPattern = settings.DenyUrlPattern;
			view._available = WebViewObject.IsWebViewAvailable();

			if (view._available)
			{
				view._webView = gameObject.AddComponent<WebViewObject>();
				view._webView.Init(
					cb: view.OnMessage,
					err: view.OnError,
					httpErr: view.OnHttpError,
					ld: view.OnLoaded,
					started: view.OnStarted,
					androidForceDarkMode: 1);
			}

			return view;
		}

		public void LoadUrl(string url)
		{
			if (_initialized)
			{
				_webView.LoadURL(url);
			}
			else
			{
				_pendingUrl = url;
			}
		}

		public void EvaluateJavaScript(string script)
		{
			if (_initialized)
			{
				_webView.EvaluateJS(script);
			}
		}

		public void SetMargins(WebViewMargins margins)
		{
			_margins = margins;
			_hasMargins = true;

			if (_initialized)
			{
				ApplyMargins();
			}
		}

		public void SetVisible(bool visible)
		{
			_visible = visible;

			if (_initialized)
			{
				_webView.SetVisibility(visible);
			}
		}

		public void GoBack()
		{
			if (_initialized)
			{
				_webView.GoBack();
			}
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (Application.isPlaying)
			{
				Destroy(gameObject);
			}
			else
			{
				DestroyImmediate(gameObject);
			}
		}

		private void Update()
		{
			if (_disposed || _initialized)
			{
				return;
			}

			if (!_available)
			{
				// 한 번만 알린다(서비스가 열기를 실패로 끝내고 이 웹뷰를 버림)
				if (!_unavailableReported)
				{
					_unavailableReported = true;
					_listener.OnLoadError("Android System WebView is not available on this device");
				}

				return;
			}

			if (!_webView.IsInitialized())
			{
				return;
			}

			_initialized = true;

			// 첫 로드 전에 건다. 걸 수 없으면(WebGL, Linux 에디터) 허용 출처 경계를 지킬 수 없으므로 열지 않는다
			if (!_webView.SetURLPattern("", _denyPattern, ""))
			{
				_listener.OnLoadError("This platform cannot block navigation off the allowed origins");
				return;
			}

			if (_hasMargins)
			{
				ApplyMargins();
			}

			_webView.SetVisibility(_visible);

			if (_pendingUrl != null)
			{
				_webView.LoadURL(_pendingUrl);
				_pendingUrl = null;
			}
		}

		private void ApplyMargins()
		{
			var screen = new Vector2Int(Screen.width, Screen.height);
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			// gree 데스크톱은 뷰 크기를 '화면 크기 - 여백'으로 정하고, 여백이 같으면 다시 계산하지 않는다. 화면 크기만 바뀌었으면 한 번 다른 값으로 넘겨 다시 계산하게 한다
			// (Android·iOS 는 네이티브 레이아웃이 여백을 유지한 채 크기를 따라감)
			if (_hasApplied && _appliedMargins == _margins && _appliedScreen != screen)
			{
				_webView.SetMargins(_margins.Left + 1, _margins.Top, _margins.Right, _margins.Bottom, true);
			}
#endif
			// relative: 화면 비율로 넘겨, Unity 렌더 해상도가 기기 해상도와 달라도(Android 해상도 조정) 같은 자리에 둔다
			_webView.SetMargins(_margins.Left, _margins.Top, _margins.Right, _margins.Bottom, true);
			_appliedMargins = _margins;
			_appliedScreen = screen;
			_hasApplied = true;
		}

		private void OnMessage(string message)
		{
			if (!_disposed)
			{
				_listener.OnMessage(message);
			}
		}

		private void OnStarted(string url)
		{
			if (!_disposed)
			{
				_listener.OnPageStarted(url);
			}
		}

		private void OnLoaded(string url)
		{
			if (!_disposed)
			{
				_listener.OnPageLoaded(url);
			}
		}

		private void OnError(string error)
		{
			if (!_disposed)
			{
				_listener.OnLoadError(error);
			}
		}

		private void OnHttpError(string status)
		{
			if (!_disposed)
			{
				_listener.OnHttpError(int.TryParse(status, NumberStyles.Integer, CultureInfo.InvariantCulture, out var code) ? code : 0);
			}
		}
	}
}
