using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmLibrary
{
    public enum DayOfWeekType
    {
        /// <summary>
        /// Used only to clear the enum. It has no other meaning.
        /// </summary>
        None = 0,
        Monday = 1,
        Tuesday = 1 << 2,
        Wednesday = 1 << 4,
        Thursday = 1 << 5,
        Friday = 1 << 6,
        Saturday = 1 << 7,
        Sunday = 1 << 8
    }
}
