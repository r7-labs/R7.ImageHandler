//
// ImageTransformBase.cs
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

namespace R7.ImageHandler
{
    /// <summary>
    /// An abstract ImageTransform class
    /// </summary>
    public abstract class ImageTransformBase {

		/// <summary>
		/// Sets the interpolation mode used for resizing images. The default is HighQualityBicubic.
		/// </summary>
		[Category("Behavior")]
		public InterpolationMode InterpolationMode { get; set; }

		/// <summary>
		/// Sets the smoothing mode used for resizing images. The default is Default.
		/// </summary>
		[Category("Behavior")]
		public SmoothingMode SmoothingMode { get; set; }

		/// <summary>
		/// Sets the pixel offset mode used for resizing images. The default is Default.
		/// </summary>
		[Category("Behavior")]
		public PixelOffsetMode PixelOffsetMode { get; set; }

		/// <summary>
		/// Sets the compositing quality used for resizing images. The default is HighSpeed.
		/// </summary>
		[Category("Behavior")]
		public CompositingQuality CompositingQuality { get; set; }

		public abstract Image ProcessImage(Image image);
        
        // REVIEW: should this property be abstract?
        [Browsable(false)]
        public virtual string UniqueString {
            get {
                return GetType().FullName;
            }
        }
    }
}
