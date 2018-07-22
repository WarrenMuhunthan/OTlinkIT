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
using WebApiMVC.DataLogic;
using System.Threading;
using Opc.Da;

namespace WebApiMVC.OPCLayer.Class
{
    public class OPCClient2
    {
        public static ThreadState Thread_status = ThreadState.Unstarted;
        private Opc.URL url;
        private Opc.Da.Server server;
        private OpcCom.Factory fact = new OpcCom.Factory();
        private Opc.Da.Subscription groupRead;
        private Opc.Da.Subscription groupWrite;
        private Opc.Da.SubscriptionState groupState;
        private Opc.Da.SubscriptionState groupStateWrite;
        private Opc.Da.Item[] items = new Opc.Da.Item[8];
        private Opc.Da.Item Item1 = new Opc.Da.Item();
        private Opc.Da.ItemResult[] itemsResult = new Opc.Da.ItemResult[8];

        public static OPCObject myOpcObject = new OPCObject("", 0);
        public static OPCObject_DateTime DataTime = new OPCObject_DateTime("", 0, 0, 0);
        public static ItemValue[] OPCItemValues = new ItemValue[8];
        public static OPCObj[] OPCObj1 = new OPCObj[8];

        public static List<ItemValueResult> OPCObjList = new List<ItemValueResult>();
        private static bool flag = false;

        public OPCClient2()
        {

            ConnectToServer();

        }
        public bool populateopclist()
        {

            if (OPCObjList.Count < 8)
            {
                OPCObjList.Insert(0, new ItemValueResult_Custom("[PLC2]LocalDateTime[0]"));
                OPCObjList.Insert(1, new ItemValueResult_Custom("[PLC2]LocalDateTime[1]"));
                OPCObjList.Insert(2, new ItemValueResult_Custom("[PLC2]LocalDateTime[2]"));
                OPCObjList.Insert(3, new ItemValueResult_Custom("[PLC2]LocalDateTime[3]"));
                OPCObjList.Insert(4, new ItemValueResult_Custom("[PLC2]LocalDateTime[4]"));
                OPCObjList.Insert(5, new ItemValueResult_Custom("[PLC2]LocalDateTime[5]"));
                OPCObjList.Insert(6, new ItemValueResult_Custom("[PLC2]LocalDateTime[6]"));
                OPCObjList.Insert(7, new ItemValueResult_Custom("[PLC2]LocalDateTime"));
            }

            if (OPCObjList[6].ItemName == "[PLC2]LocalDateTime[6]")
            {
                flag = true;
            }

            return flag;
        }
        public void ConnectToServer()
        {
            try
            {
                url = new Opc.URL("opcda://IAPENG1/RSLinx OPC Server");
                //server = new Opc.Da.Server(fact, null);
                server = new Opc.Da.Server(fact, url);
                System.Net.NetworkCredential networkCredential = new System.Net.NetworkCredential();
                Opc.ConnectData connecteddata = new Opc.ConnectData(networkCredential);
                server.Connect(url, connecteddata);
                groupState = new Opc.Da.SubscriptionState();
                groupState.Name = "Group";
                groupState.UpdateRate = 1000;// this isthe time between every reads from OPC server
                groupState.Active = true;//this must be true if you the group has to read value
                groupRead = (Opc.Da.Subscription)server.CreateSubscription(groupState);
                groupRead.DataChanged += new Opc.Da.DataChangedEventHandler(UpdateTagData);//callback when the data are readed

                if (items[0] == null)
                {
                    items[0] = new Opc.Da.Item();
                    items[1] = new Opc.Da.Item();
                    items[2] = new Opc.Da.Item();
                    items[3] = new Opc.Da.Item();
                    items[4] = new Opc.Da.Item();
                    items[5] = new Opc.Da.Item();
                    items[6] = new Opc.Da.Item();
                    items[7] = new Opc.Da.Item();
                }

                items[0].ItemName = "[PLC2]LocalDateTime[0]";
                items[1].ItemName = "[PLC2]LocalDateTime[1]";
                items[2].ItemName = "[PLC2]LocalDateTime[2]";
                items[3].ItemName = "[PLC2]LocalDateTime[3]";
                items[4].ItemName = "[PLC2]LocalDateTime[4]";
                items[5].ItemName = "[PLC2]LocalDateTime[5]";
                items[6].ItemName = "[PLC2]LocalDateTime[6]";
                items[7].ItemName = "[PLC2]LocalDateTime";

                items = groupRead.AddItems(items);

                // Create a write group            
                groupStateWrite = new Opc.Da.SubscriptionState();
                groupStateWrite.Name = "Group Write";
                groupStateWrite.Active = false;//not needed to read if you want to write only
                groupWrite = (Opc.Da.Subscription)server.CreateSubscription(groupStateWrite);
            }
            catch (Exception E)
            {

                Console.WriteLine(E.Message);
            }
        }


