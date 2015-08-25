using System;
using System.Collections;
using System.Collections.Generic;

namespace Spi.Data
{
    public class DeluxeEnumerator<T> : IEnumerator<T>
    {
        private IEnumerator<T> iter;
        private T _LastValue = default(T);

        private bool iterHasStarted = false;

        public DeluxeEnumerator(IEnumerable<T> enumerable)
        {
            this.HasMoved = false;
            this.iter = enumerable.GetEnumerator();
        }

        public T Current
        {
            get { return this.iter.Current; }
        }
        public T LastValue
        {
            get { return _LastValue; }
        }
        public bool HasMoved
        {
            get;
            private set;
        }
        public bool MoveNext()
        {
            if (iterHasStarted)
            {
                _LastValue = this.iter.Current;
            }
            else
            {
                iterHasStarted = true;
            }
            
            HasMoved = iter.MoveNext();
            return HasMoved;
        }
        public void Dispose()
        {
            this.iter.Dispose();
        }
        object IEnumerator.Current
        {
            get { return this.Current; }
        }
        public void Reset()
        {
            this.iter.Reset();
        }
    }
}
