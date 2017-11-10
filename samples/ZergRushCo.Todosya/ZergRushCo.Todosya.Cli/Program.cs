using System;
using System.Linq;

namespace ZergRushCo.Todosya.Cli
{
    /// <summary>
    /// Entry point.
    /// </summary>
    class Program
    {
        public const string Command_RunTest = "RunTest";

        static int Main(string[] args)
        {
            if (args.Any() == false)
            {
                Console.WriteLine("Specify arguments.");
                return 1;
            }

            if (args[0] == Command_RunTest)
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("{0} <file>", Command_RunTest);
                    return 1;
                }
                new RunTestCommand().Run(args[1]);
                Console.ReadKey();
            }
            return 0;
        }
    }
}
