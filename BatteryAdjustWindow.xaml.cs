using System;
using System.Windows;

namespace Phonova
{
    public partial class BatteryAdjustWindow : Window
    {
        public bool IsAddAction { get; private set; }
        public int AdjustmentValue { get; private set; }
        public bool ResultOK { get; private set; }

        public BatteryAdjustWindow()
        {
            InitializeComponent();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            ResultOK = false;
            Close();
        }

        private void MinusBtn_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AdjustmentValueText.Text, out int val) && val > 0)
            {
                AdjustmentValueText.Text = (val - 1).ToString();
            }
        }

        private void PlusBtn_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AdjustmentValueText.Text, out int val))
            {
                AdjustmentValueText.Text = (val + 1).ToString();
            }
            else
            {
                AdjustmentValueText.Text = "1";
            }
        }

        private void DeductBtn_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AdjustmentValueText.Text, out int val))
            {
                AdjustmentValue = val;
                IsAddAction = false;
                ResultOK = true;
                Close();
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AdjustmentValueText.Text, out int val))
            {
                AdjustmentValue = val;
                IsAddAction = true;
                ResultOK = true;
                Close();
            }
        }
    }
}
