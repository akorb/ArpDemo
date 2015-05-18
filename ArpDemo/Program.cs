﻿using PcapDotNet.Core;
using System;

namespace ArpDemo
{
	class Program
	{
		static void Main()
		{
			LivePacketDevice device = GetActivePacketDevice();

			Console.Clear();

			Console.Write("IP Adresse: ");
			String ipTargetInput = Console.ReadLine();

			Console.WriteLine(NetHelper.GetMacAddress(device, ipTargetInput));

			Console.ReadLine();
		}

		static LivePacketDevice GetActivePacketDevice()
		{
			if (LivePacketDevice.AllLocalMachine.Count == 1)
				return LivePacketDevice.AllLocalMachine[0];

			String input;
			int result;
			do
			{
				Console.Clear();
				for (int i = 0; i < LivePacketDevice.AllLocalMachine.Count; i++)
				{
					Console.WriteLine("[" + (i + 1) + "]: " + LivePacketDevice.AllLocalMachine[i].Description);
				}

				input = Console.ReadLine();
			}
			while (!int.TryParse(input, out result) || --result >= LivePacketDevice.AllLocalMachine.Count);

			return LivePacketDevice.AllLocalMachine[result];
		}
	}
}
