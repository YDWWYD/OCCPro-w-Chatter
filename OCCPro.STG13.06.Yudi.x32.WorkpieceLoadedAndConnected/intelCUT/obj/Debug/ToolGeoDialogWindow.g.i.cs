﻿#pragma checksum "..\..\ToolGeoDialogWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "5E79C76BD849A05E792A09A414DBB30313E9396F"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace OnlineCuttingControlProcess {
    
    
    /// <summary>
    /// ToolGeoDialogWindow
    /// </summary>
    public partial class ToolGeoDialogWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 1 "..\..\ToolGeoDialogWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OnlineCuttingControlProcess.ToolGeoDialogWindow ToolGeometryDialogWindow;
        
        #line default
        #line hidden
        
        
        #line 6 "..\..\ToolGeoDialogWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ToolDiameterTextBox;
        
        #line default
        #line hidden
        
        
        #line 7 "..\..\ToolGeoDialogWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ToolLenghtTextBox;
        
        #line default
        #line hidden
        
        
        #line 8 "..\..\ToolGeoDialogWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ToolNumbOfTeethTextBox;
        
        #line default
        #line hidden
        
        
        #line 9 "..\..\ToolGeoDialogWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button toolGeoDialogOkay;
        
        #line default
        #line hidden
        
        
        #line 10 "..\..\ToolGeoDialogWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button toolGeoDialogClear;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/intelCUT;component/toolgeodialogwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\ToolGeoDialogWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.ToolGeometryDialogWindow = ((OnlineCuttingControlProcess.ToolGeoDialogWindow)(target));
            
            #line 4 "..\..\ToolGeoDialogWindow.xaml"
            this.ToolGeometryDialogWindow.Activated += new System.EventHandler(this.ToolGeometryDialogWindow_Activated);
            
            #line default
            #line hidden
            return;
            case 2:
            this.ToolDiameterTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 6 "..\..\ToolGeoDialogWindow.xaml"
            this.ToolDiameterTextBox.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.ToolDiameterTextBox_PreviewTextInput);
            
            #line default
            #line hidden
            return;
            case 3:
            this.ToolLenghtTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 7 "..\..\ToolGeoDialogWindow.xaml"
            this.ToolLenghtTextBox.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.ToolLenghtTextBox_PreviewTextInput);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ToolNumbOfTeethTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 8 "..\..\ToolGeoDialogWindow.xaml"
            this.ToolNumbOfTeethTextBox.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.ToolNumbOfTeethTextBox_PreviewTextInput);
            
            #line default
            #line hidden
            return;
            case 5:
            this.toolGeoDialogOkay = ((System.Windows.Controls.Button)(target));
            
            #line 9 "..\..\ToolGeoDialogWindow.xaml"
            this.toolGeoDialogOkay.Click += new System.Windows.RoutedEventHandler(this.toolGeoDialogOkay_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.toolGeoDialogClear = ((System.Windows.Controls.Button)(target));
            
            #line 10 "..\..\ToolGeoDialogWindow.xaml"
            this.toolGeoDialogClear.Click += new System.Windows.RoutedEventHandler(this.toolGeoDialogClear_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

