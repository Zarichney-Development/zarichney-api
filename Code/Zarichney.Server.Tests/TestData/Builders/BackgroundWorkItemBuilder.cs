using Zarichney.Services.BackgroundTasks;
using Zarichney.Services.Sessions;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Builder for creating test BackgroundWorkItem instances with fluent API
/// </summary>
public class BackgroundWorkItemBuilder
{
    private Func<IScopeContainer, CancellationToken, Task> _workItem = (_, _) => Task.CompletedTask;
    private Session? _parentSession;

    public BackgroundWorkItemBuilder WithWorkItem(Func<IScopeContainer, CancellationToken, Task> workItem)
    {
        _workItem = workItem ?? throw new ArgumentNullException(nameof(workItem));
        return this;
    }

    public BackgroundWorkItemBuilder WithSimpleWorkItem(Action<IScopeContainer> action)
    {
        _workItem = (scope, token) =>
        {
            action(scope);
            return Task.CompletedTask;
        };
        return this;
    }

    public BackgroundWorkItemBuilder WithAsyncWorkItem(Func<IScopeContainer, Task> asyncFunc)
    {
        _workItem = (scope, _) => asyncFunc(scope);
        return this;
    }

    public BackgroundWorkItemBuilder WithDelayedWorkItem(int delayMs)
    {
        _workItem = async (_, token) => await Task.Delay(delayMs, token);
        return this;
    }

    public BackgroundWorkItemBuilder WithFailingWorkItem(Exception exception)
    {
        _workItem = (_, _) => throw exception;
        return this;
    }

    public BackgroundWorkItemBuilder WithCancellableWorkItem(Func<CancellationToken, Task> cancellableFunc)
    {
        _workItem = (_, token) => cancellableFunc(token);
        return this;
    }

    public BackgroundWorkItemBuilder WithParentSession(Session? session)
    {
        _parentSession = session;
        return this;
    }

    public BackgroundWorkItemBuilder WithParentSession(Action<SessionBuilder> configureSession)
    {
        var sessionBuilder = new SessionBuilder();
        configureSession(sessionBuilder);
        _parentSession = sessionBuilder.Build();
        return this;
    }

    public BackgroundWorkItemBuilder WithNewParentSession()
    {
        _parentSession = new SessionBuilder()
            .WithDefaults()
            .Build();
        return this;
    }

    public BackgroundWorkItemBuilder WithoutParentSession()
    {
        _parentSession = null;
        return this;
    }

    public BackgroundWorkItem Build()
    {
        return new BackgroundWorkItem(_workItem, _parentSession);
    }
}