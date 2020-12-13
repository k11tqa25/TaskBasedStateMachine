using LoggerManagerLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskBasedStateMachineLibrary
{
    /// <summary>
    /// The configuration of this <see cref="TaskBasedStateMachineLibrary"/>
    /// </summary>
    public static class TaskBasedStateMachineLibraryConfiguration
    {
        /// <summary>
        /// Override the existing debug file (TaskBasedStateMachineLog.log)? <br></br>
        /// Default is false;
        /// </summary>
        public static bool OverrideDebugFile { get; set; } = false;

        /// <summary>
        /// Save the debug file (TaskBasedStateMachineLog.log)?
        /// Default is true;
        /// </summary>
        public static bool SaveDebugFile { get; set; } = true;
    }

    /// <summary>
    /// The class that help you the build the flow of the tasks and run the flow asynchronously. <br></br><br></br>
    /// NOTE: You'll need to use the <c>SetupTasks()</c> to setup all the tasks you'll need first, 
    /// and then setup the flow of those tasks you have assigned. 
    /// The tasks will be stored with their task name, so that you may arrange the flow with their names.
    /// </summary>
    public class TaskBasedStateMachine<TParam> : TaskBasedStateMachineBaseClass
        where TParam : IParameterClass, new()
    {
        #region Protected Properties

        /// <summary>
        /// A dictionary that stores all functions by its name. The function takes in and returns a <see cref="IParameterClass"/>
        /// This is set to private so that user cannot modify this object directly.
        /// Instead, modify it through SetupTasks method.
        /// </summary>
        protected Dictionary<string, Func<TParam, TParam>> mTasks;

        #endregion

        #region Constructor

        /// <summary>
        /// When you new up a TaskBasedStateMachine, it's log information will be saved in a 
        /// "TaskBasedStateMachineLog.log" file under the current application directory.
        /// </summary>
        public TaskBasedStateMachine() : base()
        {
            List<int> list = new List<int>();
            
            // Setup Configuration
            LoggerManagerConfiguration.SaveDebugFile = TaskBasedStateMachineLibraryConfiguration.SaveDebugFile;
            // Setup Configuration
            LoggerManagerConfiguration.OverrideDebugFile = TaskBasedStateMachineLibraryConfiguration.OverrideDebugFile;

            // Initialize the logger
            TaskBasedStateMachineLogger.InitiateLogger();

            // Use the file logger for debug message
            BasicLogger.Construct().UseFileLogger(@".\TaskBasedStateMachineLog.log");

            // Initialize task dictionary
            mTasks = new Dictionary<string, Func<TParam, TParam>>()
            {
                { InitialTask, null},
                { ExceptionTask, null},
                {UnhandledExceptionTask, null }
            };


            // Register the error from the helper
            GraphvizHelper.OnErrorOccurs += GraphvizHelper_OnErrorOccurs;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Setup the tasks before building the flow of the tasks 
        /// </summary>
        /// <param name="tasks">A <see cref="IEnumerable{T}"/> object that contains a tuple with a task name and the task function.
        /// <br></br>
        /// Note that the task name should be unique, 
        /// and the tasks function should take in a <see cref="object"/> as an parameter (that will be passed through the whole process)
        /// and return tuple of an <see cref="int"/> to indicate different conditions that need to go after.
        /// and an <see cref="object"/> to pass the modified object down to the next task.</param>
        public void SetupTasks(List<(string, Func<TParam, TParam>)> tasks)
        {
            if (tasks == null) return;

            // For each elements in the tasks...
            tasks.ToList().ForEach(t =>
            {
                // Add elements to the task dictionary
                mTasks.Add(t.Item1, t.Item2);

                // Initialize the flow dictionary with all the existing tasks.
                //(This can be used to show the warning message if the user forget to setup tasks.)
                Flow[t.Item1] = null;
            });

            // Log it.
            BasicLogger.Log($"The tasks has been setup. Here's the tasks that has been setup:\r\n{string.Join(", ", mTasks.Keys)}");
        }

        /// <summary>
        /// Clear up all the tasks
        /// </summary>
        public void Clear()
        {
            mTasks.Clear();
            Flow = new Dictionary<string, List<string>>()
            {
                { InitialTask, null},
                { ExceptionTask, null}
            };
            BasicLogger.Log($"The tasks has been cleared up.");
        }

        /// <summary>
        /// The initial task of the flow. This should be used in the very beginning.<br></br>
        /// <br></br>
        /// <strong>NOTE: ONLY USE THIS ONCE!! If you use this in the conditional flow, you'll screw up everything.</strong>
        /// </summary>
        /// <param name="taskName">The name of the task</param>
        /// <returns></returns>
        public new TaskBasedStateMachine<TParam> StartWith(string taskName)
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
        public new TaskBasedStateMachine<TParam> FollowedBy(string taskName)
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
        public TaskBasedStateMachine<TParam> FollowedByASeriesOfTasks(TaskBasedStateMachine<TParam> taskSeries)
        {
            // Shortcut
            Dictionary<string, List<string>> newFlow = taskSeries.Flow;

            // Loop through all the keys in the tasks to copy all the tasks from new source.
            foreach (var t in newFlow.Keys)
            {
                // Do not include the exception task
                if (t == ExceptionTask) continue;
                // Special handle for initial task
                else if (t == InitialTask)
                {
                    // Get the initail task name of the new task series
                    var initialTaskName = newFlow[InitialTask][0];

                    // Add the new key and value to mTasks
                    mTasks[initialTaskName] = taskSeries.mTasks[InitialTask];
                }
                // Else, load every tasks to the dictionary
                else mTasks[t] = taskSeries.mTasks[t];

            }

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
        public new TaskBasedStateMachine<TParam> ConditionalFlow(params TaskBasedStateMachineBaseClass[] taskCases)
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

        /// <summary>
        /// This is a bunch of state for you to handle all the exceptions occurred in whichever state. 
        /// Specify the task names and you can go to these state from all the other states when an error occurs. <br></br>
        /// <br></br>
        /// <strong>NOTE: The task function must be setup by using <c>SetupTask()</c> beforehand.</strong><br></br>
        /// <br></br>
        /// <remark>Use <c>AddUnhandledExceptionTask</c> after this method.</remark>
        /// </summary>
        public TaskBasedStateMachine<TParam> AddHandledExceptionTasks(params string[] taskName)
        {
            // Add the tasks to the excption task            
            Flow[ExceptionTask] = taskName.ToList();

            // Chain the method
            return this;
        }

        /// <summary>
        /// This is the state that handles a unhandled exception that is capture by this <see cref="TaskBasedStateMachine{TParam}"/> when running the tasks.<br></br>
        /// It should be the last method to call and it can only be assigned once.<br></br>
        /// <br></br>
        /// <strong>NOTE: This method is usually followed by <c>AddHandledExceptionTasks</c></strong>
        /// </summary>
        /// <param name="func">The function of the task that takes in an object as the parameter and returns a tuple of (int, object)</param>
        public void AddUnhandledExceptionTask(Func<TParam, TParam> func)
        {
            // Add a exception task to the task dictionary
            mTasks[UnhandledExceptionTask] = func;
        }

        /// <summary>
        /// Run the flow of the tasks asynchronously. So remember to jump back to UI thread if you try to update something on a UI.
        /// </summary>
        /// <param name="paramter">The parameter to pass in for every task methods. You can then unbox this parameter in your own method implementation.
        /// <br></br>
        /// <strong>NOTE: Highly recommend that this should also be incorporated in the parameter to stop your own methods.</strong></param>
        public async Task RunAsync(TParam paramter)
        {
            // Null exception
            if (mTasks == null)
            {
                HandleException((nameof(RunAsync), new Exception("The task flow is empty. You may want to setup tasks first and then setup the task flow.")));
                return;
            }

            // Null exception
            if (paramter.cancellationToken == null)
            {
                HandleException((nameof(RunAsync), new Exception("The cancellation token haven't been initialized yet. You must give a cancellation token in case you need to abort the tasks.")));
                return;
            }

            #region Run

            // Get the initial task
            CurrentTaskName = GetNextTaskName(InitialTask);

            // Handle the exception task
            bool exceptionHadOccurred = false;

            try
            {
                // Run the tasks asynchronously
                await Task.Run(async () =>
                {
                    // Log it
                    BasicLogger.Log($"Task flow started.");

                    // Fires started event
                    OnTaskCourseStarted.Invoke();

                    // Run to the end node or task is cancelled
                    while (CurrentTaskName != null && !paramter.cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            //// Log it
                            //BasicLogger.Log($"Start running task {CurrentTaskName}...");

                            // Fires state changed event
                            OnStateChanged.Invoke((CurrentTaskName, Flow[CurrentTaskName]?.ToArray()));

                            // Set the next state to the default state before the current task start running.
                            paramter.NextState = GetNextTaskName(CurrentTaskName);

                            // If the task is not null
                            if (mTasks[CurrentTaskName] != null)
                            {
                                // Run the task here
                                await Task.Run(() => paramter = mTasks[CurrentTaskName].Invoke(paramter), paramter.cancellationToken);

                                if (paramter.cancellationToken.IsCancellationRequested)
                                    // Log for cancelling the task.
                                    BasicLogger.Log($"Task {CurrentTaskName} has been cancelled.");
                                //else
                                //    // Log for fininshing the task.
                                //    BasicLogger.Log($"Task {CurrentTaskName} finished.");

                                // Gext next task
                                CurrentTaskName = GetNextTaskName(CurrentTaskName, paramter);
                            }
                            // If the current task is null
                            else
                            {
                                // Log it
                                BasicLogger.Log($"The task {CurrentTaskName} has never been setup.");

                                // Stop the loop.
                                CurrentTaskName = null;
                            }

                        }
                        catch (TaskCanceledException)
                        {
                            // Task cancelled
                        }
                        catch (Exception ex)
                        {
                            HandleException((nameof(RunAsync), ex));

                            // Return exception or null
                            CurrentTaskName = exceptionHadOccurred ? null : UnhandledExceptionTask;

                            // Exception had occurred. If any exception happens in exception state, then return null to stop the loop.
                            exceptionHadOccurred = true;
                        }
                    }

                }, paramter.cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Task cancelled
            }
            catch (Exception ex)
            {
                // Other type of exceptions
                HandleException((nameof(RunAsync), ex));
            }

            if (paramter.cancellationToken.IsCancellationRequested)
            {
                // Log for cancelling
                BasicLogger.Log("The task has been cancelled successfully.");

                // Fires the task aborted event.
                OnTaskCourseAborted.Invoke();
            }

            // Fires the task complete event
            OnTaskCourseCompleted.Invoke();

            #endregion
        }

        /// <summary>
        /// Draw the diagram that has been created as an Image.
        /// </summary>
        /// <param name="diagramTitle">The title of this diagram.</param>
        /// <param name="save">Save the diagram as a png image whose filename is the application file directory + <paramref name="diagramTitle"/>.png</param>
        /// <returns>Returns a <see cref="Image"/></returns>
        public Image DrawDiagram(string diagramTitle = "Diagram", bool save = false)
        {
            // Set the starter string
            StringBuilder graphVizString = new StringBuilder(@"digraph g 
            {ratio = fill;
            node[style = filled]");

            // Set the title 
            string title = diagramTitle == string.Empty ? "Diagram" : diagramTitle;

            // Set the title for the graph
            graphVizString.AppendLine($"lable = \"{title}\";");

            // Set up color string for displaying the graph (the coloring setup could be opened to user in the future)
            string colorRed = "0.000 0.500 1.000";
            string colorLightBlue = "0.590 0.273 1.000";
            string colorLightGreen = "0.449 0.447 1.000";
            string colorLightGray = "0.000 0.000 0.800";
            string arrowColor = "0.650 0.700 0.700";

            #region BFS algorithm

            // BFS queue
            List<string> BFS = new List<string>()
            {
                // Initialize with the start task node
                Flow[InitialTask][0]
            };
            // Visited nodes
            HashSet<string> visited = new HashSet<string>()
            {
                // Initialize with the start task node
                Flow[InitialTask][0]
            };

            // Draw a node for start task
            graphVizString.AppendLine($"Start[color=\"{colorLightGreen}\"]");
            graphVizString.AppendLine($"Start->{Flow[InitialTask][0]}[color=\"{arrowColor}\"]");

            try
            {
                // Start the algorithm
                while (BFS.Count != 0)
                {
                    // Pop and get the current node from BFS queue
                    string current = BFS.PopFromStart();

                    // Assign a node to the graph
                    if (Flow[current] == null)
                    {
                        // This is a leaf (end node)
                        graphVizString.AppendLine($"{current}[color=\"{colorLightGreen}\"]");
                        continue;
                    }
                    else if (Flow[current].Count > 1)
                        // This is a conditional flow node
                        graphVizString.AppendLine($"{current}[color=\"{colorLightBlue}\"]");
                    else
                        // All the other nodes
                        graphVizString.AppendLine($"{current}[color=\"{colorLightGray}\"]");

                    // Loop through all its child nodes
                    int i = 0;
                    foreach (var node in Flow[current])
                    {
                        // Assign an arrow to its childe node
                        graphVizString.AppendLine($"{current}->{node}[color=\"{arrowColor}\"][label =\"{i}\"]");

                        // Check if the node has been visited
                        if (visited.Add(node))
                        {
                            //If it hasn't been visited, add the node to the BFS queue
                            BFS.Add(node);
                        }

                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException((this, ex));
                return null;
            }

            #endregion

            // Draw a node for global exception state
            graphVizString.AppendLine($"GlobalException[color=\"{colorRed}\"]");

            // Close the string
            graphVizString.AppendLine("}");

            // set the image to the picture box
            try
            {
                // Run the dot.exe to render image
                Image image = GraphvizHelper.Run(graphVizString.ToString(), (save) ? title + ".png" : "");

                // Return this diagram image
                return image;
            }
            catch (Exception ex)
            {
                HandleException((this, ex));
                return null;
            }
        }

        /// <summary>
        /// Display the content of the flow as an dictioanry
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{ " + string.Join(",", Flow.Select(kv => "[ " + kv.Key + " ] = [ " + kv.Value.JoinWith(",") + " ]\r\n").ToArray()) + " }";
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get the task name of the next task
        /// </summary>
        /// <param name="currentTask">The name of the current task</param>
        /// <param name="p">The parameter that contains the name of the next step.</param>
        /// <returns></returns>
        private string GetNextTaskName(string currentTask, IParameterClass p)
        {
            if (!mTasks.ContainsKey(currentTask)) new Exception($"Task {currentTask} does not exist in the flow. Please check for your task setup");
            if (Flow[currentTask] == null) return null;
            else
            {
                var next = Flow.Select(v => v.Key == p.NextState);
                // Return next step if exists
                if (next != null) return p.NextState;
                else
                {
                    // Check if the next step is error state
                    foreach (var s in Flow[ExceptionTask])
                    {
                        if (s == p.NextState) return s;
                    }

                    // If the next state is neither in the following states nor in the exception states, throw an error.
                    throw new Exception($"Task {p.NextState} does not follow the current task {currentTask}. Please check for your task flow.");
                }
            }
        }

        /// <summary>
        /// This is used to get the default next step
        /// </summary>
        /// <param name="currentTask">The name of the current task.</param>
        /// <returns></returns>
        private string GetNextTaskName(string currentTask)
        {
            if (!mTasks.ContainsKey(currentTask)) new Exception($"Task {currentTask} does not exist in the flow. Please check for your task setup");
            if (!Flow.ContainsKey(currentTask) || Flow[currentTask] == null) return null;
            else return Flow[currentTask][0];
        }

        /// <summary>
        /// Handle the exception caught in this class.
        /// </summary>
        /// <param name="detail">The detail of the excption. It is a tuple with a sender and an exception.</param>
        private void HandleException((object sender, Exception ex) detail)
        {
            string msg = $"Error sent from {detail.sender}:\r\n{detail.ex.Message}";

            // Log it
            BasicLogger.Log(msg, LogLevel.Error);

            // Catch any unhandled exception
            OnErrorOccurs.Invoke(detail);

        }

        #endregion

        #region Public Events

        /// <summary>
        ///  Fires when an error occurs
        /// </summary>
        public event Action<(object sender, Exception ex)> OnErrorOccurs = (detail) => { };

        /// <summary>
        /// Fires when state changes. 
        /// </summary>
        public event Action<(string currentState, string[] followingStates)> OnStateChanged = (detail) => { };

        /// <summary>
        /// Fires when the task course is started.
        /// </summary>
        public event Action OnTaskCourseStarted = () => { };

        /// <summary>
        /// Fires when the task course is completed. This will be fired after the <see cref="OnTaskCourseAborted"/> event if any.
        /// </summary>
        public event Action OnTaskCourseCompleted = () => { };

        /// <summary>
        /// Fires when the task is aborted. This will be fired before the <see cref="OnTaskCourseCompleted"/> event.
        /// </summary>
        public event Action OnTaskCourseAborted = () => { };

        #endregion

        #region Private Events

        private void GraphvizHelper_OnErrorOccurs((object sender, Exception ex) obj)
        {
            HandleException(obj);
        }


        #endregion
    }

    /// <summary>
    /// The logger for the <see cref="TaskBasedStateMachine{TParam}"/>. This is used to initiate the global logger for the whole <see cref="TaskBasedStateMachine{TParam}"/>.
    /// When you new up a <see cref="TaskBasedStateMachine{TParam}"/>, the details of the class will be store in the "TaskBasedStateMachineLog.log" file automatically.
    /// </summary>
    public static class TaskBasedStateMachineLogger
    {
        private static bool HasBeenInitiated = false;

        /// <summary>
        /// Initiate the basic logger file.
        /// </summary>
        public static void InitiateLogger()
        {
            if (!HasBeenInitiated)
            {
                // Initialize any logger you want to use
                BasicLogger.Construct().UseFileLogger("TaskBasedStateMachineLog.log");
            }
            HasBeenInitiated = true;
        }
    }
}
