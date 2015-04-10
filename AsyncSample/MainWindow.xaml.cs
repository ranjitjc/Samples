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

// Add a using directive for SyndicationClient. 
//using Windows.Web.Syndication;
// Add a using directive for Tasks. 
using System.Threading.Tasks;
// Add a using directive for CancellationToken. 
using System.Threading;
using System.ServiceModel.Syndication;
using System.Xml;
using System.IO;
using System.Net; 


namespace AsyncSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // ***Declare a System.Threading.CancellationTokenSource. 
        CancellationTokenSource cts; 

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Cancel Demo

        private void btnStart_Click123(object sender, RoutedEventArgs e)
        {
            ResultsTextBox.Text = "";
            // Prevent unexpected reentrance. 
            StartButton.IsEnabled = false;

            // ***Instantiate the CancellationTokenSource. 
            cts = new CancellationTokenSource();

            try
            {
                // ***Send a token to carry the message if cancellation is requested. 
                DownloadBlogsAsync(cts.Token);
            }
            // ***Check for cancellations. 
            catch (OperationCanceledException)
            {
                // In practice, this catch block often is empty. It is used to absorb 
                // the exception, 
                ResultsTextBox.Text += "\r\nCancellation exception bubbles up to the caller.";
            }
            // Check for other exceptions. 
            catch (Exception ex)
            {
                ResultsTextBox.Text =
                    "Page could not be loaded.\r\n" + "Exception: " + ex.ToString();
            }

            // ***Set the CancellationTokenSource to null when the work is complete. 
            cts = null;

            // In case you want to try again. 
            StartButton.IsEnabled = true; 

        }

        // ***Provide a parameter for the CancellationToken. 
        async Task DownloadBlogsAsync(CancellationToken ct)
        {
            //Windows.Web.Syndication.SyndicationClient client = new SyndicationClient();

                   
            

            var uriList = CreateUriList();

            // Force the SyndicationClient to download the information. 
            //client.BypassCacheOnRetrieve = true;

            // The following code avoids the use of implicit typing (var) so that you  
            // can identify the types clearly. 

            foreach (var uri in uriList)
            {
                // ***These three lines are combined in the single statement that follows them. 
                //IAsyncOperationWithProgress<SyndicationFeed, RetrievalProgress> feedOp =  
                //    client.RetrieveFeedAsync(uri); 
                //Task<SyndicationFeed> feedTask = feedOp.AsTask(ct); 
                //SyndicationFeed feed = await feedTask; 

                // ***You can combine the previous three steps in one expression. 
                //SyndicationFeed feed = await client.RetrieveFeedAsync(uri).AsTask(ct);
                XmlReader reader = XmlReader.Create(uri.OriginalString);


                SyndicationFeed feed = SyndicationFeed.Load(reader);


                DisplayResults(feed);
            }
        } 


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
                ResultsTextBox.Text += "\r\nDownloads canceled by the Cancel button.";
            } 

        }

        List<Uri> CreateUriList()
        {
            // Create a list of URIs. 
            List<Uri> uriList = new List<Uri>  
            {  
                new Uri("http://windowsteamblog.com/windows/b/developers/atom.aspx"), 
                new Uri("http://windowsteamblog.com/windows/b/extremewindows/atom.aspx"), 
                new Uri("http://windowsteamblog.com/windows/b/bloggingwindows/atom.aspx"), 
                new Uri("http://windowsteamblog.com/windows/b/business/atom.aspx"), 
                new Uri("http://windowsteamblog.com/windows/b/windowsexperience/atom.aspx"), 
                new Uri("http://windowsteamblog.com/windows/b/windowssecurity/atom.aspx"), 
                new Uri("http://windowsteamblog.com/windows/b/windowshomeserver/atom.aspx"), 
                new Uri("http://windowsteamblog.com/windows/b/springboard/atom.aspx") 
            };
            return uriList;
        }

        // You can pass the CancellationToken to this method if you think you might use a 
        // cancellable API here in the future. 
        void DisplayResults(SyndicationFeed sf)
        {
            // Title of the blog. 
            ResultsTextBox.Text += sf.Title.Text + "\r\n";


            int cnt = (sf.Items.Count() < 3) ? sf.Items.Count() : 3;
            // Titles and dates for the first three blog posts. 
            for (int i = 0; i < cnt; i++)    // Is Math.Min better? 
            {
                ResultsTextBox.Text += "\t" + sf.Items.ElementAt(i).Title.Text + ", " +
                    sf.Items.ElementAt(i).PublishDate.ToString() + "\r\n";
            }

            ResultsTextBox.Text += "\r\n";
        }

