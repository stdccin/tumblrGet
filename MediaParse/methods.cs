using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace tumblrCrawler.MediaParse
{
    public partial class Parser
    {

        /// <summary>
        /// PageLinkstr list from startPoint 
        /// </summary>
        /// <param name="StartPage"></param>
        /// <returns></returns>
        public List<string> GetAllPageFromStartPoint(string StartPage)
        {
            List<string> Pagelist = new List<string>();
            string PageLink = StartPage;
            while ((PageLink = GetNextPage(PageLink)) != string.Empty)
            {
                Pagelist.Add(PageLink);
            }
            return Pagelist;
        }

        /// <summary>
        /// Fetch next page 
        /// 1.  from blog mainPage or archive main page 
        /// 2.  from  ....om/archive?before_time=1477673   link page
        /// </summary>
        /// <param name="curPage"></param>
        /// <returns></returns>

        public string GetNextPage(string curPage)
        {
            //curPage is like  https://zhling1994.tumblr.com/archive?before_time=1477673156
            bool blmatch = false;

            string NextPage = string.Empty;
            string strHomePage = string.Empty;  //HomePage  is archive  homepage

            //curPage is homePage or archive page
            if (regexHomePage.Match(curPage).Success)
            {
                strHomePage = curPage.TrimEnd(new char[] { '/' });
                // for not archive HomePage
                if (curPage.EndsWith("archive"))
                {
                    strHomePage = curPage.Substring(0, curPage.LastIndexOf("/archive"));
                }
                else curPage = curPage + "/archive";

                blmatch = true;
            }
            // curPage is archive section pages
            else if (regexAchivePage.Match(curPage).Success)
            {
                strHomePage = curPage.Substring(0, curPage.LastIndexOf('/'));
                blmatch = true;
            }


                if (blmatch)
                {
                    string HTMLString = GetDataStringFromAddr(curPage);

                    /* 
                    * note the " in string should use \"  
                    * the \d in string without @ should use \\d
                    * note "(" group should Not use \( because ( is use as group not the literal mean
                    */
                    Match match = regexNextPage.Match(HTMLString);
                    if (match.Success)
                    {
                        NextPage = match.Groups[1].Value;
                        NextPage = strHomePage + NextPage;
                    }
                }
                return NextPage;
            }

        public List<ArchivePageLinkStruct> GetArchivePageToLinkList(string webpages)
        {
            ArchivePageLinkStruct LinkStruct = new ArchivePageLinkStruct();
            List<ArchivePageLinkStruct> lsArchivePageItemlist = new List<ArchivePageLinkStruct>();
            DateTime dt;
            string PageData = GetDataStringFromAddr(webpages);
            if (PageData != string.Empty)
            {
                // match Img Link
                foreach (Match match in regexArchiveImgItem.Matches(PageData))
                {
                    dt = new DateTime(1970, 01, 01, 00, 00, 00);    // use the unix default DateTime

                    DateTime.TryParse(match.Groups[3].Value, out dt);

                    LinkStruct.ThumbnailAddr = match.Groups[1].Value;
                    LinkStruct.PageLink = match.Groups[2].Value;
                    LinkStruct.CreateDate = dt;
                    LinkStruct.Type = MediaType.Img;
                    try
                    {
                        LinkStruct.HotDegree = int.Parse(match.Groups[4].Value);
                    }
                    catch (Exception ex)
                    {
                        LinkStruct.HotDegree = 0;
                    }
                    lsArchivePageItemlist.Add(LinkStruct);
                }
                //Match Vedio Item link
                foreach (Match match in regexArchivePageVideoItem.Matches(PageData))
                {
                    dt = new DateTime(1970, 01, 01, 00, 00, 00);    // use the unix default DateTime

                    DateTime.TryParse(match.Groups[3].Value, out dt);

                    LinkStruct.ThumbnailAddr = match.Groups[1].Value;
                    LinkStruct.PageLink = match.Groups[2].Value;
                    LinkStruct.CreateDate = dt;
                    LinkStruct.Type = MediaType.Video;
                    try
                    {
                        LinkStruct.HotDegree = int.Parse(match.Groups[4].Value);
                    }
                    catch (Exception ex)
                    {
                        LinkStruct.HotDegree = 0;
                    }
                    lsArchivePageItemlist.Add(LinkStruct);
                }
            }
            return lsArchivePageItemlist;
        }

        string GetDataStringFromAddr(string address)
        {
            string DataStr = string.Empty;
            int i = 0;
        start:
            try
            {
                DataStr = QueryWebClient.DownloadString(address);
            }
            catch (WebException)
            {
                /// debug msg
                if (i < webclientWebExceptionMaxRetry)
                    i++;
                goto start;
            }
            catch (Exception) { return string.Empty; }
            return DataStr;
        }

    }
}
