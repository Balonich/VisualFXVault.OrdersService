namespace BusinessLogicLayer.MessageQueue.Interfaces.Consumers;

public interface IProductNameUpdatedConsumer
{
    Task ConsumeAsync();
    void Dispose();
}
