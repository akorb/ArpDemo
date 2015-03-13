using System;

namespace GetMacByIp
{
	class Program
	{
		static void Main()
		{
			Console.Write("IP Adresse: ");
			String ipTargetInput = Console.ReadLine();

			Console.WriteLine(NetHelper.GetMacAddress(ipTargetInput));

			Console.ReadLine();
		}
	}
}
