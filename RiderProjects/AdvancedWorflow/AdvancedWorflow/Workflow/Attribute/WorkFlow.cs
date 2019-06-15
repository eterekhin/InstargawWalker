using System;
using System.Linq;
using AdvancedWorflow.Workflow.Binder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AdvancedWorflow.Workflow.Attribute
{
    public class WorkFlow : ModelBinderAttribute
    {
        public Type WorkflowType { get; set; }

        public WorkFlow(Type workflowType)
        {
            WorkflowType = workflowType;
            BinderType = typeof(WorkflowBinder);
        }

        public override BindingSource BindingSource => BindingSource.Services;
    }

    // use override BinderType in WorkFlowAttribute
    public class WorkflowBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata is DefaultModelMetadata t &&
                t.Attributes.Attributes.Any(x => x.GetType() == typeof(WorkFlow)))
            {
                return new BinderTypeModelBinder(typeof(WorkflowBinder));
            }

            return null;
        }
    }
}