using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Text.RegularExpressions;

namespace OnlineCuttingControlProcess
{
    /// <summary>
    /// Interaction logic for ToolGeoDialogWindow.xaml
    /// </summary>
    public partial class ToolGeoDialogWindow : Window
    {
        public MainWindow mainWindow = null;
        //private bool decPointExist = false;
                
        public ToolGeoDialogWindow()
        {
            InitializeComponent();
        }

        private void toolGeoDialogOkay_Click(object sender, RoutedEventArgs e)
        {
            // First check the fields, if they are empty. means no char, then set them to zero 
            if (ToolDiameterTextBox.Text == "") { ToolDiameterTextBox.Text = "0"; }
            if (ToolLenghtTextBox.Text == "") { ToolLenghtTextBox.Text = "0"; }
            if (ToolNumbOfTeethTextBox.Text == "") { ToolNumbOfTeethTextBox.Text = "0"; }
                        
            mainWindow.ToolDiameterTextBox.Text = Convert.ToString(Math.Round(Convert.ToDecimal(ToolDiameterTextBox.Text), 4));
            mainWindow.ToolLenghtTextBox.Text = Convert.ToString(Math.Round(Convert.ToDecimal(ToolLenghtTextBox.Text), 4));
            mainWindow.ToolNumbOfTeethTextBox.Text = Convert.ToString(Math.Round(Convert.ToDecimal(ToolNumbOfTeethTextBox.Text), 4));

            // Tool Diameter
            if (Convert.ToDecimal(ToolDiameterTextBox.Text) != 0)
            {
                mainWindow.ToolDiameterTextBox.Background = Brushes.LightGreen;
            }
            else { mainWindow.ToolDiameterTextBox.Background = Brushes.LightGray; }

            // Tool Lenght
            if (Convert.ToDecimal(ToolLenghtTextBox.Text) != 0)
            {
                mainWindow.ToolLenghtTextBox.Background = Brushes.LightGreen;
            }
            else { mainWindow.ToolLenghtTextBox.Background = Brushes.LightGray; }

            // Number of Teeth
            if (Convert.ToDecimal(ToolNumbOfTeethTextBox.Text) != 0)
            {
                mainWindow.ToolNumbOfTeethTextBox.Background = Brushes.LightGreen;
            }
            else { mainWindow.ToolNumbOfTeethTextBox.Background = Brushes.LightGray; }

            this.Close();
        }

        private void toolGeoDialogClear_Click(object sender, RoutedEventArgs e)
        {
            ToolDiameterTextBox.Text = "";
            ToolLenghtTextBox.Text = "";
            ToolNumbOfTeethTextBox.Text = "";
        }

        private void ToolGeometryDialogWindow_Activated(object sender, EventArgs e)
        {
            //Read the current data from the tool settings in the main window
            if (Convert.ToDecimal(mainWindow.ToolDiameterTextBox.Text) != 0) { ToolDiameterTextBox.Text = mainWindow.ToolDiameterTextBox.Text; }
            if (Convert.ToDecimal(mainWindow.ToolLenghtTextBox.Text) != 0) { ToolLenghtTextBox.Text = mainWindow.ToolLenghtTextBox.Text; }
            if (Convert.ToDecimal(mainWindow.ToolNumbOfTeethTextBox.Text) != 0) { ToolNumbOfTeethTextBox.Text = mainWindow.ToolNumbOfTeethTextBox.Text; }
        }

        // Events procs. -------------------------------------------------------------------------------------------------
        //

        // KeyDown for Float numbers. This good, but found another solution below
        //
        //private void ToolDiameterTextBox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    //Console.WriteLine("Keyboard pressed:" + e.Key);

        //    if (e.Key == Key.OemPeriod && ToolDiameterTextBox.Text.IndexOf('.') != -1)
        //    {
        //        e.Handled = true;
        //        return;
        //    }

        //    if (e.Key != Key.D0 && e.Key != Key.D1 && e.Key != Key.D2 && e.Key != Key.D3 && e.Key != Key.D4 && e.Key != Key.D5
        //        && e.Key != Key.D6 && e.Key != Key.D7 && e.Key != Key.D8 && e.Key != Key.D9 && e.Key != Key.OemPeriod)
        //    {
        //        e.Handled = true;
        //    }
        //}

        // ToolDiameterTextBox: PreviewTextInput for Double
        //
        private void ToolDiameterTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && ToolDiameterTextBox.Text.Length == 0) { ToolDiameterTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && ToolDiameterTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
        }

        // ToolLenghtTextBox: PreviewTextInput for Double
        //
        private void ToolLenghtTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && ToolLenghtTextBox.Text.Length == 0) { ToolLenghtTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && ToolLenghtTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");

            //if (decPointExist)
            //{
            //    //if (ToolDiameterTextBox.Text.Length > 2) e.Handled = true;
            //}
            //else if (ToolDiameterTextBox.Text.Length > 2) e.Handled = true; // max 3 digits allowed if no dot
        }

        // ToolNumbOfTeethTextBox: PreviewTextInput for Integer only
        //
        private void ToolNumbOfTeethTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }
    }
}
