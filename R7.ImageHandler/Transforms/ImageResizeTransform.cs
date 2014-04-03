//
// ImageResizeTransform.cs
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace R7.ImageHandler
{
	public class ImageResizeTransform : ImageTransformBase
	{
		private int _width = 0, _height = 0, _border = 0, _maxWidth = 0, _maxHeight = 0;
		private Color _backColor = Color.White;

		/// <summary>
		/// Sets the resize mode. The default value is Fit.
		/// </summary>
		[DefaultValue (ImageResizeMode.Fit)]
		[Category ("Behavior")]
		public ImageResizeMode Mode { get; set; }

		/// <summary>
		/// Sets the width of the resulting image
		/// </summary>
		[DefaultValue (0)]
		[Category ("Behavior")]
		public int Width
		{
			get { return _width; }
			set 
			{ 
				CheckValue (value);
				_width = value;
			}
		}

		/// <summary>
		/// Sets the Max width of the resulting image
		/// </summary>
		[DefaultValue (0)]
		[Category ("Behavior")]
		public int MaxWidth
		{
			get { return _maxWidth; }
			set
			{
				CheckValue (value);
				_maxWidth = value;
			}
		}

		/// <summary>
		/// Sets the height of the resulting image
		/// </summary>
		[DefaultValue (0)]
		[Category ("Behavior")]
		public int Height
		{
			get { return _height; }
			set
			{
				CheckValue (value);
				_height = value;
			}
		}

		/// <summary>
		/// Sets the height of the resulting image
		/// </summary>
		[DefaultValue (0)]
		[Category ("Behavior")]
		public int MaxHeight
		{
			get { return _maxHeight; }
			set
			{
				CheckValue (value);
				_maxHeight = value;
			}
		}

		/// <summary>
		/// Sets the border width of the resulting image
		/// </summary>
		[DefaultValue (0)]
		[Category ("Behavior")]
		public int Border
		{
			get { return _border; }
			set
			{
				CheckValue (value);
				_border = value;
			}
		}

		/// <summary>
		/// Sets the Backcolor 
		/// </summary>
		[Category ("Behavior")]
		public Color BackColor
		{
			get	{ return _backColor; }
			set { _backColor = value; }
		}

		public ImageResizeTransform ()
		{
			InterpolationMode = InterpolationMode.HighQualityBicubic;
			SmoothingMode = SmoothingMode.Default;
			PixelOffsetMode = PixelOffsetMode.Default;
			CompositingQuality = CompositingQuality.HighSpeed;
			Mode = ImageResizeMode.Fit;
		}

		private static void CheckValue (int value)
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException ("value");
		}

		public override Image ProcessImage (Image img)
		{
			if (this.MaxWidth > 0)
			{
				if (img.Width > this.MaxWidth)
					this.Width = this.MaxWidth;
				else
					this.Width = img.Width;
			}

			if (this.MaxHeight > 0)
			{
				if (img.Height > this.MaxHeight)
					this.Height = this.MaxHeight;
				else
					this.Height = img.Height;
			}

			var scaledHeight = (int)(img.Height * ((float)this.Width / (float)img.Width));
			var scaledWidth = (int)(img.Width * ((float)this.Height / (float)img.Height));

			Image procImage;
			switch (Mode)
			{
			case ImageResizeMode.Fit:
				procImage = FitImage (img, scaledHeight, scaledWidth);
				break;
			case ImageResizeMode.Crop:
				procImage = CropImage (img, scaledHeight, scaledWidth);
				break;
			case ImageResizeMode.FitSquare:
				procImage = FitSquareImage (img, scaledHeight, scaledWidth);
				break;
			default:
				Debug.Fail ("Should not reach this");
				return null;
			}
			return procImage;
		}

		private Image FitImage (Image img, int scaledHeight, int scaledWidth)
		{
			var resizeWidth = 0;
			var resizeHeight = 0;

			if (this.Height == 0)
			{
				resizeWidth = this.Width;
				resizeHeight = scaledHeight;
			}
			else if (this.Width == 0)
			{
				resizeWidth = scaledWidth;
				resizeHeight = this.Height;
			}
			else
			{
				if (((float)this.Width / (float)img.Width < this.Height / (float)img.Height))
				{
					resizeWidth = this.Width;
					resizeHeight = scaledHeight;
				}
				else
				{
					resizeWidth = scaledWidth;
					resizeHeight = this.Height;
				}
			}

			var newimage = new Bitmap (resizeWidth + 2 * _border, resizeHeight + 2 * _border);
			var graphics = Graphics.FromImage (newimage);

			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality;
			graphics.InterpolationMode = InterpolationMode;
			graphics.SmoothingMode = SmoothingMode;

			graphics.FillRectangle (new SolidBrush (BackColor), 
				new Rectangle (0, 0, resizeWidth + 2 * _border, resizeHeight + 2 * _border));

			graphics.DrawImage (img, 
				new Rectangle (_border, _border, resizeWidth, resizeHeight),
				// HACK: makes 2px border less visible
				new Rectangle (2, 2, img.Width - 4, img.Height - 4),
				GraphicsUnit.Pixel
			);
	
			return newimage;
		}

		private Image FitSquareImage (Image img, int scaledHeight, int scaledWidth)
		{
			var resizeWidth = 0;
			var resizeHeight = 0;

			if (img.Height > img.Width)
			{
				resizeWidth = Convert.ToInt32 ((float)img.Width / (float)img.Height * this.Width);
				resizeHeight = this.Width;
			}
			else
			{
				resizeWidth = this.Width;
				resizeHeight = Convert.ToInt32 ((float)img.Height / (float)img.Width * this.Width);
			}

			var newimage = new Bitmap (this.Width + 2 * _border, this.Width + 2 * _border);
			
			var graphics = Graphics.FromImage (newimage);
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality;
			graphics.InterpolationMode = InterpolationMode;
			graphics.SmoothingMode = SmoothingMode;

			graphics.FillRectangle (new SolidBrush (BackColor), new Rectangle (0, 0, this.Width + 2 * _border, this.Width + 2 * _border));
			graphics.DrawImage (img, 
				new Rectangle ((this.Width - resizeWidth) / 2 + _border, 
					(this.Width - resizeHeight) / 2 + _border, resizeWidth, resizeHeight),
				// HACK: makes 2px border less visible
				new Rectangle (2, 2, img.Width - 4, img.Height - 4), 
				GraphicsUnit.Pixel
			);

			return newimage;
		}

		private Image CropImage (Image img, int scaledHeight, int scaledWidth)
		{
			var resizeWidth = 0;
			var resizeHeight = 0;
			if (((float)this.Width / (float)img.Width > this.Height / (float)img.Height))
			{
				resizeWidth = this.Width;
				resizeHeight = scaledHeight;
			}
			else
			{
				resizeWidth = scaledWidth;
				resizeHeight = this.Height;
			}

			var newImage = new Bitmap (this.Width, this.Height);
			
			var graphics = Graphics.FromImage (newImage);
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality;
			graphics.InterpolationMode = InterpolationMode;
			graphics.SmoothingMode = SmoothingMode;
			graphics.PixelOffsetMode = PixelOffsetMode;

			graphics.DrawImage (img, (this.Width - resizeWidth) / 2, (this.Height - resizeHeight) / 2, resizeWidth, resizeHeight);
			return newImage;
		}

		[Browsable (false)]
		public override string UniqueString
		{
			get
			{
				return base.UniqueString + Width + InterpolationMode + Height + Mode;
			}
		}

		public override string ToString ()
		{
			return "ImageResizeTransform";
		}
	}
}