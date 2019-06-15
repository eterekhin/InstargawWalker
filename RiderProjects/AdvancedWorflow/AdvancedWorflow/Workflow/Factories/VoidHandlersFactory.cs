using AdvancedWorflow.Workflow.HandlerWrappers;
using AdvancedWorflow.Workflow.WorkflowInterfaces;
using Force;

namespace AdvancedWorflow.Workflow.Factories
{
    public class VoidHandlersFactory
    {
        public IAsyncHandler<TIn> Create<TIn>(IHandler<TIn> asyncRollBackHandler)
            => new VoidHandlerWrapper<TIn>(asyncRollBackHandler);

        public IAsyncHandler<TIn> Create<TIn>(IAsyncHandler<TIn> asyncRollBackHandler)
            => new VoidAsyncHandlerWrapper<TIn>(asyncRollBackHandler);

        public IAsyncHandler<object> Create(object obj) => null;
    }
}