using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace WebApiMVC.OPCLayer.Class
{
    public class OPCObject_DateTime
    {
        public OPCObject_DateTime(string Name, int hour, int min, int sec)
        {
            this.TagName = Name;
            this.Hour = hour;
            this.Minute = min;
            this.Sec = sec;
        }

        public string TagName { get; set; }
        public int Hour { get; set; }
        public int Minute { get; private set; }
        public int Sec { get; private set; }
    }
}