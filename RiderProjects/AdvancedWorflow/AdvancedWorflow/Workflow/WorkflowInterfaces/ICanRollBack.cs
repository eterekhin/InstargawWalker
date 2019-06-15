namespace AdvancedWorflow.Workflow.WorkflowInterfaces
{
    public interface ICanRollBack<in TIn>
    {
        ErrorMessage RollBack(TIn input);
    }
}