using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CopyAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Globar Variables
        /// TextBox highlight colors
        /// Check if the textbox contents are changed or not
        /// File name and path
        /// </summary>
        SolidColorBrush brushSuccess = new SolidColorBrush(Color.FromArgb(255, 153, 255, 204));
        SolidColorBrush brushFail = new SolidColorBrush(Color.FromArgb(255, 255, 112, 77));
        SolidColorBrush brushDefault = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        bool _isChanged = false;
        string _filePathFull = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This assigns the "Copy" buttons to the corresponding textboxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            // Get the button Name
            var element = (System.Windows.Controls.Button)sender;
            string buttonNumber = element.Name.Replace("CopyBtn", "");

            // Get the TextBoxes
            foreach (TextBox tb in FindVisualChildren<TextBox>(CopyAssistant))
                if (tb.Name.Replace("TextBox", "") == buttonNumber)
                    CopyToClipBoard(tb);
        }


        /// <summary>
        /// This will copy the contents of the corresponding textbox to the clipboard
        /// It is rare but sometimes the copy to clipboard function may fail. 
        /// This will try 3 times, before declaring a failed copy operation.
        /// </summary>
        /// <param name="sender"></param>
        private void CopyToClipBoard(TextBox sender)
        {
            bool success = false;

            // try maximum 3 times to copy to clipboard, give 50ms between the retries
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    // write to clipboard
                    Clipboard.SetDataObject(sender.Text);

                    // check the clipboard
                    success = String.Equals(sender.Text, Clipboard.GetText());
                }
                catch (Exception e) { }

                if (success == true)
                    break;

                if (success == false)
                    System.Threading.Thread.Sleep(50);
            }

            // highlight the textbox
            TextBoxColor(sender, success);
        }


        /// <summary>
        /// This willl highlight the textbox when a "copy to clipboard" event takes place
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="success"></param>
        private void TextBoxColor(System.Windows.Controls.TextBox sender, bool success)
        {
            // restore the default color
            foreach (TextBox tb in FindVisualChildren<TextBox>(CopyAssistant))
                tb.Background = brushDefault;

            if (success == true)
                sender.Background = brushSuccess;

            if (success == false)
                sender.Background = brushFail;
        }


        /// <summary>
        /// Function to check the Main window and find all textboxes.
        /// This will be also used in future releases when there will be an option to dynamically add/remove textboxes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }


        /// <summary>
        /// Save button handler.. see below...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_filePathFull == null)
                SaveFunction(true);
            else
                SaveFunction(false);
        }

        /// <summary>
        /// Save As button handler... nothing special here :)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFunction(true);
        }

        /// <summary>
        /// Check if the content of each textbox is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentChanged(object sender, RoutedEventArgs e)
        {
            _isChanged = true;
        }


        /// <summary>
        /// Save function, It support save (without confirming the file name and path) and Save As (will open the dialog window)
        /// </summary>
        /// <param name="saveAs"></param>
        private void SaveFunction(bool saveAs)
        {
            // Get the textbox data
            List<string> textboxContents = new List<string>();
            foreach (TextBox tb in FindVisualChildren<TextBox>(CopyAssistant))
                textboxContents.Add(tb.Text);

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            if (_filePathFull == null)
                saveFileDialog.FileName = "contents";
            else
                saveFileDialog.FileName = System.IO.Path.GetFileName(_filePathFull);

            if (saveAs)
            {             
                saveFileDialog.Filter = "Text file (*.txt)|*.txt| Json file (*.json)|*.json";
                if (saveFileDialog.ShowDialog() == true)
                {
                    SaveFunction(saveFileDialog.FileName);
                }
            }
            else
            {
                SaveFunction(_filePathFull);
            }

        }


        /// <summary>
        /// Save Function only, this will create either a .json or a .txt file. 
        /// The .txt files are separated by linechanges. In case there are also linechanges within the textbox contents, they will be handled as well.
        /// </summary>
        /// <param name="filePathFull"></param>
        private void SaveFunction(string filePathFull)
        {
            // Get the textbox data
            List<string> textboxContents = new List<string>();
            foreach (TextBox tb in FindVisualChildren<TextBox>(CopyAssistant))
                textboxContents.Add(tb.Text);


            if (System.IO.Path.GetExtension(filePathFull) == ".json")
            {
                // Convert to json
                string jsonData = JsonConvert.SerializeObject(textboxContents);

                // Save the data
                File.WriteAllText(filePathFull, jsonData);
            }
            else
            {
                // Convert to txt with linechanges
                for (int i = 0; i < textboxContents.Count(); i++)
                {
                    textboxContents[i] = textboxContents[i]
                        .Replace("\r\n", "\\r\\n")
                        .Replace("\n", "\\n")
                        .Replace("\r", "\\r");
                }
                string textData = string.Join(Environment.NewLine, textboxContents);

                // Save the data
                File.WriteAllText(filePathFull, textData);
            }

            _isChanged = false;
            _filePathFull = filePathFull;
            UpdateTitle(System.IO.Path.GetFileName(filePathFull));
        }


        /// <summary>
        /// Close button automation, will check if there are any unsaved changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // check if there are any changes before overwriting the textbox contents
            if (_isChanged)
            {
                MessageBoxResult result = MessageBox.Show("There are unsaved changes. Do you want to save first?", "Unsaved changes", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        if (_filePathFull == null)
                            SaveFunction(true);
                        else
                            SaveFunction(false);
                        break;
                    case MessageBoxResult.No:
                        this.Close();
                        break;
                    case MessageBoxResult.Cancel:
                        break;
                }
            }
            else
            {
                this.Close();
            }
        }


        /// <summary>
        /// New File automation, will check if there are any unsaved changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void New_Click(object sender, RoutedEventArgs e)
        {
            // check if there are any changes before overwriting the textbox contents
            if (_isChanged)
            {
                MessageBoxResult result = MessageBox.Show("There are unsaved changes. Do you want to save first?", "Unsaved changes", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        if (_filePathFull == null)
                            SaveFunction(true);
                        else
                            SaveFunction(false);
                        break;
                    case MessageBoxResult.No:
                        Clear();
                        break;
                    case MessageBoxResult.Cancel:
                        break;
                }
            }
            else
            {
                Clear();
            }
        }


        /// <summary>
        /// This will clear all the textbox contents.
        /// </summary>
        private void Clear()
        {
            // clear all textboxes
            foreach (TextBox tb in FindVisualChildren<TextBox>(CopyAssistant))
                tb.Text = "";

            _isChanged = false;
            _filePathFull = null;
            UpdateTitle(System.IO.Path.GetFileName(_filePathFull));
        }


        /// <summary>
        /// File Loading automatiuon, will check if there are any unsaved changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            // check if there are any changes before overwriting the textbox contents
            if (_isChanged)
            {
                MessageBoxResult result = MessageBox.Show("There are unsaved changes. Do you want to save first?", "Unsaved changes", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        if (_filePathFull == null)
                            SaveFunction(true);
                        else
                            SaveFunction(false);
                        break;
                    case MessageBoxResult.No:
                        OpenFile();
                        break;
                    case MessageBoxResult.Cancel:                    
                        break;
                }
            }
            else
            {
                OpenFile();
            }
        }


        /// <summary>
        /// File Loading Function, will open the file selection Dialog. this will load .txt and .json files.
        /// The .txt files are separated by linechanges. In case there are also linechanges within the textbox contents, they will be handled as well.
        /// </summary>
        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text file (*.txt)|*.txt| Json file (*.json)|*.json";

            if (openFileDialog.ShowDialog() == true)
            {
                string loadedText = File.ReadAllText(openFileDialog.FileName);

                List<string> textboxContents = new List<string>();

                if (System.IO.Path.GetExtension(openFileDialog.FileName) == ".json")
                {
                    // Split using json deserializer
                    textboxContents = JsonConvert.DeserializeObject<List<string>>(loadedText);
                }
                else
                {
                    // Split using linechanges
                    textboxContents = loadedText.Split(
                            new[] { Environment.NewLine },
                            StringSplitOptions.None
                        ).ToList();
                }

                // recover linechanges if any
                for (int i = 0; i < textboxContents.Count(); i++)
                {
                    textboxContents[i] = textboxContents[i]
                        .Replace("\\r\\n", "\r\n")
                        .Replace("\\n", "\n")
                        .Replace("\\r", "\r");
                }

                // fill the textboxes
                List<TextBox> textBoxes = FindVisualChildren<TextBox>(CopyAssistant).ToList();
                int maxItems = textboxContents.Count();
                for (int i = 0; i < textBoxes.Count(); i++)
                {
                    if (i < maxItems)
                    {
                        textBoxes[i].Text = textboxContents[i];
                        textBoxes[i].Background = brushDefault;
                    }
                }

                _isChanged = true;
                _filePathFull = openFileDialog.FileName;
                UpdateTitle(System.IO.Path.GetFileName(_filePathFull));
            }
        }


        /// <summary>
        /// Update the Main Window Title, according to the filename
        /// </summary>
        /// <param name="fileName"></param>
        private void UpdateTitle(string fileName)
        {
            if (_filePathFull == null)
                CopyAssistant.Title = "CopyAssistant - New";
            else
                CopyAssistant.Title = "CopyAssistant - " + fileName;
        }


        /// <summary>
        /// Check for any unsaved contents before closing the main window from the top-right X button
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // check if there are any changes before overwriting the textbox contents
            if (_isChanged)
            {
                MessageBoxResult result = MessageBox.Show("There are unsaved changes. Do you want to save first?", "Unsaved changes", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        if (_filePathFull == null)
                            SaveFunction(true);
                        else
                            SaveFunction(false);
                        break;
                    case MessageBoxResult.No:
                        // This will close the window
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        base.OnClosing(e);
                        break;
                }
            }

        }


        /// <summary>
        /// This will open the "About" Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenAboutWindow(object sender, RoutedEventArgs e)
        {
            Windows.About AboutWindow = new Windows.About();
            AboutWindow.Show();
        }


        /// <summary>
        /// Double click on each textbox to open the editor window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = (System.Windows.Controls.TextBox)sender;
            Windows.TextEdit TextEditWindow = new Windows.TextEdit(element);
            TextEditWindow.Show();
        }


        /// <summary>
        /// Update the "Main" Window once you press the OK button in the Editor
        /// </summary>
        /// <param name="textBoxName"></param>
        /// <param name="textBoxContents"></param>
        public void UpdateTextBoxContents(string textBoxName, string textBoxContents)
        {
            foreach (TextBox tb in FindVisualChildren<TextBox>(CopyAssistant))
            {
                // replace the old textbox contents
                if (tb.Name == textBoxName)
                    tb.Text = textBoxContents;

                // restore textbox color
                // Ok I need to do this to the current texbox only..
                tb.Background = brushDefault;
            }
        }


    }
}
