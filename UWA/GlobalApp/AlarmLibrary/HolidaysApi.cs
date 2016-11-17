using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.Web.Http;

namespace AlarmLibrary
{
    public sealed class HolidaysApi
    {
        public static IEnumerable<Holiday> GetHolidays(int year, CountryCode countryCode)
        {
            var holidays = new List<Holiday>();
            const uint WEB_E_INVALID_JSON_STRING = 0x83750007;
            var uriStr = $"http://kayaposoft.com/enrico/json/v1.0/?action=getPublicHolidaysForYear&year={year}&country={countryCode}&region=";
            var client = new HttpClient();
            var result = client.GetStringAsync(new Uri(uriStr)).AsTask().GetAwaiter().GetResult();

            try
            {
                var jsonHolidays = JsonArray.Parse(result);

                foreach (var jsonHolidayValue in jsonHolidays)
                {
                    var jsonHoliday = jsonHolidayValue.GetObject();
                    var jsonDate = jsonHoliday["date"].GetObject();
                    var dayValue = jsonDate["day"].GetNumber();
                    var monthValue = jsonDate["month"].GetNumber();
                    var yearValue = jsonDate["year"].GetNumber();
                    var weekValue = jsonDate["dayOfWeek"].GetNumber();

                    var localNameValue = jsonHoliday["localName"].GetString();
                    var englishNameValue = jsonHoliday["englishName"].GetString();

                    var holiday = new Holiday();
                    checked { holiday.Date = new DateTime((int)yearValue, (int)monthValue, (int)dayValue); }
                    holiday.LocalDescription = localNameValue;
                    holiday.EnglishDescription = englishNameValue;
                    holidays.Add(holiday);
                }

                return holidays;
            }
            catch (Exception ex) // it can occur when error is returned which is returned as JSON object not JSON array
            {
                if ((uint)ex.HResult == WEB_E_INVALID_JSON_STRING)
                {
                    var errorObject = JsonObject.Parse(result);
                    var message = errorObject["error"].GetString();
                    throw new HolidayException(message);
                }
                else // unknown problem
                {
                    throw;
                }
            }
        }
    }

    public class HolidayException : Exception
    {
        public HolidayException(string message)
            :base(message)
        {

        }
    }

    public class Holiday
    {
        public DateTime Date { get; set; }
        public string LocalDescription { get; set; }
        public string EnglishDescription { get; set; }
    }

    public enum CountryCode
    {
        /// <summary>
        /// Czech Republic
        /// </summary>
        Cze = 3
    }
}
