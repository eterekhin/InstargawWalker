using System.Threading;
using System.Threading.Tasks;
using Force;

namespace AdvancedWorflow.Workflow.HandlerWrappers
{
    public class ResultHandlerWrapper<TIn, TOut> : IAsyncHandler<TIn, TOut>
    {
        private readonly IHandler<TIn, TOut> _handler;

        public ResultHandlerWrapper(IHandler<TIn, TOut> handler)
        {
            _handler = handler;
        }

        public Task<TOut> Handle(TIn obj, CancellationToken ct)
        {
            var result = _handler.Handle(obj);
            return Task.FromResult(result);
        }
    }
}