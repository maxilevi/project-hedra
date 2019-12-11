using System.Collections.Generic;

namespace Hedra.Mission
{
    public class DialogObject
    {
        private readonly List<string> _beforeLines;
        private readonly List<string> _afterLines;

        public DialogObject()
        {
            _beforeLines = new List<string>();
            _afterLines = new List<string>();
        }

        public void AddBeforeLine(string Text)
        {
            _beforeLines.Add(Text);
        }
        
        public void AddAfterLine(string Text)
        {
            _afterLines.Add(Text);
        }
        
        
        public string Keyword { get; set; }
        public object[] Arguments { get; set; }
        public string[] BeforeDialog => _beforeLines.ToArray();
        public string[] AfterDialog => _afterLines.ToArray();
    }
}