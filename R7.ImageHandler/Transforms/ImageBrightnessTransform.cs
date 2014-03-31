//
// ImageBrightnessTransform.cs
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
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace R7.ImageHandler
{
	public class ImageBrightnessTransform : ImageTransformBase
	{
		/// <summary>
		/// Sets the counter value. Defaultvalue is 0
		/// </summary>
		[DefaultValue(0)]
		[Category("Behavior")]
		public int Brightness { get; set; }


		public override string UniqueString
		{
			get { return base.UniqueString + "-" + this.Brightness.ToString(); }
		}

		public ImageBrightnessTransform()
		{
			InterpolationMode = InterpolationMode.HighQualityBicubic;
			SmoothingMode = SmoothingMode.Default;
			PixelOffsetMode = PixelOffsetMode.Default;
			CompositingQuality = CompositingQuality.HighSpeed;
			this.Brightness = 0;
		}

		public override Image ProcessImage(Image image)
		{
			Bitmap temp = (Bitmap)image;
			Bitmap bmap = (Bitmap)temp.Clone();
			if (Brightness < -255) Brightness = -255;
			if (Brightness > 255) Brightness = 255;
			Color c;
			for (int i = 0; i < bmap.Width; i++)
			{
				for (int j = 0; j < bmap.Height; j++)
				{
					c = bmap.GetPixel(i, j);
					int cR = c.R + Brightness;
					int cG = c.G + Brightness;
					int cB = c.B + Brightness;

					if (cR < 0) cR = 1;
					if (cR > 255) cR = 255;

					if (cG < 0) cG = 1;
					if (cG > 255) cG = 255;

					if (cB < 0) cB = 1;
					if (cB > 255) cB = 255;

					bmap.SetPixel(i, j, Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
				}
			}
			return (Bitmap)bmap.Clone();
		}
	}
}