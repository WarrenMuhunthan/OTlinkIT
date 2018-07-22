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
using WebApiMVC.BaseClasses;
using WebApiMVC.GlobalData;
using WebApiMVC.Resources;
using WebApiMVC.Utility;

namespace WebApiMVC.OPCLayer.Class
{
    public class OPCClient
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
                //url = new Opc.URL("opcda://MUHUMTHA/Kepware.KEPServerEX.V6");
                //url = new Opc.URL("opcda://IAPENG1/RSLinx OPC Server");
                //REMOTE OPC CONNECTION
                url = new Opc.URL("opcda://MUHUMTHA/RSLinx OPC Server.MUHUNTHANPC");
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
                //Opc.Da.Item[] items_read = groupRead.AddItems(createSomeTags());
                Opc.Da.Item[] items_read = groupRead.AddItems(createSomeTags());

                // Create a write group            
                //groupStateWrite = new Opc.Da.SubscriptionState();
                //groupStateWrite.Name = "Group_Write";
                //groupStateWrite.UpdateRate = 1000;
                //groupStateWrite.Active = false;//not needed to read if you want to write only
                //groupWrite = (Opc.Da.Subscription)server.CreateSubscription(groupStateWrite);
                //groupWrite.DataChanged += new Opc.Da.DataChangedEventHandler(ReadCompleted);

