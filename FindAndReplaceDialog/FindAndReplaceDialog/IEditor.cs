using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindAndReplaceDialog
{
    public interface IEditor
    {
        public string Text { get; }
        public int SelectionStart { get; }
        public int SelectionLength { get; }
        public void Select(int start, int length);
        public void Replace(int start, int length, string ReplaceWith);
        public void BeginChange();
        public void EndChange();
    }
}
