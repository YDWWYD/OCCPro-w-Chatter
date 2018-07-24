using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OnlineCuttingControlProcess
{
    /// <summary>
    /// Interaction logic for CustomizationDialogWindow.xaml
    /// </summary>
    public partial class CustomizationDialogWindow : Window
    {
        public MainWindow mainWindow = null;
        // Customization to understand any entry data is cahnged or not
        public bool isCustomizationOn = false;
        private bool okayButtonSkiped = true;

        // Default Constants
        const double KtXDefault = 1.964;
        const double KtYDefault = 1.964;
        const double KtZDefault = 1.964;
        const double KtCDefault = 1.522;
        const double KtADefault = 2.106;
        const double KtSDefault = 1.571;
        const double thresholdDefault = 40;
        const double samplingTimeDefault = 0.1;
        const double trackingToleranceDefault = 0.1;
        const double spindleExhaustDefault = 0.05;
        const double gpcForceDefault = 350;
        const double lambdaDefault = 0.2;
        const double forceOverShootDefault = 15;
        const double goToCheckDefault = 0.5;
        const double delayCompensationDefault = 0.15;

        // Values read from current settings
        private double thresholdPercentage = thresholdDefault;
        private double samplingTimeFactor = samplingTimeDefault;
        public double trackingTol = trackingToleranceDefault;
        private double spindleExhaustFactor = spindleExhaustDefault;
        private double rRef = gpcForceDefault;
        private double lambdaFactor = lambdaDefault;
        private double forceOvershootTolPercent = forceOverShootDefault;
        private double goToCheckTime = goToCheckDefault;
        private double delayCompansationTime = delayCompensationDefault;

        // Temporary clicks
        private bool tempToolBreakage;
        private bool tempAdaptiveCont;
        private bool tempVirtualReal;
        private bool tempFeedback;

        public CustomizationDialogWindow()
        {
            InitializeComponent();

            // For now, Chatter is not supported
            ChatterCheckBox.IsEnabled = false;

            // Set the default values
            CurrentsForCustomization();

            // Initializing Data Entry ComboBox
            ToolBreakageActionsComboBox.Items.Add("Alarm & Stop");      // index=0
            ToolBreakageActionsComboBox.Items.Add("Alarm & Stop Later");// index=1
            ToolBreakageActionsComboBox.Items.Add("Alarm & Continue");  // index=2

            switch (LSV2Caller.GetToolBreakageAction())
            {
                case toolBreakageActionType.alarmAndStop:
                    ToolBreakageActionsComboBox.SelectedIndex = 0;
                    break;
                case toolBreakageActionType.alarmAndDelayStop:
                    ToolBreakageActionsComboBox.SelectedIndex = 1;
                    break;
                case toolBreakageActionType.alarmAndContinue:
                    ToolBreakageActionsComboBox.SelectedIndex = 2;
                    break;
            }
        }

        //----------------------------------------------------------------------------
        // Utility procedures
        //----------------------------------------------------------------------------
        //
        private void CurrentsForCustomization()
        {
            ToolBreakageCheckBox.IsChecked = LSV2Caller.GetToolBreakage(); if (!(bool)ToolBreakageCheckBox.IsChecked) ToolBreakageCheckBox.Background = Brushes.Orange;
            tempToolBreakage = LSV2Caller.GetToolBreakage();
            AdaptiveContCheckBox.IsChecked = LSV2Caller.GetAdaptiveControl(); if (!(bool)AdaptiveContCheckBox.IsChecked) AdaptiveContCheckBox.Background = Brushes.Orange;
            tempAdaptiveCont = LSV2Caller.GetAdaptiveControl();
            VirtualRealConnCheckBox.IsChecked = LSV2Caller.GetVirtualRealConnection(); if (!(bool)VirtualRealConnCheckBox.IsChecked) VirtualRealConnCheckBox.Background = Brushes.Orange;
            tempVirtualReal = LSV2Caller.GetVirtualRealConnection();
            FeedBackCheckBox.IsChecked = LSV2Caller.GetFeedbacktoReal(); if (!(bool)FeedBackCheckBox.IsChecked) FeedBackCheckBox.Background = Brushes.Orange;
            tempFeedback = LSV2Caller.GetFeedbacktoReal();

            if (LSV2Caller.GetIsPeekForceUsed()) PeekForceCheckBox.IsChecked = true;
            else AverageForceCheckBox.IsChecked = true;
                       
            thresholdPercentage = LSV2Caller.GetThresholdPercentage(); thresholdTextBox.Text = thresholdPercentage.ToString();
            samplingTimeFactor = LSV2Caller.GetSamplingTime(); SamplingTimeTextBox.Text = samplingTimeFactor.ToString();
            //trackingTol = mainWindow.trackTolerans; TrackTolTextBox.Text = mainWindow.trackTolerans.ToString(); // done in main window, because address is not passed yet.
            spindleExhaustFactor = LSV2Caller.GetSpindleExhaustion(); SpindleExhaustTextBox.Text = spindleExhaustFactor.ToString();
            rRef = LSV2Caller.GetGPCforce(); GPCforceTextBox.Text = rRef.ToString();
            lambdaFactor = LSV2Caller.GetLambda(); LambdaTextBox.Text = lambdaFactor.ToString();
            forceOvershootTolPercent = LSV2Caller.GetForceOvershootTolPer(); ForceOvershootTextBox.Text = forceOvershootTolPercent.ToString();
            goToCheckTime = LSV2Caller.GetFeatureCheckTime(); GoToCheckTextBox.Text = goToCheckTime.ToString();
            delayCompansationTime = LSV2Caller.GetCompCheckTime(); DelayCompTextBox.Text = delayCompansationTime.ToString();
        }

        //----------------------------------------------------------------------------
        // CLICK procedures
        //----------------------------------------------------------------------------
        //
        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            LSV2Caller.SetToolBreakage((bool)ToolBreakageCheckBox.IsChecked);
            if ((bool)ToolBreakageCheckBox.IsChecked) mainWindow.TabItemToolProcessButton.Background = Brushes.LightSeaGreen;
            else mainWindow.TabItemToolProcessButton.Background = Brushes.Gray;

            LSV2Caller.SetAdaptiveControl((bool)AdaptiveContCheckBox.IsChecked);
            if ((bool)AdaptiveContCheckBox.IsChecked) mainWindow.TabItemAdaptProcessButton.Background = Brushes.LightSeaGreen;
            else mainWindow.TabItemAdaptProcessButton.Background = Brushes.Gray;

            LSV2Caller.SetVirtualRealConnection((bool)VirtualRealConnCheckBox.IsChecked);
            if ((bool)VirtualRealConnCheckBox.IsChecked) mainWindow.TabItemVirtualFeedButton.Background = Brushes.LightSeaGreen;
            else mainWindow.TabItemVirtualFeedButton.Background = Brushes.Gray;
            // Note: When Chatter is ready, add a line just like above

            // We need to combine this with Virtual Real Connection
            LSV2Caller.SetFeedbacktoReal((bool)FeedBackCheckBox.IsChecked);

            LSV2Caller.SetIsPeekForceUsed((bool)PeekForceCheckBox.IsChecked);

            //if (eta1LimitTextBox.Text != "") LSV2Caller.SetEta1Reference(Convert.ToDouble(eta1LimitTextBox.Text)); else return;
            //if (eta2LimitTextBox.Text != "") LSV2Caller.SetEta2Reference(Convert.ToDouble(eta2LimitTextBox.Text)); else return;
            //if (thresholdTextBox.Text != "") LSV2Caller.SetThresholdPercentage(Convert.ToInt16(thresholdTextBox.Text)); else return;
            //if (LambdaTextBox.Text != "") LSV2Caller.SetLambda(Convert.ToDouble(LambdaTextBox.Text)); else return;
            //if (AbarValueTextBox.Text != "") LSV2Caller.SetAbarvalue(Convert.ToDouble(AbarValueTextBox.Text)); else return;
            //if (GPCforceTextBox.Text != "") LSV2Caller.SetGPCforce(Convert.ToDouble(GPCforceTextBox.Text)); else return;
            //if (SpindleExhaustTextBox.Text != "") LSV2Caller.SetSpindleExhaustion(Convert.ToDouble(SpindleExhaustTextBox.Text)); else return;
            //if (ForceOvershootTextBox.Text != "") LSV2Caller.SetForceOvershootTolPer(Convert.ToDouble(ForceOvershootTextBox.Text)); else return;
            //if (GoToCheckTextBox.Text != "") LSV2Caller.SetFeatureCheckTime(Convert.ToDouble(GoToCheckTextBox.Text)); else return;
            //if (DelayCompTextBox.Text != "") LSV2Caller.SetCompCheckTime(Convert.ToDouble(DelayCompTextBox.Text)); else return;

            if ((thresholdTextBox.Text == "") || (LambdaTextBox.Text == "") || (SamplingTimeTextBox.Text == "") 
                || (GPCforceTextBox.Text == "") || (SpindleExhaustTextBox.Text == "") || (ForceOvershootTextBox.Text == "")
                || (GoToCheckTextBox.Text == "") || (DelayCompTextBox.Text == "") || (TrackTolTextBox.Text == "")) return;

            LSV2Caller.SetThresholdPercentage(Convert.ToInt16(thresholdTextBox.Text));
            LSV2Caller.SetLambda(Convert.ToDouble(LambdaTextBox.Text));
            LSV2Caller.SetGPCforce(Convert.ToDouble(GPCforceTextBox.Text));
            LSV2Caller.SetSamplingTime(Convert.ToDouble(SamplingTimeTextBox.Text));
            LSV2Caller.SetSpindleExhaustion(Convert.ToDouble(SpindleExhaustTextBox.Text));
            LSV2Caller.SetForceOvershootTolPer(Convert.ToDouble(ForceOvershootTextBox.Text));
            LSV2Caller.SetFeatureCheckTime(Convert.ToDouble(GoToCheckTextBox.Text));
            LSV2Caller.SetCompCheckTime(Convert.ToDouble(DelayCompTextBox.Text));
            mainWindow.trackTolerans = Convert.ToDouble(TrackTolTextBox.Text);

            switch (ToolBreakageActionsComboBox.SelectedIndex)
            {
                case 0:
                    LSV2Caller.SetToolBreakageAction(toolBreakageActionType.alarmAndStop);
                    break;
                case 1:
                    LSV2Caller.SetToolBreakageAction(toolBreakageActionType.alarmAndDelayStop);
                    break;
                case 2:
                    LSV2Caller.SetToolBreakageAction(toolBreakageActionType.alarmAndContinue);
                    break;
            }

            // Check whether any field is changed
            //
            isCustomizationOn = false;

            if (!(ToolBreakageCheckBox.IsChecked == true)) isCustomizationOn = true;
            if (!(AdaptiveContCheckBox.IsChecked == true)) isCustomizationOn = true;
            if (!(VirtualRealConnCheckBox.IsChecked == true)) isCustomizationOn = true;
            if (!(FeedBackCheckBox.IsChecked == true)) isCustomizationOn = true;

            if (!(PeekForceCheckBox.IsChecked == true)) isCustomizationOn = true;

            if (Convert.ToInt16(thresholdTextBox.Text) != thresholdDefault) isCustomizationOn = true;
            if (Convert.ToDouble(SamplingTimeTextBox.Text) != samplingTimeDefault) isCustomizationOn = true;
            if (Convert.ToDouble(TrackTolTextBox.Text) != trackingToleranceDefault) isCustomizationOn = true;
            if (Convert.ToDouble(SpindleExhaustTextBox.Text) != spindleExhaustDefault) isCustomizationOn = true;
            if (Convert.ToDouble(GPCforceTextBox.Text) != gpcForceDefault) isCustomizationOn = true;
            if (Convert.ToDouble(LambdaTextBox.Text) != lambdaDefault) isCustomizationOn = true;
            if (Convert.ToDouble(ForceOvershootTextBox.Text) != forceOverShootDefault) isCustomizationOn = true;
            if (Convert.ToDouble(GoToCheckTextBox.Text) != goToCheckDefault) isCustomizationOn = true;
            if (Convert.ToDouble(DelayCompTextBox.Text) != delayCompensationDefault) isCustomizationOn = true;

            mainWindow.currCustomizationStatus = isCustomizationOn;

            // Okay button is not skiped. This is required for Clicks
            okayButtonSkiped = false;

            this.Close();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ToolBreakageCheckBox.IsChecked = true;
            AdaptiveContCheckBox.IsChecked = true;
            VirtualRealConnCheckBox.IsChecked = true;
            FeedBackCheckBox.IsChecked = true;

            PeekForceCheckBox.IsChecked = true;
            AverageForceCheckBox.IsChecked = false;
            
            ToolBreakageActionsComboBox.SelectedIndex = 0;

            thresholdTextBox.Text = thresholdDefault.ToString();
            SamplingTimeTextBox.Text = samplingTimeDefault.ToString();
            TrackTolTextBox.Text = trackingToleranceDefault.ToString();
            SpindleExhaustTextBox.Text = spindleExhaustDefault.ToString();
            GPCforceTextBox.Text = gpcForceDefault.ToString();
            LambdaTextBox.Text = lambdaDefault.ToString();
            ForceOvershootTextBox.Text = forceOverShootDefault.ToString();
            GoToCheckTextBox.Text = goToCheckDefault.ToString();
            DelayCompTextBox.Text = delayCompensationDefault.ToString();
        }

        //-------------------------------------------------------------------------------------------------------
        // Check/Uncheck control
        //-------------------------------------------------------------------------------------------------------
        //
        private void ToolBreakageCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ToolBreakageCheckBox.Background = Brushes.White;
        }

        private void ToolBreakageCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ToolBreakageCheckBox.Background = Brushes.Orange;
        }

        private void AdaptiveContCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AdaptiveContCheckBox.Background = Brushes.White;
        }

        private void AdaptiveContCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AdaptiveContCheckBox.Background = Brushes.Orange;
        }

        private void VirtualRealConnCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            VirtualRealConnCheckBox.Background = Brushes.White;
        }

        private void VirtualRealConnCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            VirtualRealConnCheckBox.Background = Brushes.Orange;
        }

        private void FeedBackCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FeedBackCheckBox.Background = Brushes.White;
        }

        private void FeedBackCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FeedBackCheckBox.Background = Brushes.Orange;
        }

        private void PeekForceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AverageForceCheckBox.IsChecked = false;
        }

        private void PeekForceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AverageForceCheckBox.IsChecked = true;
        }

        private void AverageForceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AverageForceCheckBox.Background = Brushes.Orange;
            PeekForceCheckBox.IsChecked = false;
        }

        private void AverageForceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AverageForceCheckBox.Background = Brushes.White;
            PeekForceCheckBox.IsChecked = true;
        }

        //-------------------------------------------------------------------------------------------------------
        // Entry fields control
        //-------------------------------------------------------------------------------------------------------
        //
        private void KtXTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { KtXTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != KtXDefault) { KtXTextBox.Background = Brushes.LightBlue; } else KtXTextBox.Background = Brushes.White;
        }

        private void KtYTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { KtYTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != KtYDefault) { KtYTextBox.Background = Brushes.LightBlue; } else KtYTextBox.Background = Brushes.White;
        }

        private void KtZTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { KtZTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != KtZDefault) { KtZTextBox.Background = Brushes.LightBlue; } else KtZTextBox.Background = Brushes.White;
        }

        private void KtCTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { KtCTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != KtCDefault) { KtCTextBox.Background = Brushes.LightBlue; } else KtCTextBox.Background = Brushes.White;
        }

        private void KtATextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { KtATextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != KtADefault) { KtATextBox.Background = Brushes.LightBlue; } else KtATextBox.Background = Brushes.White;
        }

        private void KtSTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { KtSTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != KtSDefault) { KtSTextBox.Background = Brushes.LightBlue; } else KtSTextBox.Background = Brushes.White;
        }

        private void thresholdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { thresholdTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != thresholdDefault) { thresholdTextBox.Background = Brushes.LightBlue; } else thresholdTextBox.Background = Brushes.White;
        }

        private void TrackTolTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { TrackTolTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != trackingToleranceDefault) { TrackTolTextBox.Background = Brushes.LightBlue; } else TrackTolTextBox.Background = Brushes.White;
        }

        private void SpindleExhaustTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { SpindleExhaustTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != spindleExhaustDefault) { SpindleExhaustTextBox.Background = Brushes.LightBlue; } else SpindleExhaustTextBox.Background = Brushes.White;
        }

        private void GPCforceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { GPCforceTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != gpcForceDefault) { GPCforceTextBox.Background = Brushes.LightBlue; } else GPCforceTextBox.Background = Brushes.White;
        }

        private void LambdaTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { LambdaTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != lambdaDefault) { LambdaTextBox.Background = Brushes.LightBlue; } else LambdaTextBox.Background = Brushes.White;
        }

        private void ForceOvershootTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { ForceOvershootTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != forceOverShootDefault) { ForceOvershootTextBox.Background = Brushes.LightBlue; } else ForceOvershootTextBox.Background = Brushes.White;
        }

        private void GoToCheckTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { GoToCheckTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != goToCheckDefault) { GoToCheckTextBox.Background = Brushes.LightBlue; } else GoToCheckTextBox.Background = Brushes.White;
        }

        private void DelayCompTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { DelayCompTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != delayCompensationDefault) { DelayCompTextBox.Background = Brushes.LightBlue; } else DelayCompTextBox.Background = Brushes.White;
        }

        private void SamplingTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (txt.Text == "") { SamplingTimeTextBox.Background = Brushes.LightPink; return; /* no text, just return */ }
            if (Convert.ToDouble(txt.Text) != samplingTimeDefault) { SamplingTimeTextBox.Background = Brushes.LightBlue; } else SamplingTimeTextBox.Background = Brushes.White;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (okayButtonSkiped)
            {
                LSV2Caller.SetToolBreakage(tempToolBreakage);
                LSV2Caller.SetAdaptiveControl(tempAdaptiveCont);
                LSV2Caller.SetVirtualRealConnection(tempVirtualReal);
                LSV2Caller.SetFeedbacktoReal(tempFeedback);

                isCustomizationOn = mainWindow.currCustomizationStatus;
            }
            //Console.WriteLine("Closing");
        }

        //-------------------------------------------------------------------------------------------------------
        // Field range and numeric checks control
        //-------------------------------------------------------------------------------------------------------
        //
        private void thresholdTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");

            if (!e.Handled && (Convert.ToInt32((thresholdTextBox.Text + e.Text))) > 100) e.Handled = true;
        }

        private void KtXTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && KtXTextBox.Text.Length == 0) { KtXTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && KtXTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
            //if (TrackTolTextBox.Text.Length > 3) e.Handled = true;
        }

        private void KtYTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && KtYTextBox.Text.Length == 0) { KtYTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && KtYTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
        }

        private void KtZTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && KtZTextBox.Text.Length == 0) { KtZTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && KtZTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
        }

        private void KtCTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && KtCTextBox.Text.Length == 0) { KtCTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && KtCTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
        }

        private void KtATextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && KtATextBox.Text.Length == 0) { KtATextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && KtATextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
        }

        private void KtSTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && KtSTextBox.Text.Length == 0) { KtSTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && KtSTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
        }

        private void TrackTolTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && TrackTolTextBox.Text.Length == 0) { TrackTolTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && TrackTolTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
            if (TrackTolTextBox.Text.Length > 3) e.Handled = true;
        }

        private void SpindleExhaustTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && SpindleExhaustTextBox.Text.Length == 0) { SpindleExhaustTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && SpindleExhaustTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
        }

        private void GPCforceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");

            if (!e.Handled && (Convert.ToInt32(GPCforceTextBox.Text + e.Text) > 10000 || Convert.ToInt32(GPCforceTextBox.Text + e.Text) < 1)) e.Handled = true;
        }

        private void LambdaTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && LambdaTextBox.Text.Length == 0) { LambdaTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && LambdaTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
        }

        private void ForceOvershootTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && ForceOvershootTextBox.Text.Length == 0) { ForceOvershootTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && ForceOvershootTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
        }

        private void GoToCheckTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && GoToCheckTextBox.Text.Length == 0) { GoToCheckTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && GoToCheckTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
        }

        private void DelayCompTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && DelayCompTextBox.Text.Length == 0) { DelayCompTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && DelayCompTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
        }

        private void SamplingTimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txtBx = sender as TextBox;

            // If only '.' is entered as a first char, it is converted to '0.' 
            if (e.Text == "." && SamplingTimeTextBox.Text.Length == 0) { SamplingTimeTextBox.Text = "0."; txtBx.CaretIndex = txtBx.CaretIndex + 2; }

            // Second '.' is not allowed
            if (e.Text == "." && SamplingTimeTextBox.Text.IndexOf('.') != -1) { e.Handled = true; return; }

            // Valid entries, numbers + '.'
            e.Handled = Regex.IsMatch(e.Text, "[^0-9.]+");
        }

    }
}
