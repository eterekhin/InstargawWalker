using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AdvancedWorflow.Workflow.Attribute;
using AdvancedWorflow.Workflow.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace AdvancedWorflow.Workflow.Binder
{
    public class WorkflowBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var actionParameters = bindingContext?.ActionContext?.ActionDescriptor?.Parameters;

            if (actionParameters.Count != 2)
            {
                throw new Exception("должно быть только два аргумента, один - тип вокфлоу, второй - инпут");
            }

            Type outputGenericType = null;
            var actionReturnType = ((ControllerActionDescriptor) bindingContext.ActionContext.ActionDescriptor)
                .MethodInfo.ReturnType;

            if (!actionReturnType.IsGenericType)
                throw new Exception(
                    "Используйте action'ы, которые возвращают ActionResult<T> или Task<ActionResult<T>>");

            var genericType = actionReturnType.GetGenericArguments().First();
            if (actionReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                if (genericType.GetGenericTypeDefinition() == typeof(ActionResult<>))
                {
                    outputGenericType = genericType.GetGenericArguments().First();
                }
            }

            else if (actionReturnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
            {
                outputGenericType = genericType;
            }

            var workFlowType = ((ControllerParameterDescriptor) actionParameters
                    .First(x => x.Name == bindingContext.FieldName))
                .ParameterInfo
                .GetCustomAttribute<WorkFlow>()
                .WorkflowType;

            var dtoParameter = actionParameters.FirstOrDefault(x => x.Name != bindingContext.FieldName);
            
            if (dtoParameter == null)
            {
                throw new Exception();
            }

            var dtoType = dtoParameter.ParameterType;

            var workflowManagerType = typeof(WorkflowManager<,>).MakeGenericType(dtoType, outputGenericType);


            var serviceProvider = bindingContext.HttpContext.RequestServices;
            
            var workflowManager = (IInitializeWorkflowManager) ActivatorUtilities.CreateInstance(
                serviceProvider, workflowManagerType, serviceProvider);

            var workflowInfo = new WorkflowInfo {WorkflowName = workFlowType};
            workflowManager.Initialize(workflowInfo);
            bindingContext.Model = workflowManager;
            bindingContext.Result = ModelBindingResult.Success(workflowManager);
        }
    }
}