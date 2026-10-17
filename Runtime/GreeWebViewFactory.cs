using System;

// ReSharper disable once CheckNamespace

namespace Geuneda.WebView.Gree
{
	/// <summary>
	/// gree unity-webview 로 네이티브 웹뷰를 만듭니다. <c>new WebViewService(new WebViewServiceOptions { Factory = new GreeWebViewFactory(), ... })</c>.
	/// </summary>
	/// <remarks>
	/// 지원: Android, iOS, macOS 에디터·플레이어. 이동 제한(<c>SetURLPattern</c>)을 걸 수 없는 플랫폼(WebGL, Linux 에디터)과
	/// System WebView 가 없는 Android 기기에서는 열기가 <c>WebViewResultCodes.LoadFailed</c> 입니다.
	/// </remarks>
	public sealed class GreeWebViewFactory : IWebViewFactory
	{
		public IWebViewNative Create(WebViewNativeSettings settings, IWebViewNativeListener listener)
		{
			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			if (listener == null)
			{
				throw new ArgumentNullException(nameof(listener));
			}

			return GreeWebView.Create(settings, listener);
		}
	}
}
