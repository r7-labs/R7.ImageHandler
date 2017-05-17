﻿//
// ImageHandlerInternal.cs
//
// Author:
//       Roman M. Yagodin <roman.yagodin@gmail.com>
//
// Copyright (c) 2014-2017
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using DotNetNuke.Entities.Host;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.GeneratedImage.ImageQuantization;

namespace R7.ImageHandler
{
	public class ImageHandlerInternal
	{
		public ImageHandlerSettings Settings { get; set; }

		public ImageFormat ContentType { get; set; }

		private IImageStore ImageStore
		{
			get { return DiskImageStore.Instance; }
		}

		public List<ImageTransformBase> ImageTransforms { get; private set; }

		public ImageHandlerInternal ()
		{
			// read settings
			Settings = ImageHandlerSettings.Instance;

			ContentType = ImageFormat.Jpeg;
			ImageTransforms = new List<ImageTransformBase> ();
		}

        private HttpCacheability GetDnnCacheability (HttpContextBase context)
		{
            if (context.Request.IsAuthenticated)
			{
				switch (Host.AuthenticatedCacheability)
				{
					case "0":
						return HttpCacheability.NoCache;
					case "1":
						return HttpCacheability.Private;
					case "2":
						return HttpCacheability.Public;
					case "3":
						return HttpCacheability.Server;
					case "4":
						return HttpCacheability.ServerAndNoCache;
					case "5":
						return HttpCacheability.ServerAndPrivate;
				}
			}
			return HttpCacheability.Public;
		}

		public void HandleImageRequest (HttpContextBase context, 
		                                Func<NameValueCollection, string, ImageInfo> imageGenCallback, 
		                                Func<NameValueCollection, string> imageFilenameCallback, 
		                                string uniqueIdStringSeed)
		{
			context.Response.Clear ();
			context.Response.ContentType = Utils.GetImageMimeType (ContentType);

			// NOTE: uniqueIdStringSeed = R7.ImageHandler.ToString()
			var cacheId = GetUniqueIDString (context, uniqueIdStringSeed);

			var cachePolicy = context.Response.Cache;
			cachePolicy.SetValidUntilExpires (true);
			
			if (Settings.EnableClientCache)
			{
				if (!string.IsNullOrEmpty (context.Request.Headers ["If-Modified-Since"]) && 
					!string.IsNullOrEmpty (context.Request.Headers ["If-None-Match"]))
				{
                    try {
					    var provider = CultureInfo.InvariantCulture;
                        var lastMod = DateTime.ParseExact (context.Request.Headers ["If-Modified-Since"], "r", provider).ToLocalTime ();
                        var etag = context.Request.Headers ["If-None-Match"];
                        if (lastMod + Settings.ClientCacheExpiration > Settings.Now && etag == cacheId) {
                            // send 304 when cache time is not expired
                            context.Response.StatusCode = 304;
                            context.Response.StatusDescription = "Not Modified";
                            context.Response.End ();
                            return;
                        }
                    }
                    catch (Exception ex) {
                        var logEntry = new LogInfo ();
                        logEntry.Exception = new ExceptionInfo (ex);
                        logEntry.LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString ();
                        EventLogController.Instance.AddLog (logEntry);

                    }
				}

                cachePolicy.SetCacheability (GetDnnCacheability (context));
				cachePolicy.SetLastModified (Settings.Now);

				// REVIEW: Check if DNN has option about client cache expiration?
				cachePolicy.SetExpires (Settings.Now + Settings.ClientCacheExpiration);
				cachePolicy.SetETag (cacheId);
			}

			// get image filename, if any
			var imgFile = imageFilenameCallback (context.Request.QueryString);

			// try get image from server cache
			if (Settings.EnableServerCache)
			{
				if (ImageStore.TryTransmitIfContains (cacheId, context.Response, imgFile))
				{
					context.Response.End ();
					return;
				}
			}

			// invoke generate image callback
			var imageMethodData = imageGenCallback (context.Request.QueryString, imgFile);

			if (imageMethodData == null)
			{
				throw new InvalidOperationException ("The image generation handler cannot return null.");
			}

			if (imageMethodData.HttpStatusCode != null)
			{
				context.Response.StatusCode = (int)imageMethodData.HttpStatusCode;
				context.Response.End ();
				return;
			}

			var imageOutputBuffer = new MemoryStream ();

			Debug.Assert (!(imageMethodData.Image == null && imageMethodData.ImageByteBuffer == null));

			if (imageMethodData.Image != null)
			{
				RenderImage (GetImageThroughTransforms (imageMethodData.Image), imageOutputBuffer);
			}
			else if (imageMethodData.ImageByteBuffer != null)
			{
				RenderImage (GetImageThroughTransforms (imageMethodData.ImageByteBuffer), imageOutputBuffer);
			}

			var buffer = imageOutputBuffer.GetBuffer ();
			context.Response.OutputStream.Write (buffer, 0, buffer.Length);

			// add image to server cache, if enabled
			if (Settings.EnableServerCache)
				ImageStore.Add (cacheId, buffer, imgFile != "");

			context.Response.End ();
		}

		private string GetUniqueIDString (HttpContextBase context, string uniqueIdStringSeed)
		{
			var builder = new StringBuilder ();
			builder.Append (uniqueIdStringSeed);

			foreach (var key in context.Request.QueryString.AllKeys.OrderBy(k => k))
			{
				builder.Append (key);
				builder.Append (context.Request.QueryString.Get (key));
			}

			foreach (var tran in ImageTransforms)
				builder.Append (tran.UniqueString);

			return Utils.GetIDFromBytes (ASCIIEncoding.ASCII.GetBytes (builder.ToString ()));
		}

		private Image GetImageThroughTransforms (Image image)
		{
			var tmpImage = image;

			foreach (var tran in ImageTransforms)
				tmpImage = tran.ProcessImage (tmpImage);

			return tmpImage;
		}

		private Image GetImageThroughTransforms (byte[] buffer)
		{
			var memoryStream = new MemoryStream (buffer);
			return GetImageThroughTransforms (Image.FromStream (memoryStream));
		}

		private void RenderImage (Image image, Stream outStream)
		{
			if (ContentType == ImageFormat.Gif)
			{
				var quantizer = new OctreeQuantizer (255, 8);
				using (var quantizedBitmap = quantizer.Quantize (image))
				{
					quantizedBitmap.Save (outStream, ImageFormat.Gif);
				}
			}
			else
			{
				var encParams = new EncoderParameters (1);
				encParams.Param [0] = new EncoderParameter (System.Drawing.Imaging.Encoder.Quality, Settings.ImageCompression);
				ImageCodecInfo ici = Utils.GetEncoderInfo (Utils.GetImageMimeType (ContentType));
				image.Save (outStream, ici, encParams);
			}
		}


	}
}
