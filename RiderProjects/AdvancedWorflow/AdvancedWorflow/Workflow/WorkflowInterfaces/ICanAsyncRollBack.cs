using System.Threading;
using System.Threading.Tasks;

namespace AdvancedWorflow.Workflow.WorkflowInterfaces
{
    public interface ICanAsyncRollBack<in TIn>
    {
        Task<ErrorMessage> RollBack(TIn input, CancellationToken cancellationToken);
    }
}