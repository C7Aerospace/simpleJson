using System;
using System.Collections.Generic;
using JsonSharp;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {   
            string read = System.IO.File.ReadAllText("hmcl.json");
            DateTime now = DateTime.Now;
            JsonObject obj = JsonObject.Parse(read);
            DateTime finish = DateTime.Now;
            Console.WriteLine("Parse in {0} s", (finish - now).TotalSeconds);
            System.IO.File.WriteAllText("serialize.json", obj.Format());
        }
    }
}
