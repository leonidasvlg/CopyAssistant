using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CopyAssistant.Windows
{
    /// <summary>
    /// Interaction logic for TextEdit.xaml
    /// </summary>
    public partial class TextEdit : Window
    {

        /// <summary>
        /// Window initialization, will transfer the source textbox text and the textbox name to the window title
        /// </summary>
        /// <param name="inputText"></param>
        public TextEdit(TextBox inputText)
        {
            InitializeComponent();
       
            TextBoxEditor.Text = inputText.Text;
            TextEditWindow.Title = inputText.Name;
        }

        /// <summary>
        /// once the "OK" button is pressed, it will transfer the title and the textbot contents back to the Main Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAboutOk_Click(object sender, RoutedEventArgs e)
        {
            // We need to set TextEdit on an instance of the MainWindow class.
            ((MainWindow)Application.Current.MainWindow).UpdateTextBoxContents(TextEditWindow.Title, TextBoxEditor.Text);
            this.Close();
        }
    }
}
