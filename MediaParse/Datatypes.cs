using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tumblrCrawler.MediaParse
{
    public partial class Parser
    {

        public enum MediaType { unknow = 0, Video = 1, Img = 2 };
        /// <summary>
        /// main page link struct contains link filetype,thumbnail
        /// </summary>
        public struct ArchivePageLinkStruct
        {
            public string PageLink;
            public string ThumbnailAddr;
            public int HotDegree;
            public DateTime CreateDate;
            public MediaType Type;
        }
        /// <summary>
        /// mediaStruct use for two types of files
        /// note hotdegree can createtime use for sets of Img
        /// </summary>
        public struct MediaStruct
        {
            public string ThumbnailAddr;
            public DateTime CreateDate;
            public int HotDegree;
            public MediaType Type;
            public string FileAddr;
            public string Parameter;
        }


    }
}
