using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdvancedWorflow.Workflow.WorkflowInterfaces;
using JetBrains.Annotations;

namespace AdvancedWorflow.Workflow.Executors
{
    public class VoidHandlersExecutor
    {
        private IList<object> handlers = new List<object>();

        public void AddHandler<TIn>(IAsyncHandler<TIn> voidAsyncHandler)
        {
            handlers.Add(voidAsyncHandler);
        }

        [UsedImplicitly]
        public async Task TryHandle<TIn>(TIn input, CancellationToken cancellationToken = default)
        {
            var asyncHandlers = GetHandler<TIn>();
            if (asyncHandlers == null)
                return;

            foreach (var asyncHandler in asyncHandlers)
            {
                await asyncHandler.Handle(input, cancellationToken);
            }
        }

        private IEnumerable<IAsyncHandler<TIn>> GetHandler<TIn>()
        {
            var s = typeof(IAsyncHandler<>).MakeGenericType(typeof(TIn));

            var foundedHandlers = handlers.Where(x => x.GetType().GetInterfaces().Single() == s)
                .Cast<IAsyncHandler<TIn>>();

            // ReSharper disable once ConvertIfStatementToReturnStatement
            // ReSharper disable  PossibleMultipleEnumeration
            if (!foundedHandlers.Any())
            {
                return null;
            }

            return foundedHandlers;
        }
    }
}