#endregion


        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // Disable the button until the operation is complete.
            StartButton.IsEnabled = false;

            // ***Instantiate the CancellationTokenSource. 
            cts = new CancellationTokenSource();


            ResultsTextBox.Text="";

             try
            {
                // One-step async call.
                await SumPageSizesAsync(cts.Token);
            }
             // ***Check for cancellations. 
             catch (OperationCanceledException)
             {
                 // In practice, this catch block often is empty. It is used to absorb 
                 // the exception, 
                 ResultsTextBox.Text += "\r\nCancellation exception bubbles up to the caller.";
             }
             // Check for other exceptions. 
             catch (Exception ex)
             {
                 ResultsTextBox.Text =
                     "Page could not be loaded.\r\n" + "Exception: " + ex.ToString();
             }
            // Two-step async call. 
            //Task sumTask = SumPageSizesAsync(); 
            //await sumTask;

             // ***Set the CancellationTokenSource to null when the work is complete. 
             cts = null;

            ResultsTextBox.Text += "\r\nControl returned to startButton_Click.\r\n";

            // Reenable the button in case you want to run the operation again.
            StartButton.IsEnabled = true;
        }

        private async Task SumPageSizesAsync(CancellationToken ct)
        {
            // Make a list of web addresses.
            List<string> urlList = SetUpURLList();

            var total = 0;

            foreach (var url in urlList)
            {
                byte[] urlContents = await GetURLContentsAsync(url);

                
                // The previous line abbreviates the following two assignment statements. 

                // GetURLContentsAsync returns a Task<T>. At completion, the task 
                // produces a byte array. 
                //Task<byte[]> getContentsTask = GetURLContentsAsync(url); 
                //byte[] urlContents = await getContentsTask;

                DisplayResults(url, urlContents);

                // Update the total.          
                total += urlContents.Length;

                //if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();
                
            }
            // Display the total count for all of the websites.
            ResultsTextBox.Text +=
                string.Format("\r\n\r\nTotal bytes returned:  {0}\r\n", total);
        }

        private List<string> SetUpURLList()
        {
            List<string> urls = new List<string> 
            { 
                "http://msdn.microsoft.com/library/windows/apps/br211380.aspx",
                "http://msdn.microsoft.com",
                "http://msdn.microsoft.com/en-us/library/hh290136.aspx",
                "http://msdn.microsoft.com/en-us/library/ee256749.aspx",
                "http://msdn.microsoft.com/en-us/library/hh290138.aspx",
                "http://msdn.microsoft.com/en-us/library/hh290140.aspx",
                "http://msdn.microsoft.com/en-us/library/dd470362.aspx",
                "http://msdn.microsoft.com/en-us/library/aa578028.aspx",
                "http://msdn.microsoft.com/en-us/library/ms404677.aspx",
                "http://msdn.microsoft.com/en-us/library/ff730837.aspx"
            };
            return urls;
        }

        private async Task<byte[]> GetURLContentsAsync(string url)
        {
            // The downloaded resource ends up in the variable named content. 
            var content = new MemoryStream();

            // Initialize an HttpWebRequest for the current URL. 
            var webReq = (HttpWebRequest)WebRequest.Create(url);

            // Send the request to the Internet resource and wait for 
            // the response.                 
            using (WebResponse response = await webReq.GetResponseAsync())

            // The previous statement abbreviates the following two statements. 
            //await client.RetrieveFeedAsync(uri).AsTask(ct);
            //Task<WebResponse> responseTask = webReq.GetResponseAsync(); 
            //using (WebResponse response = await responseTask)
            {
                // Get the data stream that is associated with the specified url. 
                using (Stream responseStream = response.GetResponseStream())
                {
                    // Read the bytes in responseStream and copy them to content. 
                    await responseStream.CopyToAsync(content);

                    // The previous statement abbreviates the following two statements. 

                    // CopyToAsync returns a Task, not a Task<T>. 
                    //Task copyTask = responseStream.CopyToAsync(content); 

                    // When copyTask is completed, content contains a copy of 
                    // responseStream. 
                    //await copyTask;
                }
            }
            // Return the result as a byte array. 
            return content.ToArray();
        }


        private void DisplayResults(string url, byte[] content)
        {
            // Display the length of each website. The string format  
            // is designed to be used with a monospaced font, such as 
            // Lucida Console or Global Monospace. 
            var bytes = content.Length;
            // Strip off the "http://".
            var displayURL = url.Replace("http://", "");
            ResultsTextBox.Text += string.Format("\n{0,-58} {1,8}", displayURL, bytes);
        }





    }
}

// Sample Output: 
// Developing for Windows 
//     New blog for Windows 8 app developers, 5/1/2012 2:33:02 PM -07:00 
//     Trigger-Start Services Recipe, 3/24/2011 2:23:01 PM -07:00 
//     Windows Restart and Recovery Recipe, 3/21/2011 2:13:24 PM -07:00 

// Extreme Windows Blog 
//     Samsung Series 9 27” PLS Display: Amazing Picture, 8/20/2012 2:41:48 PM -07:00 
//     NVIDIA GeForce GTX 660 Ti Graphics Card: Affordable Graphics Powerhouse, 8/16/2012 10:56:19 AM -07:00 
//     HP Z820 Workstation: Rising To the Challenge, 8/14/2012 1:57:01 PM -07:00 

// Blogging Windows 
//     Windows Upgrade Offer Registration Now Available, 8/20/2012 1:01:00 PM -07:00 
//     Windows 8 has reached the RTM milestone, 8/1/2012 9:00:00 AM -07:00 
//     Windows 8 will be available on…, 7/18/2012 1:09:00 PM -07:00 

// Windows for your Business 
//     What Windows 8 RTM Means for Businesses, 8/1/2012 9:01:00 AM -07:00 
//     Higher-Ed Learning with Windows 8, 7/26/2012 12:03:00 AM -07:00 
//     Second Public Beta of App-V 5.0 Now Available with Office Integration, 7/24/2012 10:07:26 AM -07:00 

// Downloads canceled.