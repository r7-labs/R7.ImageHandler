//
// ImagePercentageTransform.cs
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

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace R7.ImageHandler
{
	public class ImagePercentageTransform : ImageTransformBase
	{
		/// <summary>
		/// Sets the percentage value for the radial indicator
		/// </summary>
		[DefaultValue("")]
		[Category("Behavior")]
		public int Percentage { get; set; }

		/// <summary>
		/// Sets the Color of the indicator element
		/// </summary>
		[DefaultValue("")]
		[Category("Behavior")]
		public Color Color { get; set; }

		public override string UniqueString
		{
			get { return base.UniqueString + this.Percentage.ToString() + "-" + this.Color.ToString(); }
		}

		public ImagePercentageTransform()
		{
			InterpolationMode = InterpolationMode.HighQualityBicubic;
			SmoothingMode = SmoothingMode.Default;
			PixelOffsetMode = PixelOffsetMode.Default;
			CompositingQuality = CompositingQuality.HighSpeed;
		}

		public override Image ProcessImage(Image image)
		{
			Bitmap bitmap = new Bitmap(100, 100);
			using (Graphics objGraphics = Graphics.FromImage(bitmap))
			{
				// Initialize graphics
				objGraphics.Clear(Color.White);
				objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
				objGraphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

				// Fill pie
				// Degrees are taken clockwise, 0 is parallel with x
				// For sweep angle we must convert percent to degrees (90/25 = 18/5)
				float startAngle = -90.0F;
				float sweepAngle = (18.0F/5)*Percentage;

				Rectangle rectangle = new Rectangle(5, 5, 90, 90);
				Brush colorBrush = new SolidBrush(Color);
				objGraphics.FillPie(colorBrush, rectangle, startAngle, sweepAngle);

				// Fill inner circle with white
				rectangle = new Rectangle(20, 20, 60, 60);
				objGraphics.FillEllipse(Brushes.White, rectangle);

				// Draw circles
				rectangle = new Rectangle(5, 5, 90, 90);
				objGraphics.DrawEllipse(Pens.LightGray, rectangle);
				rectangle = new Rectangle(20, 20, 60, 60);
				objGraphics.DrawEllipse(Pens.LightGray, rectangle);

				// Draw text on image
				// Use rectangle for text and align text to center of rectangle
				var font = new Font("Arial", 13, FontStyle.Bold);
				StringFormat stringFormat = new StringFormat();
				stringFormat.Alignment = StringAlignment.Center;
				stringFormat.LineAlignment = StringAlignment.Center;

				rectangle = new Rectangle(20, 40, 62, 20);
				objGraphics.DrawString(Percentage + "%", font, Brushes.DarkGray, rectangle, stringFormat);

				// Save indicator to file
				objGraphics.Flush();
			}
			return (Image) bitmap;
		}
	}
}
