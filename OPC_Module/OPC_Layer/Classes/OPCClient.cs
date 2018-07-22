using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Opc;
using OpcCom;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using WebApiMVC.OPCLayer.Class;
using System.Threading;
using Opc.Da;
using OPC_Module.Resources.Common;
using OPC_Module.Resources.Valves;
using OPC_Module.Global;

namespace OPC_Module
{
    public class OPCClient
    {
        public static ThreadState Thread_status = ThreadState.Unstarted;
        private Opc.URL url;
        private Opc.Da.Server server;
        private OpcCom.Factory fact = new OpcCom.Factory();
        private Opc.Da.Subscription groupRead;
        private Opc.Da.SubscriptionState groupState;
        private Opc.Da.Item[] items = new Opc.Da.Item[8];
        private Opc.Da.Item[] Write_items = new Opc.Da.Item[2];
        private Opc.Da.Item Item1 = new Opc.Da.Item();
        private Opc.Da.ItemResult[] itemsResult = new Opc.Da.ItemResult[7];
        public static OPCObject myOpcObject = new OPCObject("", 0);
        public static OPCObject_DateTime DataTime = new OPCObject_DateTime("", 0, 0, 0);
        public static ItemValue[] OPCItemValues = new ItemValue[7];
        public static OPCObj[] OPCObj1 = new OPCObj[7];
        public static List<ItemValueResult> OPCObjList = new List<ItemValueResult>();


        public OPCClient()
        {

        }

        public void ConnectToServer()
        {
            try
            {
                //LOCAL OPC CONNECTION kepware
                //url = new Opc.URL("opcda://PCNAME/Kepware.KEPServerEX.V6");
                //LOCAL OPC CONNECTION RSLinx
                //url = new Opc.URL("opcda://PCNAME/RSLinx OPC Server");

                //REMOTE OPC CONNECTION WHEN USING opcexpert tunneling
                url = new Opc.URL("opcda://PCNAME/RSLinx Remote OPC Server.REMOTEPCNAME");

                //REMOTE RSLinx OPC 
                //this requires DCOM setup
                //url = new Opc.URL("opcda://PCNAME/RSLinx Remote OPC Server");

                server = new Opc.Da.Server(fact, url);
                System.Net.NetworkCredential networkCredential = new System.Net.NetworkCredential();
                Opc.ConnectData connecteddata = new Opc.ConnectData(networkCredential);
                server.Connect(url, connecteddata);
                groupState = new Opc.Da.SubscriptionState();
                groupState.Name = "Group";
                groupState.UpdateRate = 1;// this isthe time between every reads from OPC server
                groupState.Active = true;//this must be true if you the group has to read value
                groupRead = (Opc.Da.Subscription)server.CreateSubscription(groupState);
                groupRead.DataChanged += new Opc.Da.DataChangedEventHandler(UpdateTagData);//callback when the data are readed

                Opc.Da.Item[] items_read = groupRead.AddItems(createSomeTags());



            }
            catch (Exception E)
            {

                Console.WriteLine(E.Message);
            }
        }

        private void ReadCompleted(object subscriptionHandle, object requestHandle, ItemValueResult[] values)
        {

        }

        private void RequestHandler(object requestHandle, IdentifiedResult[] results)
        {
            foreach (Opc.IdentifiedResult writeResult in results)
            {
                Console.WriteLine("\t{0} write result: {1}", writeResult.ItemName, writeResult.ResultID);
            }
            Console.WriteLine();
        }


        public Opc.Da.Item[] createSomeTags()
        {
            string VALVE_NAME = "SDV_1600";
            List<SDV> sdvlist = Global.Global.sdvlist;

            string[] valve_names = new string[] { "SDV_1600", "SDV_1601", "SDV_1602", "SDV_1603", "SDV_1604" };
            foreach (string str in valve_names)
            {
                sdvlist.Add(new SDV(str));
            }

            SDV SDV_1600 = sdvlist[0];

            Opc.Da.Item[,] SDV_1600_Items = new Opc.Da.Item[valve_names.Length, SDV_1600.ReturnNoOfProperty()];
            for (int y = 0; y < sdvlist.Count; y++)
            {
                for (int x = 0; x < SDV_1600.ReturnNoOfProperty(); x++)
                {

                    SDV_1600_Items[y, x] = new Opc.Da.Item();
                    SDV_1600_Items[y, x].ItemName = "[WebOPC]" + VALVE_NAME + "." + SDV_1600.propertyInfo[x].Name.ToString();
                }
            }

    

            return MultiToSingle(SDV_1600_Items);
        }

        static Opc.Da.Item[] MultiToSingle(Opc.Da.Item[,] array)
        {
            int index = 0;
            int width = array.GetLength(0);
            int height = array.GetLength(1);
            Opc.Da.Item[] single = new Opc.Da.Item[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    single[index] = array[x, y];
                    index++;
                }
            }
            return single;
        }

        public Opc.Da.Item[] createTagItems()
        {
            Opc.Da.Item[] tag_items = new Opc.Da.Item[2];




            return tag_items;
        }

        public void UpdateTagData(object subscriptionHandle, object requestHandle, Opc.Da.ItemValueResult[] values)
        {
            Console.WriteLine("hit");

            for (int i = 0; i < values.Length; i++)
            {

                string Valve_Name = values[i].ItemName.Substring(8, 8);
                string valve_element = values[i].ItemName.Remove(0, 17);

                int index = Global.Global.sdvlist.FindIndex(delegate (SDV sdv)
                {
                    return sdv.valve_name == Valve_Name;
                });

                Global.Global.sdvlist[index].GetType().GetProperty(valve_element)
                    .SetValue(Global.Global.sdvlist[index],
                    System.Convert.ChangeType(values[i].Value,
                    Global.Global.sdvlist[index].GetType().GetProperty(valve_element).PropertyType), null);
            }
        }
    }
}