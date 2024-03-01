# Project Documentation: NETWORK SNIFFER (variant ZETA)
## Introduction

ipk-sniffer is a command-line network tool designed to capture packets on a specific network interface. It supports filtering packets based on TCP, UDP, ARP, ICMPv4, ICMPv6, IGMP, and MLD protocols, along with port numbers.

The following documentation describes the IPK sniffer implementation in C#. The implementation utilizes the SharpPcap library when sniffing.


## Requirements

To compile and run the IPK-SNIFFER you have to use following libraries:

-   `System`
-   `PacketDotNet`
-   `SharpPcap`

## Usage
Program can be build using command `make` and executed with the following command: 

 `./ipk-sniffer [-i interface | --interface interface] {-p port [--tcp|-t]  [--udp|-u]} [--arp]  [--icmp4]  [--icmp6]  [--igmp]  [--mld] {-n num}`
 
 options aviable are:
 - `-h `: for help message
-   `-i or --interface interface`: Listen on interface
if only -i / --interface withouth other arguments program prints list of aviable intrerfaces
-   `-p port`: 			Filter by port
-   `-t or --tcp`: 	Filter by TCP
-   `-u or --udp`: 	Filter by UDP
-   `--icmp4`: 			Filter by ICMPv4
-   `--icmp6`: 			Filter by ICMPv6
-   `--arp`: 				Filter by ARP
-   `--ndp`: 				Filter by NDP
-   `--igmp`: 				Filter by IGMP
-   `--mld`: 				Filter by MLD
-   `-n count`: 			Shows first n packets
All arguments can be in any order
If no arguments are specified program prints list of aviable interfaces  

Example usage:
`./ipk-sniffer -i eth0 --igmp --mld -n 10`   

build can also be cleaned using command `make clean`

## Functionality

The tool operates in promiscuous mode, allowing it to capture all packets on the network interface, regardless of whether they are addressed to the host running the tool.

The output of the tool displays the following information for each captured packet:

`timestamp:`  _time_

`src MAC:`  MAC address with : as separator

`dst MAC:`  MAC address with : as separator

`frame length:`  _length_

`src IP:`  IP address if any (support v4 but also v6 representation according to RFC5952)

`dst IP:`  IP address if any (support v4 but also v6 representation according to RFC5952)

`src port:`  port number if any

`dst port:`  port number if any

`byte_offset:`  byte_offset_hexa byte_offset_ASCII

example output: 
````text
./ipk-sniffer -i eth0
timestamp: 2021-03-19T18:42:52.362+01:00
src MAC: 00:1c:2e:92:03:80
dst MAC: 00:1b:3f:56:8a:00
frame length: 512 bytes
src IP: 147.229.13.223
dst IP: 10.10.10.56
src port: 4093
dst port: 80

0x0000: 00 19 d1 f7 be e5 00 04 96 1d 34 20 08 00 45 00 ........ ..4 ..
0x0010: 05 a0 52 5b 40 00 36 06 5b db d9 43 16 8c 93 e5 ..R[@.6. [..C....
0x0020: 0d 6d 00 50 0d fb 3d cd 0a ed 41 d1 a4 ff 50 18 .m.P..=. ..A...P.
0x0030: 19 20 c7 cd 00 00 99 17 f1 60 7a bc 1f 97 2e b7 . ...... .`z.....
0x0040: a1 18 f4 0b 5a ff 5f ac 07 71 a8 ac 54 67 3b 39 ....Z._. .q..Tg;9
0x0050: 4e 31 c5 5c 5f b5 37 ed bd 66 ee ea b1 2b 0c 26 N1.\_.7. .f...+.&
0x0060: 98 9d b8 c8 00 80 0c 57 61 87 b0 cd 08 80 00 a1 .......W a.......
````

for each packet that is not filtered away 
each packets are delimited by empty line

# Implementation

The IPK-SNIFFER is implemeted in C# and consists of the following classes :

### Program 
Contains only Main method that only controlls if there are any interfaces aviable and calls at first argument controll then sniffer with arguments 
### Arguments

The class contains properties to store the values of the command-line arguments, including `InterfaceName` (string), `FilterPort` (int), `FilterTcp`, `FilterUdp`, `FilterIcmp4`, `FilterIcmp6`, `FilterArp`, `FilterNdp`, `FilterIgmp`, `FilterMld` (all bool), and `NumPackets` (int). 

The `print_arguments` method is used to print the values of these properties to the console.

The `argument_controll` method is used to parse and validate the command-line arguments. It takes an array of strings (`args`) as input and returns an integer to indicate whether parsing was successful (0) or not (1). 
If parsing fails, an error message is printed to the console. If the `-h` flag is specified, a help message is printed and the program is terminated. Otherwise, the method loops through the `args` array, and if it finds a valid argument, it updates the corresponding property in the `Arguments` object. If an invalid argument is encountered, an error message is printed and the method returns 1. If the method completes successfully, the updated `Arguments` object can be used by the `ipk-sniffer` program to set up packet capture and filtering.

### Sniffer

This class represents a sniffer that captures packets from a network interface and prints their details to the console. The class contains a static method, `Sniffing`, which takes an object of the `Arguments` class and sets up the sniffer with the specified options.

The `Sniffing` method first searches for the network interface specified in the `arg` parameter by iterating through all available network interfaces using the `LibPcapLiveDeviceList` class. If the interface is not found, an error message is printed to the console and the program exits.

Next, the method sets up a filter for the sniffer based on the options in the `arg` parameter. The `filter_setup` method is used to construct the filter expression string.

After the filter is set up, the sniffer starts capturing packets by calling the `StartCapture` method of the selected network interface. As packets arrive, the `PacketHandler` method is called to parse and print packet details to the console.

The `PacketHandler` method extracts various details from the captured packet, such as the MAC address, IP address, port number, and data in hex and ASCII format. If the `NumPackets` option is specified in the `arg` parameter, the sniffer stops capturing packets after the specified number of packets has been captured.

Finally, the `Console_CancelKeyPress` method is used as a callback function to handle the user pressing the `Ctrl+C` key combination to stop the sniffer. If the `device` object is null, an error message is printed to the console and the program exits. Otherwise, the sniffer stops capturing packets and exits the program.

## Testing

testing and validation of program functionality was done during whole process of making program while comparing output to output from network sniffer application wireshark

All tests were run while connected to internet using wi-fi on operation system Windows 11 or while connected with ethernet cable on operation system NixOS which was given as reference environment for this project

## Sources 
To develop this project were utilized following sources of information:
- IPK lectures and informations from course website
- SharpPcap documentation (https://github.com/dotpcap/sharppcap)
- Wikipedia, the free encyclopedia (http://en.wikipedia.org/wiki/Pcap)
- RFC 792 - Internet Control Message Protocol a RFC 4443 - ICMPv6
- RFC 826 - ARP
- RFC 5952 - A Recommendation for IPv6 Address Text Representation
- RFC 3339 - Date and Time on the Internet: Timestamps
