using System.Runtime.InteropServices;

namespace AgentClient.Infrastructure.Native
{
    public class Kernel32
    {
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool Wow64Process);
    }
}
