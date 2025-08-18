namespace Product.Api.Application.Abstractions
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event, CancellationToken ct = default);

    }
}
