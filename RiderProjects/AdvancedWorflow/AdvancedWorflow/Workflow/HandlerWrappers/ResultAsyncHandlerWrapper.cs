using System.Threading;
using System.Threading.Tasks;
using Force;

namespace AdvancedWorflow.Workflow.HandlerWrappers
{
    public class ResultAsyncHandlerWrapper<TIn, TOut> : IAsyncHandler<TIn, TOut>
    {
        private readonly IAsyncHandler<TIn, TOut> _asyncHandler;

        public ResultAsyncHandlerWrapper(IAsyncHandler<TIn, TOut> asyncHandler)
        {
            _asyncHandler = asyncHandler;
        }

        public async Task<TOut> Handle(TIn input, CancellationToken ct)
        {
            return await _asyncHandler.Handle(input, ct);
        }
    }
}