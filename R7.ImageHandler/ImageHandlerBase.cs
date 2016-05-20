//
// ImageHandlerBase.cs
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
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Security;
using System.Web;
using R7.ImageHandler.Transforms;

namespace R7.ImageHandler
{
	public abstract class ImageHandlerBase : IHttpHandler
	{
		private ImageHandlerInternal Implementation { get; set; }

		public ImageHandlerSettings Settings
		{
			get { return Implementation.Settings; }
		}
		// REVIEW: Realize image-specific settings for ContentType, etc?
		/// <summary>
		/// Sets the type of the result image. The handler will return ouput with MIME type matching this content
		/// </summary>
		public ImageFormat ContentType
		{
			get { return Implementation.ContentType; }
			set { Implementation.ContentType = value; }
		}

		/// <summary>
		/// A list of image transforms that will be applied successively to the image
		/// </summary>
        protected List<MagickTransformBase> ImageTransforms
		{
			get { return Implementation.ImageTransforms; }
		}

		protected ImageHandlerBase ()
            : this (new ImageHandlerInternal ())
		{
		}

		private ImageHandlerBase (ImageHandlerInternal implementation)
		{
			Implementation = implementation;
		}

		#region Abstract methods

		public abstract ImageInfo GenerateImage (NameValueCollection parameters, string imgFile);

		public abstract string GetImageFilename (NameValueCollection parameters);

		#endregion

		#region IHttpHandler implementation

		public virtual bool IsReusable
		{
			get { return false; }
		}

		/// <summary>
		/// Processes the HTTP request.
		/// </summary>
		/// <param name="context">HTTP context.</param>
		public void ProcessRequest (HttpContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException ("Http context cannot be null");
			}

			var process = true;
			if (Settings.EnableSecurity && context.Request.Url.Host != "localhost")
			{
				var allowed = false;
				var allowedDomains = new StringBuilder ();
				foreach (var allowedDomain in Settings.AllowedDomains)
				{
					allowedDomains.Append (allowedDomain);
					allowedDomains.Append (',');

					if (context.Request.Url.Host.ToLowerInvariant ().Contains (allowedDomain.ToLowerInvariant ()))
					{	
						allowed = true;
						break;
					}
				}

				if (!allowed)
				{
					if (context.Request.UrlReferrer == null)
					{
						if (Settings.EnableSecurityExceptions)
							throw new SecurityException (string.Format ("Not allowed to use standalone (only localhost + {0})", allowedDomains));
						else
							process = false;
					}
					else if (context.Request.Url.Host != context.Request.UrlReferrer.Host)
					{
						if (Settings.EnableSecurityExceptions)
							throw new SecurityException (string.Format ("Not allowed to use from {0} (only localhost + {1}): ", context.Request.UrlReferrer.Host, allowedDomains));
						else
							process = false;
					}
				}
			}

			if (process)
			{
				var contextWrapper = new HttpContextWrapper (context);
				ProcessRequest (contextWrapper);
			}
		}

		internal void ProcessRequest (HttpContextBase context)
		{
			Debug.Assert (context != null);

			Implementation.HandleImageRequest (
				context, 
				delegate(NameValueCollection queryString, string imgFile) {
					return GenerateImage (queryString, imgFile);
				},
				delegate(NameValueCollection queryString) {
					return GetImageFilename (queryString);
				},
				this.ToString ()
			);
		}

		#endregion

	} // class
} // namespace
