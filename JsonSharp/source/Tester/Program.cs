using System;
using System.Collections.Generic;
using JsonSharp;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {   
            System.IO.File.WriteAllText("serialize.json",
                JsonObject.Parse(System.IO.File.ReadAllText("hmcl.json")).ToString());
        }
    }
}
