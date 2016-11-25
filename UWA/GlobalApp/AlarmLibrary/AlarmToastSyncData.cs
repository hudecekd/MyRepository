using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmLibrary
{
    /// <summary>
    /// Data used by background task and app to communicate together.
    /// </summary>
    public class AlarmToastSyncData
    {
        /// <summary>
        /// Key used to store and access data in local settings.
        /// </summary>
        public const string SyncKey = nameof(AlarmToastSyncData);

        /// <summary>
        /// Name used by <see cref="MutexWrapper"/> to sync acces to data.
        /// </summary>
        public const string SyncName = nameof(AlarmToastSyncData);

        public int AlarmId { get; set; }
        public ToastType Type { get; set; }

        public string Serialize()
        {
            var jsonObject = new Windows.Data.Json.JsonObject();
            jsonObject[nameof(AlarmId)] = Windows.Data.Json.JsonValue.CreateNumberValue(AlarmId);
            jsonObject[nameof(Type)] = Windows.Data.Json.JsonValue.CreateNumberValue((int)Type);
            return jsonObject.Stringify();
        }

        public void Deserialize(string serializedObject)
        {
            var jsonObject = Windows.Data.Json.JsonObject.Parse(serializedObject);
            AlarmId = (int)jsonObject[nameof(AlarmId)].GetNumber();
            Type = (ToastType)jsonObject[nameof(Type)].GetNumber();
        }

        public enum ToastType
        {
            Snooze,
            Dismiss
        }
    }
}
