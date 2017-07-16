using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;

namespace tumblrCrawler.MediaParse
{
    public partial class Parser
    {
        public void MonitorThreadPool()
        {
            Action<object> threadInfoPro = s =>
            {
                int workerThreads, completionPortThreads;
                ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
                Debug.WriteLine(string.Format("workerThreads count:  {0}\t\t completionPortThreads count: {1}", workerThreads.ToString(), completionPortThreads.ToString()));

            };
            // timer delegate
            Timer t = new Timer(new TimerCallback(threadInfoPro));
            t.Change(0, 5000);
        }


        public void GetImgMediaStructListFromPage(string page)
        {

            string Datastr = string.Empty;
            Datastr = GetDataStringFromAddr(page); // change gatedata exception to avoid the website update css.
                                                   // match only one Img in link
                                                   //static string regstrImgPageLinkToPicAddr =
                                                   // "((<a href=\"https?://.+?\">)|(<div class=\"photo-wrapper-inner\">))\\r?\\n\\s+<img src=\"(https?://.+?)\"";

            Match match = regexImgPageToSingleImgLink.Match(Datastr);
            if (match.Success)
            {
                MediaStruct tmpStruct = new MediaStruct();
                //   tmpStruct.FileAddr = tmpStruct.HomePage + match.Groups[3].Value;
                tmpStruct.FileAddr = match.Groups[4].Value;
            }

            //  match Img Link have multiple Imgs
            //"src=\"(/post/.+photoset_iframe.+)\"\\r?\\n\\s+></iframe></div>";

            Match matchIframe = regexImgPageIframeLink.Match(Datastr);
            if (matchIframe.Success)
            {
                string IframeAddr = "https://hsexh233.tumblr.com" + matchIframe.Groups[1].Value;

                string IframeData = GetDataStringFromAddr(IframeAddr);
                // match iframe contines Img Links

                foreach (Match matchImg in regexIframeToMultiImgLink.Matches(IframeData))
                {
                    MediaStruct tmpStruct1 = new MediaStruct();

                    tmpStruct1.FileAddr = matchImg.Groups[1].Value;
                    tmpStruct1.ThumbnailAddr = matchImg.Groups[2].Value;
                }
            }
        }
    }
}
