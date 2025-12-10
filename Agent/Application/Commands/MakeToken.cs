using AgentClient.Domain.Models.Agents;
using AgentClient.Infrastructure.Native;

namespace AgentClient.Application.Commands
{
    public class MakeToken : AgentCommand
    {
        public override string Name => "make-token";

        public override string Execute(AgentTask task)
        {
            

            var userDomain = task.Arguments[0];
            var password = task.Arguments[1];

            var split = userDomain.Split("\\");
            var domain = split[0];
            var username = split[1];

            IntPtr hToken;
            
            if (Advapi32.LogonUserA(username, domain, password,
                            Advapi32.LogonUserType.LOGON32_LOGON_NEW_CREDENTIALS,
                            Advapi32.LogonUserProvider.LOGON32_PROVIDER_DEFAULT, out hToken))
            {
                // impersonate token
            }

            return "Failed to create token";
        }
    }
}
