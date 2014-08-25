//
// ImageWatermarkTransform.cs
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace R7.ImageHandler
{
	public class ImageWatermarkTransform : ImageTransformBase
	{
		/// <summary>
		/// Sets the watermark text. Defaultvalue is empty
		/// </summary>
		[DefaultValue("")]
		[Category("Behavior")]
		public string WatermarkText { get; set; }

		/// <summary>
		/// Sets the watermark position Defaultvalue is center
		/// </summary>
		[DefaultValue(WatermarkPositionMode.Center)]
		[Category("Behavior")]
		public WatermarkPositionMode WatermarkPosition { get; set; }

		/// <summary>
		/// Sets the watermark opacity. Defaultvalue is 127 (0..255).
		/// </summary>
		[DefaultValue(127)]
		[Category("Behavior")]
		public int WatermarkOpacity { get; set; }

		/// <summary>
		/// Sets the watermark fontcolor. Default is black
		/// </summary>
		[DefaultValue(typeof(Color),"Black")]
		[Category("Behavior")]
		public Color FontColor  { get; set; }

		/// <summary>
		/// Sets the watermark font family. Default is Verdana
		/// </summary>
		[DefaultValue("Verdana")]
		[Category("Behavior")]
		public string FontFamily { get; set; }

		/// <summary>
		/// Sets the watermark font size. Default is 14
		/// </summary>
		[DefaultValue(14.0)]
		[Category("Behavior")]
		public Single FontSize { get; set; }

		public override string UniqueString
		{
			get
			{
				// MyBase.UniqueString, "-", Me.WatermarkText, "-", Me.FontFamily, "-", Me.FontSize, "-", Me.FontColor.ToString()
				return base.UniqueString + "-" + 
				       this.WatermarkText + "-" + 
				       this.WatermarkPosition.ToString()+"-"+
				       this.WatermarkOpacity.ToString()+"-"+
				       this.FontColor.ToString()+"-"+
				       this.FontFamily + "-"+
				       this.FontSize.ToString();

			}
		}

		public ImageWatermarkTransform() {
			InterpolationMode = InterpolationMode.HighQualityBicubic;
			SmoothingMode = SmoothingMode.Default;
			PixelOffsetMode = PixelOffsetMode.Default;
			CompositingQuality = CompositingQuality.HighSpeed;

			WatermarkText = string.Empty;
			WatermarkPosition = WatermarkPositionMode.Center;
			WatermarkOpacity = 127;
			FontColor = Color.Black;
			FontFamily = "Verdana";
			FontSize = 14;
		}

		public override Image ProcessImage(Image image)
		{
			Font watermarkFont = new Font(this.FontFamily, this.FontSize);

			Bitmap newBitmap = new Bitmap(image.Width, image.Height);
			Graphics graphics = Graphics.FromImage(newBitmap); 

			graphics.CompositingMode = CompositingMode.SourceOver;
			graphics.CompositingQuality = CompositingQuality;
			graphics.InterpolationMode = InterpolationMode;
			graphics.SmoothingMode = SmoothingMode;

			graphics.DrawImage(image, 0, 0); 

			SizeF sz = graphics.MeasureString(this.WatermarkText, watermarkFont);
			Single x = 0;
			Single y = 0;
			
			switch (this.WatermarkPosition)
			{
				case WatermarkPositionMode.TopLeft:
					x = 0;
					y = 0;
					break;
				case WatermarkPositionMode.TopCenter:
					x = image.Width / 2 - sz.Width / 2;
					y = 0;
					break;
				case WatermarkPositionMode.TopRight:
					x = image.Width - sz.Width;
					y = 0;
					break;
				case WatermarkPositionMode.CenterLeft:
					x = 0;
					y = image.Height / 2 - sz.Height / 2;
					break;
				case WatermarkPositionMode.Center:
					x = image.Width / 2 - sz.Width / 2;
					y = image.Height / 2 - sz.Height / 2;
					break;
				case WatermarkPositionMode.CenterRight:
					x = image.Width - sz.Width;
					y = image.Height / 2 - sz.Height / 2;
					break;
				case WatermarkPositionMode.BottomLeft:
					x = 0;
					y = image.Height - sz.Height;
					break;
				case WatermarkPositionMode.BottomCenter:
					x = image.Width / 2 - sz.Width / 2;
					y = image.Height - sz.Height;
					break;
				case WatermarkPositionMode.BottomRight:
					x = image.Width - sz.Width;
					y = image.Height - sz.Height;
					break;
				default:
					break;
			}
			Brush watermarkBrush  = new SolidBrush(Color.FromArgb(WatermarkOpacity, FontColor));
			graphics.DrawString(this.WatermarkText, watermarkFont, watermarkBrush, x, y);
			return image;
	
		}

	}
}