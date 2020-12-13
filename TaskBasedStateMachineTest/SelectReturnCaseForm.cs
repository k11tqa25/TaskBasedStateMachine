using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskBasedStateMachineTest
{
    public partial class SelectReturnCaseForm : Form
    {
        private int NumberOfCases = 0;

        public int Return { get; set; }

        public SelectReturnCaseForm()
        {
            InitializeComponent();
        }

        public SelectReturnCaseForm(int numberOfCases)
        {
            InitializeComponent();
            NumberOfCases = numberOfCases;
            for (int i = 0; i < numberOfCases; i++) mFlowLayout.Controls.Add(BuildButtons(i.ToString()));
        }

        private Button BuildButtons(string name)
        {
            Button btn = new Button();
            btn.BackColor = SystemColors.ButtonFace;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.Gray;
            btn.FlatAppearance.BorderSize = 2;
            btn.Font = new Font(new FontFamily("微軟正黑體"), 8.5f);
            btn.Size = new Size(35, 35);
            btn.Name = name;
            btn.Text = name;
            btn.Click += OnNumberButtonsClicked;
            return btn;
        }

        private void OnNumberButtonsClicked(object sender, EventArgs e)
        {
            Return = Convert.ToInt32((sender as Button).Text);
            DialogResult = DialogResult.OK;
            Close();
        }

    }
}
