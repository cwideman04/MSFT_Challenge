using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;

namespace MSFT_Challenge
{
    public static class Function1
    {
        [FunctionName("FoodTrucks")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //Get Input Parameters
            string latitude = req.Query["latitude"];
            string longitude = req.Query["longitude"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            //Catch bad JSON Syntax
            try
            {
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                latitude = latitude ?? data?.latitude;
                longitude = longitude ?? data?.longitude;
            }
            catch (Exception)
            {
                return new BadRequestObjectResult(new { Error = "Unknown JSON Syntax" });
            }



            //Validate Input
            if (!decimal.TryParse(latitude, out decimal j) | (j < -90 || j > 90))
            {
                return new BadRequestObjectResult(new { Error = "Latitude is required and must be between - 90 and 90 degrees." });
            }

            if (!decimal.TryParse(longitude, out decimal k) | (k < -180 || k > 180))
            {
                return new BadRequestObjectResult(new { Error = "Longitude is required and must be between -180 and 180 degrees." });
            }




            string returntxt;

            // Get the DB connection string from app service settings and use it to create a connection.
            //Help with transient connection errors: Connection Timeout=30,ConnectRetryInterval=10,ConnectRetryCount=3
            var conn_str = Environment.GetEnvironmentVariable("sqldb_connection");


            //SQL String to get top 5 closest Food Trucks
            //Using MS SQL spatial data type: Geography to find distance from given point(Latitude,Longitude)
            var cmdText = "SELECT TOP(5) locationid,Applicant,FacilityType,Address,FoodItems,block,lot,schedule," +
                            "ROUND(GeoLocation.STDistance(geography::Point(@lat, @long, 4326)) * 0.000621371,4,1) AS DistanceMiles, " +
                            "Location " +
                            "FROM dbo.Mobile_Food " +
                            "WHERE  Status = 'APPROVED' AND GeoLocation.STDistance(geography::Point(@lat, @long, 4326)) IS NOT NULL " +
                            "ORDER BY GeoLocation.STDistance(geography::Point(@lat, @long, 4326));";


            try
            {
                //Connect to Azure SQL database
                using (SqlConnection conn = new SqlConnection(conn_str))
                {

                    //Use parameterized Query to help defend against SQL injections 
                    SqlCommand cmd = new SqlCommand(cmdText, conn);
                    cmd.Parameters.AddWithValue("@lat", latitude);
                    cmd.Parameters.AddWithValue("@long", longitude);

                    DataTable table = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    //Open Connection, Fill Table, Close Connection
                    conn.Open();
                    da.Fill(table);
                    conn.Close();

                    //Convert Return Table to JSON 
                    returntxt = JsonConvert.SerializeObject(table, Formatting.Indented);

                }
            }
            catch (Exception ex)
            {
                //If error occures write to log and clear any returntxt
                log.LogInformation("Datebase Connection Error" + ex.ToString());
                return new BadRequestObjectResult(new { Error = "Please pass in Latitude and Longitude in EPSG:4326 format" });
            }


            return returntxt != null
                ? (ActionResult)new OkObjectResult(returntxt)
                : new BadRequestObjectResult(new { Error = "Please pass in Latitude and Longitude in EPSG:4326 format" });
        }
    }
}
