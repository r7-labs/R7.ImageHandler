//
// IEBrowser.cs
//
// Author:
//       Roman M. Yagodin <roman.yagodin@gmail.com>
//
// Copyright (c) 2014 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace R7.ImageHandler
{
	public class IEBrowser : ApplicationContext
	{
		public Image Thumb; 
		private string _html;
		private UrlRatioMode _ratio;
		AutoResetEvent ResultEvent;

		public IEBrowser(string target,  UrlRatioMode ratio, AutoResetEvent resultEvent)
		{
			ResultEvent = resultEvent;
			Thread thrd = new Thread(new ThreadStart(
			                         	delegate {
			                         	         	Init(target,ratio);
			                         	         	Application.Run(this);
			                         	})); 
			// set thread to STA state before starting
			thrd.SetApartmentState(ApartmentState.STA);
			thrd.Start(); 
		}

		private void Init(string target,UrlRatioMode ratio)
		{
			// create a WebBrowser control
			WebBrowser ieBrowser = new WebBrowser();
			ieBrowser.ScrollBarsEnabled = false;
			ieBrowser.ScriptErrorsSuppressed = true;
        
			// set WebBrowser event handle
			ieBrowser.DocumentCompleted += IEBrowser_DocumentCompleted;

			_ratio = ratio;

			if (target.ToLower().StartsWith("http:"))
			{
				_html = "";
				ieBrowser.Navigate(target);
			}
			else
			{
				ieBrowser.Navigate("about:blank");
				_html = target;
			}
		}

		// DocumentCompleted event handle
		void IEBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			try
			{
				WebBrowser browser = (WebBrowser)sender;
				HtmlDocument doc = browser.Document;
				if (_html != String.Empty)
				{
					doc.OpenNew(true);
					doc.Write(_html);
				}
				switch (_ratio)
				{
					case UrlRatioMode.Full:
						browser.Width = doc.Body.ScrollRectangle.Width;
						browser.Height = doc.Body.ScrollRectangle.Height;
						break;
					case UrlRatioMode.Screen:
						browser.Width = doc.Body.ScrollRectangle.Width;
						browser.Height = Convert.ToInt32(browser.Width / 3 * 2);
						break;
					case UrlRatioMode.Cinema:
						browser.Width = doc.Body.ScrollRectangle.Width;
						browser.Height = Convert.ToInt32(browser.Width / 16 * 9);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
	
				Bitmap bitmap = new Bitmap(browser.Width, browser.Height);
				GetImage(browser.ActiveXInstance, bitmap, Color.White);
				
				browser.Dispose();
				Thumb = (Image) bitmap;
			}
			catch (Exception)
			{
			}
			finally
			{
				ResultEvent.Set();
			}

		}

		[ComImport]
		[Guid("0000010D-0000-0000-C000-000000000046")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface IViewObject
		{
			void Draw([MarshalAs(UnmanagedType.U4)] uint dwAspect, int lindex, IntPtr pvAspect, [In] IntPtr ptd, IntPtr hdcTargetDev, IntPtr hdcDraw, [MarshalAs(UnmanagedType.Struct)] ref RECT lprcBounds, [In] IntPtr lprcWBounds, IntPtr pfnContinue, [MarshalAs(UnmanagedType.U4)] uint dwContinue);
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		public static void GetImage(object obj, Image destination, Color backgroundColor)
		{
			using (Graphics graphics = Graphics.FromImage(destination))
			{
				IntPtr deviceContextHandle = IntPtr.Zero;
				RECT rectangle = new RECT();

				rectangle.Right = destination.Width;
				rectangle.Bottom = destination.Height;

				graphics.Clear(backgroundColor);

				try
				{
					deviceContextHandle = graphics.GetHdc();

					IViewObject viewObject = obj as IViewObject;
					viewObject.Draw(1, -1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, deviceContextHandle, ref rectangle, IntPtr.Zero, IntPtr.Zero, 0);
				}
				finally
				{
					if (deviceContextHandle != IntPtr.Zero)
					{
						graphics.ReleaseHdc(deviceContextHandle);
					}
				}
			}
		}
	}
}