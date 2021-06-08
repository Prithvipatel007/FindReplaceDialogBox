using ICSharpCode.AvalonEdit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FindAndReplaceDialog
{
    /// <summary>
    /// Interaction logic for FindReplaceDialog.xaml
    /// </summary>
    public partial class FindReplaceDialog : Window
    {
        FindReplace fr;
        public string selectedText;

        public FindReplaceDialog(FindReplace findReplace)
        {
            InitializeComponent();

            DataContext = fr = findReplace;

            var element = AutomationElement.FocusedElement;

            if (element != null)
            {
                object pattern;
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out pattern))
                {
                    var tp = (TextPattern)pattern;
                    var sb = new StringBuilder();

                    foreach (var r in tp.GetSelection())
                    {
                        sb.AppendLine(r.GetText(-1));
                    }

                    selectedText = sb.ToString().TrimEnd(new char[] { '\r', '\n' });
                }
            }

        }

        private void FindNextClick(object sender, RoutedEventArgs e)
        {
            fr.FindNext();
        }

        private void ReplaceClick(object sender, RoutedEventArgs e)
        {
            fr.Replace();
        }

        private void ReplaceAllClick(object sender, RoutedEventArgs e)
        {
            fr.ReplaceAll();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }

    public class FindReplace : DependencyObject
    {
        private FindReplaceDialog _dialog = null;

        FindReplaceDialog dialog
        {
            get
            {
                if (_dialog == null)
                {
                    _dialog = new FindReplaceDialog(this);
                    _dialog.Closed += delegate { _dialog = null; };
                    if (OwnerWindow != null)
                        _dialog.Owner = OwnerWindow;
                }
                return _dialog;
            }
        }

        public FindReplace()
        {
            ReplacementText = "";

            SearchIn = SearchScope.CurrentDocument;
        }

        #region Exposed CommandBindings
        public CommandBinding FindBinding
        {
            get { return new CommandBinding(ApplicationCommands.Find, (s, e) => ShowAsFind()); }
        }
        public CommandBinding FindNextBinding
        {
            get { return new CommandBinding(NavigationCommands.Search, (s, e) => FindNext(e.Parameter == null ? false : true)); }
        }
        public CommandBinding ReplaceBinding
        {
            get { return new CommandBinding(ApplicationCommands.Replace, (s, e) => { if (AllowReplace) ShowAsReplace(); }); }
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// The list of editors in which the search should take place.
        /// The elements must either implement the IEditor interface, or 
        /// InterfaceConverter should bne set.
        /// </summary>
        public IEnumerable Editors
        {
            get { return (IEnumerable)GetValue(EditorsProperty); }
            set { SetValue(EditorsProperty, value); }
        }
        public static readonly DependencyProperty EditorsProperty =
            DependencyProperty.Register(nameof(Editors), typeof(IEnumerable), typeof(FindReplace), new PropertyMetadata(null));


        /// <summary>
        /// The editor in which the current search operation takes place.
        /// </summary>
        public object CurrentEditor
        {
            get { return (object)GetValue(CurrentEditorProperty); }
            set { SetValue(CurrentEditorProperty, value); }
        }
        public static readonly DependencyProperty CurrentEditorProperty =
            DependencyProperty.Register(nameof(CurrentEditor), typeof(object), typeof(FindReplace), new PropertyMetadata(0));


        /// <summary>
        /// Objects in the Editors list that do not implement the IEditor interface are converted to IEditor using this converter.
        /// </summary>
        public IValueConverter InterfaceConverter
        {
            get { return (IValueConverter)GetValue(InterfaceConverterProperty); }
            set { SetValue(InterfaceConverterProperty, value); }
        }
        public static readonly DependencyProperty InterfaceConverterProperty =
            DependencyProperty.Register(nameof(InterfaceConverter), typeof(IValueConverter), typeof(FindReplace), new PropertyMetadata(null));

        public static readonly DependencyProperty TextToFindProperty =
        DependencyProperty.Register(nameof(TextToFind), typeof(string),
        typeof(FindReplace), new UIPropertyMetadata(""));
        public string TextToFind
        {
            get { return (string)GetValue(TextToFindProperty); }
            set { SetValue(TextToFindProperty, value); }
        }

        public string ReplacementText { get; set; }

        public bool UseWildcards
        {
            get { return (bool)GetValue(UseWildcardsProperty); }
            set { SetValue(UseWildcardsProperty, value); }
        }
        public static readonly DependencyProperty UseWildcardsProperty =
            DependencyProperty.Register(nameof(UseWildcards), typeof(bool), typeof(FindReplace), new UIPropertyMetadata(false));

        public bool SearchUp
        {
            get { return (bool)GetValue(SearchUpProperty); }
            set { SetValue(SearchUpProperty, value); }
        }
        public static readonly DependencyProperty SearchUpProperty =
            DependencyProperty.Register(nameof(SearchUp), typeof(bool), typeof(FindReplace), new UIPropertyMetadata(false));

        public bool CaseSensitive
        {
            get { return (bool)GetValue(CaseSensitiveProperty); }
            set { SetValue(CaseSensitiveProperty, value); }
        }
        public static readonly DependencyProperty CaseSensitiveProperty =
            DependencyProperty.Register(nameof(CaseSensitive), typeof(bool), typeof(FindReplace), new UIPropertyMetadata(false));

        public bool UseRegEx
        {
            get { return (bool)GetValue(UseRegExProperty); }
            set { SetValue(UseRegExProperty, value); }
        }
        public static readonly DependencyProperty UseRegExProperty =
            DependencyProperty.Register(nameof(UseRegEx), typeof(bool), typeof(FindReplace), new UIPropertyMetadata(false));

        public bool WholeWord
        {
            get { return (bool)GetValue(WholeWordProperty); }
            set { SetValue(WholeWordProperty, value); }
        }
        public static readonly DependencyProperty WholeWordProperty =
            DependencyProperty.Register(nameof(WholeWord), typeof(bool), typeof(FindReplace), new UIPropertyMetadata(false));

        public bool AcceptsReturn
        {
            get { return (bool)GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }
        public static readonly DependencyProperty AcceptsReturnProperty =
            DependencyProperty.Register(nameof(AcceptsReturn), typeof(bool), typeof(FindReplace), new UIPropertyMetadata(false));

        public enum SearchScope { CurrentDocument, AllDocuments }
        public SearchScope SearchIn
        {
            get { return (SearchScope)GetValue(SearchInProperty); }
            set { SetValue(SearchInProperty, value); }
        }
        public static readonly DependencyProperty SearchInProperty =
            DependencyProperty.Register(nameof(SearchIn), typeof(SearchScope), typeof(FindReplace), new UIPropertyMetadata(SearchScope.CurrentDocument));


        /// <summary>
        /// Determines whether to display the Search in combo box
        /// </summary>
        public bool ShowSearchIn
        {
            get { return (bool)GetValue(ShowSearchInProperty); }
            set { SetValue(ShowSearchInProperty, value); }
        }
        public static readonly DependencyProperty ShowSearchInProperty =
            DependencyProperty.Register(nameof(ShowSearchIn), typeof(bool), typeof(FindReplace), new UIPropertyMetadata(true));


        /// <summary>
        /// Determines whether the "Replace"-page in the dialog in shown or not.
        /// </summary>
        public bool AllowReplace
        {
            get { return (bool)GetValue(AllowReplaceProperty); }
            set { SetValue(AllowReplaceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AllowReplace.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowReplaceProperty =
            DependencyProperty.Register(nameof(AllowReplace), typeof(bool), typeof(FindReplace), new UIPropertyMetadata(true));



        /// <summary>
        /// The Window that serves as the parent of the Find/Replace dialog
        /// </summary>
        public Window OwnerWindow
        {
            get { return (Window)GetValue(OwnerWindowProperty); }
            set { SetValue(OwnerWindowProperty, value); }
        }
        public static readonly DependencyProperty OwnerWindowProperty =
            DependencyProperty.Register(nameof(OwnerWindow), typeof(Window), typeof(FindReplace), new UIPropertyMetadata(null));



        #endregion

        IEditor GetCurrentEditor()
        {
            if (CurrentEditor == null)
                return null;
            if (CurrentEditor is IEditor)
                return null;

            return InterfaceConverter.Convert(CurrentEditor, typeof(IEditor), null, CultureInfo.CurrentCulture) as IEditor;
        }

        IEditor GetNextEditor(bool previous = false)
        {
            if (!ShowSearchIn || SearchIn == SearchScope.CurrentDocument || Editors == null)
                return GetCurrentEditor();

            List<object> l = new List<object>(Editors.Cast<object>());
            int i = l.IndexOf(CurrentEditor);
            if (i >= 0)
            {
                i = (i + (previous ? l.Count - 1 : +1)) % l.Count;
                CurrentEditor = l[i];
            }
            return GetCurrentEditor();
        }

        public Regex GetRegEx(bool ForceLeftToRight = false)
        {
            Regex r;
            RegexOptions o = RegexOptions.None;
            if (SearchUp && !ForceLeftToRight)
                o = o | RegexOptions.RightToLeft;
            if (!CaseSensitive)
                o = o | RegexOptions.IgnoreCase;

            if (UseRegEx)
                r = new Regex(TextToFind, o);
            else
            {
                string s = Regex.Escape(TextToFind);
                if (UseWildcards)
                    s = s.Replace("\\*", ".*").Replace("\\?", ".");
                if (WholeWord)
                    s = "\\W" + s + "\\W";
                r = new Regex(s, o);
            }

            return r;
        }

        public void ReplaceAll(bool AskBefore = true)
        {
            IEditor CE = GetCurrentEditor();
            if (CE == null) return;

            if (!AskBefore || MessageBox.Show("Do you really want to replace all occurences of '" + TextToFind + "' with '" + ReplacementText + "'?",
                "Replace all", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                object InitialEditor = CurrentEditor;
                // loop through all editors, until we are back at the starting editor                
                do
                {
                    Regex r = GetRegEx(true);   // force left to right, otherwise indices are screwed up
                    int offset = 0;
                    CE.BeginChange();
                    foreach (Match m in r.Matches(CE.Text))
                    {
                        CE.Replace(offset + m.Index, m.Length, ReplacementText);
                        offset += ReplacementText.Length - m.Length;
                    }
                    CE.EndChange();
                    CE = GetNextEditor();
                } while (CurrentEditor != InitialEditor);
            }
        }

        public void ShowAsFind()
        {
            dialog.tabMain.SelectedIndex = 0;
            dialog.txtFind.SelectAll();
            dialog.Show();
            dialog.Activate();
            dialog.txtFind.Focus();

            dialog.txtFind.Text = dialog.selectedText;

        }
        public void ShowAsFind(TextEditor target)
        {
            CurrentEditor = target;
            ShowAsFind();
        }
        /// <summary>
        /// Shows this instance of FindReplaceDialog, with the Replace page active
        /// </summary>
        public void ShowAsReplace()
        {
            dialog.tabMain.SelectedIndex = 1;
            dialog.txtFind.SelectAll();
            dialog.Show();
            dialog.Activate();
            dialog.txtFind2.Focus();
            
            dialog.txtFind2.Text = dialog.selectedText;
        }
        public void ShowAsReplace(object target)
        {
            CurrentEditor = target;
            ShowAsReplace();
        }
        //static TextEditor txtCode;
        public void FindNext(object target, bool InvertLeftRight = false)
        {
            CurrentEditor = target;
            FindNext(InvertLeftRight);
        }
        public void FindNext(bool InvertLeftRight = false)
        {
            IEditor CE = GetCurrentEditor();
            if (CE == null) return;
            Regex r;
            if (InvertLeftRight)
            {
                SearchUp = !SearchUp;
                r = GetRegEx();
                SearchUp = !SearchUp;
            }
            else
                r = GetRegEx();

            Match m = r.Match(CE.Text, r.Options.HasFlag(RegexOptions.RightToLeft) ? CE.SelectionStart : CE.SelectionStart + CE.SelectionLength);
            if (m.Success)
            {
                CE.Select(m.Index, m.Length);
            }
            else
            {
                // we have reached the end of the document
                // start again from the beginning/end,
                object OldEditor = CurrentEditor;
                do
                {
                    if (ShowSearchIn)
                    {
                        CE = GetNextEditor(r.Options.HasFlag(RegexOptions.RightToLeft));
                        if (CE == null) return;
                    }
                    if (r.Options.HasFlag(RegexOptions.RightToLeft))
                        m = r.Match(CE.Text, CE.Text.Length - 1);
                    else
                        m = r.Match(CE.Text, 0);
                    if (m.Success)
                    {
                        CE.Select(m.Index, m.Length);
                        break;
                    }
                } while (CurrentEditor != OldEditor);
            }
        }

        public void FindPrevious()
        {
            FindNext(true);
        }

        public void Replace()
        {
            IEditor CE = GetCurrentEditor();
            if (CE == null) return;

            // if currently selected text matches -> replace; anyways, find the next match
            Regex r = GetRegEx();
            string s = CE.Text.Substring(CE.SelectionStart, CE.SelectionLength); // CE.SelectedText;
            Match m = r.Match(s);
            if (m.Success && m.Index == 0 && m.Length == s.Length)
            {
                var txt = ReplacementText;
                if (UseRegEx)
                    txt = r.Replace(s, ReplacementText);
                CE.Replace(CE.SelectionStart, CE.SelectionLength, txt);
                //CE.SelectedText = ReplacementText;
            }

            FindNext();
        }
        /// <summary>
        /// Closes the Find/Replace dialog, if it is open
        /// </summary>
        public void CloseWindow()
        {
            dialog.Close();
        }
    }


    public class SearchScopeToInt : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (FindReplace.SearchScope)value;
        }

    }

    public class BoolToInt : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return 1;
            return 0;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }


}
