using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AdvancedWorflow.Workflow.Attribute;
using AuthProject.WorkflowTest;
using Force;
using Force.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AuthProject.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<ConfirmInfoDto>> Index(
            [WorkFlow(typeof(TestWorkflow))] IAsyncHandler<CreateNewUserInputDto, ActionResult<ConfirmInfoDto>> handler,
            CreateNewUserInputDto dto) => await handler.Handle(dto, CancellationToken.None);
    }
}