using System;
using System.Collections.Generic;
using System.Threading;

namespace ArduinoCommunication
{
    public delegate void WhenFoundNewJson(string jsonString);

    internal class MultipleStringToJsonParser : IDisposable
    {
        public event WhenFoundNewJson foundNewJson;

        private ParsingState _parsingState = ParsingState.SearchingStart;
        private readonly Queue<string> _readQueue = new Queue<string>(1024);
        private Timer _processTimer;
        private string _currentJson;
        private string _afterJsonRemainder;

        public MultipleStringToJsonParser()
        {
            _processTimer = new Timer(ProcessQueue, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public void AddString(string received)
        {
            _readQueue.Enqueue(received);
        }

        private void ProcessQueue(object state)
        {
            if (_readQueue.Count == 0) return;

            while (_readQueue.Count > 0)
            {
                var current = _readQueue.Dequeue();
                if (current == null) continue;
                while (current.Length != 0)
                {
                    switch (_parsingState)
                    {
                        case ParsingState.SearchingStart:
                            {
                                current = SearchStart(current);
                                break;
                            }

                        case ParsingState.SearchingEnd:
                            {
                                current = SearchEnd(current);
                                break;
                            }
                    }
                }
            }
        }

        private string SearchEnd(string current)
        {
            var endPos = current.LastIndexOf('}');

            if (endPos == -1)
            {
                _currentJson += current;
                current = "";
            }

            if (endPos != -1)
            {
                _currentJson += current.Substring(0, endPos + 1);
                current = current.Substring(endPos + 1);

                foundNewJson(_currentJson);

                _currentJson = current;
                current = "";
                _parsingState = ParsingState.SearchingStart;
            }

            return current;
        }

        private string SearchStart(string current)
        {
            current = _afterJsonRemainder + current;
            _afterJsonRemainder = "";

            var startPos = current.IndexOf('{');
            if (startPos != -1)
            {
                current = current.Substring(startPos);
                _parsingState = ParsingState.SearchingEnd;
            }

            if (startPos == -1)
                current = "";
            return current;
        }

        public void Dispose()
        {
            _processTimer?.Dispose();
        }
    }
}
