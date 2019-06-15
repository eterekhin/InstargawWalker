using System;
using System.Collections.Generic;

namespace AdvancedWorflow.Workflow.Dtos
{
    public class WorkflowException : Exception
    {
        public WorkflowException(string exception) : base(exception)
        {
        }

        public WorkflowException(IEnumerable<string> results) : base(string.Join("\n", results))
        {
        }
    }
}