#define INPROC              // use HeidenhainDNC in-process with this application

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OnlineCuttingControlProcess
{
    //#define NOTUSED

    // Delegate definitions
    // IJHMachine
    internal delegate void OnConnectionStateChangedHandler(HeidenhainDNCLib.DNC_EVT_STATE stateEvent);
    // IJHAutomatic
    internal delegate void OnProgramStateChangedHandler(HeidenhainDNCLib.DNC_EVT_PROGRAM progStatEvent);
    internal delegate void OnDncModeChangedHandler(HeidenhainDNCLib.DNC_MODE dncMode);
    // IJHAutomatic2
    internal delegate void OnExecutionMessageHandler(int a_lChannel, object a_varNumericValue, string a_strValue);
    internal delegate void OnProgramChangedHandler(int a_lChannel, System.DateTime a_dTimeStamp, string a_strNewProgram);
    internal delegate void OnToolChangedHandler(int a_lChannel, HeidenhainDNCLib.JHToolId a_pidToolOut, HeidenhainDNCLib.JHToolId a_pidToolIn, System.DateTime a_dTimeStamp);
    // IJHAutomatic3
    //internal delegate void OnExecutionModeChangedHandler(int a_lChannel, HeidenhainDNCLib.DNC_EXEC_MODE executionMode);
    /// <summary>
    /// Interaction logic for HeidenhainDNCDialog.xaml
    /// </summary>
    internal class HeidenhainDNC
    {
        internal HeidenhainDNCLib.DNC_STATE m_connectionState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_NOT_INITIALIZED;
        private HeidenhainDNCLib.DNC_STS_PROGRAM m_programState = HeidenhainDNCLib.DNC_STS_PROGRAM.DNC_PRG_STS_IDLE;

        private string address = null;
        private string connectionName = null;

#if INPROC
        private HeidenhainDNCLib.JHMachineInProcess m_machine = null;
#else
        private HeidenhainDNCLib.JHMachine m_machine = null;
#endif

        private HeidenhainDNCLib.JHAutomatic m_automatic = null;

        internal HeidenhainDNC(string ipAddress)
        {
            address = ipAddress;

            // Create the machine object
#if INPROC
            m_machine = new HeidenhainDNCLib.JHMachineInProcess();
#else
            m_machine = new HeidenhainDNCLib.JHMachine();
#endif
            //string name = FindConnection("127.0.0.1");

        }

        internal void ConfigureConnection(HeidenhainDNCLib.DNC_CONFIGURE_MODE mode, ref object obj)
        {
            m_machine.ConfigureConnection(HeidenhainDNCLib.DNC_CONFIGURE_MODE.DNC_CONFIGURE_MODE_ALL, ref obj);
        }

        internal HeidenhainDNCLib.DNC_STATE GetConnectionState()
        {
            return m_connectionState;
        }

        internal HeidenhainDNCLib.DNC_STS_PROGRAM GetProgramState()
        {
            return m_programState;
        }

        internal bool RequestConnection()
        {
            m_connectionState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_HOST_IS_NOT_AVAILABLE;

            // Request the DNC connection and connect the OnStateChanged event handler
            try
            {
                connectionName = FindConnection(address);

                if (connectionName != null)
                {
                    m_machine.Connect(connectionName);

                    m_connectionState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_MACHINE_IS_AVAILABLE;

                    // Machine is available, so complete the connection
                    Connect();

                    return true;
                }
                else
                {
                    Console.WriteLine("Error: Cannot find DNC connection to CNC machine.  Please check DNC settings.");
                    return false;
                }
            }
            catch (COMException cex)
            {
                Disconnect();
                m_connectionState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_NOT_INITIALIZED;

                Console.WriteLine("Error in DNC connection");
                Console.WriteLine(cex.Message);

                return false;
            }
        }

        // Make the DNC connection
        private void Connect()
        {
            //Text = "VC# Example - Connected to " + ConnectionList.Text;

            // Create the Automatic object and connect the OnProgramStateChanged event handler
            m_automatic = (HeidenhainDNCLib.JHAutomatic)m_machine.GetInterface(HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHAUTOMATIC);
            // Hook up the OnProgramStateChanged event handler to the OnProgramStatusChanged event
            //((HeidenhainDNCLib._IJHAutomaticEvents3_Event)m_automatic).OnProgramStatusChanged +=
            //((HeidenhainDNCLib._IJHAutomaticEvents2_Event)m_automatic).OnProgramStatusChanged +=
            //new HeidenhainDNCLib._IJHAutomaticEvents_OnProgramStatusChangedEventHandler(OnCOMProgramStateChanged);

#if NOTUSED
            ((HeidenhainDNCLib._IJHAutomaticEvents3_Event)m_automatic).OnDncModeChanged +=
                new HeidenhainDNCLib._IJHAutomaticEvents_OnDncModeChangedEventHandler(OnCOMDncModeChanged);

            ((HeidenhainDNCLib._IJHAutomaticEvents3_Event)m_automatic).OnToolChanged +=
                new HeidenhainDNCLib._IJHAutomaticEvents2_OnToolChangedEventHandler(OnCOMToolChanged);

            ((HeidenhainDNCLib._IJHAutomaticEvents3_Event)m_automatic).OnProgramChanged +=
                new HeidenhainDNCLib._IJHAutomaticEvents2_OnProgramChangedEventHandler(OnCOMProgramChanged);

            ((HeidenhainDNCLib._IJHAutomaticEvents3_Event)m_automatic).OnExecutionMessage +=
                new HeidenhainDNCLib._IJHAutomaticEvents2_OnExecutionMessageEventHandler(OnCOMExecutionMessage);
#endif
            // Get the initial program status.
            m_programState = m_automatic.GetProgramStatus();
        }

        internal string GetConnectionIPAddress()
        {
            if (m_machine != null)
            {
                var property = m_machine.ListConnections().get_Connection(m_machine.currentMachine);
                return GetConnectionIPAddress(property);
            }
            else
            {
                Console.WriteLine("Error getting connection IP address!");
                return null;
            }
        }

        internal string GetConnectionIPAddress(HeidenhainDNCLib.IJHConnection connection)
        {
            string host = (string)connection.get_ConnectionProperty(HeidenhainDNCLib.DNC_CONNECTION_PROPERTY.DNC_CP_HOST).value;
            var port = connection.get_ConnectionProperty(HeidenhainDNCLib.DNC_CONNECTION_PROPERTY.DNC_CP_PORT).value;

            //Console.WriteLine(host);
            //Console.WriteLine(port);

            return host;
        }

        internal string FindConnection(string ipAddress)
        {
            if (m_machine != null && !String.IsNullOrEmpty(ipAddress))
            {
                foreach (HeidenhainDNCLib.IJHConnection connection in m_machine.ListConnections())
                {
                    string connectionAddress = GetConnectionIPAddress(connection);
                    if (connectionAddress.Equals(ipAddress))
                    {
                        return connection.name;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        internal string GetConnectionName()
        {
            return connectionName;
        }

        internal HeidenhainDNCLib.IJHConnectionList ListConnections()
        {
            return m_machine.ListConnections();
        }

        internal void GetOverrideInfo(out int feed, out int speed, out int rapid)
        {
            object feedObj = null;
            object speedObj = null;
            object rapidObj = null;
            m_automatic.GetOverrideInfo(ref feedObj, ref speedObj, ref rapidObj);

            feed = (int)feedObj;
            speed = (int)speedObj;
            rapid = (int)rapidObj;
        }

        internal void SetOverrideFeed(int percentage)
        {
            if (m_automatic != null)
            {
                object feed = null;
                object speed = null;
                object rapid = null;

                // I added TRY command to handle exception when the main window is closed
                //m_automatic.GetOverrideInfo(ref feed, ref speed, ref rapid);
                //m_automatic.SetOverrideFeed(percentage);
                //m_automatic.GetOverrideInfo(ref feed, ref speed, ref rapid);

                try
                {
                    m_automatic.GetOverrideInfo(ref feed, ref speed, ref rapid);
                    m_automatic.SetOverrideFeed(percentage);
                    m_automatic.GetOverrideInfo(ref feed, ref speed, ref rapid);
                }
                catch (Exception ave)
                {
                    // It occurs when the main window is closed, do Nothing
                }
            }
        }

        internal void SetOverrideSpeed(int percentage)
        {
            if (m_automatic != null)
            {
                m_automatic.SetOverrideSpeed(percentage);
            }
        }

        internal void SetOverrideRapid(int percentage)
        {
            if (m_automatic != null)
            {
                object feed = null;
                object speed = null;
                object rapid = null;

                m_automatic.GetOverrideInfo(ref feed, ref speed, ref rapid);

                m_automatic.SetOverrideRapid(percentage);

                m_automatic.GetOverrideInfo(ref feed, ref speed, ref rapid);
            }

        }

        // Break the DNC connection
        private void Disconnect()
        {
            /*  MSDN: How to: Handle Events Raised by a COM Source
             *  Note that COM objects that raise events within a .NET client require two Garbage Collector (GC) collections before they are released.
             *  This is caused by the reference cycle that occurs between COM objects and managed clients.
             *  If you need to explicitly release a COM object you should call the Collect method twice.
             *
             *  This is important for COM events that pass COM objects as arguments, s.a. JHError::OnError2.
             *  If the argument was not explicitly released, it will hold a reference to the parent JHError object, thus preventing a successful Disconnect().
             */
            System.GC.Collect();
            System.GC.Collect();

            if (m_automatic != null)
            {
#if NOTUSED
                // Unhook the OnCOMProgramStateChanged event handler from the OnProgramStatusChanged event
                //((HeidenhainDNCLib._IJHAutomaticEvents3_Event)m_automatic).OnProgramStatusChanged -=
                ((HeidenhainDNCLib._IJHAutomaticEvents2_Event)m_automatic).OnProgramStatusChanged -=
                    new HeidenhainDNCLib._IJHAutomaticEvents_OnProgramStatusChangedEventHandler(OnCOMProgramStateChanged);

                    ((HeidenhainDNCLib._IJHAutomaticEvents3_Event)m_automatic).OnToolChanged -=
                        new HeidenhainDNCLib._IJHAutomaticEvents2_OnToolChangedEventHandler(OnCOMToolChanged);

                    ((HeidenhainDNCLib._IJHAutomaticEvents3_Event)m_automatic).OnDncModeChanged -=
                        new HeidenhainDNCLib._IJHAutomaticEvents_OnDncModeChangedEventHandler(OnCOMDncModeChanged);

                    ((HeidenhainDNCLib._IJHAutomaticEvents3_Event)m_automatic).OnProgramChanged -=
                        new HeidenhainDNCLib._IJHAutomaticEvents2_OnProgramChangedEventHandler(OnCOMProgramChanged);

                    ((HeidenhainDNCLib._IJHAutomaticEvents3_Event)m_automatic).OnExecutionMessage -=
                        new HeidenhainDNCLib._IJHAutomaticEvents2_OnExecutionMessageEventHandler(OnCOMExecutionMessage);
#endif
                // Explicitly release the JHAutomatic COM object, to allow a successful disconnect.
                System.Runtime.InteropServices.Marshal.ReleaseComObject(m_automatic);
                m_automatic = null;
            }
            
            try
            {
                m_machine.Disconnect();

                // Explicitly release the JHMachine COM object, to allow a successful new connection.
                System.Runtime.InteropServices.Marshal.ReleaseComObject(m_machine);
                m_machine = null;
            }
            catch (Exception)
            {
                // Ignore errors
            }
        }

        internal void OnToolChangedImpl(int a_lChannel, HeidenhainDNCLib.IJHToolId a_pidToolOut, HeidenhainDNCLib.IJHToolId a_pidToolIn, System.DateTime a_dTimeStamp)
        {
            System.Diagnostics.Trace.WriteLine("OnToolChangedImpl: out: "
                + a_pidToolOut.lToolId + "." + a_pidToolOut.lSpareToolId + "." + a_pidToolOut.lIndex
                + ", in: "
                + a_pidToolIn.lToolId + "." + a_pidToolIn.lSpareToolId + "." + a_pidToolIn.lIndex
                );

            // release argument COM object
            System.Runtime.InteropServices.Marshal.ReleaseComObject(a_pidToolIn);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(a_pidToolOut);
        }

        internal void OnDncModeChangedImpl(HeidenhainDNCLib.DNC_MODE newDncMode)
        { }

        internal void OnProgramChangedImpl(int a_lChannel, System.DateTime a_dTimeStamp, string a_strNewProgram)
        { }

        public void OnExecutionMessageImpl(int a_lChannel, object a_varNumericValue, string a_strValue)
        {
            System.Diagnostics.Trace.WriteLine("OnExecutionMessageImpl:"
                + "\n numeric: "
                + a_varNumericValue.ToString()
                + "\n text:    "
                + a_strValue
                );
        }

        internal void OnExecutionModeChangedImpl(int lChannel, HeidenhainDNCLib.DNC_EXEC_MODE executionMode)
        { }

        /*
                // Connection state changed COM event handler
                private void OnCOMConnectionStateChanged(HeidenhainDNCLib.DNC_EVT_STATE eventValue)
                {
                    System.Diagnostics.Trace.WriteLine("OnCOMConnectionStateChanged: " + eventValue);
                    // Thread-safe invocation of the OnConnectionStateChanged handler
                    this.Dispatcher.Invoke(new OnConnectionStateChangedHandler(OnConnectionStateChanged), eventValue);
                }

                // Program state changed COM event handler
                private void OnCOMProgramStateChanged(HeidenhainDNCLib.DNC_EVT_PROGRAM eventValue)
                {
                    System.Diagnostics.Trace.WriteLine("OnCOMProgramStateChanged: " + eventValue);
                    // Thread-safe invocation of the OnProgramStateChanged handler
                    this.Dispatcher.Invoke(new OnProgramStateChangedHandler(OnProgramStateChanged), eventValue);
                }

                private void OnCOMToolChanged(int a_lChannel, HeidenhainDNCLib.IJHToolId a_pidToolOut, HeidenhainDNCLib.IJHToolId a_pidToolIn, System.DateTime a_dTimeStamp)
                {
                    System.Diagnostics.Trace.WriteLine("OnCOMToolChanged");

                    // Thread-safe invocation of the event handler
                    this.Dispatcher.Invoke(new OnToolChangedHandler(OnToolChangedImpl), a_lChannel, a_pidToolOut, a_pidToolIn, a_dTimeStamp);
                }

                private void OnCOMDncModeChanged(HeidenhainDNCLib.DNC_MODE eventValue)
                {
                    System.Diagnostics.Trace.WriteLine("OnCOMDncModeChanged: " + eventValue);

                    // Thread-safe invocation of the event handler
                    this.Dispatcher.Invoke(new OnDncModeChangedHandler(OnDncModeChangedImpl), eventValue);
                }

                private void OnCOMProgramChanged(int a_lChannel, System.DateTime a_dTimeStamp, string a_strNewProgram)
                {
                    System.Diagnostics.Trace.WriteLine("OnCOMProgramChanged");

                    // Thread-safe invocation of the event handler
                    this.Dispatcher.Invoke(new OnProgramChangedHandler(OnProgramChangedImpl), a_lChannel, a_dTimeStamp, a_strNewProgram);
                }

                private void OnCOMExecutionMessage(int a_lChannel, object a_varNumericValue, string a_strValue)
                {
                    System.Diagnostics.Trace.WriteLine("OnCOMExecutionMesage");

                    // Thread-safe invocation of the event handler
                    this.Dispatcher.Invoke(new OnExecutionMessageHandler(OnExecutionMessageImpl), a_lChannel, a_varNumericValue, a_strValue);
                }

                private void OnCOMExecutionModeChanged(int lChannel, HeidenhainDNCLib.DNC_EXEC_MODE executionMode)
                {
                    System.Diagnostics.Trace.WriteLine("OnCOMExecutionModeChanged: " + executionMode);

                    // Thread-safe invocation of the event handler
                    //this.Dispatcher.Invoke(new OnExecutionModeChangedHandler(OnExecutionModeChangedImpl), lChannel, executionMode);
                }

                private void OnCOMError(HeidenhainDNCLib.DNC_ERROR_GROUP errorGroup, int lErrorNumber, HeidenhainDNCLib.DNC_ERROR_CLASS errorClass, string bstrError, int lChannel)
                {
                    System.Diagnostics.Trace.WriteLine("OnCOMError: " + errorClass + ", " + lErrorNumber);

                    // Thread-safe invocation of the event handler
                    Invoke(new OnErrorHandler(OnErrorImpl), errorGroup, lErrorNumber, errorClass, bstrError, lChannel);
                }

                private void OnCOMErrorCleared(int lErrorNumber, int lChannel)
                {
                    System.Diagnostics.Trace.WriteLine("OnCOMErrorCleared: " + lErrorNumber);

                    // Thread-safe invocation of the event handler
                    Invoke(new OnErrorClearedHandler(OnErrorClearedImpl), lErrorNumber, lChannel);
                }

                // Connection state change handler
                internal void OnConnectionStateChanged(HeidenhainDNCLib.DNC_EVT_STATE stateEvent)
                {
                    if (stateEvent == HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_MACHINE_AVAILABLE)
                    {   // CNC is available, so make the actual connect
                        Connect();
                    }
                    else if (stateEvent == HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_PERMISSION_DENIED)
                    {   // DNC access permission has been denied by CNC operator
                        this.Content = "VC# Example - Access denied by " + ConnectionList.Text;
                    }
                    else if ((stateEvent == HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_MACHINE_SHUTTING_DOWN)
                             || (stateEvent == HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_CONNECTION_LOST))
                    {   // CNC is shutting down or connection lost
                        this.Content = "VC# Example - Connection closed by " + ConnectionList.Text;
                    }
                    m_connectionState = m_machine.GetState();
                    UpdateControls();
                }

                // Program state change handler
                internal void OnProgramStateChanged(HeidenhainDNCLib.DNC_EVT_PROGRAM progStatEvent)
                {
                    m_programState = m_automatic.GetProgramStatus();
                    if (m_programState == HeidenhainDNCLib.DNC_STS_PROGRAM.DNC_PRG_STS_RUNNING)
                    {
                        Console.WriteLine("Running");
                        double x, y, z;
                        GetCutterLocationDNC(out x, out y, out z);
                        Console.WriteLine("X: " + x + " Y: " + y + "Z: " + z);
                    }
                    UpdateControls();
                }*/

        internal void GetCutterLocationDNC(out double x, out double y, out double z)
        {
            var cutterLocation = m_automatic.GetCutterLocation(0);
            x = cutterLocation[0].dPosition;
            y = cutterLocation[1].dPosition;
            z = cutterLocation[2].dPosition;
        }

        internal void CloseConnection()
        {
            if (m_machine.connected)
            {
                Disconnect();
            }
            m_connectionState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_NOT_INITIALIZED;
        }
    }
}
