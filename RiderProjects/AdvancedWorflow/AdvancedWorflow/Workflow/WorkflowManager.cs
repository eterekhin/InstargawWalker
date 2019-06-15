using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdvancedWorflow.Workflow.Dtos;
using AdvancedWorflow.Workflow.Executors;
using AdvancedWorflow.Workflow.Factories;
using AdvancedWorflow.Workflow.WorkflowInterfaces;
using AuthProject.WorkflowTest;
using Force;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedWorflow.Workflow
{
    public class WorkflowManager<TIn, TOut> : IInitializeWorkflowManager, IAsyncHandler<TIn, ActionResult<TOut>>
        where TOut : new()
    {
        #region factories
        private readonly VoidHandlersFactory _voidHandlersFactory = new VoidHandlersFactory();
        private readonly RollBackHandlerFactory _rollBackHandlerFactory = new RollBackHandlerFactory();
        private readonly ResultHandlersFactory _resultHandlersFactory = new ResultHandlersFactory();
        #endregion
        
        #region executors
        private readonly ResultHandlersExecutor _resultHandlersExecutor = new ResultHandlersExecutor();
        private readonly VoidHandlersExecutor _voidHandlersExecutor = new VoidHandlersExecutor();
        private readonly ResultHandlersExecutor _rollBackHandlersExecutor = new ResultHandlersExecutor();
        #endregion
        private readonly IServiceProvider _serviceProvider;
        private List<object> ChainHandlersInputs { get; } = new List<object>();
        private List<Type> ChainTypes { get; set; }

        private InputTypeParametersToObject InvokeVoidTryHandleMethod =>
            typeof(VoidHandlersExecutor).Invoke(_voidHandlersExecutor, "TryHandle");

        private InputTypeParametersToObject InvokeResultTryHandleMethod =>
            typeof(ResultHandlersExecutor).Invoke(_resultHandlersExecutor, "TryHandle");

        private InputTypeParametersToObject InvokeRollBackTryHandleMethod =>
            typeof(ResultHandlersExecutor).Invoke(_rollBackHandlersExecutor, "TryHandle");

        private static GetPropertyValue ResultPropertyValue => ReflectionExtensions.GetPropertyValue("Result");

        public WorkflowManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        void IInitializeWorkflowManager.Initialize(WorkflowInfo input)
        {
            if (WorkflowNotContainsWithoutParametersConstructor(input))
            {
                throw new Exception("У типа задающего ворклфоу должен быть безпараметрический конструктор");
            }

            var services = input.WorkflowName.GetNestedTypes().Select(_serviceProvider.GetService).ToList();

            var chainMethodParameter = services.Select(x => new
                {
                    method = x.GetType().GetMethod("Handle"),
                    parameter = x.GetType().GetParametersByMethodName("Handle").FirstOrDefault()
                })
                .Where(x => x.parameter != default)
                .ToList();

            var parameterInfosList = chainMethodParameter.ToList();

            ChainTypes = chainMethodParameter.SkipLast(1)
                .Aggregate(new[] {typeof(TIn)}.AsEnumerable(), (a, c) =>
                {
                    var currentHandlerInChain = parameterInfosList.First(x => x.parameter == a.Last());
                    return a.Concat(currentHandlerInChain.method.GetGenericTypesReturnValue());
                })
                .Concat(new[] {typeof(TOut)})
                .ToList();


            foreach (var service in services)
            {
                var voidHandler = _voidHandlersFactory.Create((dynamic) service);
                if (voidHandler != null)
                {
                    _voidHandlersExecutor.AddHandler(voidHandler);
                    continue;
                }

                var rollBackHandler = _rollBackHandlerFactory.Create((dynamic) service);
                if (rollBackHandler != null)
                {
                    _rollBackHandlersExecutor.AddHandler(rollBackHandler);
                }

                var resultHandler = _resultHandlersFactory.Create((dynamic) service);
                if (resultHandler != null)
                {
                    _resultHandlersExecutor.AddHandler(resultHandler);
                }
            }
        }


        public async Task<ActionResult<TOut>> Handle(TIn input, CancellationToken ct)
        {
            object chainInputOut = input;
            var chainTypesCounter = 0;

            while (chainInputOut.GetType() != typeof(TOut))
            {
                try
                {
                    ChainHandlersInputs.Add(chainInputOut);

                    var inputHandlerParameterType = ChainTypes[chainTypesCounter];
                    var outputHandlerReturnType = ChainTypes[++chainTypesCounter];

                    var voidHandler = InvokeVoidTryHandleMethod(inputHandlerParameterType)(chainInputOut, ct);

                    await (Task) voidHandler;

                    var resultTask =
                        InvokeResultTryHandleMethod(inputHandlerParameterType, outputHandlerReturnType)(chainInputOut,
                            ct);

                    await (Task) resultTask;

                    chainInputOut = ResultPropertyValue(resultTask);
                }
                catch (WorkflowException)
                {
                    object error = null;
                    while (chainTypesCounter > 0)
                    {
                        var inputHandlerType = ChainTypes[--chainTypesCounter];

                        var rollBackHandler = InvokeRollBackTryHandleMethod(inputHandlerType, typeof(ErrorMessage))
                            (ChainHandlersInputs[chainTypesCounter], ct);

                        await (Task) rollBackHandler;

                        if (error == null)
                        {
                            error = ResultPropertyValue(rollBackHandler);
                        }
                    }

                    return new JsonResult(error)
                    {
                        StatusCode = 422
                    };
                }
            }

            return (TOut) chainInputOut;
        }

        private static bool WorkflowNotContainsWithoutParametersConstructor(WorkflowInfo input)
        {
            return input.WorkflowName.GetConstructors().All(x => x.GetParameters().Length != 0);
        }
    }
}