namespace TeamServer.Application.Services.ListenerServices.HttpListenerService.HttpCore
{
    public interface IHttpCore
    {
        Task StartHttpListenerAsync(int bindPort);
        Task StopHttpListenerAsync();
    }
}
