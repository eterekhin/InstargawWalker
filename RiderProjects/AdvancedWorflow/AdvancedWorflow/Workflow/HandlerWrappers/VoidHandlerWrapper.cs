using System.Threading;
using System.Threading.Tasks;
using AdvancedWorflow.Workflow.WorkflowInterfaces;
using Force;

namespace AdvancedWorflow.Workflow.HandlerWrappers
{
    public class VoidHandlerWrapper<TIn> : IAsyncHandler<TIn>
    {
        private readonly IHandler<TIn> _handler;

        public VoidHandlerWrapper(IHandler<TIn> handler)
        {
            _handler = handler;
        }

        public Task Handle(TIn input, CancellationToken cancellationToken)
        {
            _handler.Handle(input);
            return Task.CompletedTask;
        }
    }
}