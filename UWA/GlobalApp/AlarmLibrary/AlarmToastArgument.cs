using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace AlarmLibrary
{
    public class AlarmToastArgument
    {
        [XmlAttribute]
        public AlarmToastArgumentType Type { get; private set; }
        [XmlAttribute]
        public int AlarmId { get; private set; }

        public AlarmToastArgument(AlarmToastArgumentType type, int alarmId)
        {
            this.Type = type;
            this.AlarmId = alarmId;
        }

        private const string Separator = ":";

        public string ToArgumentString()
        {
            var s = new XmlSerializer(typeof(AlarmToastArgument));
            using (var sw = new System.IO.StringWriter())
            {
                s.Serialize(sw, this);
                return sw.ToString();
            }
        }

        public static AlarmToastArgument FromArgumentString(string str)
        {
            var s = new XmlSerializer(typeof(AlarmToastArgument));
            using (var sw = new System.IO.StringReader(str))
            {
                return s.Deserialize(sw) as AlarmToastArgument;
            }
        }
    }

    public enum AlarmToastArgumentType
    {
        Snooze,
        Dismiss
    }
}
