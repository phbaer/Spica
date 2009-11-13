using System;
using System.Collections.Generic;
using Castor;

namespace Spica
{
    public class ListDictionary<Key, Value>
    {
        protected IList<Key> keys = null;
        protected IList<Value> values = null;

        public ListDictionary()
        {
            this.keys = new List<Key>();
            this.values = new List<Value>();
        }

        public int Count
        {
            get { return this.keys.Count; }
        }

        public IList<Key> Keys
        {
            get { return this.keys; }
        }

        public IList<Value> Values
        {
            get { return this.values; }
        }

        public Value this[Key key]
        {
            get
            {
                if (!this.keys.Contains(key))
                {
                    return default(Value);
                }
                return this.values[this.keys.IndexOf(key)];
            }
            set
            {
                if (this.keys.Contains(key))
                {
                    this.values[this.keys.IndexOf(key)] = value;
                }
                else
                {
                    this.keys.Add(key);
                    this.values.Add(value);
                }
            }
        }

        public bool Contains(Key key)
        {
            return this.keys.Contains(key);
        }

        public void Add(Key key, Value value)
        {
            if (this.keys.Contains(key))
            {
                throw new CException("ListDictionary: Key '{0}' already defined!", key);
            }

            this.keys.Add(key);
            this.values.Add(value);
        }

        public void Add(ListDictionary<Key, Value> dict)
        {
            for (int i = 0; i < dict.Count; i++)
            {
                Add(dict.Keys[i], dict.Values[i]);
            }
        }
    }
}
