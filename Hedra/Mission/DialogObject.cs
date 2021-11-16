using System.Collections.Generic;

namespace Hedra.Mission
{
    public class DialogObject
    {
        private readonly List<string> _afterLines;
        private readonly List<string> _beforeLines;

        public DialogObject()
        {
            _beforeLines = new List<string>();
            _afterLines = new List<string>();
        }

        public DialogObject(string Keyword) : this()
        {
            this.Keyword = Keyword;
        }

        public DialogObject(string Keyword, object[] Arguments) : this(Keyword)
        {
            this.Arguments = Arguments;
        }


        public string Keyword { get; set; }
        public object[] Arguments { get; set; }
        public string[] BeforeDialog => _beforeLines.ToArray();
        public string[] AfterDialog => _afterLines.ToArray();

        public void AddBeforeLine(string Text)
        {
            _beforeLines.Add(Text);
        }

        public void AddAfterLine(string Text)
        {
            _afterLines.Add(Text);
        }
    }
}