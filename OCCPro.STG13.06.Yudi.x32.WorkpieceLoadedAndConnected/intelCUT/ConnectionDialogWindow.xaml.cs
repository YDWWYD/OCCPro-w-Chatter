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

using System.Net.NetworkInformation;
using OnlineCuttingControlProcess.Properties;
using System.Net;

using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.IO;

namespace OnlineCuttingControlProcess
{
    /// <summary>
    /// Interaction logic for ConnectionDialogWindow.xaml
    /// </summary>
    public partial class ConnectionDialogWindow : Window
    {
        public MainWindow mainWindow = null;
        string ipAddressFile = ".\\CNCMachineIPAddresses.txt";

        // Connectio Dialog Window
        //
        public ConnectionDialogWindow()
        {
            InitializeComponent();
            ReadIPAddressesFromFile();
        }

        // Saving IP name and address in a file
        //
        private void WriteIPAddressToFile(string ipString)
        {
            using (var sw = System.IO.File.AppendText(ipAddressFile))
            {
                sw.WriteLine(ipString);
            }
        }

        // reading IP name and address in a file
        //
        private void ReadIPAddressesFromFile()
        {
            if (System.IO.File.Exists(ipAddressFile))
            {
                var simulatedDataLines = System.IO.File.ReadLines(ipAddressFile);
                foreach (string line in simulatedDataLines)
                {
                    // Read each line and split the CNC name and IP address
                    string[] words = line.Split(',');

                    // Write to the items
                    cncNameList.Items.Add(words[0]);
                    cncIPAddress.Items.Add(words[1]);
                }
            }
        }

        // Validate IP address format
        //
        public bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }
        
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textName.Text) && !string.IsNullOrWhiteSpace(textIP.Text))
            {
                if (ValidateIPv4(textIP.Text))
                {
                    // Add the items
                    cncNameList.Items.Add(textName.Text);
                    cncIPAddress.Items.Add(textIP.Text);
                    Console.WriteLine("added");

                    // Write to the file
                    string myRecord = textName.Text + "," + textIP.Text;
                    WriteIPAddressToFile(myRecord);
                }
                else
                {
                    pingingStatusDisplay.Text = "IP Address format is wrong";
                    textIP.Clear();
                    // Console.WriteLine("IP format is wrong");
                    return;
                }
            }

            // After adding or wrong entry,  clear the text boxes and make ping button unclickable
            textName.Clear();
            textIP.Clear();

            if (cncNameList.SelectedIndex == -1)
            {
                testPingButton.IsEnabled = false;
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (cncNameList.SelectedIndex != -1)
            {
                cncIPAddress.SelectedIndex = cncNameList.SelectedIndex;
 
                cncNameList.Items.RemoveAt(cncNameList.SelectedIndex);
                cncIPAddress.Items.RemoveAt(cncIPAddress.SelectedIndex);
                Console.WriteLine("deleted");

                // Then update ".\\CNCMachineIPAddresses.txt" file by copying the new content
                string tempFile = ".\\CNCMachineIPAddressesTEMP.txt";

                using (var sw = File.AppendText(tempFile))
                {
                    for (int k = 0; k < cncNameList.Items.Count; k++)
                    {
                        cncNameList.SelectedIndex = k;
                        cncIPAddress.SelectedIndex = k;
                        //Console.WriteLine(cncNameList.SelectedItem);

                        string myRecord = cncNameList.SelectedItem + "," + cncIPAddress.SelectedItem;
                        sw.WriteLine(myRecord);
                    }
                }

                File.Delete(ipAddressFile);
                File.Move(tempFile, ipAddressFile);
            }

            testPingButton.IsEnabled = false;
        }

        private void pingingOkay_Click(object sender, RoutedEventArgs e)
        {
            // Since this button is active, pinging is sucessfull, 
            // host public var is updated with the successful IP address, then close the dialog
            // 
            this.Close();
        }

        // Ping test
        //
        private async Task<bool> CanPingHost(string ipAddress)
        {
            bool success = false;
            var pingSender = new Ping();
            await Task.Run(() =>
            {
                try
                {
                    var reply = pingSender.Send(ipAddress, 4000); //wait time 4s
                    if (reply.Status == IPStatus.Success)
                    {
                        success = true;
                    }
                    else
                    {
                        Console.WriteLine("Ping error: " + reply.Status.ToString());
                        success = false;
                    }
                }
                catch (PingException e)
                {
                    Console.WriteLine("Error: " + e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine(e.Source);
                    if (e.InnerException != null)
                    {
                        string innerException = "Inner exception: " + e.InnerException.ToString();
                        string baseException = "Base exception: " + e.GetBaseException().ToString();
                        Console.WriteLine(innerException);
                        Console.WriteLine(baseException);
                    }
                }
            });
            return success;
        }

        // Test the pinging for a selected IP address
        //
        private async void testPingButton_Click(object sender, RoutedEventArgs e)
        {
            pingingOkay.IsEnabled = false;

            string address = cncIPAddress.Items.GetItemAt(cncNameList.SelectedIndex).ToString();

            // Just start pinging
            Task<bool> returnCanPingHost = CanPingHost(address);
            pingingStatusDisplay.Text = "Pinging IP Address...";
            bool pingResult = await returnCanPingHost;

            if (pingResult)
            {
                pingingOkay.IsEnabled = true;
                pingingStatusDisplay.Text = "Ping successful.  Click 'Connect' to accept selection";
                mainWindow.host = address;
            }
            else
            {
                pingingStatusDisplay.Text = "Unable to ping!!!";
                mainWindow.host = null;
            }
        }

        // Events -----------------------------------------------------------------------------------------
        //

        private void cncNameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cncNameList.SelectedIndex != -1)
            {
                testPingButton.IsEnabled = true;
                cncIPAddress.SelectedIndex = cncNameList.SelectedIndex;
            }
        }

        // textIP: PreviewTextInput for Integer only
        //
        private void textIP_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
        }
    }
}
