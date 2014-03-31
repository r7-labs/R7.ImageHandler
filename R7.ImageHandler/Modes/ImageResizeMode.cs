//
// ImageResizeMode.cs
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

namespace R7.ImageHandler
{
	public enum ImageResizeMode
	{
		/// <summary>
		/// Fit mode maintains the aspect ratio of the original image while ensuring that the dimensions of the result
		/// do not exceed the maximum values for the resize transformation.
		/// </summary>
		Fit,
		/// <summary>
		/// Crop resizes the image and removes parts of it to ensure that the dimensions of the result are exactly 
		/// as specified by the transformation.
		/// </summary>
		Crop,
		/// <summary>
		/// Resizes the image with the given width or height and maintains the aspect ratio. The image will be centered in a 
		/// square area of the chosen background color
		/// </summary>
		FitSquare,
		// TODO: Realize Stretch image resize mode 
		Stretch
	}
}