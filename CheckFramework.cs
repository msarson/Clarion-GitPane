using System;
using System.Reflection;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: CheckFramework.exe <dll-path>");
            return;
        }
        
        try
        {
            var asm = Assembly.LoadFile(args[0]);
            Console.WriteLine("Assembly: " + asm.GetName().Name);
            Console.WriteLine("Version: " + asm.GetName().Version);
            Console.WriteLine("Runtime: " + asm.ImageRuntimeVersion);
            
            var targetFramework = asm.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false);
            if (targetFramework.Length > 0)
            {
                var attr = (System.Runtime.Versioning.TargetFrameworkAttribute)targetFramework[0];
                Console.WriteLine("Target Framework: " + attr.FrameworkName);
            }
            else
            {
                Console.WriteLine("Target Framework: Not specified (legacy .NET 2.0/3.5)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
