// ImageListView - A listview control for image files
// Copyright (C) 2009 Ozgur Ozcitak
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Ozgur Ozcitak (ozcitak@yahoo.com)

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Drawing.Design;

namespace Manina.Windows.Forms
{
    /// <summary>
    /// Represents an item in the image list view.
    /// </summary>
    [TypeConverter(typeof(ImageListViewItemTypeConverter))]
    public class ImageListViewItem : ICloneable
    {
        #region Member Variables
        // Property backing fields
        private Color mBackColor;
        private Color mForeColor;
        internal int mIndex;
        private Guid mGuid;
        internal ImageListView mImageListView;
        internal bool mChecked;
        internal bool mSelected;
        internal bool mEnabled;
        private string mText;
        private int mZOrder;
        // File info
        // File info
        private string mName;
        private string mMediaType;
        private long mMediaSize;
        private float mFPS;
        private uint mMediaStartTime;
        private uint mMediaDuration;
        private string mMediaDurationStr;
        private string mFilePath;
        private Size mMediaInfo;
        private int mUsageCount;
        internal Image clonedThumbnail;
        // Adaptor
        internal object mVirtualItemKey;
        internal ImageListView.ImageListViewItemAdaptor mAdaptor;
        // Used for custom columns
        private Dictionary<Guid, string> subItems;
        // Group info
        internal string group;
        internal int groupOrder;

        internal ImageListView.ImageListViewItemCollection owner;
        internal bool isDirty;
        private bool editing;
        #endregion

