using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Geuneda.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace

namespace Geuneda.WebView.Gree.Tests
{
	/// <summary>
	/// 실제 gree 웹뷰로 페이지와 왕복합니다(키를 모르는 sandbox iframe 의 명령은 막힘). com.geuneda.webview 의 <c>Web~</c> 폴더를 정적 서버로 띄우고(<c>python3 -m http.server 5700 --bind 127.0.0.1 --directory "Web~"</c>)
	/// <c>GEUNEDA_WEBVIEW_INTEROP_URL=http://127.0.0.1:5700/tests/interop.html</c> 로 이 테스트를 이름으로 골라 실행합니다(macOS 에디터, 그래픽 장치 필요).
	/// </summary>
	[TestFixture]
	public class GreeWebViewInteropTest
	{
		private const string Text = "A+B 50% 한글 \u2028 \"quote\"";
		private static readonly TimeSpan Wait = TimeSpan.FromSeconds(20);

		[UnityTest]
		[Explicit("Needs a static server for Web~ and GEUNEDA_WEBVIEW_INTEROP_URL")]
		public IEnumerator Page_RoundTripsCommandsAndEvents()
		{
			return UniTask.ToCoroutine(async () =>
			{
				var url = Environment.GetEnvironmentVariable("GEUNEDA_WEBVIEW_INTEROP_URL");
				Assert.IsNotEmpty(url, "Set GEUNEDA_WEBVIEW_INTEROP_URL");
				var origin = new Uri(url).GetLeftPart(UriPartial.Authority);

				var service = new WebViewService(new WebViewServiceOptions
				{
					Factory = new GreeWebViewFactory(),
					AllowedOrigins = new[] { origin },
					LoadTimeout = Wait
				});

				var done = new UniTaskCompletionSource<string>();
				var unknown = new UniTaskCompletionSource<int>();
				var pong = new UniTaskCompletionSource<int>();
				var stayed = new UniTaskCompletionSource<string>();
				var iframeCalls = 0;
				service.AddCommand("test.echo", (command, token) => Answer(command, "text"));
				service.AddCommand("test.done", (command, token) => Complete(done, command, "text"));
				service.AddCommand("test.unknown", (command, token) => Complete(unknown, command, "code"));
				service.AddCommand("test.pong", (command, token) => Complete(pong, command, "n"));
				service.AddCommand("test.stayed", (command, token) => Complete(stayed, command, "url"));
				service.AddCommand("test.iframe", (command, token) =>
				{
					iframeCalls++;
					return UniTask.FromResult(Result<object>.Ok(null));
				});

				try
				{
					var open = await service.OpenAsync(url);
					Assert.IsTrue(open.IsSuccess, open.ToString());

					Assert.AreEqual(Text, await done.Task.Timeout(Wait), "text survives the trip through the page");
					Assert.AreEqual(WebViewResultCodes.UnknownCommand, await unknown.Task.Timeout(Wait));

					service.Emit("test.ping", new JObject { ["n"] = 7 });
					Assert.AreEqual(7, await pong.Task.Timeout(Wait));

					service.Emit("test.leave", new JObject { ["url"] = "https://example.org/" });
					Assert.AreEqual(url, await stayed.Task.Timeout(Wait), "navigation off the allowed origins is blocked");
					Assert.AreEqual(url, service.PageUrl);
					Assert.AreEqual(0, iframeCalls, "a frame without the session key cannot run commands");
				}
				finally
				{
					service.Dispose();
				}
			});
		}

		private static UniTask<Result<object>> Answer(WebViewCommand command, string name)
		{
			return UniTask.FromResult(Result<object>.Ok(new JObject { [name] = command.Args[name] }));
		}

		private static UniTask<Result<object>> Complete<T>(UniTaskCompletionSource<T> source, WebViewCommand command, string name)
		{
			source.TrySetResult(command.Args[name].ToObject<T>());
			return UniTask.FromResult(Result<object>.Ok(null));
		}
	}
}
