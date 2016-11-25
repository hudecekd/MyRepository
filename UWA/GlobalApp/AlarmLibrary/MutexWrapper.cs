using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlarmLibrary
{
    /// <summary>
    /// Logic of this class is taken from
    /// http://stackoverflow.com/questions/35843812/how-to-synchronize-resource-access-between-uwp-app-and-its-background-tasks.
    /// </summary>
    public class MutexWrapper : IDisposable
    {
        // TODO: add destructor (if dispose will be missed)
        private Mutex _mutex;
        private bool _owner;
        public MutexWrapper(TimeSpan timeout, Guid uniqueId, string name)
        {
            bool created;
            try
            {
                string mutextFullname = uniqueId + name;
                _mutex = new Mutex(false, mutextFullname, out created);
                _owner = _mutex.WaitOne(timeout);
                if (_owner == false) throw new TimeoutException("Unable to acquire mutex!");
            }
            catch (AbandonedMutexException ex)
            {
                _owner = true; // another thread abonded it we can still continue
            }
        }

        public void Dispose()
        {
           if (_owner)
            {
                _mutex.ReleaseMutex();
            }
            _mutex.Dispose();
        }
    }
}
