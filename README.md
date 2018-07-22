# OTlinkIT
This console application reads Shutdown valve information from a ControlLogix PLC.
RSLogixEmulator used as AB ControlLogix PLC. RSLinx Remote OPC Server is used as OPC server.
RSLinx Classic is used to configure OPC topic. The code runs in a rmote machine that connects to the 
OPC server via a OPC tunnel software. This is to avoid any DCOM issues.

OpcNetApi, OpcNetApi.com, OpcNetApi.Xml are downloaded from OPC foundation website.

This program reads valves namely SDV_1600, SDV_1601, SDV_1602 TO SDV_1604 from PLC emulator and 
updates their values in realtime to a static list. From this static list. Further code can be implemented.
