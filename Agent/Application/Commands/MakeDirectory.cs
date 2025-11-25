using AgentClient.Domain.Models.Agents;

namespace AgentClient.Application.Commands
{
    public class MakeDirectory : AgentCommand
    {
        public override string Name => "mkdir";

        public override string Execute(AgentTask task)
        {
            if (task.Arguments == null || task.Arguments.Length == 0)
            {
                return "No path provided";
            }

            var path = task.Arguments[0];
            Directory.CreateDirectory(path);

            if (Directory.Exists(path))
            {
                return $"{path} created";
            }

            return $"The directory {path} already exists";
        }
    }
}
