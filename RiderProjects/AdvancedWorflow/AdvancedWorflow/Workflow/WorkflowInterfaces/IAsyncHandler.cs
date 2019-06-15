using System.Threading;
using System.Threading.Tasks;

namespace AdvancedWorflow.Workflow.WorkflowInterfaces
{
    public interface IAsyncHandler<in TIn>
    {
        Task Handle(TIn input, CancellationToken cancellationToken);
    }
}