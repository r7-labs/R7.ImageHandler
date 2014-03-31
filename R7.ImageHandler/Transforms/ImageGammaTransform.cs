//
// ImageGammaTransform.cs
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
	public class ImageGammaTransform : ImageTransformBase
	{
		/// <summary>
		/// Sets the counter value. Defaultvalue is 0
		/// </summary>
		[DefaultValue(0)]
		[Category("Behavior")]
		public double Gamma { get; set; }

		public override string UniqueString
		{
			get { return base.UniqueString + "-" + this.Gamma.ToString(); }
		}

		public ImageGammaTransform()
		{
			InterpolationMode = InterpolationMode.HighQualityBicubic;
			SmoothingMode = SmoothingMode.Default;
			PixelOffsetMode = PixelOffsetMode.Default;
			CompositingQuality = CompositingQuality.HighSpeed;
			Gamma = 1;
		}

		public override Image ProcessImage(Image image)
		{
			var temp = (Bitmap)image;
			var bmap = (Bitmap)temp.Clone();
			Color c;
			var gammaArray = new byte[256];
			for (var i = 0; i < 256; ++i)
			{
				gammaArray[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / Gamma)) + 0.5));
			}
			
			for (var i = 0; i < bmap.Width; i++)
			{
				for (var j = 0; j < bmap.Height; j++)
				{
					c = bmap.GetPixel(i, j);
					bmap.SetPixel(i, j, Color.FromArgb(gammaArray[c.R],
					   gammaArray[c.G], gammaArray[c.B]));
				}
			}
			return (Image)bmap.Clone();
		}
	}
}