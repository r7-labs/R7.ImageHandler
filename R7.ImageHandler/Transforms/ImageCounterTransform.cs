//
// ImageCounterTransform.cs
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
	public class ImageCounterTransform : ImageTransformBase
	{
		/// <summary>
		/// Sets the counter value. Defaultvalue is 0
		/// </summary>
		[DefaultValue(0)]
		[Category("Behavior")]
		public int Counter { get; set; }

		/// <summary>
		/// Sets the number of digits. Defaultvalue is 5
		/// </summary>
		[DefaultValue(5)]
		[Category("Behavior")]
		public int Digits { get; set; }

		public override string UniqueString
		{
			get
			{
				return base.UniqueString + "-" +
				       this.Counter.ToString() + "-" +
				       this.Digits.ToString() + "-";
			}
		}

		public ImageCounterTransform()
		{
			InterpolationMode = InterpolationMode.HighQualityBicubic;
			SmoothingMode = SmoothingMode.Default;
			PixelOffsetMode = PixelOffsetMode.Default;
			CompositingQuality = CompositingQuality.HighSpeed;

			this.Counter = 0;
			this.Digits = 5;
		}

		public override Image ProcessImage(Image image)
		{
			//Get measurements of a digit 
			int digitWidth = image.Width / 10;
			int digitHeight = image.Height;

			// Create output grahics
			Bitmap imgOutput = new Bitmap(digitWidth * this.Digits, digitHeight, PixelFormat.Format24bppRgb);
			Graphics graphics = Graphics.FromImage(imgOutput);

			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality;
			graphics.InterpolationMode = InterpolationMode;
			graphics.SmoothingMode = SmoothingMode;
			graphics.PixelOffsetMode = PixelOffsetMode;


			// Sampling the output together
			string strCountVal = this.Counter.ToString().PadLeft(this.Digits, '0');
			for (int i = 0; i < this.Digits; i++)
			{
				// Extract digit from countVal
				int digit = Convert.ToInt32(strCountVal.Substring(i, 1));

				// Add digit to output graphics
				Rectangle targetRect = new Rectangle(i * digitWidth, 0, digitWidth, digitHeight);
				Rectangle sourceRect = new Rectangle(digit * digitWidth, 0, digitWidth, digitHeight);
				graphics.DrawImage(image, targetRect, sourceRect, GraphicsUnit.Pixel);
			}
			return imgOutput;
		}
	}
}