using AdvancedWorflow.Workflow.HandlerWrappers;
using Force;
using JetBrains.Annotations;

namespace AdvancedWorflow.Workflow.Factories
{
    public class ResultHandlersFactory
    {
        public IAsyncHandler<TIn, TOut> Create<TIn, TOut>(IAsyncHandler<TIn, TOut> handler) =>
            new ResultAsyncHandlerWrapper<TIn, TOut>(handler);

        public IAsyncHandler<TIn, TOut> Create<TIn, TOut>(IHandler<TIn, TOut> handler) =>
            new ResultHandlerWrapper<TIn, TOut>(handler);

        [UsedImplicitly]
        public IAsyncHandler<TIn, TOut> Create<TIn, TOut>(object obj) => null;
    }
}