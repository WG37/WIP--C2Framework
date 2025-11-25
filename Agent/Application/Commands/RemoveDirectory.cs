

using AgentClient.Domain.Models.Agents;

namespace AgentClient.Application.Commands
{
    public class RemoveDirectory : AgentCommand
    {
        public override string Name => "rmdir";

        public override string Execute(AgentTask task)
        {
            if (task.Arguments == null || task.Arguments.Length == 0)
            {
                return "No path provided";
            }

            var path = task.Arguments[0];
            Directory.Delete(path, true);

            if (!Directory.Exists(path))
            {
                return $"{path} deleted";
            }

            return "Failed to delete the directory";
        }
    }
}
