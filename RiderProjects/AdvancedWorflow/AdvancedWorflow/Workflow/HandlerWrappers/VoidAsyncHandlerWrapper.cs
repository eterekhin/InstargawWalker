using System.Threading;
using System.Threading.Tasks;
using AdvancedWorflow.Workflow.WorkflowInterfaces;

namespace AdvancedWorflow.Workflow.HandlerWrappers
{
    public class VoidAsyncHandlerWrapper<TIn> : IAsyncHandler<TIn>
    {
        private readonly IAsyncHandler<TIn> _handler;

        public VoidAsyncHandlerWrapper(IAsyncHandler<TIn> handler)
        {
            _handler = handler;
        }

        public async Task Handle(TIn input, CancellationToken cancellationToken)
        {
            await _handler.Handle(input, cancellationToken);
        }
    }
}