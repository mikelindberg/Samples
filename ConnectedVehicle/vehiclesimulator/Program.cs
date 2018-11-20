using System;

namespace vehiclesimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                foreach (string connectionString in args)
                {
                    DeviceSimulator simulator = new DeviceSimulator(connectionString);
                }
            }

            Console.ReadLine();
        }
    }
}
