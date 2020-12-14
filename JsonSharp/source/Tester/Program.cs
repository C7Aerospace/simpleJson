using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JsonSharp;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {   
            string read = System.IO.File.ReadAllText("in.json");
            DateTime now = DateTime.Now;
            // JsonObject obj = JsonObject.Parse(read);
            JsonValue obj = JsonValue.Parse(read);
            DateTime finish = DateTime.Now;
            Console.WriteLine("Parse in {0} ms", (finish - now).TotalMilliseconds);
            now = DateTime.Now;
            string ts = "";
            ts = obj.Serialize();
            // obj.Serialize(ref strb, "", "    ");
            finish = DateTime.Now;
            Console.WriteLine("Serialize in {0} ms", (finish - now).TotalMilliseconds);
            System.IO.File.WriteAllText("out.json", ts);
        }
    }
}