        #region Properties
        [Category("Appearance"), Browsable(true), Description("Gets or sets the foreground color of the item."), DefaultValue(typeof(Color), "WindowText")]
        public Color ForeColor
        {
            get
            {
                return mForeColor;
            }
            set
            {
                if (value != mForeColor)
                {
                    mForeColor = value;
                    if (mImageListView != null)
                        mImageListView.Refresh();
                }
            }
        }
        [Category("Appearance"), Browsable(true), Description("Gets or sets the background color of the item."), DefaultValue(typeof(Color), "Transparent")]
        public Color BackColor
        {
            get
            {
                return mBackColor;
            }
            set
            {
                if (value != mBackColor)
                {
                    mBackColor = value;
                    if (mImageListView != null)
                        mImageListView.Refresh();
                }
            }
        }
        /// <summary>
        /// Gets the cache state of the item thumbnail.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets the cache state of the item thumbnail.")]
        public CacheState ThumbnailCacheState
        {
            get
            {
                return mImageListView.thumbnailCache.GetCacheState(mGuid, mImageListView.ThumbnailSize, mImageListView.UseEmbeddedThumbnails,
                    mImageListView.AutoRotateThumbnails, mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.ThumbnailsOnly);
            }
        }
        /// <summary>
        /// Gets a value determining if the item is focused.
        /// </summary>
        [Category("Appearance"), Browsable(false), Description("Gets a value determining if the item is focused."), DefaultValue(false)]
        public bool Focused
        {
            get
            {
                if (owner == null || owner.FocusedItem == null) return false;
                return (this == owner.FocusedItem);
            }
            set
            {
                if (owner != null)
                    owner.FocusedItem = this;
            }
        }
        /// <summary>
        /// Gets a value determining if the item is enabled.
        /// </summary>
        [Category("Appearance"), Browsable(true), Description("Gets a value determining if the item is enabled."), DefaultValue(true)]
        public bool Enabled
        {
            get
            {
                return mEnabled;
            }
            set
            {
                mEnabled = value;
                if (!mEnabled && mSelected)
                {
                    mSelected = false;
                    if (mImageListView != null)
                        mImageListView.OnSelectionChangedInternal();
                }
                if (mImageListView != null && mImageListView.IsItemVisible(mGuid))
                    mImageListView.Refresh();
            }
        }
        /// <summary>
        /// Gets the unique identifier for this item.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets the unique identifier for this item.")]
        internal Guid Guid { get { return mGuid; } private set { mGuid = value; } }
        /// <summary>
        /// Gets the adaptor of this item.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets the adaptor of this item.")]
        public ImageListView.ImageListViewItemAdaptor Adaptor { get { return mAdaptor; } }
        /// <summary>
        /// Gets the virtual item key associated with this item.
        /// Returns null if the item is not a virtual item.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets the virtual item key associated with this item.")]
        public object VirtualItemKey { get { return mVirtualItemKey; } }
        /// <summary>
        /// Gets the ImageListView owning this item.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets the ImageListView owning this item.")]
        public ImageListView ImageListView { get { return mImageListView; } private set { mImageListView = value; } }
        /// <summary>
        /// Gets the index of the item.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets the index of the item."), EditorBrowsable(EditorBrowsableState.Advanced)]
        public int Index { get { return mIndex; } }
        /// <summary>
        /// Gets or sets a value determining if the item is checked.
        /// </summary>
        [Category("Appearance"), Browsable(true), Description("Gets or sets a value determining if the item is checked."), DefaultValue(false)]
        public bool Checked
        {
            get
            {
                return mChecked;
            }
            set
            {
                if (value != mChecked)
                {
                    mChecked = value;
                    if (mImageListView != null)
                        mImageListView.OnItemCheckBoxClickInternal(this);
                }
            }
        }
        /// <summary>
        /// Gets or sets a value determining if the item is selected.
        /// </summary>
        [Category("Appearance"), Browsable(false), Description("Gets or sets a value determining if the item is selected."), DefaultValue(false)]
        public bool Selected
        {
            get
            {
                return mSelected;
            }
            set
            {
                if (value != mSelected && mEnabled)
                {
                    mSelected = value;
                    if (mImageListView != null)
                    {
                        mImageListView.OnSelectionChangedInternal();
                        if (mImageListView.IsItemVisible(mGuid))
                            mImageListView.Refresh();
                    }
                }
            }
        }
        /// <summary>
        /// Gets or sets the user-defined data associated with the item.
        /// </summary>
        [Category("Data"), Browsable(true), Description("Gets or sets the user-defined data associated with the item."), TypeConverter(typeof(StringConverter))]
        public object Tag { get; set; }
        /// <summary>
        /// Gets or sets the text associated with this item. If left blank, item Text 
        /// reverts to the name of the image file.
        /// </summary>
        [Category("Appearance"), Browsable(true), Description("Gets or sets the text associated with this item. If left blank, item Text reverts to the name of the image file.")]
        public string Text
        {
            get
            {
                return mText;
            }
            set
            {
                mText = value;
                if (mImageListView != null && mImageListView.IsItemVisible(mGuid))
                    mImageListView.Refresh();
            }
        }
        /// <summary>
        /// Gets or sets the name of the image file represented by this item.
        /// </summary>        
        [Category("File Properties"), Browsable(true), Description("Gets or sets the name of the image file represented by this item.")]
        [Editor(typeof(OpenFileDialogEditor), typeof(UITypeEditor))]
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("FileName cannot be null");

