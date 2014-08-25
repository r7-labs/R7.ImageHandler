//#define INDIVIDUAL_LOCKS
//
// DiskImageStore.cs
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
using System.IO;
using System.Web;
using System.Web.Hosting;

#if INDIVIDUAL_LOCKS
using System.Collections;
#endif
namespace R7.ImageHandler
{
	public class DiskImageStore : IImageStore
	{
		private const string tmpFileExtension = ".tmp";
		private static DiskImageStore instance;
		private static object instanceLock = new object ();

		#if INDIVIDUAL_LOCKS
        private Hashtable _fileLocks = new Hashtable();
		#else
		private object _fileLock = new object ();
		#endif

		private static string cachePath = null;

		public static string CachePath
		{
			get 
			{	
				if (cachePath == null)
					// map app relative path
					cachePath = HostingEnvironment.MapPath (ImageHandlerSettings.Instance.ServerCachePath);
				return cachePath; 
			}
		}

		public static TimeSpan CacheExpiration
		{
			get { return ImageHandlerSettings.Instance.ServerCacheExpiration; }
		}

		static DiskImageStore ()
		{
		}

		internal DiskImageStore ()
		{
			// REVIEW: temporary hack to get unit tests running
			if (CachePath != null && !Directory.Exists (CachePath))
			{
				Directory.CreateDirectory (CachePath);
			}
		}

		internal static IImageStore Instance
		{
			get
			{
				if (instance == null)
				{
					lock (instanceLock)
					{
						if (instance == null)
							instance = new DiskImageStore ();
					}
				}
				return instance;
			}
		}

		private void Add (string id, byte[] data, bool localFile)
		{
			var path = CachePath + id;

			if (!localFile)
				path += "_" + (ImageHandlerSettings.Instance.Now + CacheExpiration).ToFileTime () + tmpFileExtension;
			else 
				path += tmpFileExtension;

			lock (GetFileLockObject(id))
			{
				try
				{
					File.WriteAllBytes (path, data);
				}
				catch (Exception)
				{
					// TODO: Log error about cache write
				}
			}
		}

		private bool TryTransmitIfContains (string id, HttpResponseBase response, string imgFile)
		{
			var tmpFiles = Directory.GetFiles (CachePath, id + "_*" + tmpFileExtension);

			// check if we have cache files
			if (tmpFiles.Length > 0)
			{
				// TODO: Log warning about duplicate cache entries

				// we expect to find just one file for each ID
				var tmpFile = tmpFiles [0];

				lock (GetFileLockObject(tmpFile))
				{
					// get tmp short filename without extension
					var tmpFileName = Path.GetFileNameWithoutExtension (Path.GetFileName (tmpFile));
					// extract expire time

					var expireTime = DateTime.MinValue;
					var timeIndex = tmpFileName.LastIndexOf ("_") + 1;

					if (timeIndex > 0)
						expireTime = DateTime.FromFileTime (
							Convert.ToInt64(tmpFileName.Substring (timeIndex)));

					var hitCache = false;

					if (imgFile == "")
					{
						// check if cache is expired
						hitCache = expireTime >= DateTime.Now;
					}
					else if (timeIndex == 0)
					{
						// use original file last write time
						var imgFileInfo = new FileInfo (imgFile);
						var tmpFileInfo = new FileInfo (tmpFile);

						hitCache = tmpFileInfo.LastWriteTime > imgFileInfo.LastWriteTime;
					}

					if (hitCache)
					{
						response.TransmitFile (tmpFile);
						return true;
					}
					else
					{
						File.Delete (tmpFile);
						return false;
					}
				}
			}
			return false;
		}

		private object GetFileLockObject (string id)
		{
			#if INDIVIDUAL_LOCKS
            object lockObject = _fileLocks[id];

            if (lockObject == null) {
                // lock on the hashtable to prevent other writers
                lock (_fileLocks) {
                    lockObject = new object();
                    _fileLocks[id] = lockObject;
                }
            }

            return lockObject;
			#else
			return _fileLock;
			#endif
		}
		#if INDIVIDUAL_LOCKS
        private static string GetEntryId(FileInfo fileinfo) {
            string id = fileinfo.Name.Substring(0, fileinfo.Name.Length - s_tempFileExtension.Length);
            return id;
        }

        private void DiscardFileLockObject(string id) {
            // lock on hashtable to prevent other writers
            lock (_fileLocks) {
                _fileLocks.Remove(id);
            }
        }
		#endif

		#region IImageStore implementation

		void IImageStore.Add (string id, byte[] data, bool localFile)
		{
			this.Add (id, data, localFile);
		}

		bool IImageStore.TryTransmitIfContains (string id, HttpResponseBase response, string imgFile)
		{
			return this.TryTransmitIfContains (id, response, imgFile);
		}

		#endregion

	} // class
} // namespace
