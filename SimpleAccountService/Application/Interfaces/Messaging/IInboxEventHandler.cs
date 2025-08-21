using MediatR;

namespace Simple_Account_Service.Application.Interfaces.Messaging;

public interface IInboxEventHandler<in T> : INotificationHandler<T>
    where T : INotification;