using PcapDotNet.Core;
using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

namespace GetMacByIp
{
	public static class NetHelper
	{
		public static String GetMacAddress(LivePacketDevice device, String targetAddress)
		{
			DeviceAddress address = device.Addresses[1];

			NetworkInterface currentInterface = NetworkInterface.GetAllNetworkInterfaces().First(x => x.OperationalStatus == OperationalStatus.Up);
			byte[] mac = currentInterface.GetPhysicalAddress().GetAddressBytes();
			byte[] ipSender = currentInterface.GetIPProperties().UnicastAddresses.First(x => x.PrefixLength == 24).Address.GetAddressBytes();
			byte[] ipTarget = targetAddress.Split('.').Select(x => byte.Parse(x)).ToArray();

			byte[] arpData = new byte[28];
			// HRD
			arpData[1] = 1;
			// PRO
			arpData[2] = 8;
			arpData[3] = 0;
			// HLN
			arpData[4] = 6;
			// PLN
			arpData[5] = 4;
			// OP
			arpData[6] = 0;
			arpData[7] = 1;
			// SHA
			arpData[8] = mac[0];
			arpData[9] = mac[1];
			arpData[10] = mac[2];
			arpData[11] = mac[3];
			arpData[12] = mac[4];
			arpData[13] = mac[5];
			// SPA
			arpData[14] = ipSender[0];
			arpData[15] = ipSender[1];
			arpData[16] = ipSender[2];
			arpData[17] = ipSender[3];
			// THA
			arpData[18] = byte.MaxValue;
			arpData[19] = byte.MaxValue;
			arpData[20] = byte.MaxValue;
			arpData[21] = byte.MaxValue;
			arpData[22] = byte.MaxValue;
			arpData[23] = byte.MaxValue;
			// TPA
			arpData[24] = ipTarget[0];
			arpData[25] = ipTarget[1];
			arpData[26] = ipTarget[2];
			arpData[27] = ipTarget[3];


			byte[] ethernetData = new byte[14];
			// Destination MAC address
			ethernetData[0] = byte.MaxValue;
			ethernetData[1] = byte.MaxValue;
			ethernetData[2] = byte.MaxValue;
			ethernetData[3] = byte.MaxValue;
			ethernetData[4] = byte.MaxValue;
			ethernetData[5] = byte.MaxValue;
			//Source MAC address
			ethernetData[6] = mac[0];
			ethernetData[7] = mac[1];
			ethernetData[8] = mac[2];
			ethernetData[9] = mac[3];
			ethernetData[10] = mac[4];
			ethernetData[11] = mac[5];
			// Type
			ethernetData[12] = 8;
			ethernetData[13] = 6;

			var fullPacketData = new List<byte>();
			fullPacketData.AddRange(ethernetData);
			fullPacketData.AddRange(arpData);

			String macAsText = currentInterface.GetPhysicalAddress().ToString();
			for (int i = macAsText.Length - 2; i >= 2; i -= 2)
				macAsText = macAsText.Insert(i, ":");


			Packet pkt = new Packet(fullPacketData.ToArray(), DateTime.Now, DataLinkKind.Ethernet);

			String tmp = currentInterface.GetIPProperties().UnicastAddresses.First(x => x.PrefixLength == 24).Address.ToString();

			PacketCommunicator connection = device.Open(65536, PacketDeviceOpenAttributes.None, 100);
			connection.SetFilter("arp and ether dst " + macAsText + " and src host " + targetAddress);
			connection.SendPacket(pkt);

			Stopwatch watch = Stopwatch.StartNew();

			// Retrieve the packets
			Packet packet;
			do
			{
				PacketCommunicatorReceiveResult result = connection.ReceivePacket(out packet);
				switch (result)
				{
					case PacketCommunicatorReceiveResult.Timeout:
						break;
					case PacketCommunicatorReceiveResult.Ok:
						return packet.Ethernet.Source.ToString();
					default:
						throw new InvalidOperationException("The result " + result + " should never be reached here");
				}
			}
			while (watch.ElapsedMilliseconds < 5000);

			return "Not found";
		}
	}
}
