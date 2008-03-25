﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Common
{
    public class ReportCreator
    {
        static string fileName = string.Format("CheckTime {0}.xml", DateTime.Now.ToString("yyyy-MM-dd hh_mm_ss"));
        
        public static void Write(DSTestTime dsTestTime)
        {
            dsTestTime.WriteXml(fileName);
        }


    }
}