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
                    "SOAP Code: " + fEx.Code
                    );
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
