using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tumblrCrawler.MediaParse
{
    public partial class Parser
    {
        #region regstr  --Img--
        /// <summary>
        /// group[1] is thumbnail address
        /// group[2] is pic page address
        /// group[3] is create date
        /// group[4] is hot degree
        /// </summary>
        static string regstrArchivePageLinkImgPage = "<div class=\"post post_micro is_photo.+data-imageurl=\"(https?://.+?)\">.+?href=\"(https?://.+?)\".+<span class=\"post_date\">\\r?\\n\\s+(.+?)\\s+</span><span class=\"post_notes\"\\r?\\n.*data-notes=\"(\\d+)\">";
        /// <summary>
        /// group[1] is the address of Img 
        /// note the \\r?\\n  for unix and windows compatibility
        /// </summary>
        static string regstrImgPageLinkToPicAddr = "<a href=\"https?://.+?\">\\r?\\n\\s+<img src=\"(https?://.+?)\"";
        /// <summary>
        /// group[1]: link to  real Imgs Page. format is "/post/....." 
        /// </summary>
        static string regstrImgPageIframeLink = "src=\"(/post/.+photoset_iframe.+)\"\\r?\\n\\s+></iframe></div>";
        /// <summary>
        ///group[1]: the ImgAddr
        ///group[2]: thumbnial ImgAddr
        /// </summary>
        static string regstrIframeToMultiImgLink = "<a\\r?\\n\\s+href=\"(.+?)\" class=\"photoset_photo.+><img\\r?\\n.+\\r?\\n.+\\r?\\n.+\\r?\\n\\s+src=\"(.+?)\"";
        #endregion
        #region  regstr  --Video---
        /// <summary>
        /// group[1]: thumbnail video webaddress
        /// group[2]: video page address
        /// group[3]: create date
        /// group[4]: hot degree
        /// </summary>
        static string regstrArchivePageVideoItem = "<div class=\"post post_micro is_video.+data-imageurl=\"(https?://.+?)\">.+?href=\"(https?://.+?)\".+<span class=\"post_date\">\\r?\\n\\s+(.+?)\\s+</span><span class=\"post_notes\"\\r?\\n.*data-notes=\"(\\d+)\">";
        /// <summary>
        /// group[1] is the address of the Video Page
        /// find iframe src then use it to find the address
        /// </summary>
        static string regstrVideoPageiframeToVideoPage = "<iframe src=['\"](https?://.+?)['\"]";
        /// <summary>
        /// group[1] is the real address of the file  
        /// group[2] is the postfix of the file
        /// </summary>
        static string RegStrVideoiframeLinkToVideoAddr = "<source src=\"(https?://.+?)\" type=\"video/(.+?)\"";
        #endregion
        #region  regstr  --Page--
        static string regstrHomePage = @"(^https?://\w+\.tumblr\.com/?$)|(^https?://\w+\.tumblr\.com/archive/?$)";
        static string regstrAchivePages = @"^https?://\w+\.tumblr\.com/archive\?before_time=\d+$";
        static string  regstrNextPage=
                        "<a id=\"next_page_link\" href=\"(/archive\\?before_time=\\d+)\">((Next page)|(下一页)) &rarr;</a>";
        #endregion
        #region webclient 
        static int webclientWebExceptionMaxRetry = 3;

        #endregion


    }
}
