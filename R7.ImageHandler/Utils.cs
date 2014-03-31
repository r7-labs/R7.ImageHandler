//
// Utils.cs
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
using System.Linq;
using System.Text;
using System.Globalization;
using System.Drawing.Imaging;
using System.Security.Cryptography;

namespace R7.ImageHandler
{
	internal static class Utils
	{
		public static ImageFormat GetImageFormatByExtension (string extension)
		{
			switch (extension.ToLowerInvariant ())
			{
			case "jpg":
			case "jpeg":
			case ".jpg":
			case ".jpeg":
				return ImageFormat.Jpeg;
			case "png":
			case ".png":
				return ImageFormat.Png;
			case "gif":
			case ".gif":
				return ImageFormat.Gif;
			case "bmp":
			case ".bmp":
				return ImageFormat.Bmp;
			default:
				// unknown format
				return null;
			}
		}

		public static string GetImageMimeType (ImageFormat format)
		{
			string mimeType = "image/x-unknown";

			if (format.Equals (ImageFormat.Gif))
			{
				mimeType = "image/gif";
			}
			else if (format.Equals (ImageFormat.Jpeg))
			{
				mimeType = "image/jpeg";
			}
			else if (format.Equals (ImageFormat.Png))
			{
				mimeType = "image/png";
			}
			else if (format.Equals (ImageFormat.Bmp) || format.Equals (ImageFormat.MemoryBmp))
			{
				mimeType = "image/bmp";
			}
			else if (format.Equals (ImageFormat.Tiff))
			{
				mimeType = "image/tiff";
			}
			else if (format.Equals (ImageFormat.Icon))
			{
				mimeType = "image/x-icon";
			}

			return mimeType;
		}

		internal static string GetIDFromBytes (byte[] buffer)
		{
			var result = SHA1.Create ().ComputeHash (buffer);

			var sb = new StringBuilder ();
			for (var i = 0; i < result.Length; i++)
				sb.Append (result [i].ToString ("X2", CultureInfo.InvariantCulture));

			return sb.ToString ();
		}

		/// <summary>
		/// Returns the encoder for the specified mime type
		/// </summary>
		/// <param name="mimeType">The mime type of the content</param>
		/// <returns>ImageCodecInfo</returns>
		public static ImageCodecInfo GetEncoderInfo (String mimeType)
		{
			var encoders = ImageCodecInfo.GetImageEncoders ();
			var encoder = encoders.Where (x => x.MimeType == mimeType).FirstOrDefault ();
			return encoder;
		}
	}
}

