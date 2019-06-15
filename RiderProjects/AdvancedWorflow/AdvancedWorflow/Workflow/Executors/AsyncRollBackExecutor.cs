using System.Threading;
using System.Threading.Tasks;
using AdvancedWorflow.Workflow.WorkflowInterfaces;
using Force;

namespace AdvancedWorflow.Workflow.Executors
{
    public class AsyncRollBackExecutor<TIn> : IAsyncHandler<TIn, ErrorMessage>
    {
        private readonly ICanAsyncRollBack<TIn> _asyncRollBack;

        public AsyncRollBackExecutor(ICanAsyncRollBack<TIn> asyncRollBack)
        {
            _asyncRollBack = asyncRollBack;
        }

        public async Task<ErrorMessage> Handle(TIn input, CancellationToken cancellationToken)
        {
            return await _asyncRollBack.RollBack(input, cancellationToken);
        }
    }
}