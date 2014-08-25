//
// ImageHandlerSettings.cs
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
using System.Configuration;
using System.Drawing.Drawing2D;
using DotNetNuke.Entities.Portals;

namespace R7.ImageHandler
{
	public class ImageHandlerSettings
	{
		private static ImageHandlerSettings instance = null;
		private static object instanceLock = new object();

		public static ImageHandlerSettings Instance
		{
			get 
			{
				if (instance == null)
				{
					lock (instanceLock)
					{
						if (instance == null)
							instance = new ImageHandlerSettings ();
					}
				}
				return instance;
			}
		}

		private ImageHandlerSettings ()
		{
			// default settings
			Now = DateTime.Now;
			EnableClientCache = true;
			EnableServerCache = true;
			EnableSecurity = true;
			EnableSecurityExceptions = true;
			ImageCompression = 92;
			AllowedDomains = new string[] { "localhost" };
			ServerCachePath = "~/App_Data/R7.ImageHandler";
			ServerCacheExpiration = new TimeSpan (0, 20, 0); // 20 minutes = 1200 seconds
			ClientCacheExpiration = new TimeSpan (1, 0, 0); // 1 hour = 3600 seconds
			InterpolationMode = InterpolationMode.HighQualityBicubic;
			PixelOffsetMode = PixelOffsetMode.HighQuality;
			SmoothingMode = SmoothingMode.HighQuality;
			CompositingQuality = CompositingQuality.HighQuality;

			try
			{
				// REVIEW: Add sepatare keys for cache and image settings?
				var settings = ConfigurationManager.AppSettings ["R7.ImageHandler"];
				if (!string.IsNullOrEmpty (settings))
				{
					var pairs = settings.Split (';');
					foreach (var pair in pairs)
					{
						var nameValue = pair.Split ('=');
						var name = nameValue [0].ToLowerInvariant ();
						var value = nameValue [1];

						switch (name)
						{
						case "enablecache":
							EnableClientCache = Convert.ToBoolean (value);
							EnableServerCache = Convert.ToBoolean (value);
							break;
						case "enableclientcache":
							EnableClientCache = Convert.ToBoolean (value);
							break;
						case "enableservercache":
							EnableServerCache = Convert.ToBoolean (value);
							break;
						case "servercachepath":
							// allow portal-level cache storage
							if (value.Contains("[PORTALID]"))
								ServerCachePath = value.Replace("[PORTALID]", PortalSettings.Current.PortalId.ToString());
							else
								ServerCachePath = value;
							break;
						case "enablesecurity":
							EnableSecurity = Convert.ToBoolean (value);
							break;
						case "imagecompression":
							ImageCompression = Convert.ToInt32 (value);
							break;
						case "alloweddomains":
							AllowedDomains = value.Split (',');
							break;
						case "enablesecurityexceptions":
							EnableSecurityExceptions = Convert.ToBoolean (value);
							break;
						case "clientcacheexpiration":
							ClientCacheExpiration = new TimeSpan(0, 0, Convert.ToInt32 (value));
							break;
						case "servercacheexpiration":
							ServerCacheExpiration = new TimeSpan(0, 0, Convert.ToInt32 (value));
							break;
						case "cacheexpiration":
							SetCacheExpiration(new TimeSpan(0, 0, Convert.ToInt32 (value)));
							break;
						case "interpolationmode":
							InterpolationMode = (InterpolationMode)Enum.Parse(typeof(InterpolationMode), value);
							break;
						case "pixeloffsetmode":
							PixelOffsetMode = (PixelOffsetMode)Enum.Parse(typeof(PixelOffsetMode), value);
							break;
						case "smoothingmode":
							SmoothingMode = (SmoothingMode)Enum.Parse(typeof(SmoothingMode), value);
							break;
						case "compositingquality":
							CompositingQuality = (CompositingQuality)Enum.Parse(typeof(CompositingQuality), value);
							break;
						default:
							break;
						}
					}
				}
			}
			catch (System.Exception)
			{
				throw new ConfigurationErrorsException ("Error parsing R7.ImageHandler configuration");
			}
		}

		public DateTime Now { get; private set; }

		public bool EnableClientCache { get; set; }

		public bool EnableServerCache { get; set; }

		public string ServerCachePath { get; private set; }

		public string[] AllowedDomains { get; private set; }

		public bool EnableSecurity { get; private set; }

		public bool EnableSecurityExceptions { get; private set; }

		public int ImageCompression { get; private set; }

		public TimeSpan ServerCacheExpiration { get; private set; }

		public TimeSpan ClientCacheExpiration { get; private set; }

		public void SetCacheExpiration (TimeSpan cacheExpiration)
		{ 
			ServerCacheExpiration = cacheExpiration;
			ClientCacheExpiration = cacheExpiration;
		}

		public InterpolationMode InterpolationMode  { get; private set; }

		public PixelOffsetMode PixelOffsetMode { get; private set; }

		public SmoothingMode SmoothingMode { get; private set; }

		public CompositingQuality CompositingQuality { get; private set; }

	} // class
} // namespace
	