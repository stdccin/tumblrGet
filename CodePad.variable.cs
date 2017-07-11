using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace tumblrCrawler
{
    public partial class CodePad
    {
        public struct ElementStruct
        {
            public string ThumbnailAddr;
            public string CreateDate;
            public int HotDegree;
            public enElementType Type;
            public string FileAddr;
        }
        public enum enElementType { Photo = 0, Video = 1, Other = 2 };
    }





}
