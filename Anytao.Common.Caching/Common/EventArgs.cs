using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anytao.Common
{
    public class EventArgs<T> : EventArgs
    {
        public EventArgs(T value)
        {
            this.value = value;
        }

        public T Value
        {
            get
            {
                return this.value;
            }
        }

        private T value;
    }

    public class EventArgs<T, U> : EventArgs
    {
        public EventArgs()
        { }

        public EventArgs(T t, U u)
        {
            this.tValue = t;
            this.uValue = u;
        }


        public T TValue
        {
            get
            {
                return this.tValue;
            }
        }

        public U UValue
        {
            get
            {
                return this.uValue;
            }
        }

        private T tValue;
        private U uValue;
    }
}
