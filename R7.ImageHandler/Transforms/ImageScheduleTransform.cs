//
// ImageScheduleTransform.cs
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
using System.Drawing.Imaging;
using System.Globalization;
using System.Threading;

namespace R7.ImageHandler
{
	public class ImageScheduleTransform : ImageTransformBase
	{
		[DefaultValue("")]
		[Category("Behavior")]
		public string Matrix { get; set; }

		[DefaultValue("")]
		[Category("Behavior")]
		public string Culture { get; set; }

		[DefaultValue("")]
		[Category("Behavior")]
		public Color BackColor { get; set; }

		public override string UniqueString
		{
			get { return base.UniqueString + this.Matrix; }
		}

		public ImageScheduleTransform()
		{
			InterpolationMode = InterpolationMode.HighQualityBicubic;
			SmoothingMode = SmoothingMode.Default;
			PixelOffsetMode = PixelOffsetMode.Default;
			CompositingQuality = CompositingQuality.HighSpeed;
		}

		public override Image ProcessImage(Image image)
		{
			Bitmap bmp = new Bitmap(486, 224, PixelFormat.Format32bppPArgb);

			SolidBrush emptyBrush = new SolidBrush(Color.FromArgb(204, 204, 204));
			SolidBrush captionBrush = new SolidBrush(Color.FromArgb(204, 204, 204));

			SolidBrush freeBrush = new SolidBrush(Color.FromArgb(1, 151, 0));
			SolidBrush reservedBrush = new SolidBrush(Color.FromArgb(255, 204, 0));
			SolidBrush occupiedBrush = new SolidBrush(Color.FromArgb(155, 0, 3));
			SolidBrush selectedBrush = new SolidBrush(Color.FromArgb(1,79,255));

			SolidBrush transBrush = new SolidBrush(Color.FromArgb(100, 255, 255, 255));

			using (var gr = Graphics.FromImage(bmp))
			{
				gr.Clear(BackColor);

				// Paint Month Name boxes
				int x = 1;
				int y = 2;
				for (int i = 0; i < 12; i++)
				{
					y += 17;
					gr.FillRectangle(captionBrush, x, y, 50, 16);
					using (Font drawFont = new Font("Arial", 6.5f))
					{
						Thread.CurrentThread.CurrentCulture = new CultureInfo(Culture);
						string month = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[i];
						StringFormat stringFormat = new StringFormat()
						{
							Alignment = StringAlignment.Far,
							LineAlignment = StringAlignment.Center
						};
						gr.DrawString(month, drawFont, new SolidBrush(Color.Black), new RectangleF(x, y, 48, 16),
							stringFormat);
					}

				}

				// Paint Day Number boxes
				x = 38;
				y = 1;
				for (int i = 0; i < 31; i++)
				{
					x += 14;
					gr.FillRectangle(captionBrush, x, y, 13, 16);
					using (Font drawFont = new Font("Arial", 6.5f))
					{
						string day = (i + 1).ToString().PadLeft(2, '0');
						StringFormat stringFormat = new StringFormat()
						{
							Alignment = StringAlignment.Center,
							LineAlignment = StringAlignment.Center
						};
						gr.DrawString(day, drawFont, new SolidBrush(Color.Black), new RectangleF(x, y, 13, 16),
							stringFormat);
					}

				}

				int[,] matrix = new int[12, 31];
				char[] chars = Matrix.ToCharArray();

				for (int month = 1; month < 13; month++)
				{
					for (int day = 1; day < 32; day++)
					{
						char value = chars[(month - 1)*31 + day - 1];
						matrix[month - 1, day - 1] = int.Parse(value.ToString());
					}
				}

				int yesterday = 1;

				x = 38;
				y = 2;
				for (int month = 0; month < 12; month++)
				{
					y += 17;
					for (int day = 0; day < 31; day++)
					{
						x += 14;
						Point[] firstHalf = {new Point(x, y), new Point(x + 13, y), new Point(x, y + 16)};
						Point[] lastHalf = {new Point(x + 13, y), new Point(x + 13, y + 16), new Point(x, y + 16)};

						int today = matrix[month, day];
						if ( today == 0)
						{
							gr.FillRectangle(emptyBrush, x, y, 13, 16);
						}
						else
						{
							switch (today)
							{
							case 1:
							case 6:
								gr.FillPolygon(freeBrush, lastHalf);
								break;
							case 2:
							case 7:
								gr.FillPolygon(reservedBrush, lastHalf);
								break;
							case 3:
							case 8:
								gr.FillPolygon(occupiedBrush, lastHalf);
								break;
							case 4:
							case 9:
								gr.FillPolygon(selectedBrush, lastHalf);
								break;
							}

							switch (yesterday)
							{
							case 1:
							case 6:
								gr.FillPolygon(freeBrush, firstHalf);
								break;
							case 2:
							case 7:
								gr.FillPolygon(reservedBrush, firstHalf);
								break;
							case 3:
							case 8:
								gr.FillPolygon(occupiedBrush, firstHalf);
								break;
							case 4:
							case 9:
								gr.FillPolygon(selectedBrush, firstHalf);
								break;

							}
							if (today > 4)
							{
							gr.FillRectangle(transBrush, x, y, 13, 16);
							}
							yesterday = matrix[month, day];
						}
					}
					x = 38;
				}

				return (Image) bmp;
			}
		}
	}
}