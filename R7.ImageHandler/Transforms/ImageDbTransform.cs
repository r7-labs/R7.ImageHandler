//
// ImageDbTransform.cs
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
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Web;
using Microsoft.ApplicationBlocks.Data;

namespace R7.ImageHandler
{
	public class ImageDbTransform : ImageTransformBase
	{
		/// <summary>
		/// Sets the Connectionstring Descriptor from web.config. Defaultvalue is empty
		/// </summary>
		[DefaultValue("")]
		[Category("Behavior")]
		public string ConnectionString { get; set; }

		/// <summary>
		/// Sets the Table to select from. Defaultvalue is empty
		/// </summary>
		[DefaultValue("")]
		[Category("Behavior")]
		public string Table { get; set; }

		/// <summary>
		/// Sets the ID Field name to select from. Defaultvalue is empty
		/// </summary>
		[DefaultValue("")]
		[Category("Behavior")]
		public string IdFieldName { get; set; }

		/// <summary>
		/// Sets the ID Field value to select from. Defaultvalue is empty
		/// </summary>
		[DefaultValue("")]
		[Category("Behavior")]
		public int IdFieldValue { get; set; }

        /// <summary>
        /// Sets the ID Field value to select from. Defaultvalue is empty
        /// </summary>
        [DefaultValue("")]
        [Category("Behavior")]
        public int UserId { get; set; }

        /// <summary>
        /// Sets the ID Field value to select from. Defaultvalue is empty
        /// </summary>
        [DefaultValue("")]
        [Category("Behavior")]
        public int PortalId { get; set; }

		/// <summary>
		/// Sets the Image Field name to select from. Defaultvalue is empty
		/// </summary>
		[DefaultValue("")]
		[Category("Behavior")]
		public string ImageFieldName { get; set; }

        /// <summary>
        /// Sets the empty Image
        /// </summary>
        [DefaultValue("")]
        [Category("Behavior")]
        public Image EmptyImage { get; set; }

		public override string UniqueString
		{
			get
			{
				return base.UniqueString + "-" + this.ConnectionString + "-" + this.Table + "-" +
                       this.IdFieldName + "-" + this.IdFieldValue + "-" + this.ImageFieldName + "-" + this.UserId.ToString() + this.PortalId.ToString();
			}
		}

		public ImageDbTransform()
		{
			InterpolationMode = InterpolationMode.HighQualityBicubic;
			SmoothingMode = SmoothingMode.Default;
			PixelOffsetMode = PixelOffsetMode.Default;
			CompositingQuality = CompositingQuality.HighSpeed;
		}

		public override Image ProcessImage(Image image)
		{
		    string sqlCmd;
            
			if (UserId > 0)
            {
                sqlCmd = "SELECT RTRIM(Files.Folder) + RTRIM(LTRIM(Files.FileName)) AS profilepic"+
                         " FROM UserProfile " +
                         " INNER JOIN ProfilePropertydefinition on ProfilePropertydefinition.PropertyDefinitionID = UserProfile.PropertyDefinitionID" +
                         " INNER JOIN Files on UserProfile.PropertyValue = Files.FileId" +
                         " WHERE ProfilePropertydefinition.Propertyname = 'Photo'" +
                         " AND ProfilePropertydefinition.PortalId = " + this.PortalId.ToString() +
                         " AND Userprofile.UserId = " + this.UserId.ToString();
                
                object result = SqlHelper.ExecuteScalar(this.ConnectionString, CommandType.Text, sqlCmd);
                if (result != null)
                {
                    string imgFile = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "Portals\\" + this.PortalId.ToString());
                    imgFile = Path.Combine(imgFile, ((string) result).Replace('/', '\\'));
                    if (File.Exists(imgFile) == true)
                    {
                        return new Bitmap(imgFile);
                    }
                 }
            }
            else
            {
                sqlCmd = "SELECT " + this.ImageFieldName + " FROM " +
                         this.Table + " WHERE " + this.IdFieldName + " = @Value";


		        object result = SqlHelper.ExecuteScalar(this.ConnectionString, CommandType.Text, sqlCmd, new SqlParameter("Value", this.IdFieldValue));
		        if (result != null)
		        {
		            MemoryStream ms = new MemoryStream((byte[]) result);
		            return Image.FromStream(ms);
		        }
		        
		    }
            return EmptyImage;
		}
	}
}
