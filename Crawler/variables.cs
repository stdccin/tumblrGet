﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;

namespace tumblrCrawler.MediaParse
{
    public partial class Parser
    {


        private object lockobj_archiveImgLink = new object();  //use lock for convient, avoid .net 4 specify ConcurrentBag
        private object lockobj_archiveVideoLink = new object();  //use lock for convient, avoid .net 4 specify ConcurrentBag
        private List<ArchivePageLinkStruct> lsArchivePageImgLink = new List<ArchivePageLinkStruct>();
        private List<ArchivePageLinkStruct> lsArchivePageVideoLink = new List<ArchivePageLinkStruct>();

        private List<MediaStruct> lsMediaStructVideos = new List<MediaStruct>();
        private List<MediaStruct> lsMediaStrutImgs = new List<MediaStruct>();

        #region  regex:--Img  relate--
        /// <summary>
        /// group[1]:  thumbnail jpg webaddress
        /// group[2]:  pic page address
        /// group[3]:  create date
        /// group[4]:  hot degree
        /// </summary>
        private static Regex regexArchiveImgItem = new Regex(regstrArchivePageLinkImgPage);
        /// <summary>
        /// group[4] is imgaddr
        /// this is for signleImg Style
        /// </summary>
        private static Regex regexImgPageToSingleImgLink = new Regex(regstrImgPageLinkToPicAddr);
        /// <summary>
        ///group[1]: link to  real Imgs Page. format is "/post/....."
        /// find the first Match
        /// </summary>
        private static Regex regexImgPageIframeLink = new Regex(regstrImgPageIframeLink);
        /// <summary>
        ///group[1]: the ImgAddr
        ///group[2]: thumbnial ImgAddr
        /// use foreach(matches) find all ImgAddr
        /// </summary>
        private static Regex regexIframeToMultiImgLink = new Regex(regstrIframeToMultiImgLink);
        #endregion
        #region Regex:--video relate--
        /// <summary>
        /// group[1]:  thumbnail video webaddress
        /// group[2]:  video page address
        /// group[3]:  create date
        /// group[4]:  hot degree
        /// </summary>
        private static Regex regexArchivePageVideoItem= new Regex(regstrArchivePageVideoItem);
        /// <summary>
        /// group[1]:  address of the Video Page
        /// find iframe src then use it to find the address of video page
        /// </summary>
        private static Regex regexVideoPageIframeLink = new Regex(regstrVideoPageiframeToVideoPage);
        /// <summary>
        /// group[1]:  address of the video file, use response head to identify the real address
        /// group[2]:  postfix of the file
        /// </summary>
        static Regex regexVideoiframeLinkToVideoAddr = new Regex(regstrVideoiframeLinkToVideoAddr);
        #endregion
        #region regex:--Pages--relate
        /// <summary>
        ///  match mainpage or archive homepage
        /// regstr: @"(^https?://\w+\.tumblr\.com/?$)|(^https?://\w+\.tumblr\.com/archive/?$)";
        /// </summary>
        static Regex regexHomePage = new Regex(regstrHomePage);
        /// <summary>
        ///regstr  @"^https?://\w+\.tumblr\.com/archive\?before_time=\d+$";
        /// </summary>
        static Regex regexAchivePage = new Regex(regstrAchivePages);
        /// <summary>
        /// group[1] next page link  start with "/archive/before....."
        /// </summary>
        static Regex regexNextPage = new Regex(regstrNextPage);
        #endregion
    }
}
