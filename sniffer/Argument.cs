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
namespace ipk_sniffer
{
    using SharpPcap;
    using SharpPcap.LibPcap;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Text;
    using System.Threading.Tasks;

    public class  Argument
    {
        // defines the arguments
        // by default they are null or false
        public string? InterfaceName { get; set; }
        public int? FilterPort { get; set; }
        public bool FilterTcp { get; set; }
        public bool FilterUdp { get; set; }
        public bool FilterIcmp4 { get; set; }
        public bool FilterIcmp6 { get; set; }
        public bool FilterArp { get; set; }
        public bool FilterNdp { get; set; }
        public bool FilterIgmp { get; set; }
        public bool FilterMld { get; set; }
        public int NumPackets { get; set; }

        // controll function which prints the arguments
        public void print_arguments()
        {
            Console.WriteLine($"InterfaceName: {InterfaceName}");
            Console.WriteLine($"FilterPort: {FilterPort}");
            Console.WriteLine($"FilterTcp: {FilterTcp}");
            Console.WriteLine($"FilterUdp: {FilterUdp}");
            Console.WriteLine($"FilterIcmp4: {FilterIcmp4}");
            Console.WriteLine($"FilterIcmp6: {FilterIcmp6}");
            Console.WriteLine($"FilterArp: {FilterArp}");
            Console.WriteLine($"FilterNdp: {FilterNdp}");
            Console.WriteLine($"FilterIgmp: {FilterIgmp}");
            Console.WriteLine($"FilterMld: {FilterMld}");
            Console.WriteLine($"NumPackets: {NumPackets}");
        }



        // controll function which checks the arguments and fills the data
        public int argument_controll(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine();
                foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    Console.WriteLine("{0}", networkInterface.Name);
                }
                Environment.Exit(0);
            }
            if (args.Length == 1)
            {
                if (args[0] == "-h")
                {
                    // help message
                    Console.WriteLine("Usage:./ipk-sniffer [-i interface | --interface interface] {-p port [--tcp|-t] [--udp|-u]} [--arp] [--icmp4] [--icmp6] [--igmp] [--mld] {-n num}");
                    Console.WriteLine("Options:");
                    Console.WriteLine("-i or --interface interface\tListen on interface");
                    Console.WriteLine("-p port\t\t\t\tFilter by port");
                    Console.WriteLine("-t or --tcp\t\t\tFilter by TCP");
                    Console.WriteLine("-u or --udp\t\t\tFilter by UDP");
                    Console.WriteLine("--icmp4\t\t\t\tFilter by ICMPv4");
                    Console.WriteLine("--icmp6\t\t\t\tFilter by ICMPv6");
                    Console.WriteLine("--arp\t\t\t\tFilter by ARP");
                    Console.WriteLine("--ndp\t\t\t\tFilter by NDP");
                    Console.WriteLine("--igmp\t\t\t\tFilter by IGMP");
                    Console.WriteLine("--mld\t\t\t\tFilter by MLD");
                    Console.WriteLine("-n count\t\t\tShows first n packets");
                    Environment.Exit(0);
                }
                

            }
            // for loop for all arguments that if finds change values otherwise prints error massage and returns 1 
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-i" || args[i] == "--interface")
                {
                    if (i < args.Length - 1)
                    {
                        InterfaceName = args[i + 1];
                        i++;
                    }
                    else
                    {
                        // prints all aviable interfaces
                        Console.WriteLine();
                        foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                        {
                            Console.WriteLine("{0}", networkInterface.Name);
                        }
                        Console.WriteLine();
                        Environment.Exit(0);
                    }
                }
                else if (args[i] == "-p")
                {
                    if (i < args.Length - 1)
                    {
                        if (int.TryParse(args[i + 1], out int port))
                        {
                            FilterPort = port;
                            i++;
                        }
                        else
                        {
                            Console.Error.WriteLine($"Invalid port number: {args[i + 1]}");
                            return 1;
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("Port number not specified.");
                        return 1;
                    }
                }
                else if (args[i] == "-t" || args[i] == "--tcp")
                {
                    FilterTcp = true;
                }
                else if (args[i] == "-u" || args[i] == "--udp")
                {
                    FilterUdp = true;
                }
                else if (args[i] == "--icmp4")
                {
                    FilterIcmp4 = true;
                }
                else if (args[i] == "--icmp6")
                {
                    FilterIcmp6 = true;
                }
                else if (args[i] == "--arp")
                {
                    FilterArp = true;
                }
                else if (args[i] == "--ndp")
                {
                    FilterNdp = true;
                }
                else if (args[i] == "--igmp")
                {
                    FilterIgmp = true;
                }
                else if (args[i] == "--mld")
                {
                    FilterMld = true;
                }
                else if (args[i] == "-n")
                {
                    if (i < args.Length - 1)
                    {
                        if (int.TryParse(args[i + 1], out int count))
                        {
                            NumPackets = count;
                            i++;
                        }
                        else
                        {
                            Console.Error.WriteLine($"Invalid packet count: {args[i + 1]}");
                            return 1;
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("Packet count not specified.");
                        return 1;
                    }
                }
                else
                {
                    Console.Error.WriteLine($"Invalid argument: {args[i]}");
                    return 1;
                }
            }
            if (InterfaceName == null)
            {
                Console.Error.WriteLine("Interface not specified.");
                return 1;
            }

            if (FilterPort != null && FilterTcp == false && FilterUdp == false) 
            {
                Console.Error.WriteLine("port specified but neither Tcp nor Udp.");
                return 1;
            }
            return 0;
        }
    }
}
