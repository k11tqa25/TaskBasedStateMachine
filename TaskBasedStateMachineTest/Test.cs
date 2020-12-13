using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskBasedStateMachineLibrary;
namespace TaskBasedStateMachineTest
{
    public partial class Test : Form
    {
        TaskBasedStateMachine<MyParam> tbsm;
        CancellationTokenSource source;
        CancellationToken token;

        public class MyParam : IParameterClass
        {
            public string NextState { get; set; }

            public CancellationToken cancellationToken { get; set; }

            public string StringFromPreivousTask;

        }

        public Test()
        {
            InitializeComponent();            

            source = new CancellationTokenSource();
            token = source.Token;
            Initialize();
        }

        private void Initialize()
        {
            tbsm = new TaskBasedStateMachine<MyParam>();
            tbsm.OnErrorOccurs += Tbsm_OnErrorOccurs;
            tbsm.OnTaskCourseAborted += Tbsm_OnTaskCourseAborted;
            
            // Initialize classes
            DummyClass A = new DummyClass(this, "A", 0);
            DummyClass B = new DummyClass(this, "B", 0);
            DummyClass C = new DummyClass(this, "C", 5);
            DummyClass D = new DummyClass(this, "D", 0);
            DummyClass E = new DummyClass(this, "E", 4);
            DummyClass F = new DummyClass(this, "F", 0);
            DummyClass G = new DummyClass(this, "G", 0);
            DummyClass H = new DummyClass(this, "H", 0);
            DummyClass I = new DummyClass(this, "I", 0);
            DummyClass J = new DummyClass(this, "J", 0); 
            DummyClass K = new DummyClass(this, "K", 0);
            DummyClass L = new DummyClass(this, "L", 0);

            // Setup tasks
            tbsm.SetupTasks(new List<(string, Func<MyParam, MyParam>)> {
            (nameof(A), A.Dummy),
            (nameof(B), B.Dummy),
            (nameof(C), C.DummyWithConditions),
            (nameof(D), D.Dummy),
            (nameof(E), E.DummyWithConditions),
            (nameof(F), F.DummyWithUnhandledException),
            (nameof(G), G.Dummy),
            (nameof(H), H.Dummy),
            (nameof(I), I.Dummy),
            (nameof(J), J.Dummy),
            (nameof(K), K.DummyWithHandledException),
            (nameof(L), K.DummyWithAnotherHandledException),
            (nameof(HandledExceptionTask), HandledExceptionTask),
            (nameof(AnotherHandledExceptionTask), AnotherHandledExceptionTask)
            });

            // define the flow
            tbsm.StartWith(nameof(A))
                .FollowedBy(nameof(B))
                .FollowedBy(nameof(C))
                .ConditionalFlow(
            #region First Layor
                    // 1
                    new TaskBasedStateMachineBaseClass().StartWith(nameof(D)).FollowedBy(nameof(G)).FollowedBy(nameof(H)),
                    //2
                    new TaskBasedStateMachineBaseClass().StartWith(nameof(E))
                    .ConditionalFlow(
            #region Second Layer
                        //2.1
                        new TaskBasedStateMachineBaseClass().StartWith(nameof(I)).FollowedBy(nameof(J)).FollowedBy(nameof(D)),
                        //2.2
                        new TaskBasedStateMachineBaseClass().StartWith(nameof(B)),
                        //2.3
                        new TaskBasedStateMachineBaseClass().StartWith(nameof(C)),
                        //2.4
                        new TaskBasedStateMachineBaseClass().StartWith(nameof(E))
                    ),
            #endregion
                    //3
                    new TaskBasedStateMachineBaseClass().StartWith(nameof(F)).FollowedBy(nameof(A)),
                    //4
                    new TaskBasedStateMachineBaseClass().StartWith(nameof(K)).FollowedBy(nameof(A)),
                    //5 
                    new TaskBasedStateMachineBaseClass().StartWith(nameof(L)).FollowedBy(nameof(A))
                    )
            #endregion
                .AddHandledExceptionTasks(nameof(HandledExceptionTask), nameof(AnotherHandledExceptionTask))
                .AddUnhandledExceptionTask(ExceptionHandlingTask);
        }

        private MyParam ExceptionHandlingTask(MyParam obj)
        {
            Invoke(new Action(() => mRichTextBox.AppendText("**This is the unhandled exception state.\r\nI'm here to handle the global exception!\r\n")));
            return obj;
        }

        public MyParam HandledExceptionTask(MyParam p)
        {
            Invoke(new Action(() => mRichTextBox.AppendText("**This is the handled exception state.\r\nI can do somthing afterwards.")));
            return p;
        }

        public MyParam AnotherHandledExceptionTask(MyParam p)
        {
            Invoke(new Action(() => mRichTextBox.AppendText("**This is another handled exception state.\r\n")));
            return p;
        }
        private void Tbsm_OnErrorOccurs((object sender, Exception ex) obj)
        {
            MessageBox.Show($"Error sent from {obj.sender}:\r\n{obj.ex.Message}");
        }

        private void Tbsm_OnTaskCourseAborted()
        {
            MessageBox.Show("Task Aborted.");
        }

