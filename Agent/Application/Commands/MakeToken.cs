using AgentClient.Domain.Models.Agents;
using AgentClient.Infrastructure.Native;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace AgentClient.Application.Commands
{
    public class MakeToken : AgentCommand
    {
        public override string Name => "make-token";

        public override string Execute(AgentTask task)
        {
            var userDomain = task.Arguments[0];
            var password = task.Arguments[1];

            var split = userDomain.Split('\\');
            var domain = split[0];
            var username = split[1];

            IntPtr hToken = IntPtr.Zero;

            try
            {
                if (!Advapi32.LogonUserA(username, domain, password,
                            Advapi32.LogonUserType.LOGON32_LOGON_INTERACTIVE,
                            Advapi32.LogonUserProvider.LOGON32_PROVIDER_DEFAULT,
                            out hToken))
                {
                    int error = Marshal.GetLastWin32Error();

                    hToken = IntPtr.Zero;
                    return $"Failed to create token. Win32 Error: {error}";
                }

                if (!Advapi32.ImpersonateLoggedOnUser(hToken))
                {
                    return "Successful token creation, but failed to impersonate";
                }

                // TODO: Privileged tasks/actions 
                using (var identity = new WindowsIdentity(hToken))
                {
                    return $"Successful token impersonation {identity.Name}";
                }
            }
            finally
            {
                Advapi32.RevertToSelf();

                if (hToken != IntPtr.Zero)
                {
                    Kernel32.CloseHandle(hToken);
                }
            }
        }
    }
}
