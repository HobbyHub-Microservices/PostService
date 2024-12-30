namespace PostService.EventProcessor;

public interface IEventProcessor
{
    void ProcessEvent(string message);
}