                Global.flag1 = true;

            }
            catch (Exception E)
            {

                Console.WriteLine(E.Message);
            }
        }

        private void ReadCompleted(object subscriptionHandle, object requestHandle, ItemValueResult[] values)
        {

        }


        public void WriteData(string[] itemName, int[] value)
        {

            groupWrite.RemoveItems(groupWrite.Items);

            List<Item> writeList = new List<Item>();
            List<ItemValue> valueList = new List<ItemValue>();

            for (int x = 0; x < itemName.Length; x++)
            {
                Item itemToWrite = new Item();
                itemToWrite.ItemName = itemName[x];
                ItemValue itemValue = new ItemValue(itemToWrite);
                itemValue.Value = value[x];
                writeList.Add(itemToWrite);
                valueList.Add(itemValue);
            }



            //IMPORTANT:
            //#1: assign the item to the group so the items gets a ServerHandle
            groupWrite.AddItems(writeList.ToArray());
            // #2: assign the server handle to the ItemValue
            for (int i = 0; i < valueList.Count; i++)
                valueList[i].ServerHandle = groupWrite.Items[i].ServerHandle;
            // #3: write
            groupWrite.Write(valueList.ToArray());
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
            Opc.Da.Item[] tag_items = new Opc.Da.Item[2];
            tag_items[0] = new Opc.Da.Item();
            tag_items[0].ItemName = "SDV_1600";
            tag_items[1] = new Opc.Da.Item();
            tag_items[1].ItemName = "SDV_1601";
            return tag_items;
        }

        public Opc.Da.Item[] createTagItems()
        {
            Opc.Da.Item[] tag_items = new Opc.Da.Item[2];


            if (tag_items[0] == null)
            {
                try
                {
                    for (int tagcount = 0; tagcount < Global.tagnames.Count; tagcount++)
                    {
                        tag_items[tagcount] = new Opc.Da.Item();
                        tag_items[tagcount].ItemName = Global.tagnames[tagcount];

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }

            }

            return tag_items;
        }

        public void UpdateTagData(object subscriptionHandle, object requestHandle, Opc.Da.ItemValueResult[] values)
        {
            Console.WriteLine("hit");
            for (int i = 0; i < values.Length; i++)
            {
                {
                    var x = values[i].ItemName.Substring(0, 6);

                    if (values[i].ItemName.Substring(0, 6) == "[RTU1]")
                    {
                        HandleRTU1data(values[i]);
                    }
                    else if (values[i].ItemName.Substring(0, 6) == "[RTU2]")
                    {
                        HandleRTU2data(values[i]);
                    }
                }


            }
            UpdateTagDataStore(OPCObjList);
            // UPDATE MASTER ON STATUS

            List<MasterCommandStatus> Master_status = new List<MasterCommandStatus>();
            Master_status = UpdateMasterOnStatus();
            int index1 = Master_status.FindIndex(delegate (MasterCommandStatus masterCommand)
            {
                return masterCommand.label.Contains("RTU1");
            });

            int index2 = Master_status.FindIndex(delegate (MasterCommandStatus masterCommand)
            {
                return masterCommand.label.Contains("RTU2");
            });

            RTU1.RTU1MasterCommandStatus = Master_status[index1];
            RTU2.RTU2MasterCommandStatus = Master_status[index2];


        }


        public List<MasterCommandStatus> UpdateMasterOnStatus()
        {

            List<ConfigurationData> RTU1configuration;
            List<ConfigurationData> RTU2configuration;
            MasterCommandStatus Master_status1;
            MasterCommandStatus Master_status2;
            List<MasterCommandStatus> Master_status = new List<MasterCommandStatus>();
            RTU1configuration = RTU1.configuration;
            RTU2configuration = RTU2.configuration;

            int index1 = RTU1configuration.FindIndex(delegate (ConfigurationData config)
            {
                return config.Name == "[RTU1]CF_MASTER_ON_ACTIVE";
            });
            int index2 = RTU2configuration.FindIndex(delegate (ConfigurationData config)
            {
                return config.Name == "[RTU2]CF_MASTER_ON_ACTIVE";
            });

            int MASTER_STATUS1 = (Int32)RTU1configuration[index1].Value;
            int MASTER_STATUS2 = (Int32)RTU2configuration[index2].Value;

            bool oneccr_on1 = PartialOnCheck.CheckIfAnyCCROn(10, "RTU1");
            bool oneccr_on2 = PartialOnCheck.CheckIfAnyCCROn(9, "RTU2");

            if (MASTER_STATUS1 == 1)
            {
                Master_status1 = new MasterCommandStatus("ON", 1, "RTU1_MASTER_ON");
            }
            else if (MASTER_STATUS1 == 0 && oneccr_on1 == true)
            {
                Master_status1 = new MasterCommandStatus("PARTIAL_ON", 1, "RTU1_MASTER_ON");
            }
            else
            {
                Master_status1 = new MasterCommandStatus("OFF", 1, "RTU1_MASTER_ON");
            }



            if (MASTER_STATUS2 == 1)
            {
                Master_status2 = new MasterCommandStatus("ON", 1, "RTU2_MASTER_ON");
            }
            else if (MASTER_STATUS2 == 0 && oneccr_on2 == true)
            {
                Master_status2 = new MasterCommandStatus("PARTIAL_ON", 1, "RTU2_MASTER_ON");
            }
            else
            {
                Master_status2 = new MasterCommandStatus("OFF", 1, "RTU2_MASTER_ON");
            }

            Master_status.Add(Master_status1);
            Master_status.Add(Master_status2);

            return Master_status;
        }
        public void UpdateTagWrite(object clientHandle, Opc.IdentifiedResult[] results)
        {

        }

        private void HandleRTU2data(Opc.Da.ItemValueResult value)
        {

            switch (value.ItemName.Substring(6, 2))
            {
                case "PM":
                    UpdatePMResourceData(value, RTU2.powermeterdata);
                    break;
                case "DP":
                    UpdateDPResourceData(value, RTU2.dprdata);
                    break;
                case "ND":
                    UpdateNDResourceData(value, RTU2.networkdata);
                    break;
                case "CD":
                    UpdateCDResourceData(value, RTU2.cpudata);
                    break;
                case "MD":
                    UpdateMDResourceData(value, RTU2.moduleInfo);
                    break;
                case "MM":
                    UpdateMMResourceData(value, RTU2.modbusmoduleinfo);
                    break;
                case "CS":
                    UpdateCSResourceData(value, RTU2.ccrstatus, RTU2.textout);
                    break;
                case "AS":
                    UpdateASResourceData(value, RTU2.alarmsummary);
                    break;
                case "AH":
                    UpdateAHResourceData(value, RTU2.alarmhistory);
                    break;
                case "CF":
                    UpdateCFResourceData(value, RTU2.configuration);
                    break;
                default:
                    OPCObjList.Insert(8, value);
                    break;
            }
        }

        public void HandleRTU1data(Opc.Da.ItemValueResult value)
        {


            switch (value.ItemName.Substring(6, 2))
            {
                case "PM":
                    UpdatePMResourceData(value, RTU1.powermeterdata);
                    break;
                case "DP":
                    UpdateDPResourceData(value, RTU1.dprdata);
                    break;
                case "ND":
                    UpdateNDResourceData(value, RTU1.networkdata);
                    break;
                case "CD":
                    UpdateCDResourceData(value, RTU1.cpudata);
                    break;
                case "MD":
                    UpdateMDResourceData(value, RTU1.moduleInfo);
                    break;
                case "MM":
                    UpdateMMResourceData(value, RTU1.modbusmoduleinfo);
                    break;
                case "CS":
                    UpdateCSResourceData(value, RTU1.ccrstatus, RTU1.textout);
                    break;
                case "AS":
                    UpdateASResourceData(value, RTU1.alarmsummary);
                    break;
                case "AH":
                    UpdateAHResourceData(value, RTU1.alarmhistory);
                    break;
                case "CF":
                    UpdateCFResourceData(value, RTU1.configuration);
                    break;
                default:
                    OPCObjList.Insert(8, value);
                    break;
            }
        }

        private void UpdateCFResourceData(ItemValueResult value, List<ConfigurationData> configurationinfo)
        {
            configurationinfo[configurationinfo.FindIndex(delegate (ConfigurationData configurationdata)
            {
                return configurationdata.Name == value.ItemName;
            })]
                  .Value = System.Convert.ToDouble(value.Value);
        }

        private void UpdateMMResourceData(ItemValueResult value, List<ModbusModuleInfo> modbusmoduleinfo)
        {
            modbusmoduleinfo[modbusmoduleinfo.FindIndex(delegate (ModbusModuleInfo modbusmoduledata)
            {
                return modbusmoduledata.Name == value.ItemName;
            })]
                    .Value = System.Convert.ToDouble(value.Value);
        }

        private void UpdateAHResourceData(ItemValueResult value, List<AlarmData> alarmhistory)
        {
            alarmhistory[alarmhistory.FindIndex(delegate (AlarmData alarmhistorydata)
            {
                return alarmhistorydata.Name == value.ItemName;
            })]
                  .Value = System.Convert.ToDouble(value.Value);
        }

        private void UpdateASResourceData(ItemValueResult value, List<AlarmData> alarmsummary)
        {
            alarmsummary[alarmsummary.FindIndex(delegate (AlarmData alarmsummarydata)
            {
                return alarmsummarydata.Name == value.ItemName;
            })]
                   .Value = System.Convert.ToDouble(value.Value);
        }

        private void UpdateCSResourceData(ItemValueResult value, List<CCRStatus> ccrstatus, List<AflcmsStateMsg> textRes)
        {
            ccrstatus[ccrstatus.FindIndex(delegate (CCRStatus ccrstatusdata)
            {
                return ccrstatusdata.Name == value.ItemName;
            })]
                     .Value = System.Convert.ToDouble(value.Value);


            int index = textRes.FindIndex(delegate (AflcmsStateMsg textResdata)
            {
                return value.ItemName.Contains(textResdata.label);
            });
            if (textRes[index].label.Length == 4)
            {
                textRes[index].status = UpdateCCrStatus(ccrstatus, Int32.Parse(textRes[index].label[3].ToString()));
            }
            else if (textRes[index].label.Length == 5)
            {
                string ccr10 = textRes[index].label.Substring(3, 2).ToString();
                textRes[index].status = UpdateCCrStatus(ccrstatus, Int32.Parse(ccr10.ToString()));
            }

        }

        private string UpdateCCrStatus(List<CCRStatus> ccrstatus, int id)
        {
            string status = "OFF";
            string label = null;

            if (ccrstatus == RTU1.ccrstatus)
            {
                Int32 ccrstatus_int_value1 = System.Convert.ToInt32(RTU1.ccrstatus[id - 1].Value);
                bool AC_STATUS1 = BitChecker.XamineIfBitOn(ccrstatus_int_value1, 25);
                if (AC_STATUS1 == true)
                {
                    //GET INDIVIDUAL CIRCICUIT FEEDBACKS
                    bool C1_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value1, 14);
                    bool C2_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value1, 15);
                    bool C3_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value1, 16);
                    bool C4_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value1, 17);

                    bool _1PC_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value1, 9);
                    bool _3PC_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value1, 10);
                    bool _10PC_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value1, 11);
                    bool _30PC_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value1, 12);
                    bool _100PC_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value1, 13);

                    //CHECK IF ANY IF THE CIRCUIT IS ON
                    if (BitChecker.CS_Checker(id) == true)
                    {
                        if (C1_FB || C2_FB || C3_FB || C4_FB)
                        {
                            if (_1PC_FB || _3PC_FB || _10PC_FB || _30PC_FB || _100PC_FB)
                            {
                                status = "ON";
                            }
                        }
                    }
                    else
                    {

                        if (_1PC_FB || _3PC_FB || _10PC_FB || _30PC_FB || _100PC_FB)
                        {
                            status = "ON";
                        }

                    }
                }
                else
                {
                    status = "OFF";
                }
            }
            else if (ccrstatus == RTU2.ccrstatus)
            {
                Int32 ccrstatus_int_value2 = System.Convert.ToInt32(RTU2.ccrstatus[id - 1].Value);
                bool AC_STATUS2 = BitChecker.XamineIfBitOn(ccrstatus_int_value2, 25);

                if (AC_STATUS2 == true)
                {
                    //GET INDIVIDUAL CIRCICUIT FEEDBACKS
                    bool C1_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value2, 14);
                    bool C2_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value2, 15);
                    bool C3_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value2, 16);
                    bool C4_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value2, 17);

                    bool _1PC_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value2, 9);
                    bool _3PC_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value2, 10);
                    bool _10PC_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value2, 11);
                    bool _30PC_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value2, 12);
                    bool _100PC_FB = BitChecker.XamineIfBitOn(ccrstatus_int_value2, 13);

                    //CHECK IF ANY IF THE CIRCUIT IS ON
                    if (BitChecker.CS_Checker(id) == true)
                    {
                        if (C1_FB || C2_FB || C3_FB || C4_FB)
                        {
                            if (_1PC_FB || _3PC_FB || _10PC_FB || _30PC_FB || _100PC_FB)
                            {
                                status = "ON";
                            }
                        }
                    }
                    else
                    {

                        if (_1PC_FB || _3PC_FB || _10PC_FB || _30PC_FB || _100PC_FB)
                        {
                            status = "ON";
                        }

                    }
                }
                else
                {
                    status = "OFF";
                }
            }


            label = "CCR" + id + "_STATUS";

            return status;
        }

        private void UpdateMDResourceData(ItemValueResult value, List<ModuleInfo> moduleInfo)
        {
            moduleInfo[moduleInfo.FindIndex(delegate (ModuleInfo moduleInfodata)
            {
                return moduleInfodata.Name == value.ItemName;
            })]
                   .Value = System.Convert.ToDouble(value.Value);
        }

        private void UpdateCDResourceData(ItemValueResult value, List<CPUData> cpudata)
        {
            cpudata[cpudata.FindIndex(delegate (CPUData cpudatadata)
            {
                return cpudatadata.Name == value.ItemName;
            })]
                   .Value = System.Convert.ToDouble(value.Value);
        }

        private void UpdateNDResourceData(ItemValueResult value, List<NetWorkData> networkdata)
        {
            networkdata[networkdata.FindIndex(delegate (NetWorkData NetWorkdata)
            {
                return NetWorkdata.Name == value.ItemName;
            })]
                    .Value = System.Convert.ToDouble(value.Value);
        }

        private void UpdateDPResourceData(ItemValueResult value, List<DprData> dprdata)
        {
            dprdata[dprdata.FindIndex(delegate (DprData Dprdata)
            {
                return Dprdata.Name == value.ItemName;
            })]
                    .Value = System.Convert.ToDouble(value.Value);
        }

        private void UpdatePMResourceData(ItemValueResult value, List<PowerMeterData> powermeterdata)
        {
            powermeterdata[powermeterdata.FindIndex(delegate (PowerMeterData pmdata)
                    {
                        return pmdata.Name == value.ItemName;
                    })]
                    .Value = System.Convert.ToDouble(value.Value);

        }

        public void UpdateTagDataStore(List<ItemValueResult> OPCObjList)
        {
            Global.opclist = OPCObjList;
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