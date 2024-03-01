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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Runtime.InteropServices;

    class Program
    {
        static int Main(string[] args)
        {
            // checks if there are any interfaces aviable
            var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            if (interfaces == null)
            {
                Console.Error.WriteLine("No interfaces aviable");
                return 1;
            }



            // calls the argument controll 
            Argument arguments = new Argument();       
            // if there is an error in the arguments it returns 1
            // and the program ends
            if (arguments.argument_controll(args) == 1)
            {
                return 1;
            }

            // calls the sniffer 
            Sniffer.Sniffing(arguments);
            return 0;
        }
    }
}