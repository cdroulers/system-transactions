using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions.Workflows;

namespace System.Transactions.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string wot = "lol";
            using (var context = new WorkflowContext())
            {
                context.Act(() => wot += "wot").CompensateWith(() => wot = "lol").Execute();

                Console.WriteLine(wot);

                context.Complete();
            }

            Console.WriteLine(wot);

            Console.WriteLine("Done, press shit");
            Console.ReadLine();
        }
    }
}
