using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthProject.WorkflowTest
{
    public class CreateNewUserInputDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public IEnumerable<string> Roles { get; set; }
        [Required]
        public string UserName { get; set; }
    }
}