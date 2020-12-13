using System.Collections.Generic;

namespace TaskBasedStateMachineLibrary
{
    /// <summary>
    /// The base class of the <see cref="TaskBasedStateMachineLibrary"/>.
    /// </summary>
    public class TaskBasedStateMachineBaseClass
    {
        #region Protected Properties

        /// <summary>
        /// The protected keyword of the key indicating the value is the initial task.
        /// </summary>
        protected static string InitialTask { get; private set; } = "__StartWith__";

        /// <summary>
        /// The protected keyword of the key indicating the value is the exception task.
        /// </summary>
        protected static string ExceptionTask { get; private set; } = "__Exception__";

        /// <summary>
        /// The protected keyword of the key indicating the value is the unhandled exception task.
        /// </summary>
        protected static string UnhandledExceptionTask { get; private set; } = "__UnhandledException__";


        #endregion

        #region Public Properties

        /// <summary>
        /// When building up the task flow, this will always point at the last task (new added) of the flow, <br></br>
        /// However, when running the task flow, this will point at the task that is running.
        /// </summary>
        public string CurrentTaskName { get; protected set; }

        /// <summary>
        /// The dictionary to control the flow. <br></br>
        /// The key is the task name <br></br>
        /// The value is the task list followed by the key task.<br></br><br></br>
        /// <strong> NOTICE: SEQUENCE MATTERS !! </strong> 
        /// The first task being added is the default sequence of the flow.
        /// </summary>
        public Dictionary<string, List<string>> Flow { get; protected set; }

        #endregion

        #region Constructor

        /// <summary>
        /// The constructor
        /// </summary>
        public TaskBasedStateMachineBaseClass()
        {
            // Initialize the flow dictionary.
            Flow = new Dictionary<string, List<string>>()
            {
                { InitialTask, null},
                { ExceptionTask, null },
                {UnhandledExceptionTask, null }
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The initial task of the flow. This should be used in the very beginning.<br></br>
        /// <br></br>
        /// <strong>NOTE: ONLY USE THIS ONCE!! If you use this in the conditional flow, you'll screw up everything.</strong>
        /// </summary>
        /// <param name="taskName">The name of the task</param>
        /// <returns></returns>
        public TaskBasedStateMachineBaseClass StartWith(string taskName)
        {
            // Add the task to the "startWith" key 
            Flow[InitialTask] = new List<string>() { taskName };

            // Set the current task name to this new task
            CurrentTaskName = taskName;

            // Chain the method
            return this;
        }

        /// <summary>
        /// The task that follows the current task.
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public TaskBasedStateMachineBaseClass FollowedBy(string taskName)
        {
            // Add the task to the currentTaskName
            Flow[CurrentTaskName] = new List<string>() { taskName };

            // Set the current task name to this new task
            CurrentTaskName = taskName;

            // Chain the method
            return this;
        }

        /// <summary>
        /// A series of tasks that follow the current task. 
        /// </summary>
        /// <param name="taskSeries">Another task based state machine object that contains a certain flow of tasks. 
        /// This will not override the exsiting initial task and exception task.</param>
        /// <returns></returns>
        public TaskBasedStateMachineBaseClass FollowedByASeriesOfTasks(TaskBasedStateMachineBaseClass taskSeries)
        {
            // Shortcut
            Dictionary<string, List<string>> newFlow = taskSeries.Flow;

            // If the series contains the initial task key, it need to be appended directly to the current task.
            if (newFlow.ContainsKey(InitialTask))
            {
                // Assign this task to the current task directly if the current task haven't been initialized yet.
                if (!Flow.ContainsKey(CurrentTaskName) || Flow[CurrentTaskName] == null)
                    Flow[CurrentTaskName] = newFlow[InitialTask];
                else
                {
                    // Add the nodes if the child node is not null
                    if (newFlow[InitialTask] != null)
                        foreach (var tName in newFlow[InitialTask])
                            // Check if any task name is already added
                            if (Flow[CurrentTaskName] != null && Flow[CurrentTaskName].Contains(tName)) continue;
                            // If not, add it to the current key
                            else Flow[CurrentTaskName].Add(tName);
                }
            }

            // Add all the other tasks that are not initial keys
            foreach (var tName in newFlow.Keys)
            {
                // Pass the initial key for it has been added already
                if (tName == InitialTask) continue;

                // Add the key directly. For the same task SHOULD HAVE the same followers
                Flow[tName] = newFlow[tName];
            }

            // Move the current task name to the last task name of the new task series
            CurrentTaskName = taskSeries.CurrentTaskName;

            // Chain the method
            return this;
        }

        /// <summary>
        /// Set up the conditional flow for the current tasks. <br></br>
        /// The current task pointer will point to the last task of the last added task case.
        /// </summary>
        /// <param name="taskCases">The task flow to be added. It should be bunch of series of tasks. See <see cref="TaskBasedStateMachineBaseClass"/></param>
        /// <returns></returns>
        public TaskBasedStateMachineBaseClass ConditionalFlow(params TaskBasedStateMachineBaseClass[] taskCases)
        {
            // Store the currentTaskName so that each time a new case added, the pointer won't be lost.
            string temp = CurrentTaskName;

            // Foreach task cases...
            foreach (var tc in taskCases)
            {
                // Every time a new task is added, append the task to the original task.
                CurrentTaskName = temp;

                // Add the new series of tasks to the flow
                FollowedByASeriesOfTasks(tc);
            }

            // Chain the method
            return this;
        }
        #endregion
    }
}
