namespace BusinessLogicLayer.MessageQueue.Messages;

public record ProductNameUpdatedMessage(Guid ProductId, string NewProductName);