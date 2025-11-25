using AgentClient.Domain.Models.Agents;

namespace AgentClient.Application.Commands
{
    public class ChangeDirectory : AgentCommand
    {
        public override string Name => "cd";

        public override string Execute(AgentTask task)
        {
            string path;

            if (task.Arguments == null || task.Arguments.Length == 0)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            else
            {
                path = task.Arguments[0];
            }

            Directory.SetCurrentDirectory(path);
            return Directory.GetCurrentDirectory();
        }
    }
}
