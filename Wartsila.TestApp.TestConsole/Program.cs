namespace Wartsila.TestApp.TestConsole
{
    class Program
    {
        private static void InvokeTest(string name, Action test)
        {
            Console.WriteLine();
            Console.WriteLine($"----- {name} ----- ");
            test();
            Console.WriteLine("------------------ ");
        }
        
        static void Main()
        {
            Console.WriteLine("BEGIN");
            Console.WriteLine();

            try
            {
                InvokeTest(nameof(ProblemA), ProblemA.Tests);
                InvokeTest(nameof(ProblemB), ProblemB.Tests);
                InvokeTest(nameof(ProblemC), ProblemC.Tests);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("EXCEPTION:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
            }
            
            Console.WriteLine();
            Console.WriteLine("END");
            Console.ReadLine();
        }
    }
}