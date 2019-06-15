using AdvancedWorflow.Workflow.Executors;
using AdvancedWorflow.Workflow.WorkflowInterfaces;
using Force;

namespace AdvancedWorflow.Workflow.Factories
{
    public class RollBackHandlerFactory
    {
        public IAsyncHandler<TIn, ErrorMessage> Create<TIn>(
            ICanAsyncRollBack<TIn> asyncRollBackHandler)
            => new AsyncRollBackExecutor<TIn>(asyncRollBackHandler);

        public IAsyncHandler<TIn, ErrorMessage> Create<TIn>(ICanRollBack<TIn> asyncRollBackHandler)
            => new RollBackExecutor<TIn, ErrorMessage>(asyncRollBackHandler);

        public IAsyncHandler<object> Create(object obj) => null;
    }
}