                if (mName != value)
                {
                    mName = value;
                    mVirtualItemKey = mName;

                    if (string.IsNullOrEmpty(mText))
                        mText = Path.GetFileName(mName);

                    isDirty = true;
                    if (mImageListView != null)
                    {
                        mImageListView.thumbnailCache.Remove(mGuid, true);
                        mImageListView.metadataCache.Remove(mGuid);
                        mImageListView.metadataCache.Add(mGuid, Adaptor, mName,
                            (mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.DetailsOnly));
                        if (mImageListView.IsItemVisible(mGuid))
                            mImageListView.Refresh();
                    }
                }
            }
        }
        /// <summary>
        /// Gets the thumbnail image. If the thumbnail image is not cached, it will be 
        /// added to the cache queue and null will be returned. The returned image needs
        /// to be disposed by the caller.
        /// </summary>
        [Category("Appearance"), Browsable(false), Description("Gets the thumbnail image.")]
        public Image ThumbnailImage
        {
            get
            {
                if (mImageListView == null)
                    throw new InvalidOperationException("Owner control is null.");

                if (ThumbnailCacheState != CacheState.Cached)
                {
                    mImageListView.thumbnailCache.Add(Guid, mAdaptor, mVirtualItemKey, mImageListView.ThumbnailSize,
                        mImageListView.UseEmbeddedThumbnails, mImageListView.AutoRotateThumbnails,
                        (mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.ThumbnailsOnly));
                }

                return mImageListView.thumbnailCache.GetImage(Guid, mAdaptor, mVirtualItemKey, mImageListView.ThumbnailSize, mImageListView.UseEmbeddedThumbnails,
                    mImageListView.AutoRotateThumbnails, mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.ThumbnailsOnly, true);
            }
        }
        /// <summary>
        /// Gets or sets the draw order of the item.
        /// </summary>
        [Category("Appearance"), Browsable(true), Description("Gets or sets the draw order of the item."), DefaultValue(0)]
        public int ZOrder { get { return mZOrder; } set { mZOrder = value; } }
        #endregion

        #region Shell Properties
        /// <summary>
        /// Gets the small shell icon of the image file represented by this item.
        /// If the icon image is not cached, it will be added to the cache queue and null will be returned.
        /// </summary>
        [Category("Appearance"), Browsable(false), Description("Gets the small shell icon of the image file represented by this item.")]
        public Image SmallIcon
        {
            get
            {
                if (mImageListView == null)
                    throw new InvalidOperationException("Owner control is null.");

                string iconPath = null;
                CacheState state = mImageListView.shellInfoCache.GetCacheState(iconPath);
                if (state == CacheState.Cached)
                {
                    return mImageListView.shellInfoCache.GetSmallIcon(iconPath);
                }
                else if (state == CacheState.Error)
                {
                    if (mImageListView.RetryOnError)
                    {
                        mImageListView.shellInfoCache.Remove(iconPath);
                        mImageListView.shellInfoCache.Add(iconPath);
                    }
                    return null;
                }
                else
                {
                    mImageListView.shellInfoCache.Add(iconPath);
                    return null;
                }
            }
        }
        /// <summary>
        /// Gets the large shell icon of the image file represented by this item.
        /// If the icon image is not cached, it will be added to the cache queue and null will be returned.
        /// </summary>
        [Category("Appearance"), Browsable(false), Description("Gets the large shell icon of the image file represented by this item.")]
        public Image LargeIcon
        {
            get
            {
                if (mImageListView == null)
                    throw new InvalidOperationException("Owner control is null.");

                string iconPath = null;
                CacheState state = mImageListView.shellInfoCache.GetCacheState(iconPath);
                if (state == CacheState.Cached)
                {
                    return mImageListView.shellInfoCache.GetLargeIcon(iconPath);
                }
                else if (state == CacheState.Error)
                {
                    if (mImageListView.RetryOnError)
                    {
                        mImageListView.shellInfoCache.Remove(iconPath);
                        mImageListView.shellInfoCache.Add(iconPath);
                    }
                    return null;
                }
                else
                {
                    mImageListView.shellInfoCache.Add(iconPath);
                    return null;
                }
            }
        }

        [Category("Data"), Browsable(false), Description("Gets the shell type of the image file represented by this item.")]
        public string MediaType { get { UpdateFileInfo(); return mMediaType; } set { mMediaType = value; } }        
        [Category("Data"), Browsable(false), Description("Gets the path of the image fie represented by this item.")]
        public string FilePath { get { UpdateFileInfo(); return mFilePath; } set { mFilePath = value; } }
        [Category("Data"), Browsable(false), Description("Gets file size in bytes.")]
        public long MediaSize { get { UpdateFileInfo(); return mMediaSize; } set { mMediaSize = value; } }
        [Category("Data"), Browsable(false), Description("Gets file size in bytes.")]
        public float FPS { get { UpdateFileInfo(); return mFPS; } set { mFPS = value; } }
        [Category("Data"), Browsable(false), Description("Gets file size in bytes.")]
        public uint MediaStartTime { get { UpdateFileInfo(); return mMediaStartTime; } set { mMediaStartTime = value; } }
        [Category("Data"), Browsable(false), Description("Gets file size in bytes.")]
        public uint MediaDuration { get { UpdateFileInfo(); return mMediaDuration; } set { mMediaDuration = value; } }
        [Category("Data"), Browsable(false), Description("Gets file size in bytes.")]
        public string MediaDurationStr { get { mMediaDurationStr = TimeSpan.FromMilliseconds(mMediaDuration).ToString(@"hh\:mm\:ss\.ff"); return mMediaDurationStr; } }
        [Category("Data"), Browsable(false), Description("Gets file size in bytes.")]
        public Size MediaInfo { get { UpdateFileInfo(); return mMediaInfo; } set { mMediaInfo = value; } }
        [Category("Data"), Browsable(false), Description("사용 빈도수.")]
        public int UsageCount { get { UpdateFileInfo(); return mUsageCount; } set { mUsageCount = value; } }

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageListViewItem"/> class.
        /// </summary>
        public ImageListViewItem()
        {
            mBackColor = Color.Transparent;
            mForeColor = SystemColors.WindowText;

            mIndex = -1;
            owner = null;

            mZOrder = 0;

            Guid = Guid.NewGuid();
            ImageListView = null;
            Checked = false;
            Selected = false;
            Enabled = true;

            isDirty = true;
            editing = false;

            mVirtualItemKey = null;

            Tag = null;

            subItems = new Dictionary<Guid, string>();

            groupOrder = 0;
            group = string.Empty;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageListViewItem"/> class.
        /// </summary>
        /// <param name="filename">The image filename representing the item.</param>
        public ImageListViewItem(string filename)
            : this(filename, string.Empty)
        {
            ;
        }
        /// <summary>
        /// Initializes a new instance of a virtual <see cref="ImageListViewItem"/> class.
        /// </summary>
        /// <param name="key">The key identifying this item.</param>
        /// <param name="text">Text of this item.</param>
        public ImageListViewItem(object key, string text)
            : this()
        {
            mVirtualItemKey = key;
            mText = text;
        }
        /// <summary>
        /// Initializes a new instance of a virtual <see cref="ImageListViewItem"/> class.
        /// </summary>
        /// <param name="key">The key identifying this item.</param>
        public ImageListViewItem(object key)
            : this(key, string.Empty)
        {
            ;
        }
        #endregion

        #region Instance Methods
        /// <summary>
        /// Begins editing the item.
        /// This method must be used while editing the item
        /// to prevent collisions with the cache manager.
        /// </summary>
        public void BeginEdit()
        {
            if (editing == true)
                throw new InvalidOperationException("Already editing this item.");

            if (mImageListView == null)
                throw new InvalidOperationException("Owner control is null.");

            mImageListView.thumbnailCache.BeginItemEdit(mGuid);
            mImageListView.metadataCache.BeginItemEdit(mGuid);

            editing = true;
        }
        /// <summary>
        /// Ends editing and updates the item.
        /// </summary>
        /// <param name="update">If set to true, the item will be immediately updated.</param>
        public void EndEdit(bool update)
        {
            if (editing == false)
                throw new InvalidOperationException("This item is not being edited.");

            if (mImageListView == null)
                throw new InvalidOperationException("Owner control is null.");

            mImageListView.thumbnailCache.EndItemEdit(mGuid);
            mImageListView.metadataCache.EndItemEdit(mGuid);

            editing = false;
            if (update) Update();
        }
        /// <summary>
        /// Ends editing and updates the item.
        /// </summary>
        public void EndEdit()
        {
            EndEdit(true);
        }
        /// <summary>
        /// Updates item thumbnail and item details.
        /// </summary>
        public void Update()
        {
            isDirty = true;
            if (mImageListView != null)
            {
                mImageListView.thumbnailCache.Remove(mGuid, true);
                mImageListView.metadataCache.Remove(mGuid);
                mImageListView.metadataCache.Add(mGuid, mAdaptor, mVirtualItemKey,
                    (mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.DetailsOnly));
                mImageListView.Refresh();
            }
        }
        /// <summary>
        /// Returns the sub item item text corresponding to the custom column with the given index.
        /// </summary>
        /// <param name="index">Index of the custom column.</param>
        /// <returns>Sub item text text for the given custom column type.</returns>
        public string GetSubItemText(int index)
        {
            int i = 0;
            foreach (string val in subItems.Values)
            {
                if (i == index)
                    return val;
                i++;
            }

            throw new IndexOutOfRangeException();
        }
        /// <summary>
        /// Sets the sub item item text corresponding to the custom column with the given index.
        /// </summary>
        /// <param name="index">Index of the custom column.</param>
        /// <param name="text">New sub item text</param>
        public void SetSubItemText(int index, string text)
        {
            int i = 0;
            Guid found = Guid.Empty;
            foreach (Guid guid in subItems.Keys)
            {
                if (i == index)
                {
                    found = guid;
                    break;
                }

                i++;
            }

            if (found != Guid.Empty)
            {
                subItems[found] = text;
                if (mImageListView != null && mImageListView.IsItemVisible(mGuid))
                    mImageListView.Refresh();
            }
            else
                throw new IndexOutOfRangeException();
        }
        /// <summary>
        /// Returns the sub item item text corresponding to the specified column type.
        /// </summary>
        /// <param name="type">The type of information to return.</param>
        /// <returns>Formatted text for the given column type.</returns>
        public string GetSubItemText(ColumnType type)
        {
            switch (type)
            {
                case ColumnType.UsageCount:
                    return mUsageCount.ToString();
                case ColumnType.Name:
                    return Name;
                case ColumnType.FilePath:
                    return FilePath;
                case ColumnType.MediaSize:
                    if (MediaSize == 0)
                        return "";
                    else
                        return Utility.FormatSize(MediaSize);
                case ColumnType.MediaType:
                    return MediaType;
                case ColumnType.MediaInfo:
                    if (MediaInfo == SizeF.Empty)
                        return "";
                    else
                        return string.Format("{0} x {1}", MediaInfo.Width, MediaInfo.Height);
                case ColumnType.FPS:
                    if (FPS == 0.0f)
                        return "";
                    else
                        return FPS.ToString("f2");
                case ColumnType.MediaStartTime:
                    if (MediaStartTime == 0)
                        return "";
                    else
                        return MediaStartTime.ToString();
                case ColumnType.MediaDuration:
                    if (MediaDuration == 0)
                        return "";
                    else
                        return MediaDuration.ToString();
                case ColumnType.MediaDurationStr:
                    if (MediaDuration == 0)
                        return "";
                    else
                        return MediaDurationStr;
                default:
                    throw new ArgumentException("Unknown column type", "type");
            }
        }
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(mText))
                return mText;
            else if (!string.IsNullOrEmpty(mFilePath))
                return Path.GetFileName(mFilePath);
            else
                return string.Format("Item {0}", mIndex);
        }
        #endregion

        #region Helper Methods        
        /// <summary>
        /// Gets an image from the cache manager.
        /// If the thumbnail image is not cached, it will be 
        /// added to the cache queue and DefaultImage of the owner image list view will
        /// be returned. If the thumbnail could not be cached ErrorImage of the owner
        /// image list view will be returned.
        /// </summary>
        /// <param name="imageType">Type of cached image to return.</param>
        /// <returns>Requested thumbnail or icon.</returns>
        public Image GetCachedImage(CachedImageType imageType)
        {
            if (mImageListView == null)
                throw new InvalidOperationException("Owner control is null.");

            string iconPath = null;

            if (imageType == CachedImageType.SmallIcon || imageType == CachedImageType.LargeIcon)
            {
                if (string.IsNullOrEmpty(iconPath))
                    return mImageListView.DefaultImage;

                CacheState state = mImageListView.shellInfoCache.GetCacheState(iconPath);
                if (state == CacheState.Cached)
                {
                    if (imageType == CachedImageType.SmallIcon)
                        return mImageListView.shellInfoCache.GetSmallIcon(iconPath);
                    else
                        return mImageListView.shellInfoCache.GetLargeIcon(iconPath);
                }
                else if (state == CacheState.Error)
                {
                    if (mImageListView.RetryOnError)
                    {
                        mImageListView.shellInfoCache.Remove(iconPath);
                        mImageListView.shellInfoCache.Add(iconPath);
                    }
                    return mImageListView.ErrorImage;
                }
                else
                {
                    mImageListView.shellInfoCache.Add(iconPath);
                    return mImageListView.DefaultImage;
                }
            }
            else
            {
                Image img = null;
                CacheState state = ThumbnailCacheState;

                if (state == CacheState.Error)
                {
                    if (mImageListView.ShellIconFallback && !string.IsNullOrEmpty(iconPath))
                    {
                        CacheState iconstate = mImageListView.shellInfoCache.GetCacheState(iconPath);
                        if (iconstate == CacheState.Cached)
                        {
                            if (mImageListView.ThumbnailSize.Width > 32 && mImageListView.ThumbnailSize.Height > 32)
                                img = mImageListView.shellInfoCache.GetLargeIcon(iconPath);
                            else
                                img = mImageListView.shellInfoCache.GetSmallIcon(iconPath);
                        }
                        else if (iconstate == CacheState.Error)
                        {
                            if (mImageListView.RetryOnError)
                            {
                                mImageListView.shellInfoCache.Remove(iconPath);
                                mImageListView.shellInfoCache.Add(iconPath);
                            }
                        }
                        else
                        {
                            mImageListView.shellInfoCache.Add(iconPath);
                        }
                    }

                    if (img == null)
                        img = mImageListView.ErrorImage;
                    return img;
                }

                img = mImageListView.thumbnailCache.GetImage(Guid, mAdaptor, mVirtualItemKey, mImageListView.ThumbnailSize, mImageListView.UseEmbeddedThumbnails,
                    mImageListView.AutoRotateThumbnails, mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.ThumbnailsOnly, false);

                if (state == CacheState.Cached)
                    return img;

                mImageListView.thumbnailCache.Add(Guid, mAdaptor, mVirtualItemKey, mImageListView.ThumbnailSize,
                    mImageListView.UseEmbeddedThumbnails, mImageListView.AutoRotateThumbnails,
                    (mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.ThumbnailsOnly));

                if (img == null && string.IsNullOrEmpty(iconPath))
                    return mImageListView.DefaultImage;

                if (img == null && mImageListView.ShellIconFallback && mImageListView.ThumbnailSize.Width > 16 && mImageListView.ThumbnailSize.Height > 16)
                    img = mImageListView.shellInfoCache.GetLargeIcon(iconPath);
                if (img == null && mImageListView.ShellIconFallback)
                    img = mImageListView.shellInfoCache.GetSmallIcon(iconPath);
                if (img == null)
                    img = mImageListView.DefaultImage;

                return img;
            }
        }
        /// <summary>
        /// Adds a new subitem for the specified custom column.
        /// </summary>
        /// <param name="guid">The Guid of the custom column.</param>
        internal void AddSubItemText(Guid guid)
        {
            subItems.Add(guid, "");
        }
        /// <summary>
        /// Returns the sub item item text corresponding to the specified custom column.
        /// </summary>
        /// <param name="guid">The Guid of the custom column.</param>
        /// <returns>Formatted text for the given column.</returns>
        internal string GetSubItemText(Guid guid)
        {
            return subItems[guid];
        }
        /// <summary>
        /// Removes the sub item item text corresponding to the specified custom column.
        /// </summary>
        /// <param name="guid">The Guid of the custom column.</param>
        /// <returns>true if the item was removed; otherwise false.</returns>
        internal bool RemoveSubItemText(Guid guid)
        {
            return subItems.Remove(guid);
        }
        /// <summary>
        /// Removes all sub item item texts.
        /// </summary>
        internal void RemoveAllSubItemTexts()
        {
            subItems.Clear();
        }
        /// <summary>
        /// Updates file info for the image file represented by this item.
        /// Item details will be updated synchronously without waiting for the
        /// cache thread.
        /// </summary>
        private void UpdateFileInfo()
        {
            if (!isDirty) return;
        }
        /// <summary>
        /// Invoked by the worker thread to update item details.
        /// </summary>
        /// <param name="info">Item details.</param>
        internal void UpdateDetailsInternal(Utility.Tuple<ColumnType, string, object>[] info)
        {
            if (!isDirty) return;

            // File info
            foreach (Utility.Tuple<ColumnType, string, object> item in info)
            {
                switch (item.Item1)
                {
                    case ColumnType.UsageCount:
                        mUsageCount = (int)item.Item3;
                        break;
                    case ColumnType.Name:
                        mName = (string)item.Item3;
                        break;
                    case ColumnType.MediaType:
                        mMediaType = (string)item.Item3;
                        break;
                    case ColumnType.MediaDuration:
                        mMediaDuration = (uint)item.Item3;
                        break;
                    case ColumnType.FilePath:
                        mFilePath = (string)item.Item3;
                        break;
                    case ColumnType.MediaDurationStr:
                        mMediaDurationStr = (string)item.Item3;
                        break;
                    case ColumnType.FPS:
                        mFPS = (float)item.Item3;
                        break;
                    case ColumnType.MediaInfo:
                        mMediaInfo = (Size)item.Item3;
                        break;
                    case ColumnType.MediaSize:
                        mMediaSize = (long)item.Item3;
                        break;
                    case ColumnType.Custom:
                        string label = item.Item2;
                        string value = (string)item.Item3;
                        Guid columnID = Guid.Empty;
                        foreach (ImageListView.ImageListViewColumnHeader column in mImageListView.Columns)
                        {
                            if (label == column.Text)
                                columnID = column.Guid;
                        }
                        if (columnID == Guid.Empty)
                        {
                            ImageListView.ImageListViewColumnHeader column = new ImageListView.ImageListViewColumnHeader(ColumnType.Custom, label);
                            columnID = column.Guid;
                        }
                        if (subItems.ContainsKey(columnID))
                            subItems[columnID] = value;
                        else
                            subItems.Add(columnID, value);
                        break;
                    default:
                        throw new Exception("Unknown column type.");
                }
            }

            isDirty = false;
        }
        /// <summary>
        /// Updates group order and name of the item.
        /// </summary>
        /// <param name="column">The group column.</param>
        internal void UpdateGroup(ImageListView.ImageListViewColumnHeader column)
        {
            if (column == null)
            {
                groupOrder = 0;
                group = string.Empty;
                return;
            }

            Utility.Tuple<int, string> groupInfo = new Utility.Tuple<int, string>(0, string.Empty);

            switch (column.Type)
            {
                case ColumnType.Name:
                    groupInfo = Utility.GroupTextAlpha(Name);
                    break;
                case ColumnType.FilePath:
                    groupInfo = Utility.GroupTextAlpha(FilePath);
                    break;
                case ColumnType.MediaType:
                    groupInfo = Utility.GroupTextAlpha(mMediaType);
                    break;                
                case ColumnType.Custom:
                    groupInfo = Utility.GroupTextAlpha(GetSubItemText(column.Guid));
                    break;
                case ColumnType.MediaInfo:
                    groupInfo = new Utility.Tuple<int, string>((int)mMediaInfo.Width, mMediaInfo.Width.ToString());
                    break;
                default:
                    groupInfo = new Utility.Tuple<int, string>(0, "Unknown");
                    break;
            }

            groupOrder = groupInfo.Item1;
            group = groupInfo.Item2;
        }        
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            ImageListViewItem item = new ImageListViewItem();

            item.mText = mText;

            // File info
            item.Name = mName;
            item.FilePath = mFilePath;
            item.FPS = mFPS;
            item.MediaDuration = mMediaDuration;
            item.MediaType = mMediaType;
            item.MediaSize = mMediaSize;
            item.mFilePath = mFilePath;
            item.MediaInfo = mMediaInfo;

            // Virtual item properties
            item.mAdaptor = mAdaptor;
            item.mVirtualItemKey = mVirtualItemKey;

            // Sub items
            foreach (KeyValuePair<Guid, string> kv in subItems)
                item.subItems.Add(kv.Key, kv.Value);

            // Current thumbnail
            if (mImageListView != null)
            {
                item.clonedThumbnail = mImageListView.thumbnailCache.GetImage(Guid, mAdaptor, mVirtualItemKey, mImageListView.ThumbnailSize,
                    mImageListView.UseEmbeddedThumbnails, mImageListView.AutoRotateThumbnails,
                    mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.ThumbnailsOnly, true);
            }

            return item;
        }
        #endregion
    }
}
