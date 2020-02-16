using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PureMVC.Manager
{
    public class Data
    {
        private byte[] _bytes;
        public byte[] Bytes
        {
            get
            {
                return _bytes;
            }
        }
        private int _length;
        public int Length
        {
            get
            {
                return _length;
            }
        }

        public Data(byte[] bytes)
        {
            _bytes = (byte[])bytes.Clone();
            _length = _bytes.Length;
        }

        public Data(byte[] bytes, ref int startIndex, int len)
        {
            _bytes = new byte[len];
            for (int i = 0; i < len; i++)
            {
                _bytes[i] = bytes[startIndex + i];
            }
            _length = _bytes.Length;
            startIndex += _length;
        }
    }
}

