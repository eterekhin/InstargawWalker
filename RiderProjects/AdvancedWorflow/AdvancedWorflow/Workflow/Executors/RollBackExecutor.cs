using System.Threading;
using System.Threading.Tasks;
using AdvancedWorflow.Workflow.WorkflowInterfaces;
using Force;

namespace AdvancedWorflow.Workflow.Executors
{
    public class RollBackExecutor<TIn, TOut> : IAsyncHandler<TIn, ErrorMessage>
    {
        private readonly ICanRollBack<TIn> _canRollBack;

        public RollBackExecutor(ICanRollBack<TIn> canRollBack)
        {
            _canRollBack = canRollBack;
        }

        public async Task<ErrorMessage> Handle(TIn input, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_canRollBack.RollBack(input));
        }
    }
}