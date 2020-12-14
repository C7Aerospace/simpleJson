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
            string read = System.IO.File.ReadAllText("MOCK_DATA.json");
            DateTime now = DateTime.Now;
            // JsonObject obj = JsonObject.Parse(read);
            JsonObject obj = JsonObject.Parse(read);
            DateTime finish = DateTime.Now;
            Console.WriteLine("Parse in {0} s", (finish - now).TotalSeconds);
            StringBuilder strb = new StringBuilder(16384);
            now = DateTime.Now;
            string ts = "";
            ts = obj.Serialize();
            // obj.Serialize(ref strb, "", "    ");
            finish = DateTime.Now;
            Console.WriteLine("Serialize in {0} s", (finish - now).TotalSeconds);
            System.IO.File.WriteAllText("serialize.json", ts);
        }
    }
}
