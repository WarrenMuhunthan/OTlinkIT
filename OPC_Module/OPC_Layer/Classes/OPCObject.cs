using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiMVC.OPCLayer.Class
{
    public class OPCObject
    {
        
        public OPCObject(string Name, Int16 Value)
        {
            this.TagName = Name;
            this.TagValue = Value;
        }
            public string TagName { get; set; }
            public Int16 TagValue { get; set; }
        public int timerSec { get; set; }

    }
}