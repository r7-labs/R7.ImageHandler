//
// R7.ImageHandler.cs
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
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Security;
using System.Threading;
using System.Web;
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.FileSystem;

namespace R7.ImageHandler
{
	public class ImageHandler : ImageHandlerBase
	{
		private string defaultImageFile = "";

		private Image EmptyImage
		{
			get
			{
				var emptyBmp = new Bitmap (1, 1, PixelFormat.Format1bppIndexed);
				emptyBmp.MakeTransparent ();
				ContentType = ImageFormat.Png;

				if (!string.IsNullOrEmpty (defaultImageFile))
				{
					var fi = new System.IO.FileInfo (defaultImageFile);

					ContentType = Utils.GetImageFormatByExtension (fi.Extension);
					ContentType = (ContentType != null) ? ContentType : ImageFormat.Png;
				
					/* string format = fi.Extension;
					switch (format)
					{
					case "jpg":
					case "jpeg":
						ContentType = ImageFormat.Jpeg;
						break;
					case "bmp":
						ContentType = ImageFormat.Bmp;
						break;
					case "gif":
						ContentType = ImageFormat.Gif;
						break;
					case "png":
						ContentType = ImageFormat.Png;
						break;
					}
					*/

					if (File.Exists (defaultImageFile))
					{
						emptyBmp = new Bitmap (Image.FromFile (defaultImageFile, true));
					}
					else
					{
						defaultImageFile = Path.GetFullPath (HttpContext.Current.Request.PhysicalApplicationPath + defaultImageFile);
						if (File.Exists (defaultImageFile))
						{
							emptyBmp = new Bitmap (Image.FromFile (defaultImageFile, true));
						}
					}
				}
				return emptyBmp;
			}
		}

		public ImageHandler ()
		{
		}

		public override string GetImageFilename(NameValueCollection parameters)
		{
			var imgFile = string.Empty;

			// DNN FileId & Fileticket
			if (!string.IsNullOrEmpty (parameters ["fileid"]) ||
			    !string.IsNullOrEmpty (parameters ["fileticket"]))
			{
				// TODO: Check current user permissions to view this file

				var fileId = Null.NullInteger;

				if (!string.IsNullOrEmpty (parameters ["fileid"]))
				{
					fileId = int.Parse (parameters ["fileid"]);
				}
				else if (!string.IsNullOrEmpty (parameters ["fileticket"]))
				{
					// get fileId from fileticket value
					fileId = DotNetNuke.Services.FileSystem.FileLinkClickController.
						Instance.GetFileIdFromLinkClick (parameters);
				}

				// check if we really have such file in a DB
				var fileInfo = DotNetNuke.Services.FileSystem.FileManager.Instance.GetFile (fileId);
				if (fileInfo != null)
				{
					// check if file exists
					if (File.Exists (fileInfo.PhysicalPath))
						imgFile = fileInfo.PhysicalPath;
				}
			}
			else if (!string.IsNullOrEmpty (parameters ["file"]))
			{
				imgFile = parameters ["file"].Trim ();

				if (!File.Exists (imgFile))
				{
					imgFile = Path.GetFullPath (HttpContext.Current.Request.PhysicalApplicationPath + imgFile);
					if (!File.Exists (imgFile))
						return string.Empty;
				}
			}
			else if (!string.IsNullOrEmpty (parameters ["path"]))
			{
				var imgIndex = Convert.ToInt32 (parameters ["index"]);
				var imgPath = parameters ["path"];

				if (!Directory.Exists (imgPath))
				{
					imgPath = Path.GetFullPath (HttpContext.Current.Request.PhysicalApplicationPath + imgPath);
					if (!Directory.Exists (imgPath))
						return string.Empty;
				}

				var files = Directory.GetFiles (imgPath, "*");
				if (files.Length > 0 && files.Length - 1 >= imgIndex)
				{
					Array.Sort (files);
					imgFile = files [imgIndex];
					if (!File.Exists (imgFile))
						return string.Empty;
				}
			}

			return imgFile;
		}