        public void UpdateTagData(object subscriptionHandle, object requestHandle, Opc.Da.ItemValueResult[] values)
        {
            populateopclist();

            for (int i = 0; i < values.Length; i++)
            {

                switch (values[i].ItemName)
                {
                    case "[PLC2]LocalDateTime[0]":
                        OPCObjList[OPCObjList.FindIndex(delegate (ItemValueResult ItemVaR) { return ItemVaR.ItemName == "[PLC2]LocalDateTime[0]"; })].Value = values[i].Value;
                        break;
                    case "[PLC2]LocalDateTime[1]":
                        OPCObjList[OPCObjList.FindIndex(delegate (ItemValueResult ItemVaR) { return ItemVaR.ItemName == "[PLC2]LocalDateTime[1]"; })].Value = values[i].Value;
                        break;
                    case "[PLC2]LocalDateTime[2]":
                        OPCObjList[OPCObjList.FindIndex(delegate (ItemValueResult ItemVaR) { return ItemVaR.ItemName == "[PLC2]LocalDateTime[2]"; })].Value = values[i].Value;
                        break;
                    case "[PLC2]LocalDateTime[3]":
                        OPCObjList[OPCObjList.FindIndex(delegate (ItemValueResult ItemVaR) { return ItemVaR.ItemName == "[PLC2]LocalDateTime[3]"; })].Value = values[i].Value;
                        break;
                    case "[PLC2]LocalDateTime[4]":
                        OPCObjList[OPCObjList.FindIndex(delegate (ItemValueResult ItemVaR) { return ItemVaR.ItemName == "[PLC2]LocalDateTime[4]"; })].Value = values[i].Value;
                        break;
                    case "[PLC2]LocalDateTime[5]":
                        OPCObjList[OPCObjList.FindIndex(delegate (ItemValueResult ItemVaR) { return ItemVaR.ItemName == "[PLC2]LocalDateTime[5]"; })].Value = values[i].Value;
                        break;
                    case "[PLC2]LocalDateTime[6]":
                        OPCObjList[OPCObjList.FindIndex(delegate (ItemValueResult ItemVaR) { return ItemVaR.ItemName == "[PLC2]LocalDateTime[6]"; })].Value = values[i].Value;
                        break;
                    case "[PLC2]LocalDateTime":
                        OPCObjList[OPCObjList.FindIndex(delegate (ItemValueResult ItemVaR) { return ItemVaR.ItemName == "[PLC2]LocalDateTime"; })].Value = values[i].Value;
                        break;
                    default:
                        OPCObjList.Insert(8, values[i]);
                        break;
                }


            }

            UpdateTagDataStore(OPCObjList);
        }

        public void UpdateTagDataStore(List<ItemValueResult> OPCObjList)
        {


            Global.opclist2 = OPCObjList;

            if (Thread_status != ThreadState.Running)
            {

                Thread pollingThread = new Thread(new ThreadStart(() =>
                {



                    SendDataToHub.RunPolling();



                }));

                pollingThread.Name = "polling";
                pollingThread.Start();
                Thread_status = pollingThread.ThreadState;


            }

        }
    }
}