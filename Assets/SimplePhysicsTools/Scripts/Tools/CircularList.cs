using System;
using System.Collections.Generic;

namespace SimplePhysicsTools.Tools
{
    [Serializable]
    public class CircularList<T> : List<T>
    {
        public CircularList(IEnumerable<T> collection) : base(collection) {}
        public CircularList(int capacity) : base(capacity) {}
        public CircularList() : base() {}

        public int PreviousIndex(int index) {
            return (Count + index - 1) % Count;
        }
        
        public int NextIndex(int index) {
            return (index + 1) % Count;
        }

        public bool IsFirstIndex(int index)
        {
            return index == 0;
        }

        public bool IsLastIndex(int index)
        {
            return index >= Count - 1;
        }
    }
}