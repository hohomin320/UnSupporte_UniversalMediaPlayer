﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Services.Helpers
{
    internal partial class Query : IDictionary<string, string>, IReadOnlyDictionary<string, string>
    {
        private int count;
        private readonly string baseUri;
        private KeyValuePair<string, string>[] pairs;

        public Query(string uri)
        {
            var divide = uri.IndexOf('?');

            if (divide == -1)
            {
                var amp = uri.IndexOf('&');
                if (amp == -1)
                {
                    // no query parameters
                    baseUri = uri;
                    return;
                }

                // no base URL
                baseUri = null;
            }
            else
            {
                // normal URL
                baseUri = uri.Substring(0, divide);
                uri = uri.Substring(divide + 1);
            }

            var keyValues = uri.Split('&');
            pairs = new KeyValuePair<string, string>[keyValues.Length];

            for (int i = 0; i < keyValues.Length; i++)
            {
                var pair = keyValues[i];
                var equals = pair.IndexOf('=');
                var key = string.Empty;
                var value = string.Empty;

                if (equals != pair.LastIndexOf('='))
                {
                    key = pair.Substring(0, equals);
                    value = String.Empty;
                }
                else
                {
                    key = pair.Substring(0, equals);
                    value = pair.Substring(equals + 1);
                }

                pairs[i] = new KeyValuePair<string, string>(key, value);
            }

            count = keyValues.Length;
        }

        public string this[string key]
        {
            get
            {
                for (int i = 0; i < count; i++)
                {
                    var pair = pairs[i];
                    if (pair.Key == key)
                        return pair.Value;
                }

                throw new KeyNotFoundException();
            }

            set
            {
                for (int i = 0; i < count; i++)
                {
                    var pair = pairs[i];
                    if (pair.Key == key)
                    {
                        pairs[i] = new KeyValuePair<string, string>(key, value);
                        return;
                    }
                }

                throw new KeyNotFoundException();
            }
        }

        public string BaseUri => baseUri;

        public int Count => count;

        public bool IsReadOnly => false;

        public KeyCollection Keys => new KeyCollection(this);

        ICollection<string> IDictionary<string, string>.Keys
        {
            get
            {
                var keys = new List<string>();

                foreach (var pair in pairs)
                    keys.Add(pair.Key);

                return keys;
            }
        }

        public KeyValuePair<string, string>[] Pairs
        {
            get { return pairs; }
        }

        public ValueCollection Values
        {
            get
            {
                return new ValueCollection(this);
            }
        }

        ICollection<string> IDictionary<string, string>.Values
        {
            get
            {
                var values = new List<string>();

                foreach (var pair in pairs)
                    values.Add(pair.Value);

                return values;
            }
        }
        
        IEnumerable<string> IReadOnlyDictionary<string, string>.Keys => Keys;

        IEnumerable<string> IReadOnlyDictionary<string, string>.Values => Values;

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(string key, string value)
        {
            EnsureCapacity(count + 1);
            pairs[count++] = new KeyValuePair<string, string>(key, value);
        }

        public void Clear()
        {
            if (count == 0)
                return;

            Array.Clear(pairs, 0, count);
            count = 0;
        }

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        {
            for (int i = 0; i < count; i++)
            {
                var pair = pairs[i];

                if (item.Key == pair.Key && 
                    item.Value == pair.Value)
                    return true;
            }
            return false;
        }

        public bool ContainsKey(string key)
        {
            for (int i = 0; i < count; i++)
            {
                if (key == pairs[i].Key)
                    return true;
            }
            return false;
        }

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            Array.Copy(pairs, 0, array, arrayIndex, count);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
                yield return pairs[i];
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            return Remove(item.Key);
        }

        public bool Remove(string key)
        {
            for (int i = 0; i < count; i++)
            {
                var pair = pairs[i];
                if (pair.Key == key)
                {
                    // found it
                    if (i != count--)
                        Array.Copy(pairs, i + 1, pairs, i, count - i);
                    pairs[count] = default(KeyValuePair<string, string>);
                    return true;
                }
            }
            return false;
        }

        public bool TryGetValue(string key, out string value)
        {
            for (int i = 0; i < count; i++)
            {
                var pair = pairs[i];
                if (key == pair.Key)
                {
                    value = pair.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            if (count == 0)
                return baseUri;

            var builder = new StringBuilder();
            if (baseUri != null)
                builder.Append(baseUri).Append('?');

            var pair = pairs[0]; // OK since we know count is at least 1
            builder.Append(pair.Key)
                .Append('=').Append(pair.Value);

            for (int i = 1; i < count; i++)
            {
                pair = pairs[i];

                builder.Append('&').Append(pair.Key)
                    .Append('=').Append(pair.Value);
            }

            return builder.ToString();
        }

        private void EnsureCapacity(int capacity)
        {
            if (capacity > pairs.Length)
            {
                capacity = Math.Max(capacity, pairs.Length * 2);

                Array.Resize(ref pairs, capacity);
            }
        }
    }
}
