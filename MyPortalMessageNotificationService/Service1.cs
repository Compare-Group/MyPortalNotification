using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Configuration;
using Flurl.Http;
using Newtonsoft.Json;



namespace MyPortalMessageNotificationService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.WriteToFile("Service started {0}");
            this.ScheduleService();
        }

        protected override void OnStop()
        {
            this.WriteToFile("Service stopped {0}");
            this.Schedular.Dispose();
        }

        private Timer Schedular;

        public void ScheduleService()
        {
            try
            {
                Schedular = new Timer(new TimerCallback(SchedularCallback));
                string mode = ConfigurationManager.AppSettings["Mode"].ToUpper();
                this.WriteToFile("Service Mode: " + mode + " {0}");

                //Set the Default Time.
                DateTime scheduledTime = DateTime.MinValue;

                if (mode == "DAILY")
                {
                    //Get the Scheduled Time from AppSettings.
                    scheduledTime = DateTime.Parse(System.Configuration.ConfigurationManager.AppSettings["ScheduledTime"]);
                    if (DateTime.Now > scheduledTime)
                    {
                        //If Scheduled Time is passed set Schedule for the next day.
                        scheduledTime = scheduledTime.AddDays(1);
                    }
                }

                if (mode.ToUpper() == "INTERVAL")
                {
                    //Get the Interval in Minutes from AppSettings.
                    int intervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalMinutes"]);

                    //Set the Scheduled Time by adding the Interval to Current Time.
                    scheduledTime = DateTime.Now.AddMinutes(intervalMinutes);
                    if (DateTime.Now > scheduledTime)
                    {
                        //If Scheduled Time is passed set Schedule for the next Interval.
                        scheduledTime = scheduledTime.AddMinutes(intervalMinutes);
                    }
                }

                TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
                string schedule = string.Format("{0} day(s) {1} hour(s) {2} minute(s) {3} seconds(s)", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

                this.WriteToFile("Service scheduled to run after: " + schedule + " {0}");

                //Get the difference in Minutes between the Scheduled and Current Time.
                int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);

                //Change the Timer's Due Time.
                Schedular.Change(dueTime, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                WriteToFile("Service Error on: {0} " + ex.Message + ex.StackTrace);

                //Stop the Windows Service.
                //using (System.ServiceProcess.ServiceController serviceController = new System.ServiceProcess.ServiceController("SimpleService"))
                //{
                //    serviceController.Stop();
                //}
            }
        }

        private void SchedularCallback(object e)
        {
            try
            {

            
            //this.WriteToFile("Service Log: {0}");
            ProcessMyPortalNotifications();
            this.ScheduleService();
            }
            catch (Exception ex)
            {

            }
        }

        private void WriteToFile(string text)
        {
            try
            {

            
            string path = ConfigurationManager.AppSettings["LogFile"];//"C:\\ServiceLog.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
            }
            catch (Exception ex)
            {

            }
        }
        private async void ProcessMyPortalNotifications()
        {
            try
            {

            
            WriteToFile(string.Format("Processing Notifications - {0}", DateTime.Now.ToString("F")));

            var customerApiUrl = "https://customerapi.businessinsurancesolutions.co.uk/customer/send-portal-link";
            var connString = "Data Source = 192.168.10.15; Initial Catalog = VCIL; User ID = VCApps; Password = vancompare; Application Name = MyPortalNotify";
            var sql = "select * from vcil.dbo.AvayaMyPortalNotify where processed is null";

            var toProcess = Sql.ExecuteQuery(connString, sql);

            
            foreach (var item in toProcess)
            {
                try
                {


                    WriteToFile(string.Format("Processing[{0}] at {1}", item[1].ToString(), DateTime.Now.ToString("F")));

                    var json = new MyPortalNotification { phone = item[1].ToString() };
                    var jsonstring = JsonConvert.SerializeObject(json);

                    var resp = await customerApiUrl.WithHeader("Content-Type", "application/json").WithHeaders(new { ApiKey = "YRQKMf7b24o82YTZYHkXLwjeHVF25D9O" }).PostStringAsync(jsonstring).ReceiveString();

                    var updateSQl = string.Format("update vcil.dbo.AvayaMyPortalNotify set processed = getdate() OUTPUT INSERTED.AvayaMyPortalNotifyId where AvayaMyPortalNotifyId = {0}", item[0]);
                    Sql.ExecuteScalar(connString, updateSQl);

                }
                catch (Exception ex)
                {
                    WriteToFile(ex.Message);
                    WriteToFile(ex.StackTrace);

                }

            }
            }
            catch (Exception ex)
            {

                WriteToFile(ex.Message);
                WriteToFile(ex.StackTrace);
            }
        }
    }
}

