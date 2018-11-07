#if TODO // TODO: OscillatorQuadruped
using System;

namespace OscillatorQuadruped
{
    public enum RunState
    {
        Stopped,
        Stopping,
        Running
    }

    public class Form1
    {
        private RunState _state;
        private readonly System.Threading.CancellationTokenSource _cancel = new System.Threading.CancellationTokenSource();

        public Form1()
        {
            InitializeComponent();
            radioButtonCTRNN.Checked = true;
            radioButtonObjective.Checked = true;
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (_state == RunState.Stopped)
            {
                System.Threading.Tasks.Task.Run(() => {
                    var mp = new MainProgram();
                    var evaluation = radioButtonObjective.Checked ? 0 : 1;
                    if (radioButtonCTRNN.Checked)
                        mp.run(0, evaluation, _cancel.Token);
                    else if (radioButtonSUPG.Checked)
                        mp.run(1, evaluation, _cancel.Token);
                    else
                        mp.run(2, evaluation, _cancel.Token);
                    UpdateState(RunState.Stopped);
                }, _cancel.Token);
                UpdateState(RunState.Running);
            }
            else if (_state == RunState.Running)
            {
                _cancel.Cancel();
                UpdateState(RunState.Stopping);
            }
        }

        private void UpdateState(RunState state)
        {
            _state = state;
            buttonRun.Invoke((Action)(() => { buttonRun.Text = "HyperNEAT " + _state + "..."; }));
        }

        private void buttonOpenGenome_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBoxGenome.Text = openFileDialog1.FileName;
            }
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            MainProgram mp = new MainProgram();
            if (radioButtonCTRNN.Checked)
                mp.showMovie(textBoxGenome.Text, 0);
            else if (radioButtonSUPG.Checked)
                mp.showMovie(textBoxGenome.Text, 1);
            else
                mp.showMovie(textBoxGenome.Text, 2);
        }

        private void buttonCalcFitness_Click(object sender, EventArgs e)
        {
            MainProgram mp = new MainProgram();
            if (radioButtonCTRNN.Checked)
                mp.calcFitness(textBoxGenome.Text, 0);
            else if (radioButtonSUPG.Checked)
                mp.calcFitness(textBoxGenome.Text, 1);
            else
                mp.calcFitness(textBoxGenome.Text, 2);
        }
    }
}
#endif