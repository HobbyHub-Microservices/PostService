using PostService.DTOs;

namespace PostService.AsyncDataServices;

public interface IMesssageBusClient
{
    void PublishNewPost(PostPublishedDTO userPublishedDto);

    // void SendMessageToUser(string message);
    //
    // void SendMessageToHobby(string message);

}