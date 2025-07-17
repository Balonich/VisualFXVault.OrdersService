namespace BusinessLogicLayer.MessageQueue.Interfaces.Consumers;

public interface IProductDeletedConsumer
{
    Task ConsumeAsync();
    void Dispose();
}
