using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Force;
using JetBrains.Annotations;

namespace AdvancedWorflow.Workflow.Executors
{
    public class ResultHandlersExecutor
    {
        private IList<object> handlers = new List<object>();

        public void AddHandler<TIn, TOut>(IAsyncHandler<TIn, TOut> asyncHandler)
        {
            if (GetHandler<TIn, TOut>() != null)
                throw new ArgumentException("Уже добавлен");

            handlers.Add(asyncHandler);
        }

        [UsedImplicitly]
        public async Task<TOut> TryHandle<TIn, TOut>(TIn input, CancellationToken cancellationToken)
        {
            var asyncHandler = GetHandler<TIn, TOut>();
            if (asyncHandler == null)
                return await Task.FromResult<TOut>(default);
            return await asyncHandler.Handle(input, cancellationToken);
        }

        private IAsyncHandler<TIn, TOut> GetHandler<TIn, TOut>()
        {
            var s = typeof(IAsyncHandler<,>).MakeGenericType(typeof(TIn), typeof(TOut));
            var handler = handlers.FirstOrDefault(x => x.GetType().GetInterfaces().Single() == s);
            return (IAsyncHandler<TIn, TOut>) handler;
        }
    }
}