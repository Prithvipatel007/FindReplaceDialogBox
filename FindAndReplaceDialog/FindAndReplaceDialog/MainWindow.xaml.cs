using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using ICSharpCode.AvalonEdit;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Forms.Integration;

namespace FindAndReplaceDialog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();

            FindReplace fr = Resources["FRep"] as FindReplace;
            fr.OwnerWindow = this;
            //CommandBindings.Add(fr.FindBinding);
            //BindingOperations.SetBinding(fr, FindReplace.CurrentEditorProperty, new Binding(nameof(MyViewData.ActiveView)) { Source=this, Mode=BindingMode.TwoWay });
            //BindingOperations.SetBinding(fr, FindReplace.EditorsProperty, new Binding(nameof(MyViewData.Views)) { Source = this });
        }

        private void MW_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

    public class MyViewData : DependencyObject
    {
        public ObservableCollection<UIElement> Views
        {
            get { return (ObservableCollection<UIElement>)GetValue(ViewsProperty); }
            set { SetValue(ViewsProperty, value); }
        }
        public static readonly DependencyProperty ViewsProperty =
            DependencyProperty.Register(nameof(Views), typeof(ObservableCollection<UIElement>), typeof(MyViewData), new UIPropertyMetadata(new ObservableCollection<UIElement>()));

        public UIElement ActiveView
        {
            get { return (UIElement)GetValue(ActiveViewProperty); }
            set { SetValue(ActiveViewProperty, value); }
        }
        public static readonly DependencyProperty ActiveViewProperty =
            DependencyProperty.Register(nameof(ActiveView), typeof(UIElement), typeof(MyViewData), new UIPropertyMetadata(null));

        public MyViewData()
        {
            Views.Clear();
            Views.Add(new TextEditor() { Tag = "TextEditor file", Text = TestString });
            ActiveView = Views[0];
        }
        const string TestString =
@"Python is an interpreted high-level general-purpose programming language. Python's design philosophy emphasizes code 
readability with its notable use of significant indentation. Its language constructs as well as its object-oriented 
approach aim to help programmers write clear, logical code for small and large-scale projects.

Python is dynamically-typed and garbage-collected. It supports multiple programming paradigms, including structured 
(particularly, procedural), object-oriented and functional programming. Python is often described as a batteries 
included language due to its comprehensive standard library.

";
    }

    public class StaticResourceEx : StaticResourceExtension
    {
        public PropertyPath Path { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            object o = base.ProvideValue(serviceProvider);
            return (Path == null ? o : PathEvaluator.Eval(o, Path));
        }

        class PathEvaluator : DependencyObject
        {
            private static readonly DependencyProperty DummyProperty =
                DependencyProperty.Register("Dummy", typeof(object), typeof(PathEvaluator), new UIPropertyMetadata(null));

            public static object Eval(object source, PropertyPath path)
            {
                PathEvaluator d = new PathEvaluator();
                BindingOperations.SetBinding(d, DummyProperty, new Binding(path.Path) { Source = source });
                return d.GetValue(DummyProperty);
            }
        }
    }
}
