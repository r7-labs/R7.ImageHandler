//
// ImageContrastTransform.cs
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
	public class ImageContrastTransform : ImageTransformBase
	{
		/// <summary>
		/// Sets the counter value. Defaultvalue is 0
		/// </summary>
		[DefaultValue(0)]
		[Category("Behavior")]
		public double Contrast { get; set; }


		public override string UniqueString
		{
			get { return base.UniqueString + "-" + this.Contrast.ToString(); }
		}

		public ImageContrastTransform()
		{
			InterpolationMode = InterpolationMode.HighQualityBicubic;
			SmoothingMode = SmoothingMode.Default;
			PixelOffsetMode = PixelOffsetMode.Default;
			CompositingQuality = CompositingQuality.HighSpeed;
			this.Contrast = 0;
		}

		public override Image ProcessImage(Image image)
		{
			Bitmap temp = (Bitmap)image;
			Bitmap bmap = (Bitmap)temp.Clone();
			if (Contrast < -100) Contrast = -100;
			if (Contrast > 100) Contrast = 100;
			Contrast = (100.0 + Contrast) / 100.0;
			Contrast *= Contrast;
			Color c;
			for (int i = 0; i < bmap.Width; i++)
			{
				for (int j = 0; j < bmap.Height; j++)
				{
					c = bmap.GetPixel(i, j);
					double pR = c.R / 255.0;
					pR -= 0.5;
					pR *= Contrast;
					pR += 0.5;
					pR *= 255;
					if (pR < 0) pR = 0;
					if (pR > 255) pR = 255;

					double pG = c.G / 255.0;
					pG -= 0.5;
					pG *= Contrast;
					pG += 0.5;
					pG *= 255;
					if (pG < 0) pG = 0;
					if (pG > 255) pG = 255;

					double pB = c.B / 255.0;
					pB -= 0.5;
					pB *= Contrast;
					pB += 0.5;
					pB *= 255;
					if (pB < 0) pB = 0;
					if (pB > 255) pB = 255;

					bmap.SetPixel(i, j, Color.FromArgb((byte)pR, (byte)pG, (byte)pB));
				}
			}
			return (Bitmap)bmap.Clone();
		}
	}
}