﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Net;
using System.Data;

namespace Manina.Windows.Forms
{
    /// <summary>
    /// Represents the built-in adaptors.
    /// </summary>
    public static class ImageListViewItemAdaptors
    {
        #region FileSystemAdaptor
        /// <summary>
        /// Represents a file system adaptor.
        /// </summary>
        public class FileSystemAdaptor : ImageListView.ImageListViewItemAdaptor
        {
            private bool disposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="FileSystemAdaptor"/> class.
            /// </summary>
            public FileSystemAdaptor()
            {
                disposed = false;
            }

            /// <summary>
            /// Returns the thumbnail image for the given item.
            /// </summary>
            /// <param name="key">Item key.</param>
            /// <param name="size">Requested image size.</param>
            /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
            /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
            /// <param name="useWIC">true to use Windows Imaging Component; otherwise false.</param>
            /// <returns>The thumbnail image from the given item or null if an error occurs.</returns>
            public override Image GetThumbnail(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation, bool useWIC)
            {
                if (disposed)
                    return null;

                string filename = (string)key;
                if (File.Exists(filename))
                    return ThumbnailExtractor.FromFile(filename, size, useEmbeddedThumbnails, useExifOrientation, useWIC);
                else
                    return null;
            }
            /// <summary>
            /// Returns a unique identifier for this thumbnail to be used in persistent
            /// caching.
            /// </summary>
            /// <param name="key">Item key.</param>
            /// <param name="size">Requested image size.</param>
            /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
            /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
            /// <param name="useWIC">true to use Windows Imaging Component; otherwise false.</param>
            /// <returns>A unique identifier string for the thumnail.</returns>
            public override string GetUniqueIdentifier(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation, bool useWIC)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append((string)key.ToString());// Filename
                sb.Append(':');
                sb.Append(size.Width); // Thumbnail size
                sb.Append(',');
                sb.Append(size.Height);
                sb.Append(':');
                sb.Append(useEmbeddedThumbnails);
                sb.Append(':');
                sb.Append(useExifOrientation);
                sb.Append(':');
                sb.Append(useWIC);
                return sb.ToString();
            }
            /// <summary>
            /// Returns the path to the source image for use in drag operations.
            /// </summary>
            /// <param name="key">Item key.</param>
            /// <returns>The path to the source image.</returns>
            public override string GetSourceImage(object key)
            {
                if (disposed)
                    return null;

                string filename = (string)key;
                return filename;
            }
            /// <summary>
            /// Returns the details for the given item.
            /// </summary>
            /// <param name="key">Item key.</param>
            /// <param name="useWIC">true to use Windows Imaging Component; otherwise false.</param>
            /// <returns>An array of tuples containing item details or null if an error occurs.</returns>
            public override Utility.Tuple<ColumnType, string, object>[] GetDetails(object key, bool useWIC)
            {
                if (disposed)
                    return null;

                string filename = (string)key;
                List<Utility.Tuple<ColumnType, string, object>> details = new List<Utility.Tuple<ColumnType, string, object>>();

                return details.ToArray();
            }
            /// <summary>
            /// Performs application-defined tasks associated with freeing,
            /// releasing, or resetting unmanaged resources.
            /// </summary>
            public override void Dispose()
            {
                disposed = true;
            }
        }
        #endregion

        #region URIAdaptor
        /// <summary>
        /// Represents a URI adaptor.
        /// </summary>
        public class URIAdaptor : ImageListView.ImageListViewItemAdaptor
        {
            private bool disposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="URIAdaptor"/> class.
            /// </summary>
            public URIAdaptor()
            {
                disposed = false;
            }

            /// <summary>
            /// Returns the thumbnail image for the given item.
            /// </summary>
            /// <param name="key">Item key.</param>
            /// <param name="size">Requested image size.</param>
            /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
            /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
            /// <param name="useWIC">true to use Windows Imaging Component; otherwise false.</param>
            /// <returns>The thumbnail image from the given item or null if an error occurs.</returns>
            public override Image GetThumbnail(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation, bool useWIC)
            {
                if (disposed)
                    return null;

                string uri = (string)key;
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        byte[] imageData = client.DownloadData(uri);
                        using (MemoryStream stream = new MemoryStream(imageData))
                        {
                            using (Image sourceImage = Image.FromStream(stream))
                            {
                                return ThumbnailExtractor.FromImage(sourceImage, size, useEmbeddedThumbnails, useExifOrientation, useWIC);
                            }
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
            /// <summary>
            /// Returns a unique identifier for this thumbnail to be used in persistent
            /// caching.
            /// </summary>
            /// <param name="key">Item key.</param>
            /// <param name="size">Requested image size.</param>
            /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
            /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
            /// <param name="useWIC">true to use Windows Imaging Component; otherwise false.</param>
            /// <returns>A unique identifier string for the thumbnail.</returns>
            public override string GetUniqueIdentifier(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation, bool useWIC)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append((string)key);// Uri
                sb.Append(':');
                sb.Append(size.Width); // Thumbnail size
                sb.Append(',');
                sb.Append(size.Height);
                sb.Append(':');
                sb.Append(useEmbeddedThumbnails);
                sb.Append(':');
                sb.Append(useExifOrientation);
                sb.Append(':');
                sb.Append(useWIC);
                return sb.ToString();
            }
            /// <summary>
            /// Returns the path to the source image for use in drag operations.
            /// </summary>
            /// <param name="key">Item key.</param>
            /// <returns>The path to the source image.</returns>
            public override string GetSourceImage(object key)
            {
                if (disposed)
                    return null;

                string uri = (string)key;
                try
                {
                    string filename = Path.GetTempFileName();
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(uri, filename);
                        return filename;
                    }
                }
                catch
                {
                    return null;
                }
            }
            /// <summary>
            /// Returns the details for the given item.
            /// </summary>
            /// <param name="key">Item key.</param>
            /// <param name="useWIC">true to use Windows Imaging Component; otherwise false.</param>
            /// <returns>An array of 2-tuples containing item details or null if an error occurs.</returns>
            public override Utility.Tuple<ColumnType, string, object>[] GetDetails(object key, bool useWIC)
            {
                if (disposed)
                    return null;

                string uri = (string)key;
                List<Utility.Tuple<ColumnType, string, object>> details = new List<Utility.Tuple<ColumnType, string, object>>();

                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Custom, "URL", uri));

                return details.ToArray();
            }
            /// <summary>
            /// Performs application-defined tasks associated with freeing,
            /// releasing, or resetting unmanaged resources.
            /// </summary>
            public override void Dispose()
            {
                disposed = true;
            }
        }
        #endregion
    }
}
