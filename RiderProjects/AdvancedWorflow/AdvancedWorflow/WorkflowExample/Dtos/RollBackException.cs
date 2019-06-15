using System;
using System.Collections.Generic;

namespace AuthProject.WorkflowTest
{
    public class RollBackException : Exception
    {
        public RollBackException(string exception) : base(exception)
        {
        }

        public RollBackException(IEnumerable<string> results) : base(string.Join("\n", results))
        {
        }
    }
}