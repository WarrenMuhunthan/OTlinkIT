using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPC_Module.Resources.Common;
using System.Reflection;

namespace OPC_Module.Resources.Valves
{
    public class SDV
    {
        private int _property_count;
        public PropertyInfo[] propertyInfo;
        public string valve_name;

        //private bool _eng; 
        //private bool _int; 
        //private bool force; 
        //private bool close;
        //private bool fosd;
        //private bool focd;
        //private bool swsd;
        //private bool flt;
        //private TIMER timer1;
        //private TIMER timer2;
        //private TIMER timer3;
        //private bool hmi_auto;
        //private bool hmi_man;
        //private bool mode;
        //private int stat;


        public bool ENG { get; set; }
        public bool INT { get; set; }
        public bool FORCE { get; set; }
        public bool CLOSE { get; set; }
        public bool FOSD { get; set; }
        public bool FCSD { get; set; }
        public bool SWSD { get; set; }
        public bool FLT { get; set; }
        public TIMER TIMER1 { get; set; }
        public TIMER TIMER2 { get; set; }
        public TIMER TIMER3 { get; set; }
        public bool HMI_AUTO { get; set; }
        public bool HMI_MAN { get; set; }
        public bool MODE { get; set; }
        public int STAT { get; set; }

        public SDV(string name)
        {
            _property_count = this.GetType().GetProperties().Count();
            propertyInfo = this.GetType().GetProperties();
            valve_name = name;
        }
        
        public int ReturnNoOfProperty()
        {
            return _property_count;
        }

        public PropertyInfo[] ReturnPropertyInfo()
        {
            return propertyInfo;
        }
    }
}
