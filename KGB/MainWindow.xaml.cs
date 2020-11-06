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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Web;
using System.Xml;
using System.Threading;
using System.Net.Http;
using System.Linq;

namespace KGB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Titlebar
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                Application.Current.MainWindow.DragMove();

        }
        #endregion

        void BNLookup()
        {
            string GST_SEARCH_FORM = "https://www.businessregistration-inscriptionentreprise.gc.ca/ebci/brom/registry/registryservlet";
            string GST_SEARCH_RESULTS = "https://www.businessregistration-inscriptionentreprise.gc.ca/ebci/brom/registry/pub/reg_01_Sbmt.action";
            
            string token = string.Empty;
            bool valid = true;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            CookieContainer cookies = new CookieContainer();
            

            var myRequest = (HttpWebRequest)WebRequest.Create(GST_SEARCH_FORM);

            var postData = "iagree=" + Uri.EscapeDataString("yes");
            var data = Encoding.ASCII.GetBytes(postData);

            myRequest.Method = "POST";
            myRequest.ContentType = "application/x-www-form-urlencoded";
            myRequest.ContentLength = data.Length;
            myRequest.CookieContainer = cookies;

            using (var stream = myRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            // Website requires a token to be passed in the post data, we obtain the token here
            using (var logResp = (HttpWebResponse)myRequest.GetResponse())
            {
                var result = new StreamReader(logResp.GetResponseStream()).ReadToEnd();

                foreach (string s in result.Split('\n'))
                    if (s.Contains("token"))
                        foreach (string t in s.Split(' '))
                            if (t.Contains("value"))
                                if (t.Length > 10)
                                    token = t.Replace("value=", "").Replace("\"","");

                
            }

            //POST structure for the GST search
            //ex: struts.token.name=token&token=7TKDLO1O2GV0TCYUVNMV2WKMWN91V9IX&businessNumber=123456789&businessName=ups&requestDate=2020-11-03&reg.label.submit=Search            
            myRequest = (HttpWebRequest)WebRequest.Create(GST_SEARCH_RESULTS);
            postData = "struts.token.name=" + Uri.EscapeDataString("token");
            postData += "&token=" + Uri.EscapeDataString(token);
            postData += "&businessNumber=" + Uri.EscapeDataString(bNumber.Text.Trim());
            postData += "&businessName=" + Uri.EscapeDataString(cName.Text.Trim());
            postData += "&requestDate=" + Uri.EscapeDataString(DateTime.Today.ToString("yyyy-MM-dd"));
            postData += "&reg.label.submit=" + Uri.EscapeDataString("Search");
            Debug.WriteLine(postData);
            data = Encoding.ASCII.GetBytes(postData);

            myRequest.Method = "POST";
            myRequest.ContentType = "application/x-www-form-urlencoded";
            myRequest.ContentLength = data.Length;
            myRequest.CookieContainer = cookies;

            using (var stream = myRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            // Obtain and parse results from the BN search
            // GST/HST number is not valid = GST no good
            // GST/HST number was not registered on this transaction date = GST valid but Company Name doesnt match or wasnt activated on date
            // Insufficient information entered. = gst valid company name doesnt match
            using (var logResp = (HttpWebResponse)myRequest.GetResponse())
            {
                var result = new StreamReader(logResp.GetResponseStream()).ReadToEnd();

                foreach (string s in result.Split('\n'))
                    if (s.Contains("GST/HST number is not valid"))
                        valid = false;


            }

            if (valid)
                MessageBox.Show("BN is valid");
            else
                MessageBox.Show("BN is INVALID");
                    

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BNLookup();
        }
    }
}
