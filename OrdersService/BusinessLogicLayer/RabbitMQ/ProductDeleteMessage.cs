
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public record ProductDeleteMessage(Guid ProductID,String ProductName);
