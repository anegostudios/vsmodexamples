using System;
using System.Collections.Generic;

namespace VSTreasureChest
{
    /// <summary>
    /// Data structure for picking random items
    /// </summary>
    public class ShuffleBag<T>
    {
        private readonly Random _random = new Random();
        private readonly List<T> _data;
        private T _currentItem;
        private int _currentPosition = -1;

        public int Size => _data.Count;

        public ShuffleBag(int initCapacity)
        {
            _data = new List<T>(initCapacity);
        }

        public ShuffleBag(int initCapacity, Random random)
        {
            _random = random;
            _data = new List<T>(initCapacity);
        }

        /// <summary>
        /// Adds the specified number of the given item to the bag
        /// </summary>
        public void Add(T item, int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                _data.Add(item);
            }

            _currentPosition = Size - 1;
        }

        /// <summary>
        /// Returns the next random item from the bag
        /// </summary>
        public T Next()
        {
            if (_currentPosition < 1)
            {
                _currentPosition = Size - 1;
                _currentItem = _data[0];

                return _currentItem;
            }

            var pos = _random.Next(_currentPosition);

            _currentItem = _data[pos];
            _data[pos] = _data[_currentPosition];
            _data[_currentPosition] = _currentItem;
            _currentPosition--;

            return _currentItem;
        }
    }
}