using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
//using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WcfServiceA;

namespace WPFClient1  
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    /*
     * Important note about UseSynchronizationContext beahvior!!!!!
     * if UseSynchronizationContext = true
     * then the value from the System.Threading.SynchronizationContext.
     * Current is read and cached so that when a request
     * comes to the service, the host can marshal the request onto the
     * thread that the host was created on using the cached SynchronizationContext.
     * If you try to host a service in, for example,
     * a WPF application and also call that service from the same thread in the WPF application, 
     * you will notice that you get a deadlock when the client tries to call the service.
     * The reason for this is that the default value of the UseSynchronizationContext is 
     * true and so when you create the ServiceHost on the UI thread of the WPF application,
     * then the current synchronization context is a DispatcherSynchronizationContext which
     * holds a reference to a System.Windows.Threading.Dispatcher
     * object which then holds a reference to the current thread. 
     * The DispatcherSynchronizationContext will then be used when a request comes in to marshal requests onto the UI thread.
     * But if you are calling the service from the UI thread then you have a deadlock when it tries to do this.
     */

    [CallbackBehavior(UseSynchronizationContext = false/*this takes the callbacks off of the UI event queue*/)]
    public partial class MainWindow : Window, IServiceAEvents
    {
        private IServiceA mServiceA = null;
        public MainWindow()
        {
            InitializeComponent();
            //timer to show on UI to demo responsiveness
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Start();
            timer.Elapsed += updateUI;
        }

        private void updateUI(Object source, EventArgs e)
        {
            //just updates the UI in order to make sure we are still responsive
            Application.Current.Dispatcher.BeginInvoke(new Action(() => TimeLabel.Content = DateTime.Now.ToString("HH:mm:ss")));
        }

        private void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int value = mServiceA.GetValue();
                textBox.AppendText("Returned value from service: " + value + "\n");
            }
            /*
             * If a managed exception is thrown from a service operation, client is immediately reported with a “FaultException”
             * and the communication channel of the client is faulted and the proxy object cannot be used for calling any more
             * methods. If it is used then “CommunicationObjectFaultedException” is thrown in the client application. 
             * If a “FaultException” is thrown from the service operation on server then the client application can handle this
             * FaultException and the communication channel of client and server will not be in faulted state.
             */
            catch (FaultException fEx)
            {
                textBox.AppendText("Fault exception was thrown: " +
                    fEx.Message +
                    " " + fEx.Reason +
                    " " +
                    "SOAP Code: " + fEx.Code +
                    "\n");
            }
            catch (Exception ex)
            {
                textBox.AppendText("Exception was thrown: " + ex.Message);
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            //pass this class into service to register it as a callback client.
            mServiceA = WcfServiceA.ConnectionFactory.Connect(this);
        }
        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mServiceA.DeregisterClient(this);
            }
            catch(FaultException fEx)
            {
                textBox.AppendText("Fault exception was thrown: " +
                fEx.Message +
                " " + fEx.Reason +
                " " +
                "SOAP Code: " + fEx.Code +
                "\n");
            }
            catch(Exception ex)
            {
                textBox.AppendText("Exception was thrown: " + ex.Message);
            }
        }

        private void UpdateService_Click(object sender, RoutedEventArgs e)
        {
            //send a one-way message to the service
            mServiceA.UpdateService(1);
        }

        private void GetValueAsync_Click(object sender, RoutedEventArgs e)
        {
            //send a request to the service to compute async. It will send a callback when completed... via SendValueBack
            mServiceA.GetValueAsnyc();
            //do stuff here...
            textBox.AppendText("Im still going!...\n");
        }

        //callbacks from service
        public void SendStatus(int status)
        {
            // textBox.AppendText("Recived status from service: " + status + "\n"); // this will cause service to abort
            Application.Current.Dispatcher.Invoke(new Action(() => StatusLabel.Content = status.ToString()));
        }
        //recives data async
        public void SendValueBack(string callbackStr)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => textBox.AppendText(callbackStr + "\n")));
        }
        //end callbacks
    }
}
