//
// GrayscaleQuantizer.cs
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
using System.Collections;
using System.Drawing;

namespace R7.ImageHandler
{
	/// <summary>
	/// Summary description for PaletteQuantizer.
	/// </summary>
	public  class GrayscaleQuantizer : PaletteQuantizer
	{
		/// <summary>
		/// Construct the palette quantizer
		/// </summary>
		/// <remarks>
		/// Palette quantization only requires a single quantization step
		/// </remarks>
		public GrayscaleQuantizer () : base( new ArrayList() )
		{
			_colors = new Color[256];

			var nColors = 256;

			// Initialize a new color table with entries that are determined
			// by some optimal palette-finding algorithm; for demonstration 
			// purposes, use a grayscale.
			for (uint i = 0; i < nColors; i++)
			{
				uint Alpha = 0xFF;                      // Colors are opaque.
				uint Intensity = Convert.ToUInt32(i*0xFF/(nColors-1));    // Even distribution. 

				// The GIF encoder makes the first entry in the palette
				// that has a ZERO alpha the transparent color in the GIF.
				// Pick the first one arbitrarily, for demonstration purposes.
    
				// Create a gray scale for demonstration purposes.
				// Otherwise, use your favorite color reduction algorithm
				// and an optimum palette for that algorithm generated here.
				// For example, a color histogram, or a median cut palette.
				_colors[i] = Color.FromArgb( (int)Alpha, 
					(int)Intensity, 
					(int)Intensity, 
					(int)Intensity );
			}
		}

		/// <summary>
		/// Override this to process the pixel in the second pass of the algorithm
		/// </summary>
		/// <param name="pixel">The pixel to quantize</param>
		/// <returns>The quantized value</returns>
		protected override byte QuantizePixel ( Color32 pixel )
		{
			byte colorIndex = 0 ;

			double luminance = (pixel.Red *0.299) + (pixel.Green *0.587) + (pixel.Blue  *0.114);

			// Gray scale is an intensity map from black to white.
			// Compute the index to the grayscale entry that
			// approximates the luminance, and then round the index.
			// Also, constrain the index choices by the number of
			// colors to do, and then set that pixel's index to the 
			// byte value.
			colorIndex = (byte)(luminance +0.5);

			return colorIndex ;
		}

	}
}
