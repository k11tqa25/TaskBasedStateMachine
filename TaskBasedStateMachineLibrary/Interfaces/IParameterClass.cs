using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TaskBasedStateMachineLibrary
{
    /// <summary>
    /// The interface that contains certain crucial properties that needs to be pass in as a parameter. <br></br>
    /// Implement this for your parameter class.
    /// </summary>
    public interface IParameterClass
    {
        /// <summary>
        /// The name of the next task. <br></br><br></br>
        /// <strong>NOTE: This will be automatically set to the next state (which is the first index in the list that follows the current state) before the current state starts running. </strong>
        /// <br></br>
        /// That is, you don't need to set this value if there is only one case followed by the current state. <br></br>
        /// Set this value if there are multiple cases followed by the current state.
        /// </summary>
        string NextState {get; set; }

        /// <summary>
        /// The <see cref="CancellationToken"/> that is used to abort the task.
        /// </summary>
        CancellationToken cancellationToken { get; set; }
    }
}
