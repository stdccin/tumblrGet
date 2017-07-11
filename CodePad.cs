using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
namespace tumblrCrawler
{
    public partial class CodePad
    {
        /// <summary>
        /// group[1] is thumbnail jpg webaddress
        /// group[2] is pic page address
        /// group[3] is create date
        /// group[4] is hot degree
        /// </summary>
        static string regPicPageStr = "<div class=\"post post_micro is_photo.+data-imageurl=\"(https?://.+?)\">.+?href=\"(https?://.+?)\".+<span class=\"post_date\">\\r?\\n\\s+(.+?)\\s+</span><span class=\"post_notes\"\\r?\\n.*data-notes=\"(\\d+)\">";
        /// <summary>
        /// group[1] is the address of pic
        /// note the \\r?\\n  for unix and windows compatibility
        /// </summary>
        static string regPicPageLinkToPicAddr = "<a href=\"https?://.+?\">\\r?\\n\\s+<img src=\"(https?://.+?)\"";
        /// <summary>
        /// group[1] is thumbnail video webaddress
        /// group[2] is video page address
        /// group[3] is create date
        /// group[4] is hot degree
        /// </summary>
        static string regVideoPageStr = "<div class=\"post post_micro is_video.+data-imageurl=\"(https?://.+?)\">.+?href=\"(https?://.+?)\".+<span class=\"post_date\">\\r?\\n\\s+(.+?)\\s+</span><span class=\"post_notes\"\\r?\\n.*data-notes=\"(\\d+)\">";
        /// <summary>
        /// group[1] is the address of the Video Page
        /// find iframe src then use it to find the address
        /// </summary>
       static string regVideoPageiframeToVideoPage = "<iframe src=['\"](https?://.+?)['\"]";
        /// <summary>
        /// group[1] is the real address of the file  
        /// group[2] is the postfix of the file
        /// </summary>
       static string regVideoiframeLinkToVideoAddr = "<source src=\"(https?://.+?)\" type=\"video/(.+?)\"";
        private static bool BlogAddressVaildate(string BlogAddr) { return true; }
        /// <summary>
        /// find Next Page base on  curPage have <a id="next_page_link" href</a>   element
        /// note   . $ ^ { [ ( | ) * + ? \      These characters have special meanings in regular expressions;
        /// </summary>
        /// <param name="client"></param>
        /// <param name="curPage"></param>
        /// <returns></returns>
        private static string DownloadNextPage(WebClient client, string curPage)
        {
            //curPage is like  https://zhling1994.tumblr.com/archive?before_time=1477673156
            string regStringHomePage = @"^https://\w+\.tumblr\.com/?$";
            string regStringAchivePage = @"^https://\w+\.tumblr\.com/archive\?before_time=\d+$";
            Regex regexHomePage = new Regex(regStringHomePage);
            Regex regexAchivePage = new Regex(regStringAchivePage);
            bool blmatch = false;
            string NextPage = string.Empty;
            string strHomePage = string.Empty;  //HomePage without /
            if (regexHomePage.Match(curPage).Success)  // from HomePage
            {
                strHomePage = curPage.TrimEnd(new char[] { '/' });
                curPage = curPage.TrimEnd(new char[] { '/' }) + "/archive";
                blmatch = true;
            }
            else if (regexAchivePage.Match(curPage).Success)
            {
                strHomePage = curPage.Substring(0, curPage.LastIndexOf('/'));
                blmatch = true;

            }
            if (blmatch)
            {
                //append archive to the string

                string HTMLString = client.DownloadString(curPage);
                string regstring =
                  "<a id=\"next_page_link\" href=\"(/archive\\?before_time=\\d+)\">Next page &rarr;</a>";
                /* 
                * note the " in string should use \"  
                * the \d in string without @ should use \\d
                * note "(" group should Not use \( because ( is use as group not the literal mean
                */
                Regex regNextPage = new Regex(regstring);
                Match match = regNextPage.Match(HTMLString);
                if (match.Success)
                {
                    NextPage = match.Groups[1].Value;  //note group [0] is the regstr self, the first match in "( )" is [1]
                    NextPage = strHomePage + NextPage;
                }
            }
            return NextPage;
        }


