using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CaldaiaBackend.SelfHosted
{
    public class Config
    {
        /// <summary>
        /// It's the address on which the app will be run after deployment
        /// During development will be "localhost".
        /// During test might be "services-test.sometestdomain.com"
        /// In production might be "services.proddomain.com"
        /// </summary>
        public string deployHost { get; set; }

        /// <summary>
        /// The host port where the app will be available. Eg.: 58007
        /// </summary>
        public int port { get; set; }

        /// <summary>
        /// Initial part of the uri on which the app will be accessed.
        /// This parameter should not start with a slash
        /// The app will be "http://{deployHost}:{deployPort}/{baseUrl}
        /// </summary>
        public string baseUrl { get; set; }

        /// <summary>
        /// The full composed deployUrl taking host, port and baseUrl
        /// </summary>
        public string deployUrl => $"http://{deployHost}:{port}/{baseUrl}";


        public string arduinoComPort { get; set; }
        public string pathToLast24HoursJson { get; set; }
        public string pathToLast24HoursTemperaturesJson { get; set; }
        public string pathToLastWeekAccumulatorsJson { get; set; }
        public string pathToLastWeekTemperaturesJson { get; set; }

        /// <summary>
        /// Converts a printable version of the config, with password info striped away.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var toReturn = JsonConvert.SerializeObject(this, Formatting.Indented);
            toReturn = Regex.Replace(toReturn, @"(P|p)assword(.*);", "Password=<hidden>;");
            return toReturn;
        }
    }
}
