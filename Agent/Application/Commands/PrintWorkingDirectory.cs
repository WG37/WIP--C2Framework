
using AgentClient.Domain.Models.Agents;

namespace AgentClient.Application.Commands
{
    public class PrintWorkingDirectory : AgentCommand
    {
        public override string Name => "pwd";

        public override string Execute(AgentTask task)
        {
            return Directory.GetCurrentDirectory();
        }
    }
}
