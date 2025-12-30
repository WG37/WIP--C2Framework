namespace TeamServer.Domain.Entities.Listeners
{
    public abstract class ListenerEntity
    {
        public enum Status { IDLE, DISABLED, ACTIVE };
       
        public abstract string Name { get; set; }
        public Status ListenerStatus { get; private set; } = Status.IDLE;
        protected void SetStatus(Status status)
        {
            ListenerStatus = status;
        }

    }
}