        public static Tuple<int, List<string>> GetPages(string BlogAddr)
        {
            int PagesCount = 0;
            List<string> lsPages = new List<string>();
            if (BlogAddressVaildate(BlogAddr))
            {
                PagesCount++;

                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;

                string tmpNextPage = DownloadNextPage(client, BlogAddr);
                while (tmpNextPage != string.Empty)
                {
                    PagesCount++;
                    lsPages.Add(tmpNextPage);
                    tmpNextPage = DownloadNextPage(client, tmpNextPage);
                }

            }
            return new Tuple<int, List<string>>(PagesCount, lsPages);
        }
        public static List<ElementStruct> GetElementsFromPage(string pageAddr)
        {
            ElementStruct structElement = new ElementStruct();
            List<ElementStruct> ElementList = new List<ElementStruct>();
            WebClient webclient = new WebClient();
            webclient.Encoding = Encoding.UTF8;
            string strPageDate = webclient.DownloadString(pageAddr);

            // pic regex  
            Regex regPhotoPage = new Regex(regPicPageStr);
            Regex regPhotoAddr = new Regex(regPicPageLinkToPicAddr);
            // video regexp  
            //1. find video page
            //2. find video page's iframe
            //3. find the video address from iframe
            Regex regVideoPage = new Regex(regVideoPageStr);
            Regex regVideoIframe = new Regex(regVideoPageiframeToVideoPage);
            Regex regVideoAddr = new Regex(regVideoiframeLinkToVideoAddr);

            foreach (Match matchPhoto in regPhotoPage.Matches(strPageDate))
            {
                string PhotoPageAddr = matchPhoto.Groups[2].Value;
                string strPhotoPageDate = webclient.DownloadString(PhotoPageAddr);
                Match AddrMatch = regPhotoAddr.Match(strPhotoPageDate);
                if (AddrMatch.Success)
                {
                    structElement.FileAddr = AddrMatch.Groups[1].Value;
                    structElement.ThumbnailAddr = matchPhoto.Groups[1].Value;
                    structElement.CreateDate = matchPhoto.Groups[3].Value;
                    structElement.HotDegree = int.Parse(matchPhoto.Groups[4].Value);
                    structElement.Type = enElementType.Photo;
                    ElementList.Add(structElement);
                }
            }
            foreach (Match matchVideo in regVideoPage.Matches(strPageDate))
            {
                string strVideoiframePageData = webclient.DownloadString(matchVideo.Groups[2].Value);
                Match VideoPageFromIframeMatch = regVideoIframe.Match(strVideoiframePageData);
                if (VideoPageFromIframeMatch.Success)
                {
                    string VideoFilePageAddr = VideoPageFromIframeMatch.Groups[1].Value;
                    Match FileAddrMatch = regVideoAddr.Match(webclient.DownloadString(VideoFilePageAddr));
                    if (FileAddrMatch.Success)
                    {
                        structElement.FileAddr = FileAddrMatch.Groups[1].Value;
                        structElement.ThumbnailAddr = matchVideo.Groups[1].Value;
                        structElement.CreateDate = matchVideo.Groups[3].Value;
                        structElement.HotDegree = int.Parse(matchVideo.Groups[4].Value);
                        structElement.Type = enElementType.Video;
                        ElementList.Add(structElement);
                    }
                }
            }
            webclient.Dispose();
            return ElementList;
        }
        public static Tuple<string, long,string> GetFileRealAddrRedirectInfo(string input)
        {
            string Fileaddr = string.Empty;
            long size = 0;
            string ETag = string.Empty;
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(input);
            myHttpWebRequest.AllowAutoRedirect = false;
            HttpWebResponse response = (HttpWebResponse)myHttpWebRequest.GetResponse();
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                Fileaddr = response.GetResponseHeader("location");
               Fileaddr=Fileaddr.Replace("#_=_", "");
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Fileaddr);
                HttpWebResponse response2 = (HttpWebResponse)request.GetResponse();
                size = response2.ContentLength;
                ETag = response2.GetResponseHeader("ETag").Trim(new char[] {'"'});
            }
            return new Tuple<string, long,string>(Fileaddr, size,ETag);
        }



    }

}
