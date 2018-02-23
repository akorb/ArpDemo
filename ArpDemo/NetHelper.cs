using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using PcapDotNet.Packets;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace ArpDemo
{
    public static class NetHelper
    {
        public static string GetMacAddress(LivePacketDevice device, string targetAddress)
        {
            NetworkInterface currentInterface = device.GetNetworkInterface();
            byte[] mac = currentInterface.GetPhysicalAddress().GetAddressBytes();
            byte[] ipSender = currentInterface.GetIPProperties().UnicastAddresses.First(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Address.GetAddressBytes();
            byte[] ipTarget = IPAddress.Parse(targetAddress).GetAddressBytes();

            // 14 bytes: Ethernet header
            // 28 bytes: Arp data
            byte[] buffer = new byte[14 + 28];

            /***** ETHERNET *****/
            // Destination MAC address
            buffer[0] = byte.MaxValue;
            buffer[1] = byte.MaxValue;
            buffer[2] = byte.MaxValue;
            buffer[3] = byte.MaxValue;
            buffer[4] = byte.MaxValue;
            buffer[5] = byte.MaxValue;

            //Source MAC address
            buffer[6] = mac[0];
            buffer[7] = mac[1];
            buffer[8] = mac[2];
            buffer[9] = mac[3];
            buffer[10] = mac[4];
            buffer[11] = mac[5];
            // Type
            buffer[12] = 8;
            buffer[13] = 6;

            /***** ARP *****/
            // Hardware type
            buffer[14 + 0] = 0;
            buffer[14 + 1] = 1;
            // Protocol type
            buffer[14 + 2] = 8;
            buffer[14 + 3] = 0;
            // Hardware address length
            buffer[14 + 4] = 6;
            // Protocol address length
            buffer[14 + 5] = 4;
            // Operation
            buffer[14 + 6] = 0;
            buffer[14 + 7] = 1;
            // Sender hardware address
            buffer[14 + 8] = mac[0];
            buffer[14 + 9] = mac[1];
            buffer[14 + 10] = mac[2];
            buffer[14 + 11] = mac[3];
            buffer[14 + 12] = mac[4];
            buffer[14 + 13] = mac[5];
            // Sender protocol address
            buffer[14 + 14] = ipSender[0];
            buffer[14 + 15] = ipSender[1];
            buffer[14 + 16] = ipSender[2];
            buffer[14 + 17] = ipSender[3];
            // Target hardware address
            buffer[14 + 18] = byte.MaxValue;
            buffer[14 + 19] = byte.MaxValue;
            buffer[14 + 20] = byte.MaxValue;
            buffer[14 + 21] = byte.MaxValue;
            buffer[14 + 22] = byte.MaxValue;
            buffer[14 + 23] = byte.MaxValue;
            // Target protocol address
            buffer[14 + 24] = ipTarget[0];
            buffer[14 + 25] = ipTarget[1];
            buffer[14 + 26] = ipTarget[2];
            buffer[14 + 27] = ipTarget[3];

            Packet pkt = new Packet(buffer, DateTime.Now, DataLinkKind.Ethernet);

            PacketCommunicator connection = device.Open(65536, PacketDeviceOpenAttributes.None, 100);
            connection.SetFilter($"arp and ether dst {device.GetMacAddress()} and src host {targetAddress}");
            connection.SendPacket(pkt);

            Stopwatch watch = Stopwatch.StartNew();
            // Retrieve the packets
            do
            {
                PacketCommunicatorReceiveResult result = connection.ReceivePacket(out Packet packet);
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
