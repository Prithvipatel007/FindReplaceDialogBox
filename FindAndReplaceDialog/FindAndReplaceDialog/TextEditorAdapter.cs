using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections;
using System;
using System.Windows.Forms.Integration;

namespace FindAndReplaceDialog
{
    public class TextEditorAdapter : IEditor
    {
        TextEditor textEditor;

        public TextEditorAdapter(TextEditor editor)
        {
            textEditor = editor;
        }

        public string Text { get => textEditor.Text; }
        public int SelectionStart { get => textEditor.SelectionStart; }
        public int SelectionLength { get => textEditor.SelectionLength; }

        public void BeginChange()
        {
            textEditor.BeginChange();
        }

        public void EndChange()
        {
            textEditor.EndChange();
        }

        public void Replace(int start, int length, string ReplaceWith)
        {
            textEditor.Document.Replace(start, length, ReplaceWith);
        }

        public void Select(int start, int length)
        {
            textEditor.Select(start, length);
            var locationOfText = textEditor.Document.GetLocation(start);
            textEditor.ScrollTo(locationOfText.Line, locationOfText.Column);
        }
    }

    public class IEditorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TextEditor)
                return new TextEditorAdapter(value as TextEditor);
            else return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
