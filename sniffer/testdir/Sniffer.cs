/*
 * Copyright (c) 2023 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;


namespace ipk_sniffer
{
    // sniffer class
    public class Sniffer
    {
        // private variables
        private static int _numPackets = 0;
        private static int? NumPackets = null;
        private static ICaptureDevice? device = null;

        // does the sniffing
        public static void Sniffing(Argument arg)
        {

            // puts all aviable interfaces into a list and then chooses the one with the name from the arguments
            var devices = LibPcapLiveDeviceList.Instance;
            device = devices.FirstOrDefault(x => x.ToString().Contains(arg.InterfaceName));

            // crtl+c handler 
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);


            // if didnt find the interface prints error message and ends the program
            if (device == null)
            {
                Console.Error.WriteLine("Device not found: {0}", arg.InterfaceName);
                Environment.Exit(1);
            }

            // gets number of packets from arguments
            NumPackets = arg.NumPackets;

            // on arrival of package ctivates the packet handler
            device.OnPacketArrival += new PacketArrivalEventHandler(PacketHandler); ;

            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceModes.Promiscuous, readTimeoutMilliseconds);

            // sets up Filters from arguments
            device.Filter = filter_setup(arg);

            // capturing
            device.StartCapture();
            Console.ReadLine();
            device.Close();
        }

        // function packet handler is called for each packet that arrives and is filtered
        private static void PacketHandler(object sender, PacketCapture e)
        {
            // parses each packet
            var packet = PacketDotNet.Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);

            // Increment packet count
            _numPackets++;

            // parses the packet and gets the data
            var time = e.GetPacket().Timeval.Date.ToString("s");
            var len = e.GetPacket().Data.Length;
            var ethernet = packet.Extract<EthernetPacket>();
            var ip = packet.Extract<IPPacket>();
            var tcp = packet.Extract<TcpPacket>();
            var udp = packet.Extract<UdpPacket>();

            // prints the data for packets
            Console.WriteLine($"timestamp: {time}");
            var srcMac = ethernet?.SourceHardwareAddress?.ToString().ToLower();
            var dstMac = ethernet?.DestinationHardwareAddress?.ToString().ToLower();
            if (srcMac != null && dstMac != null)
            {
                Console.WriteLine($"src MAC: {string.Join(":", Enumerable.Range(0, 6).Select(i => srcMac.Substring(i * 2, 2)))}");
                Console.WriteLine($"dst MAC: {string.Join(":", Enumerable.Range(0, 6).Select(i => dstMac.Substring(i * 2, 2)))}");
            }
            Console.WriteLine($"frame length: {len} bytes");

            if (ip != null)
            {
                Console.WriteLine($"src IP: {ip.SourceAddress}");
                Console.WriteLine($"dst IP: {ip.DestinationAddress}");
            }

            if (tcp != null)
            {
                Console.WriteLine($"src port: {tcp?.SourcePort}");
                Console.WriteLine($"dst port: {tcp?.DestinationPort}");
            }

            if (udp != null)
            {
                Console.WriteLine($"src port: {udp?.SourcePort}");
                Console.WriteLine($"dst port: {udp?.DestinationPort}");
            }

            Console.WriteLine();

            // prints the data in hex and ascii
            var data = packet.Bytes;
            for (int i = 0; i < data.Length; i += 16)
            {
                var hex = new StringBuilder();
                var ascii = new StringBuilder();
                for (int j = 0; j < 16 && i + j < data.Length; j++)
                {
                    var b = data[i + j];
                    hex.Append($"{b:x2} ");
                    ascii.Append(b >= 32 && b <= 126 ? (char)b : '.');
                }
                Console.WriteLine($"{i:x4}: {hex.ToString().PadRight(48)}{ascii.ToString()}");
            }

            Console.WriteLine();

            // Stop capturing packets if reached maximum number of packets
            if (NumPackets != null && _numPackets >= NumPackets)
            {
                if (device == null)
                {
                    Console.Error.WriteLine("ERROR while working with interface");
                    Environment.Exit(1);
                }
                device.StopCapture();
                device.Close();
                Environment.Exit(0);
            }
        }

        // filter function that sets up the filter from arguments
        private static string filter_setup(Argument arg)
        {
            string filter = "";
            if (arg.FilterTcp)
            {
                filter += "tcp";
                if (arg.FilterPort != null)
                {
                    filter += " port "+ arg.FilterPort + " or ";
                }
                else
                {
                    filter += " or ";
                }
            }
            if (arg.FilterUdp)
            {
                filter += "udp";
                if (arg.FilterPort != null)
                {
                    filter += " port "+ arg.FilterPort + " or ";
                }
                else
                {
                    filter += " or ";
                }
            }
            if (arg.FilterIcmp4)
            {
                filter += "icmp or ";
            }
            if (arg.FilterIcmp6)
            {
                filter += "icmp6 or ";
            }
            if (arg.FilterArp)
            {
                filter += "arp or ";
            }
            if (arg.FilterNdp)
            {
                filter += "icmp6[0] == 135 or icmp6[0] == 136 or ";
            }
            if (arg.FilterIgmp)
            {
                filter += "igmp or ";
            }
            if (arg.FilterMld)
            {
                filter += "icmp6[0] == 130 or icmp6[0] == 143 or ";
            }
            if (filter.EndsWith("or "))
            {
                filter = filter.Remove(filter.Length - 4);
            }
            return filter;
        }
        // function that is called when ctrl+c is pressed
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (device == null)
            {
                Console.Error.WriteLine("ERROR while working with interface");
                Environment.Exit(1);
            }
            device.StopCapture();
            device.Close();
            Environment.Exit(0);
        }

    }
}
