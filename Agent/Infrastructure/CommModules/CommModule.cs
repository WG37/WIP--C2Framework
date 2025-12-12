using AgentClient.Domain.Models.Agents;
using System.Collections.Concurrent;

namespace AgentClient.Infrastructure.CommModules
{
    public abstract class CommModule
    {
        public abstract Task Start();
        public abstract Task Stop();

        protected AgentMetadata Metadata { get; set; }

        protected ConcurrentQueue<AgentTask> Inbound { get; set; } = new();
        protected ConcurrentQueue<AgentTaskResult> Outbound { get; set; } = new();

        public virtual void Initialiser(AgentMetadata metadata)
        {
            Metadata = metadata;
        }

        // empties the inbound queue and returns all received tasks to be executed
        public bool ReceiveData(out IEnumerable<AgentTask> tasks)
        {
            var list = new List<AgentTask>();

            // checks if no tasks in q
            if (Inbound.IsEmpty)
            {
                tasks = null;
                return false;
            }

            while (Inbound.TryDequeue(out var task))
            {
                list.Add(task);
            }
            tasks = list;
            return true;
        }

        
        public void SendData(AgentTaskResult result)
        {
            Outbound.Enqueue(result);
        }


        // dequeues one outbound result so PostData() can send it to the server
        protected IEnumerable<AgentTaskResult> GetOutbound()
        {
            var list = new List<AgentTaskResult>();

            if (Outbound.TryDequeue(out var task))
            {
                list.Add(task);
            }
            return list;
        }
    }
}
