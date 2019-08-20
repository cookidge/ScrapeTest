using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using JSON=Newtonsoft.Json;

namespace ScrapeTest
{
    class Program
    {
        private static string loginUrl = "https://pv.idomoo.com/xhr-idmadm/xhr.php?cls=login";
        private static string statsUrl = "https://pv.idomoo.com/xhr-idmadm/xhr.php?cls=stats";
        private static string logoutUrl = "https://pv.idomoo.com/xhr-idmadm/xhr.php?cls=logout";
        private static string email = "zoe.skipper@tangible.uk.com";
        private static string password = "123456";
        private static string cookieInfo = "";
        private static string xsrfToken = "";
        private static string sboardId = "8269";
        private static string datePattern = "yyyy-MM-dd";
        private static string dateFrom = DateTime.Now.AddDays(-14).ToString(datePattern);
        private static string dateTo = DateTime.Now.ToString(datePattern);
        private static string outputDirectory = @"C:\Users\jcooke\Desktop\";
        private static string outputPath = outputDirectory + "percentage_watched_" + dateTo.Replace("-","") + ".csv";

        static void Main(string[] args)
        {
            /*************
                Login
            **************/
            try
            {
                Console.Write("Try login... ");

                // payload - {"remember":true,"email":"email","password":"password"}
                string payloadStr = "{\"remember\":true,\"email\":\"" + email + "\",\"password\":\"" + password + "\"}";

                // Set payload
                byte[] payload = Encoding.UTF8.GetBytes(payloadStr);

                // Create request
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(loginUrl);
                webRequest.Method = "POST";

                // Write to request stream
                Stream reqStream = webRequest.GetRequestStream();
                reqStream.Write(payload,0,payload.Length);

                // Retrieve response
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                Stream respStream = response.GetResponseStream();

                //Print response 
                using (StreamReader reader = new StreamReader(respStream))
                {
                    foreach (string headerName in response.Headers.Keys)
                    {
                        if (headerName == "Set-Cookie")
                        {
                            cookieInfo = response.GetResponseHeader(headerName);
                            xsrfToken = cookieInfo.Substring(56,40);
                            Console.Write("SUCCESS");
                            Console.WriteLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
            }

            /*************
                Stats
            **************/
            try
            {
                Console.Write("Try stats... ");

                // payload - {"chart":"progress","sb_id":"sboardId","stat_id":null,"dates":{"dateFrom":"dateFrom","dateTo":"dateTo"}}
                string payloadStr = "{\"chart\":\"progress\",\"sb_id\":" + sboardId
                        + ",\"stat_id\":null,\"dates\":{\"dateFrom\":\"" + dateFrom
                                                    + "\",\"dateTo\":\"" + dateTo
                                                    + "\"}}";

                // Set payload
                byte[] payload = Encoding.UTF8.GetBytes(payloadStr);

                // Create request
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(statsUrl);
                webRequest.Method = "POST";
                webRequest.Headers.Add("Cookie:" + cookieInfo);
                webRequest.Headers.Add("X-XSRF-TOKEN:" + xsrfToken);

                // Write to request stream
                Stream reqStream = webRequest.GetRequestStream();
                reqStream.Write(payload, 0, payload.Length);

                // Retrieve response
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                Stream respStream = response.GetResponseStream();

                //Print response 
                using (StreamReader reader = new StreamReader(respStream))
                {
                    dynamic jsonResponse = JSON.JsonConvert.DeserializeObject(reader.ReadToEnd());
                    string index = "";

                    Console.Write("SUCCESS");
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("Results from " + dateFrom + " to " + dateTo);
                    Console.WriteLine("-------------------------------------");

                    // Output to console/file
                    using (StreamWriter writer = new StreamWriter(outputPath))
                    {
                        writer.WriteLine("0,10,20,30,40,50,60,70,80,90,100");

                        for (int i = 0; i <= 10; i++)
                        {
                            index = (i > 0) ? i.ToString() + "0" : i.ToString();
                            writer.Write((i == 10) ? jsonResponse["result"]["data"][index] : jsonResponse["result"]["data"][index] + ","); 
                            Console.WriteLine(index + "% :   " + jsonResponse["result"]["data"][index] + " people");
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine("Generate time: " + jsonResponse["result"]["generate_time"]);
                    Console.WriteLine("Generated on:  " + DateTime.Now.ToString());
                    Console.WriteLine();
                    Console.WriteLine("Output to:  " + outputPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UNSUCCESSFUL: " + ex.Message);
                Console.WriteLine();
            }

            /*************
                Logout
            **************/
            try
            {
                Console.Write("Try logout...");

                // Create request
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(logoutUrl);
                webRequest.Method = "POST";
                webRequest.Headers.Add("Cookie:" + cookieInfo);
                webRequest.Headers.Add("X-XSRF-TOKEN:" + xsrfToken);

                // Retrieve response
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                Stream respStream = response.GetResponseStream();

                //Print response 
                using (StreamReader reader = new StreamReader(respStream))
                {
                    dynamic jsonResponse = JSON.JsonConvert.DeserializeObject(reader.ReadToEnd());
                    Console.Write((jsonResponse["success"] == "True") ? " Logged out successfully" : " Logout failed");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UNSUCCESSFUL: " + ex.Message);
            }
        }
    }
}
