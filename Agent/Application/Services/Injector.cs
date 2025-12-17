namespace AgentClient.Application.Services
{
    public abstract class Injector
    {
        public abstract void Inject(byte[] shellcode, int pid = 0);
    }
}
