using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace tumblrCrawler.MediaParse
{
    public partial class Parser
    {
        public void ParseArchivePageToLinkList(List<string> lsPageAddr)
        {
            List<ManualResetEvent> lsmanualEvents = new List<ManualResetEvent>();
            FetchListFromArchivePageAddrStruct FetchStruct = new FetchListFromArchivePageAddrStruct();

            foreach (string pageaddr in lsPageAddr)
            {
                FetchStruct.webpageaddr = pageaddr;

                //makesure all work done!
                FetchStruct.evnt = new ManualResetEvent(false);
                lsmanualEvents.Add(FetchStruct.evnt);
                try
                {

                    if (!ThreadPool.QueueUserWorkItem(new WaitCallback(FetchListFromArchivePageAddr), FetchStruct))
                        Debug.WriteLine("Err QueueUserWorkItem");
                }
                catch (Exception ex)
                {
                    FetchStruct.evnt.Set();
                    Debug.WriteLine(string.Format("Err: ParseArchivePageToLinkList {0}:  {1}", ex.ToString(), ex.Message));
                }

            }
            if (lsmanualEvents.Count > 0)
            {
                foreach (var e in lsmanualEvents.ToArray())
                    e.WaitOne();
                Debug.WriteLine("threadpool: all work done!");
            }
        }
        /// <summary>
        /// base on ArchiveLink lists
        /// </summary>
        public void FetchMediaLists()
        {
            List<ManualResetEvent> lsmanualEvent = new List<ManualResetEvent>();
            ManualResetEvent mRestEvent;

         
            foreach (ArchivePageLinkStruct ms in lsArchivePageVideoLink)
            {
                //    lsMediaStructVideos.Add(GetVideoMediaStructFromArchivePageLinkStruct(ms));

                mRestEvent = new ManualResetEvent(false);
                lsmanualEvent.Add(mRestEvent);
                Action<Object> AddVideoList = (obj) =>
                {
                    Debug.WriteLine("Current.thread.ID  " + Thread.CurrentThread.ManagedThreadId.ToString() + "  start");

                    ManualResetEvent Mevent = obj as ManualResetEvent;
                    lsMediaStructVideos.Add(GetVideoMediaStructFromArchivePageLinkStruct(ms));
                    Mevent.Set();

                    Debug.WriteLine("Current.Thread ID  " + Thread.CurrentThread.ManagedThreadId.ToString() + "  end");
                };
                ThreadPool.QueueUserWorkItem(new WaitCallback(AddVideoList), mRestEvent);

            }
            

            foreach (ArchivePageLinkStruct imglink in lsArchivePageImgLink)
            {

                mRestEvent = new ManualResetEvent(false);
                lsmanualEvent.Add(mRestEvent);
                Action<Object> AddImgList = (obj) =>
                {
                    Debug.WriteLine("Current.thread.ID  " + Thread.CurrentThread.ManagedThreadId.ToString() + "  start");

                    ManualResetEvent MEvent = obj as ManualResetEvent;
                    List<MediaStruct> ListTmp = GetImgMediaStructListFromArchivePageLinkStruct(imglink);
                    lsMediaStrutImgs.AddRange(ListTmp);
                    MEvent.Set();

                    Debug.WriteLine("Current.Thread ID  " + Thread.CurrentThread.ManagedThreadId.ToString() + "  end");
                };
                ThreadPool.QueueUserWorkItem(new WaitCallback(AddImgList), mRestEvent);

            }

            foreach (ManualResetEvent Event in lsmanualEvent)
            {
                Event.WaitOne();
            }
            Debug.WriteLine("all work done!");

        }

        public void WritelIstsDataToFile(string path)
        {
            FileStream fstream;
            int i = 0;
            int j = 0;
            string append = lsMediaStructVideos[1].HomePage;
            path = path + "\\" +append.Replace("https://", "").Replace("http://", "").Replace(".tumblr.com", "")+".txt";
            try
            {
                fstream = new FileStream(path, FileMode.OpenOrCreate);
            }
            catch (Exception)
            {
                return;
            }
            foreach (MediaStruct mediastruct in lsMediaStructVideos)
            {


                if (!string.IsNullOrWhiteSpace(mediastruct.ETag))
                {
                    string output = string.Format("---------------------------------------------------------------------------------\nFileaddr:\t\t{0}\nEtag:\t\t{1}\nSize:\t\t{2}\nFileType:\t\t{3}\nHotDegree:\t\t{4}\n",
                                        mediastruct.FileAddr, mediastruct.ETag, GetReadableSize(mediastruct.size), mediastruct.Type.ToString(), mediastruct.HotDegree.ToString());
                    fstream.Write(System.Text.Encoding.UTF8.GetBytes(output), 0, System.Text.Encoding.UTF8.GetByteCount(output));
                    i++;
                }
            }
            foreach (MediaStruct mediaImgstruct in lsMediaStrutImgs)
            {

                  if (!string.IsNullOrWhiteSpace(mediaImgstruct.ETag))
                {
                    string output = string.Format("---------------------------------------------------------------------------------\nFileaddr:\t\t{0}\nEtag:\t\t{1}\nSize:\t\t{2}\nFileType:\t\t{3}\nHotDegree:\t\t{4}\n",
                                        mediaImgstruct.FileAddr, mediaImgstruct.ETag, GetReadableSize(mediaImgstruct.size), mediaImgstruct.Type.ToString(), mediaImgstruct.HotDegree.ToString());
                    fstream.Write(System.Text.Encoding.UTF8.GetBytes(output), 0, System.Text.Encoding.UTF8.GetByteCount(output));
                    j++;
                }
            }

            fstream.Close();
            System.Windows.Forms.MessageBox.Show(string.Format("mediastruct: {0}entries, output: {1}entries\nimg struct: {2}entries,output: {3}entries.", lsMediaStructVideos.Count.ToString(), i.ToString(), lsMediaStrutImgs.Count.ToString(), j.ToString())); ;
        }

        public void WritelIstsDataToFileCVS(string path)
        {
            FileStream fstream;
            int i = 0;
            int j = 0;
            string append = lsMediaStructVideos[1].HomePage;
            path = path + "\\" + append.Replace("https://", "").Replace("http://", "").Replace(".tumblr.com", "") + "CVS.txt";
            try
            {
                fstream = new FileStream(path, FileMode.OpenOrCreate);
            }
            catch (Exception)
            {
                return;
            }
            foreach (MediaStruct mediastruct in lsMediaStructVideos)
            {
                if (!string.IsNullOrWhiteSpace(mediastruct.FileAddr))
                {
                    string output = string.Format("{0}#{1}#{2}#{3}#{4}\n",
                                        mediastruct.FileAddr, mediastruct.ETag, GetReadableSize(mediastruct.size), mediastruct.Type.ToString(), mediastruct.HotDegree.ToString());
                    fstream.Write(System.Text.Encoding.UTF8.GetBytes(output), 0, System.Text.Encoding.UTF8.GetByteCount(output));
                    i++;
                }
            }
            foreach (MediaStruct mediaImgstruct in lsMediaStrutImgs)
            {

                if (!string.IsNullOrWhiteSpace(mediaImgstruct.FileAddr ))
                {
                    string output = string.Format("{0}#{1}#{2}#{3}#{4}\n",
                                         mediaImgstruct.FileAddr, mediaImgstruct.ETag, GetReadableSize(mediaImgstruct.size), mediaImgstruct.Type.ToString(), mediaImgstruct.HotDegree.ToString());
                    fstream.Write(System.Text.Encoding.UTF8.GetBytes(output), 0, System.Text.Encoding.UTF8.GetByteCount(output));
                    j++;
                }
            }

            fstream.Close();
        }


        private string GetReadableSize(long longsize)
        {
            double size = Convert.ToDouble(longsize);
            {

            }
            double mb = 1024.00 * 1024.00;
            double kb = 1024.00;
            if (size >= mb)
            {
                size = size / mb;
                return size.ToString("#.##") + "MB";
            }
            else if (size >= kb)
            {
                size = size / kb;
                return size.ToString("#.##") + "KB";
            }
            else return size.ToString() + "B";

        }


        /// <summary>
        /// PageLinkstr list from startPoint 
        /// </summary>
        /// <param name="StartPage"></param>
        /// <returns></returns>
        public List<string> GetAllPageFromStartPoint(string StartPage)
        {
            List<string> Pagelist = new List<string>();
            string PageLink = GetCurrentPageFromStart(StartPage);
            Pagelist.Add(PageLink);
            while ((PageLink = GetNextPage(PageLink)) != string.Empty)
            {
                Pagelist.Add(PageLink);
                Debug.WriteLine(string.Format("Current Page in List:  {0}", Pagelist.Count.ToString()));
            }
            return Pagelist;
        }
        /// <summary>
        /// add data to list from webpages
        /// </summary>
        /// <param name="webaddr"></param>

        private void FetchListFromArchivePageAddr(object fecthobj)
        {
            //  avoid webException
            string DataStr = string.Empty;
            FetchListFromArchivePageAddrStruct fetchstruct = (FetchListFromArchivePageAddrStruct)fecthobj;
            string webaddr = fetchstruct.webpageaddr;
            DataStr = GetDataStringFromAddr(webaddr);

            if (DataStr != string.Empty) AddListsFromArchivePageData(DataStr);
            fetchstruct.evnt.Set();
        }
        private void AddListsFromArchivePageData(string PageData)
        {
            ArchivePageLinkStruct LinkStruct = new ArchivePageLinkStruct();
            DateTime dt;
            if (PageData != string.Empty)
            {
                // match Img Link
                foreach (Match match in regexArchiveImgItem.Matches(PageData))
                {
                    dt = new DateTime(1970, 01, 01, 00, 00, 00);    // use the unix default DateTime
                    DateTime.TryParse(match.Groups[3].Value, out dt);

                    LinkStruct.ThumbnailAddr = match.Groups[1].Value;
                    LinkStruct.PageLink = match.Groups[2].Value;
                    LinkStruct.HomePage = LinkStruct.PageLink.Substring(0, LinkStruct.PageLink.LastIndexOf("/post/"));
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
                    lock (lockobj_archiveImgLink)
                    {
                        lsArchivePageImgLink.Add(LinkStruct);
                    }
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
                    LinkStruct.HomePage = LinkStruct.PageLink.Substring(0, LinkStruct.PageLink.LastIndexOf("/post/"));
                    try
                    {
                        LinkStruct.HotDegree = int.Parse(match.Groups[4].Value);
                    }
                    catch (Exception ex)
                    {
                        LinkStruct.HotDegree = 0;
                    }
                    lock (lockobj_archiveVideoLink)
                    { lsArchivePageVideoLink.Add(LinkStruct); }
                }
            }

        }

        public string GetCurrentPageFromStart(string curPage)
        {
            if (regexHomePage.Match(curPage).Success)
            {
                curPage = curPage.TrimEnd(new char[] { '/' });
                // for not archive HomePage
                if (curPage.EndsWith("archive"))
                {
                    return curPage;
                }
                else return curPage = curPage + "/archive";

            }
            // curPage is archive section pages
            else if (regexAchivePage.Match(curPage).Success)
            {

                return curPage;
            }
            else return string.Empty;
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
                curPage = curPage.TrimEnd(new char[] { '/' });
                strHomePage = curPage;
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

        public List<ArchivePageLinkStruct> GetCurArchivePageToLinkList(string webpages)
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
            WebClient QueryWebClient = new WebClient() { Encoding = System.Text.Encoding.UTF8 };
            int i = 0;
        start:
            try
            {
                DataStr = QueryWebClient.DownloadString(address);
            }
            catch (WebException)
            {
                //      Debug.WriteLine("Ex:  WebException");
                if (i < WebExceptionMaxRetry)
                {
                    i++;
                    goto start;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Illegal characters in path.")
                {
                    Debug.WriteLine("error:  catch error in URI");
                    throw new Exception();
                }
                Thread.Sleep(WebExRetryTimeDeny);
                QueryWebClient.Dispose();
            }
            finally
            {
                QueryWebClient.Dispose();
            }
            return DataStr;
        }
        /// <summary>
        /// copy data from archive struct except FileAddr
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="mediastruct"></param>
        private void CopyArchivePageLinkStructCommData(ref ArchivePageLinkStruct archive, ref MediaStruct mediastruct)
        {
            mediastruct.ThumbnailAddr = archive.ThumbnailAddr;
            mediastruct.CreateDate = archive.CreateDate;
            mediastruct.HomePage = archive.HomePage;
            mediastruct.HotDegree = archive.HotDegree;
            mediastruct.Type = archive.Type;
            mediastruct.ETag = string.Empty;
            mediastruct.Parameter = string.Empty;
            mediastruct.size = 0;
        }
        /// <summary>
        /// get the video struct element note: 1. etag not set; 2. FileAddr is the host internal address
        /// already use GetFileResponseInfo to retrive etag and real fileinfo
        /// if error type is unknow
        /// </summary>
        /// <param name="arcPageStruct"></param>
        /// <returns></returns>
        MediaStruct GetVideoMediaStructFromArchivePageLinkStruct(ArchivePageLinkStruct arcPageStruct)
        {
            MediaStruct videoStruct = new MediaStruct();

            // match iframe retrive playpage link
            string strVideoMainPageData = GetDataStringFromAddr(arcPageStruct.PageLink);
            Match VideoPageIframeLinkMatch = regexVideoPageIframeLink.Match(strVideoMainPageData);
            if (VideoPageIframeLinkMatch.Success)
            {
                string strVideoPlayAddr = VideoPageIframeLinkMatch.Groups[1].Value;
                // match playpagelink  to retrive playaddress
                Match FileAddrMatch = regexVideoiframeLinkToVideoAddr.Match(GetDataStringFromAddr(strVideoPlayAddr));
                if (FileAddrMatch.Success)
                {
                    videoStruct.FileAddr = FileAddrMatch.Groups[1].Value;
                    CopyArchivePageLinkStructCommData(ref arcPageStruct, ref videoStruct);
                    // just initate all data
                    return GetFileResponseInfo(videoStruct);
                }
            }

            videoStruct.Type = MediaType.unknow;
            return videoStruct;
        }

        /// <summary>
        /// retrive MediaStruct ETag,Size,realFileAddress
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private MediaStruct GetFileResponseInfo(MediaStruct input)

        {


            /* Do not use the HttpWebRequest constructor.
            Use the WebRequest.Create method to initialize newHttpWebRequest objects. 
            If the scheme for the Uniform Resource Identifier (URI) is http:// or https://, Create returns an HttpWebRequest object.
            */

            MediaStruct NewMediaStruct = input;
            string FileAddr = input.FileAddr;
            string UserAgent = "Mozilla/5.0";  //may useless
            int i = 0;
        start:
            try
            {

                if (input.Type == MediaType.Video)
                {
                    HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(FileAddr);
                    myHttpWebRequest.AllowAutoRedirect = false;
                    myHttpWebRequest.UserAgent = UserAgent;
                    HttpWebResponse response = (HttpWebResponse)myHttpWebRequest.GetResponse();

                    if (response.StatusCode == HttpStatusCode.Redirect)
                    {
                        FileAddr = response.GetResponseHeader("location").Replace("#_=_", "");
                    }
                    response.Close();
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(FileAddr);
                request.UserAgent = UserAgent;

                HttpWebResponse response2 = (HttpWebResponse)request.GetResponse();
                NewMediaStruct.FileAddr = FileAddr;
                NewMediaStruct.size = response2.ContentLength;
                NewMediaStruct.ETag = response2.GetResponseHeader("ETag").Trim(new char[] { '"' });
                response2.Close();

            }
            catch (WebException ex)
            {
                // for after retry still get webexception
                NewMediaStruct.FileAddr = FileAddr;
                NewMediaStruct.size = 0;
                NewMediaStruct.ETag = string.Empty;

                if (i < WebExceptionMaxRetry && ex.Status != WebExceptionStatus.ProtocolError)
                {
                    Thread.Sleep(WebExRetryTimeDeny);
                    goto start;
                }
                if (ex.Status == WebExceptionStatus.ProtocolError)
                    Debug.WriteLine("file maybe already deleted!");
            }

            return NewMediaStruct;
        }
        /// <summary>
        /// retrive Img mediastruct  list( list for multiple Imgs)
        /// two cases:  1.  only one Img, 2. many Imgs,need create more struct
        /// error return empty list;
        /// </summary>
        /// <param name=""></param>

        public List<MediaStruct> GetImgMediaStructListFromArchivePageLinkStruct(ArchivePageLinkStruct ImgStruct)
        {
            List<MediaStruct> lsImg = new List<MediaStruct>();
            string Datastr = string.Empty;
            Datastr = GetDataStringFromAddr(ImgStruct.PageLink);

            // match only one Img in link
            Match match = regexImgPageToSingleImgLink.Match(Datastr);
            if (match.Success)
            {
                MediaStruct tmpStruct = new MediaStruct();
                CopyArchivePageLinkStructCommData(ref ImgStruct, ref tmpStruct);
                tmpStruct.FileAddr = match.Groups[4].Value;
                lsImg.Add(GetFileResponseInfo(tmpStruct));
            }

            //  match Img Link have multiple Imgs
            Match matchIframe = regexImgPageIframeLink.Match(Datastr);
            if (matchIframe.Success)
            {
                string IframeAddr = ImgStruct.HomePage + matchIframe.Groups[1].Value;
                string IframeData = GetDataStringFromAddr(IframeAddr);
                // match iframe contines Img Links
                // for file name in same iframe
                int i = 0;
                string name=string.Empty;
                string identy = string.Empty;
                foreach (Match matchImg in regexIframeToMultiImgLink.Matches(IframeData))
                {

                    MediaStruct tmpStruct1 = new MediaStruct();
                    CopyArchivePageLinkStructCommData(ref ImgStruct, ref tmpStruct1);
                    tmpStruct1.FileAddr = matchImg.Groups[1].Value;
                    tmpStruct1.ThumbnailAddr = matchImg.Groups[2].Value;
                    if (i == 0) {
                        name = tmpStruct1.FileAddr.Substring(tmpStruct1.FileAddr.LastIndexOf('/'))+"_";
                        identy=tmpStruct1.FileAddr.Substring(tmpStruct1.FileAddr.LastIndexOf('.'));
                             }
                    tmpStruct1.Parameter = name + i.ToString() + "." + identy;
                    i++;
                    lsImg.Add(GetFileResponseInfo(tmpStruct1));

                }
            }
            return lsImg;
        }
    }
}
