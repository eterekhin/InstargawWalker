using AdvancedWorflow.Workflow.Dtos;

namespace AdvancedWorflow.Workflow
{
    public interface IInitializeWorkflowManager
    {
        void Initialize(WorkflowInfo input);
    }
}