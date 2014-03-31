//
// ImageBarcodeTransform.cs
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
using ZXing;
using ZXing.Common;

namespace R7.ImageHandler
{
	public class ImageBarcodeTransform : ImageTransformBase
	{
		/// <summary>
		/// Sets the barcode type 
        /// (upca,ean8,ean13,code39,code128,itf,codabar,plessey,msi,qrcode,pdf417,aztec,datamatrix)
		/// </summary>
		[DefaultValue("")]
		[Category("Behavior")]
		public string Type { get; set; }

        /// <summary>
        /// Sets the barcode content 
        /// (upca,ean8,ean13,code39,code128,itf,codabar,plessey,msi,qrcode,pdf417,aztec,datamatrix)
        /// </summary>
        [DefaultValue("")]
        [Category("Behavior")]
        public string Content { get; set; }

		/// <summary>
		/// Sets the Width of the generated barcode
		/// </summary>
		[DefaultValue("")]
		[Category("Behavior")]
		public int Width { get; set; }

        /// <summary>
        /// Sets the Height of the generated barcode
        /// </summary>
        [DefaultValue("")]
        [Category("Behavior")]
        public int Height { get; set; }

        /// <summary>
        /// Sets the Border Width (not pixels, depends on barcode type)
        /// </summary>
        [DefaultValue("")]
        [Category("Behavior")]
        public int Border { get; set; }

		public override string UniqueString
		{
			get { return base.UniqueString + this.Type + "-" + this.Width.ToString() + "-" + this.Height.ToString() + this.Content + "-" + this.Border.ToString(); }
		}

        public ImageBarcodeTransform()
		{
			InterpolationMode = InterpolationMode.HighQualityBicubic;
			SmoothingMode = SmoothingMode.Default;
			PixelOffsetMode = PixelOffsetMode.Default;
			CompositingQuality = CompositingQuality.HighSpeed;
		}

		public override Image ProcessImage(Image image)
		{
		    BarcodeWriter barcodeWriter = new BarcodeWriter();
		    switch (Type)
		    {
                case "upca":
                    barcodeWriter.Format = BarcodeFormat.UPC_A;
		            break;
                case "ean8":
                    barcodeWriter.Format = BarcodeFormat.EAN_8;
		            break;
                case "ean13":
                    barcodeWriter.Format = BarcodeFormat.EAN_13;
		            break;
                case "code39":
                    barcodeWriter.Format = BarcodeFormat.CODE_39;
		            break;
                case "code128":
                    barcodeWriter.Format = BarcodeFormat.CODE_128;
		            break;
                case "itf":
                    barcodeWriter.Format = BarcodeFormat.ITF;
		            break;
                case "codabar":
                    barcodeWriter.Format = BarcodeFormat.CODABAR;
		            break;
                case "plessey":
                    barcodeWriter.Format = BarcodeFormat.PLESSEY;
		            break;
                case "msi":
                    barcodeWriter.Format = BarcodeFormat.MSI;
		            break;
                case "qrcode":
                    barcodeWriter.Format = BarcodeFormat.QR_CODE;
		            break;
                case "pdf417":
                    barcodeWriter.Format = BarcodeFormat.PDF_417;
		            break;
                case "aztec":
                    barcodeWriter.Format = BarcodeFormat.AZTEC;
		            break;
                case "datamatrix":
                    barcodeWriter.Format = BarcodeFormat.DATA_MATRIX;
		            break;
		    }
		    barcodeWriter.Options = new EncodingOptions
		                            {
		                                Height = Height,
		                                Width = Width,
		                                Margin = Border
		                            };

		    Bitmap bitmap = barcodeWriter.Write(Content);
		    return (Image) bitmap;
		}
	}
}