        private void buttonDisplay_Click(object sender, EventArgs e)
        {
            MessageBox.Show(tbsm.ToString());

            // Initialize a form
            Form form = new Form();

            // Initialize the picture box for the image
            PictureBox pBox = new PictureBox();

            string title = "TestDiagram";
            form.Text = title;

            // Draw the image
            Image image = tbsm.DrawDiagram(title, true);
            pBox.Image = image;

            // setup form size
            Size size = image.Size;
            int margin = 100;
            if (size.Width >= Screen.AllScreens[0].WorkingArea.Width) size.Width = Screen.AllScreens[0].WorkingArea.Width - margin;
            if (size.Height >= Screen.AllScreens[0].WorkingArea.Height) size.Height = Screen.AllScreens[0].WorkingArea.Height - margin;
            form.Size = Size.Add(size, new Size(30, 50));

            // Fill the form
            pBox.Dock = DockStyle.Fill;

            // Setup picture box size
            pBox.Size = size;

            // Zoom the image
            pBox.SizeMode = PictureBoxSizeMode.Zoom;

            // Add the picture box to the form
            form.Controls.Add(pBox);

            // Display the diagram
            form.Show();
        }

        private async void buttonRun_Click(object sender, EventArgs e)
        {
            source = new CancellationTokenSource();
            token = source.Token;
            MyParam p = new MyParam();
            p.StringFromPreivousTask = "Initialize";
            p.cancellationToken = token;
            await tbsm.RunAsync(p);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            source.Cancel();
        }

        public class DummyClass
        {
            private Test f = null;
            private string taskName;
            private int numberOfCases;

            public DummyClass(Form form, string taskName, int nCases)
            {
                f = (Test)form;
                this.taskName = taskName;
                numberOfCases = nCases;
            }

            public MyParam Dummy(MyParam p)
            {
                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"Run task {taskName}.\r\n")));

                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"    {p.StringFromPreivousTask}.\r\n")));
                int counter = 1;
                while (counter++ <= 10)
                {
                    f.Invoke(new Action(() => f.mRichTextBox.AppendText($"    ..{counter}")));
                    if (p.cancellationToken.IsCancellationRequested)
                    {
                        f.Invoke(new Action(() => f.mRichTextBox.AppendText($"    ........Cancelled.\r\n")));
                        return p;
                    }
                    Thread.Sleep(100);
                }

                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"\r\nTask {taskName} finished.\r\n")));

                p.StringFromPreivousTask = $"A message comes from task {taskName}";

                return p;
            }

            public MyParam DummyWithUnhandledException(MyParam p)
            {
                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"Run task {taskName}.\r\n")));

                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"    {p.StringFromPreivousTask}.\r\n")));
                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"I'm going to throw an exception in 1 sec...\r\n")));

                Thread.Sleep(1000);

                throw new Exception($"Exception thrown by {taskName}!");

            }

            public MyParam DummyWithHandledException(MyParam p)
            {
                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"Run task {taskName}.\r\n")));

                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"    {p.StringFromPreivousTask}.\r\n")));
                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"I'm going to throw an exception in 1 sec...\r\n")));

                Thread.Sleep(1000);

                try
                {
                    throw new Exception($"Exception thrown by {taskName}!");
                }
                catch
                {
                    p.NextState = nameof(f.HandledExceptionTask);
                }
                return p;

            }

            public MyParam DummyWithAnotherHandledException(MyParam p)
            {
                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"Run task {taskName}.\r\n")));

                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"    {p.StringFromPreivousTask}.\r\n")));
                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"I'm going to throw an exception in 1 sec...\r\n")));

                Thread.Sleep(1000);

                try
                {
                    throw new Exception($"Exception thrown by {taskName}!");
                }
                catch
                {
                    p.NextState = nameof(f.AnotherHandledExceptionTask);
                }
                return p;

            }

            public MyParam DummyWithConditions(MyParam p)
            {
                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"Run task {taskName}.\r\n")));
                Thread.Sleep(500);
                if (p.StringFromPreivousTask != null)
                    f.Invoke(new Action(() => f.mRichTextBox.AppendText($"    {p.StringFromPreivousTask}.\r\n")));

                p.StringFromPreivousTask = $"A message comes from task {taskName}";

                int counter = 1;
                while (counter++ <= 10)
                {
                    f.Invoke(new Action(() => f.mRichTextBox.AppendText($"    ..{counter}")));
                    if (p.cancellationToken.IsCancellationRequested)
                    {
                        f.Invoke(new Action(() => f.mRichTextBox.AppendText($"    ........Cancelled.\r\n")));
                        return p;
                    }
                    Thread.Sleep(100);
                }

                f.Invoke(new Action(() => f.mRichTextBox.AppendText($"\r\nTask {taskName} finished.\r\n")));

                SelectReturnCaseForm selectReturnCaseForm = new SelectReturnCaseForm(numberOfCases);
                int caseNumber = 0;
                if (selectReturnCaseForm.ShowDialog() == DialogResult.OK)
                    caseNumber = selectReturnCaseForm.Return;

                p.NextState = f.tbsm.Flow[f.tbsm.CurrentTaskName][caseNumber];
                return p;
            }
        }

    }
}