		public override ImageInfo GenerateImage (NameValueCollection parameters, string imgFile)
		{
			// Add image generation logic here and return an instance of ImageInfo
			var bgColor = Color.White; 
			ContentType = ImageFormat.Jpeg;

			try
			{
				// Do we override caching for this image ?
				if (!string.IsNullOrEmpty (parameters ["nocache"]))
				{
					Settings.EnableClientCache = false;
					Settings.EnableServerCache = false;
				}

				// override cache time for single image
				if (!string.IsNullOrEmpty (parameters ["cachetime"]))
				{
					Settings.SetCacheTime(new TimeSpan(0, 0, Convert.ToInt32 (parameters ["cachetime"])));
				}

				// Do we have a default image file ?
				if (!string.IsNullOrEmpty (parameters ["defaultimage"]))
				{
					defaultImageFile = parameters ["defaultimage"];
				}

				/*
				// Lets determine the 3 types of Image Source
				// TODO: Move this code to function GetImageFilename(NameValueCollection parameters)
				if (!string.IsNullOrEmpty (parameters ["file"]))
				{
					imgFile = parameters ["file"].Trim ();

					if (!File.Exists (imgFile))
					{
						imgFile = Path.GetFullPath (HttpContext.Current.Request.PhysicalApplicationPath + imgFile);
						if (!File.Exists (imgFile))
							return new ImageInfo (EmptyImage);
					}
				}
				// REVIEW: Remove path+index option?
				else if (!string.IsNullOrEmpty (parameters ["path"]))
				{
					imgIndex = Convert.ToInt32 (parameters ["index"]);
					imgPath = parameters ["path"];

					if (!Directory.Exists (imgPath))
					{
						imgPath = Path.GetFullPath (HttpContext.Current.Request.PhysicalApplicationPath + imgPath);
						if (!Directory.Exists (imgPath))
							return new ImageInfo (EmptyImage);
					}

					var Files = Directory.GetFiles (imgPath, "*");
					if (Files.Length > 0 && Files.Length - 1 >= imgIndex)
					{
						Array.Sort (Files);
						imgFile = Files [imgIndex];
						if (File.Exists (imgFile) != true)
							return new ImageInfo (EmptyImage);
					}
				}
				else if (string.IsNullOrEmpty (parameters ["url"]) &&
				         string.IsNullOrEmpty (parameters ["imageurl"]) &&
				         string.IsNullOrEmpty (parameters ["db"]) &&
				         string.IsNullOrEmpty (parameters ["fileid"]) &&
				         string.IsNullOrEmpty (parameters ["fileticket"]) &&
				         string.IsNullOrEmpty (parameters ["dnn"]) &&
				         string.IsNullOrEmpty (parameters ["percentage"]) &&
				         string.IsNullOrEmpty (parameters ["placeholder"]) &&
				         string.IsNullOrEmpty (parameters ["barcode"]) &&
				         string.IsNullOrEmpty (parameters ["schedule"]))
				{
					return new ImageInfo (EmptyImage);
				}
				*/

				if (imgFile == string.Empty &&
					string.IsNullOrEmpty (parameters ["url"]) &&
					string.IsNullOrEmpty (parameters ["imageurl"]) &&
					string.IsNullOrEmpty (parameters ["db"]) &&
					string.IsNullOrEmpty (parameters ["dnn"]) &&
					string.IsNullOrEmpty (parameters ["percentage"]) &&
					string.IsNullOrEmpty (parameters ["placeholder"]) &&
					string.IsNullOrEmpty (parameters ["barcode"]) &&
					string.IsNullOrEmpty (parameters ["schedule"]))
				{
					return new ImageInfo (EmptyImage);
				}

				// We need to determine the output format		
				if (!string.IsNullOrEmpty (parameters ["format"]))
				{
					ContentType = Utils.GetImageFormatByExtension(parameters ["format"]);
					if (ContentType == null) return new ImageInfo(EmptyImage);
				}
				else if (imgFile != string.Empty)
				{
					var fi = new System.IO.FileInfo (imgFile);

					ContentType = Utils.GetImageFormatByExtension(fi.Extension);
					if (ContentType == null) return new ImageInfo(EmptyImage);
				}

				// determine background color
				if (!string.IsNullOrEmpty (parameters ["bgcolor"]))
				{
					var color = parameters ["bgcolor"];
					bgColor = color.StartsWith ("#") ? ColorTranslator.FromHtml (color) : Color.FromName (color);
				}
			}
			catch (SecurityException)
			{
				if (Settings.EnableSecurityExceptions)
					throw;
			}
			catch (Exception)
			{
				return new ImageInfo (EmptyImage);
			}

			// Db Transform
			if (!string.IsNullOrEmpty (parameters ["db"]))
			{
				// First let us check if the Db value is a key or a connectionstring name

				var settings = ConfigurationManager.AppSettings [parameters ["db"]];
				string connectionName = "", table = "", imageField = "", idField = "";
				if (!string.IsNullOrEmpty (settings))
				{
					var values = settings.Split (new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var value in values)
					{
						var setting = value.Split (new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
						var name = setting [0].ToLowerInvariant ();
						switch (name)
						{
						case "connectionstring":
							connectionName = setting [1];
							break;
						case "table":
							table = setting [1];
							break;
						case "imagefield":
							imageField = setting [1];
							break;
						case "idfield":
							idField = setting [1];
							break;
						default:
							break;
						}
					}
				}
			    
				var dbTrans = new ImageDbTransform ();

				dbTrans.InterpolationMode = Settings.InterpolationMode;
				dbTrans.PixelOffsetMode = Settings.PixelOffsetMode;
				dbTrans.SmoothingMode = Settings.SmoothingMode;
				dbTrans.CompositingQuality = Settings.CompositingQuality;

					/*
				dbTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				dbTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				dbTrans.SmoothingMode = SmoothingMode.HighQuality;
				dbTrans.CompositingQuality = CompositingQuality.HighQuality;*/


				if (connectionName == string.Empty || table == string.Empty || imageField == string.Empty || idField == string.Empty)
				{
					connectionName = parameters ["db"];
					table = parameters ["table"];
					imageField = parameters ["imagefield"];
					idField = parameters ["idfield"];
				}

				var conn = ConfigurationManager.ConnectionStrings [connectionName];

				if (conn == null || string.IsNullOrEmpty (table) || string.IsNullOrEmpty (idField) ||
				    string.IsNullOrEmpty (parameters ["IdValue"]) || string.IsNullOrEmpty (imageField))
				{
					return new ImageInfo (EmptyImage);
				}
				
				dbTrans.ConnectionString = conn.ConnectionString;
				dbTrans.Table = table;
				dbTrans.IdFieldName = idField;
				dbTrans.IdFieldValue = Convert.ToInt32 (parameters ["idvalue"]);
				dbTrans.ImageFieldName = imageField;
				dbTrans.EmptyImage = EmptyImage;
				ImageTransforms.Add (dbTrans);
			}


			/*
			// DNN FileId & Fileticket
			if (!string.IsNullOrEmpty (parameters ["fileid"]) || !string.IsNullOrEmpty (parameters ["fileticket"]))
			{
				// TODO: Check current user permissions to view this file

				int fileId;

				if (!string.IsNullOrEmpty (parameters ["fileticket"]))
				{
					// get fileId from fileticket value
					fileId = FileLinkClickController.Instance.GetFileIdFromLinkClick (parameters);
				}
				else
				{
					// check if fileid is integer
					if (!int.TryParse (parameters ["fileid"], out fileId))
						return new ImageInfo (EmptyImage);
				}

				// check if we really have such file in a DB
				var fileInfo = FileManager.Instance.GetFile (fileId);
				if (fileInfo == null)
					return new ImageInfo (EmptyImage);

				// check if file exists
				if (!File.Exists (fileInfo.PhysicalPath))
					return new ImageInfo (EmptyImage);

				// determine output format
				// REVIEW: Is this conflicts with Format parameter?

				imgFile = fileInfo.PhysicalPath;
			} */

			// DNN Profile Pic
			// TODO: Refactor this to more tight integration with DNN
			if (!string.IsNullOrEmpty (parameters ["dnn"]))
			{
				//First let us check if the Db value is a key or a connectionstring name
                
				var userId = Null.NullInteger;
				if (!string.IsNullOrEmpty (parameters ["userid"]))
					userId = Convert.ToInt32 (parameters ["userid"]);

				var portalId = Null.NullInteger;
				if (!string.IsNullOrEmpty (parameters ["portalid"]))
					portalId = Convert.ToInt32 (parameters ["portalid"]);

				var dbTrans = new ImageDbTransform ();

								/*
				dbTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				dbTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				dbTrans.SmoothingMode = SmoothingMode.HighQuality;
				dbTrans.CompositingQuality = CompositingQuality.HighQuality;*/

				dbTrans.InterpolationMode = Settings.InterpolationMode;
				dbTrans.PixelOffsetMode = Settings.PixelOffsetMode;
				dbTrans.SmoothingMode = Settings.SmoothingMode;
				dbTrans.CompositingQuality = Settings.CompositingQuality;

				var connectionName = "SiteSqlServer"; // DNN

				var conn = ConfigurationManager.ConnectionStrings [connectionName];

				if (conn == null || string.IsNullOrEmpty ((parameters ["portalid"])) && 
					string.IsNullOrEmpty (parameters ["userid"]))
					return new ImageInfo (EmptyImage);

				dbTrans.ConnectionString = conn.ConnectionString;
				dbTrans.UserId = userId;
				dbTrans.PortalId = portalId;
				dbTrans.EmptyImage = EmptyImage;

				ImageTransforms.Add (dbTrans);
			}

			// Url Transform
			if (!string.IsNullOrEmpty (parameters ["url"]))
			{
				var urlTrans = new ImageUrlTransform ();

				urlTrans.InterpolationMode = Settings.InterpolationMode;
				urlTrans.PixelOffsetMode = Settings.PixelOffsetMode;
				urlTrans.SmoothingMode = Settings.SmoothingMode;
				urlTrans.CompositingQuality = Settings.CompositingQuality;

				/*
				urlTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				urlTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				urlTrans.SmoothingMode = SmoothingMode.HighQuality;
				urlTrans.CompositingQuality = CompositingQuality.HighQuality;*/

				urlTrans.Url = parameters ["url"];
				if (!String.IsNullOrEmpty (parameters ["ratio"]))
					urlTrans.Ratio = (UrlRatioMode)Enum.Parse (typeof(UrlRatioMode), parameters ["ratio"], true);
				else
					urlTrans.Ratio = UrlRatioMode.Full;

				ImageTransforms.Add (urlTrans);
			}

			// ImageUrl Transform
			if (!string.IsNullOrEmpty (parameters ["imageurl"]))
			{
				var imageUrlTrans = new ImageUrlImageTransform ();

				imageUrlTrans.InterpolationMode = Settings.InterpolationMode;
				imageUrlTrans.PixelOffsetMode = Settings.PixelOffsetMode;
				imageUrlTrans.SmoothingMode = Settings.SmoothingMode;
				imageUrlTrans.CompositingQuality = Settings.CompositingQuality;

				/*
				imageUrlTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				imageUrlTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				imageUrlTrans.SmoothingMode = SmoothingMode.HighQuality;
				imageUrlTrans.CompositingQuality = CompositingQuality.HighQuality;*/

				imageUrlTrans.ImageUrl = parameters ["imageurl"];

				ImageTransforms.Add (imageUrlTrans);
			}

			// Counter Transform
			if (!string.IsNullOrEmpty (parameters ["counter"]))
			{
				var counterTrans = new ImageCounterTransform ();

				counterTrans.InterpolationMode = Settings.InterpolationMode;
				counterTrans.PixelOffsetMode = Settings.PixelOffsetMode;
				counterTrans.SmoothingMode = Settings.SmoothingMode;
				counterTrans.CompositingQuality = Settings.CompositingQuality;

				/*
				counterTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				counterTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				counterTrans.SmoothingMode = SmoothingMode.HighQuality;
				counterTrans.CompositingQuality = CompositingQuality.HighQuality;
				*/

				if (!string.IsNullOrEmpty (parameters ["counter"]))
					counterTrans.Counter = Convert.ToInt32 (parameters ["counter"]);
				if (!string.IsNullOrEmpty (parameters ["digits"]))
					counterTrans.Digits = Convert.ToInt32 (parameters ["digits"]);

				ImageTransforms.Add (counterTrans);
			}

			// Radial Indicator
			if (!string.IsNullOrEmpty ((parameters ["percentage"])))
			{
				var percentTrans = new ImagePercentageTransform ();

				/*
				percentTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				percentTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				percentTrans.SmoothingMode = SmoothingMode.HighQuality;
				percentTrans.CompositingQuality = CompositingQuality.HighQuality;
				*/

				percentTrans.InterpolationMode = Settings.InterpolationMode;
				percentTrans.PixelOffsetMode = Settings.PixelOffsetMode;
				percentTrans.SmoothingMode = Settings.SmoothingMode;
				percentTrans.CompositingQuality = Settings.CompositingQuality;

				if (!string.IsNullOrEmpty (parameters ["Percentage"]))
					percentTrans.Percentage = Convert.ToInt32 (parameters ["Percentage"]);
				if (!string.IsNullOrEmpty (parameters ["BgColor"]))
					percentTrans.Color = bgColor;
				else
					percentTrans.Color = Color.Orange;

				ImageTransforms.Add (percentTrans);

			}

			// Barcode 
			if (!string.IsNullOrEmpty ((parameters ["barcode"])))
			{
				var barcodeTrans = new ImageBarcodeTransform ();

				/*
				barcodeTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				barcodeTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				barcodeTrans.SmoothingMode = SmoothingMode.HighQuality;
				barcodeTrans.CompositingQuality = CompositingQuality.HighQuality;
				*/

				barcodeTrans.InterpolationMode = Settings.InterpolationMode;
				barcodeTrans.PixelOffsetMode = Settings.PixelOffsetMode;
				barcodeTrans.SmoothingMode = Settings.SmoothingMode;
				barcodeTrans.CompositingQuality = Settings.CompositingQuality;

				barcodeTrans.Border = 0;
				barcodeTrans.Width = 100;
				barcodeTrans.Height = 100;

				if (!string.IsNullOrEmpty (parameters ["encoding"]))
					barcodeTrans.Encoding = parameters ["encoding"];

				if (!string.IsNullOrEmpty (parameters ["type"]) && 
					"upca,ean8,ean13,code39,code128,itf,codabar,plessey,msi,qrcode,pdf417,aztec,datamatrix,".LastIndexOf (parameters ["type"].ToLowerInvariant() + ",") > -1)
				{
					barcodeTrans.Type = parameters ["type"].ToLower ();
				}
				if (!string.IsNullOrEmpty (parameters ["content"]))
				{
					barcodeTrans.Content = HttpUtility.UrlDecode(parameters ["content"]);
				}
				if (!string.IsNullOrEmpty (parameters ["width"]))
				{
					barcodeTrans.Width = Convert.ToInt32 (parameters ["width"]);
				}
				if (!string.IsNullOrEmpty (parameters ["height"]))
				{
					barcodeTrans.Height = Convert.ToInt32 (parameters ["height"]);
				}
				if (!string.IsNullOrEmpty (parameters ["border"]))
				{
					barcodeTrans.Border = Convert.ToInt32 (parameters ["border"]);
				}

				ImageTransforms.Add (barcodeTrans);
			}

			if (!string.IsNullOrEmpty ((parameters ["schedule"])))
			{
				var scheduleTrans = new ImageScheduleTransform ();

				/*
				scheduleTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				scheduleTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				scheduleTrans.SmoothingMode = SmoothingMode.HighQuality;
				scheduleTrans.CompositingQuality = CompositingQuality.HighQuality;
				*/

				scheduleTrans.InterpolationMode = Settings.InterpolationMode;
				scheduleTrans.PixelOffsetMode = Settings.PixelOffsetMode;
				scheduleTrans.SmoothingMode = Settings.SmoothingMode;
				scheduleTrans.CompositingQuality = Settings.CompositingQuality;

				scheduleTrans.Matrix = parameters ["matrix"];

				// determine culture
				if (!string.IsNullOrEmpty (parameters ["culture"]))
					scheduleTrans.Culture = parameters ["culture"];
				else
					scheduleTrans.Culture = Thread.CurrentThread.CurrentCulture.Name;

				// determine bgcolor
				if (!string.IsNullOrEmpty (parameters ["bgcolor"]))
					scheduleTrans.BackColor = bgColor;
				else
					scheduleTrans.BackColor = Color.White;

				ImageTransforms.Add (scheduleTrans);
			}

			// Resize-Transformation (only if not placeholder or barcode)
			if (string.IsNullOrEmpty (parameters ["placeholder"]) && string.IsNullOrEmpty (parameters ["barcode"]) &&
			             (!string.IsNullOrEmpty (parameters ["width"]) || !string.IsNullOrEmpty (parameters ["height"]) ||
			             (!string.IsNullOrEmpty (parameters ["maxwidth"]) || !string.IsNullOrEmpty (parameters ["maxheight"]))))
			{
				var resizeTrans = new ImageResizeTransform ();
				resizeTrans.Mode = ImageResizeMode.Fit;

				resizeTrans.InterpolationMode = Settings.InterpolationMode;
				resizeTrans.PixelOffsetMode = Settings.PixelOffsetMode;
				resizeTrans.SmoothingMode = Settings.SmoothingMode;
				resizeTrans.CompositingQuality = Settings.CompositingQuality;
				/*
				resizeTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				resizeTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				resizeTrans.SmoothingMode = SmoothingMode.HighQuality;
				resizeTrans.CompositingQuality = CompositingQuality.HighQuality;
*/
				resizeTrans.BackColor = bgColor;

				// Parameter 'Mode' is obsolete. New is 'ResizeMode'	
				// if (!string.IsNullOrEmpty(parameters["Mode"]))
				//	resizeTrans.Mode = (ImageResizeMode) Enum.Parse(typeof (ImageResizeMode), parameters["Mode"]);

				if (!string.IsNullOrEmpty (parameters ["resizemode"]))
					// TODO: Use Enum.TryParse and fallback resize mode
					resizeTrans.Mode = (ImageResizeMode)Enum.Parse (typeof(ImageResizeMode), parameters ["resizemode"], true);

				if (!string.IsNullOrEmpty (parameters ["width"]))
				{
					resizeTrans.Width = Convert.ToInt32 (parameters ["width"]);
				}
				if (!string.IsNullOrEmpty (parameters ["height"]))
				{
					resizeTrans.Height = Convert.ToInt32 (parameters ["height"]);
				}
				if (!string.IsNullOrEmpty (parameters ["maxwidth"]))
				{
					resizeTrans.MaxWidth = Convert.ToInt32 (parameters ["maxwidth"]);
				}
				if (!string.IsNullOrEmpty (parameters ["maxheight"]))
				{
					resizeTrans.MaxHeight = Convert.ToInt32 (parameters ["maxheight"]);
				}
				if (!string.IsNullOrEmpty (parameters ["border"]))
				{
					resizeTrans.Border = Convert.ToInt32 (parameters ["border"]);
				}

				ImageTransforms.Add (resizeTrans);
			}

			// Watermark Transform
			if (!string.IsNullOrEmpty (parameters ["watermarktext"]))
			{
				var watermarkTrans = new ImageWatermarkTransform ();

				watermarkTrans.InterpolationMode = Settings.InterpolationMode;
				watermarkTrans.PixelOffsetMode = Settings.PixelOffsetMode;
				watermarkTrans.SmoothingMode = Settings.SmoothingMode;
				watermarkTrans.CompositingQuality = Settings.CompositingQuality;

				/*
				watermarkTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				watermarkTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				watermarkTrans.SmoothingMode = SmoothingMode.HighQuality;
				watermarkTrans.CompositingQuality = CompositingQuality.HighQuality;
				*/

				watermarkTrans.WatermarkText = parameters ["watermarktext"];
				if (!string.IsNullOrEmpty (parameters ["watermarkfontfamily"]))
					watermarkTrans.FontFamily = parameters ["watermarkfontfamily"];
				if (!string.IsNullOrEmpty (parameters ["watermarkfontcolor"]))
				{
					var color = parameters ["watermarkfontcolor"];
					watermarkTrans.FontColor = color.StartsWith ("#") ? ColorTranslator.FromHtml (color) : Color.FromName (color);
				}
				if (!string.IsNullOrEmpty (parameters ["watermarkfontsize"]))
					watermarkTrans.FontSize = Convert.ToSingle (parameters ["watermarkfontsize"]);
				if (!string.IsNullOrEmpty (parameters ["watermarkposition"]))
				{
					var enumType = typeof(WatermarkPositionMode);
					var pos = parameters ["watermarkposition"];
					watermarkTrans.WatermarkPosition = (WatermarkPositionMode)Enum.Parse (enumType, pos, true);
				}
				if (!string.IsNullOrEmpty (parameters ["watermarkopacity"]))
					watermarkTrans.WatermarkOpacity = Convert.ToInt32 (parameters ["watermarkopacity"]);

				ImageTransforms.Add (watermarkTrans);
			}

			// Gamma adjustment
			if (!string.IsNullOrEmpty (parameters ["gamma"]))
			{
				var gammaTrans = new ImageGammaTransform ();

				gammaTrans.InterpolationMode = Settings.InterpolationMode;
				gammaTrans.PixelOffsetMode = Settings.PixelOffsetMode;
				gammaTrans.SmoothingMode = Settings.SmoothingMode;
				gammaTrans.CompositingQuality = Settings.CompositingQuality;

				/*
				gammaTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				gammaTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				gammaTrans.SmoothingMode = SmoothingMode.HighQuality;
				gammaTrans.CompositingQuality = CompositingQuality.HighQuality;
			*/
				var gamma = 1.0;

				if (double.TryParse (parameters ["Gamma"], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out gamma) && (gamma >= 0.2 && gamma <= 5))
				{
					gammaTrans.Gamma = gamma;
					ImageTransforms.Add (gammaTrans);
				}
			}

			// Brightness adjustment
			if (!string.IsNullOrEmpty (parameters ["brightness"]))
			{
				var brightnessTrans = new ImageBrightnessTransform ();

				/*
				brightnessTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				brightnessTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				brightnessTrans.SmoothingMode = SmoothingMode.HighQuality;
				brightnessTrans.CompositingQuality = CompositingQuality.HighQuality;
*/
				brightnessTrans.InterpolationMode = Settings.InterpolationMode;
				brightnessTrans.PixelOffsetMode = Settings.PixelOffsetMode;
				brightnessTrans.SmoothingMode = Settings.SmoothingMode;
				brightnessTrans.CompositingQuality = Settings.CompositingQuality;

				var brightness = 0;

				if (int.TryParse (parameters ["brightness"], out brightness))
				{
					brightnessTrans.Brightness = brightness;
					ImageTransforms.Add (brightnessTrans);
				}
			}

			// Contrast adjustment
			if (!string.IsNullOrEmpty (parameters ["contrast"]))
			{
				var contrastTrans = new ImageContrastTransform ();
				/*
				contrastTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				contrastTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				contrastTrans.SmoothingMode = SmoothingMode.HighQuality;
				contrastTrans.CompositingQuality = CompositingQuality.HighQuality;
				*/

				contrastTrans.InterpolationMode = Settings.InterpolationMode;
				contrastTrans.PixelOffsetMode = Settings.PixelOffsetMode;
				contrastTrans.SmoothingMode = Settings.SmoothingMode;
				contrastTrans.CompositingQuality = Settings.CompositingQuality;

				var contrast = 0.0;

				if (double.TryParse (parameters ["contrast"], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out contrast) && (contrast >= -100 && contrast <= 100))
				{
					contrastTrans.Contrast = contrast;
					ImageTransforms.Add (contrastTrans);
				}
			}

			// Greyscale
			if (!string.IsNullOrEmpty (parameters ["greyscale"]))
			{
				var greyscaleTrans = new ImageGreyScaleTransform ();

				greyscaleTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				greyscaleTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				greyscaleTrans.SmoothingMode = SmoothingMode.HighQuality;
				greyscaleTrans.CompositingQuality = CompositingQuality.HighQuality;
				/*
				greyscaleTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				greyscaleTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				greyscaleTrans.SmoothingMode = SmoothingMode.HighQuality;
				greyscaleTrans.CompositingQuality = CompositingQuality.HighQuality;
				*/
				ImageTransforms.Add (greyscaleTrans);
			}

			// Invert
			if (!string.IsNullOrEmpty (parameters ["invert"]))
			{
				var invertTrans = new ImageInvertTransform ();

				invertTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				invertTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				invertTrans.SmoothingMode = SmoothingMode.HighQuality;
				invertTrans.CompositingQuality = CompositingQuality.HighQuality;

				/*
				invertTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				invertTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				invertTrans.SmoothingMode = SmoothingMode.HighQuality;
				invertTrans.CompositingQuality = CompositingQuality.HighQuality;*/
				ImageTransforms.Add (invertTrans);
			}

			// Rotate / Flip 
			if (!string.IsNullOrEmpty (parameters ["rotateflip"]))
			{
				var rotateFlipTrans = new ImageRotateFlipTransform ();

				rotateFlipTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				rotateFlipTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				rotateFlipTrans.SmoothingMode = SmoothingMode.HighQuality;
				rotateFlipTrans.CompositingQuality = CompositingQuality.HighQuality;

				/*
				rotateFlipTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				rotateFlipTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				rotateFlipTrans.SmoothingMode = SmoothingMode.HighQuality;
				rotateFlipTrans.CompositingQuality = CompositingQuality.HighQuality;
				*/

				var rotateFlipType = (RotateFlipType)Enum.Parse (typeof(RotateFlipType), parameters ["RotateFlip"]);
				rotateFlipTrans.RotateFlip = rotateFlipType;

				ImageTransforms.Add (rotateFlipTrans);
			}

			// Placeholder 
			if (!string.IsNullOrEmpty (parameters ["placeholder"]))
			{
				var placeHolderTrans = new ImagePlaceholderTransform ();
				/*
				placeHolderTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				placeHolderTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				placeHolderTrans.SmoothingMode = SmoothingMode.HighQuality;
				placeHolderTrans.CompositingQuality = CompositingQuality.HighQuality;*/

				placeHolderTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
				placeHolderTrans.PixelOffsetMode = PixelOffsetMode.HighQuality;
				placeHolderTrans.SmoothingMode = SmoothingMode.HighQuality;
				placeHolderTrans.CompositingQuality = CompositingQuality.HighQuality;

				var width = 0;
				var height = 0;

				if (int.TryParse (parameters ["width"], out width))
					placeHolderTrans.Width = width;
				if (int.TryParse (parameters ["height"], out height))
					placeHolderTrans.Height = height;
				if (!string.IsNullOrEmpty (parameters ["color"]))
				{
					string color = parameters ["color"];
					placeHolderTrans.Color = color.StartsWith ("#") ? ColorTranslator.FromHtml (color) : Color.FromName (color);
				}

				if (!string.IsNullOrEmpty (parameters ["text"]))
					placeHolderTrans.Text = parameters ["text"];

				if (!string.IsNullOrEmpty (parameters ["bgcolor"]))
				{
					string color = parameters ["bgcolor"];
					placeHolderTrans.BackColor = color.StartsWith ("#") ? ColorTranslator.FromHtml (color) : Color.FromName (color);
				}

				ImageTransforms.Add (placeHolderTrans);
			}

			if (imgFile == string.Empty)
			{
				// REVIEW: Return new ImageInfo(EmptyImage)?
				var dummy = new Bitmap (1, 1, PixelFormat.Format24bppRgb);
				var ms = new MemoryStream ();
				dummy.Save (ms, ImageFormat.Jpeg);
				return new ImageInfo (ms.ToArray ());
			}
			else
			{
				// read all data from file 
				return new ImageInfo (File.ReadAllBytes (imgFile));
			}

		}
	}
}