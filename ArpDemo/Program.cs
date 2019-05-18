using PcapDotNet.Core;
using System;

namespace ArpDemo
{
    class Program
    {
        static void Main()
        {
            LivePacketDevice device = GetActivePacketDevice();

            Console.Clear();

            Console.Write("IP Address: ");
            string ipTargetInput = Console.ReadLine();
            string macTarget = NetHelper.GetMacAddress(device, ipTargetInput);

            Console.WriteLine(macTarget);
            Console.WriteLine();
            Console.Write("Press Enter to exit");
            Console.ReadLine();
        }

        static LivePacketDevice GetActivePacketDevice()
        {
            var interfaces = LivePacketDevice.AllLocalMachine;

            if (interfaces.Count == 1)
                return interfaces[0];

            string input;
            int result;
            do
            {
                Console.Clear();
                for (int i = 0; i < interfaces.Count; i++)
                {
                    Console.WriteLine("[" + (i + 1) + "]: " + interfaces[i].Description);
                }

                Console.Write("Choose an index: ");
                input = Console.ReadLine();
            }
            while (!int.TryParse(input, out result) || --result >= interfaces.Count);

            return interfaces[result];
        }
    }
}
