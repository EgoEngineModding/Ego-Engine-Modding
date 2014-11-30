using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EgoJpkExtractor
{
    class Entry
    {
        int _bytesToName;
        int _size;
        int _bytestToBlockEnd;
        string _name;
        byte[] _file;

        public Entry()
        {
            _bytesToName = 0;
            _size = 0;
            _bytestToBlockEnd = 0;
            _name = "-";
            _file = null;
        }
        public Entry(int bytesToName, int size, int bytesToBlockEnd, string name, byte[] file)
        {
            _bytesToName = bytesToName;
            _size = size;
            _bytestToBlockEnd = bytesToBlockEnd;
            _name = name;
            _file = file;
        }

        public int BytesToName
        {
            get { return _bytesToName; }
            set { _bytesToName = value; }
        }

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public int BytesToBlockEnd
        {
            get { return _bytestToBlockEnd; }
            set { _bytestToBlockEnd = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public byte[] File
        {
            get { return _file; }
            set { _file = value; }
        }
    }
}
