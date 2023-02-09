using System;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;

namespace MyPortalMessageNotification
{
    class Program
    {
        static void Main(string[] args)
        {
          
            while(true)
            {
                ProcessMyPortalNotifications();
                Thread.Sleep(60000);
            }


        }

        static async void ProcessMyPortalNotifications() 
        {
            Console.WriteLine(string.Format("Processing Notifications - {0}",DateTime.Now.ToString("F")));

            var customerApiUrl = "https://customerapi.staging.businessinsurancesolutions.co.uk/customer/send-portal-link";
            var connString = "Data Source = 192.168.10.15; Initial Catalog = VCIL; User ID = VCApps; Password = vancompare; Application Name = MyPortalNotify";
            var sql = "select * from vcil.dbo.AvayaMyPortalNotify where processed is null";

            var toProcess = Sql.ExecuteQuery(connString, sql);

            //Parallel.ForEach(toProcess, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async item =>{
            //    Console.WriteLine(string.Format("Processing[{0}] at {1}", item[1].ToString(), DateTime.Now.ToString("F")));

            //    var json = new MyPortalNotification { phone = item[1].ToString() };
            //    var jsonstring = JsonConvert.SerializeObject(json);

            //    var resp = await customerApiUrl.WithHeader("Content-Type", "application/json").WithHeaders(new { ApiKey = "FAbWcRqKfKVGzDQ58CyV3J5eFyIhYoTk" }).PostStringAsync(jsonstring).ReceiveString();

            //    var updateSQl = string.Format("update vcil.dbo.AvayaMyPortalNotify set processed = getdate() OUTPUT INSERTED.AvayaMyPortalNotifyId where AvayaMyPortalNotifyId = {0}", item[0]);
            //    Sql.ExecuteScalar(connString, updateSQl);

            //});
            foreach (var item in toProcess)
            {
                try
                {

                
                Console.WriteLine(string.Format("Processing[{0}] at {1}", item[1].ToString(), DateTime.Now.ToString("F")));

                var json = new MyPortalNotification { phone = item[1].ToString() };
                var jsonstring = JsonConvert.SerializeObject(json);

                var resp = await customerApiUrl.WithHeader("Content-Type", "application/json").WithHeaders(new { ApiKey = "FAbWcRqKfKVGzDQ58CyV3J5eFyIhYoTk" }).PostStringAsync(jsonstring).ReceiveString();

                var updateSQl = string.Format("update vcil.dbo.AvayaMyPortalNotify set processed = getdate() OUTPUT INSERTED.AvayaMyPortalNotifyId where AvayaMyPortalNotifyId = {0}", item[0]);
                Sql.ExecuteScalar(connString, updateSQl);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    
                }

            }
        }
    }
}
