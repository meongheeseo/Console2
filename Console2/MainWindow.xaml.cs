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
using System.IO;
using System.IO.Ports;
using Microsoft.Win32;

namespace Console2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int manufacturer_id = 1; // 1 - Shinwoo, 2 - Bombardier, 3 - Siemens

        public MainWindow()
        {
            InitializeComponent();
            //msgbox.AppendText("HI", "Red");
            position_gpbox.Visibility = Visibility.Hidden;
            progressLabel.Text = String.Format("{0}%", progressBar.Value);
        }

        // ----- CHECK WHICH MANUFACTURER IS SELECTED ----- //
        //
        // Only enable position group box if Bombardier is selected
        //
        private void manu_shinwoo_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                position_gpbox.Visibility = Visibility.Hidden;
                manufacturer_id = 1;
            }
            catch (Exception) { }
        }

        private void manu_bombardier_Checked(object sender, RoutedEventArgs e)
        {
            position_gpbox.Visibility = Visibility.Visible;
            manufacturer_id = 2;
        }

        private void manu_siemens_Checked(object sender, RoutedEventArgs e)
        {
            position_gpbox.Visibility = Visibility.Hidden;
            manufacturer_id = 3;
        }
        
        
        // ----- CHECKS IF BALISE IS SELECTED ----- //
        Boolean balise_checked = true;

        private void pos_balise_Checked(object sender, RoutedEventArgs e)
        {
            balise_checked = true;
        }

        private void pos_leu_Checked(object sender, RoutedEventArgs e)
        {
            balise_checked = false;
        }

        // ----- READ, WRITE, RESET BUTTON CONTROL ----- //
        private void posread_btn_Click(object sender, RoutedEventArgs e)
        {
            if (balise_checked)
            {
                MessageBox.Show("Connect Balise Cable", "Warning", MessageBoxButton.OKCancel);
            }
            else
            {
                MessageBox.Show("Connect LEU Cable", "Warning", MessageBoxButton.OKCancel);
            }
        }

        private void poswrite_btn_Click(object sender, RoutedEventArgs e)
        {
            if (balise_checked)
            {
                MessageBox.Show("Connect Balise Cable", "Warning", MessageBoxButton.OKCancel);
            }
            else
            {
                MessageBox.Show("Connect LEU Cable and Check the LEU Power", "Warning", MessageBoxButton.OKCancel);
            }
        }

        private void reset_btn_Click(object sender, RoutedEventArgs e)
        {
            countrycode_txt.Text = String.Empty;
            groupcode_txt.Text = String.Empty;
            poscode_txt.Text = String.Empty;
        }

        // ----- MESSAGEBOX SAVE & CLEAR FUNCTIONS ----- //
        private void msgSave_btn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();

            // if you don't need to keep the color of the text, use *.txt instead.
            saveFile.Filter = "Rich Text Format (*.rtf)|*.rtf";
            saveFile.DefaultExt = "rtf";
            saveFile.AddExtension = true;

            if (saveFile.ShowDialog() == true && saveFile.FileName.Length > 0)
            {
                TextRange range;
                FileStream fStream;

                // if you don't need to keep the color of the text, use RichTextBoxStreamType.PlainText
                range = new TextRange(msgbox.Document.ContentStart, msgbox.Document.ContentEnd);
                fStream = new FileStream(saveFile.FileName, FileMode.Create);
                range.Save(fStream, DataFormats.Rtf);
                fStream.Close();
            }
        }
        
        private void msgClear_btn_Click(object sender, RoutedEventArgs e)
        {
            msgbox.Document.Blocks.Clear();
        }

        // ----- SERIAL PORT ----- //
        private SerialPort serialport;

        private void serialConnect_btn_Click(object sender, RoutedEventArgs e)
        {
            // Add inserted value into serialportbox
            var value = serialportbox.Text;

            serialportbox.Items.Remove(value);
            serialportbox.Items.Add(value);
            serialportbox.Items.SortDescriptions.Add(
                new System.ComponentModel.SortDescription("", System.ComponentModel.ListSortDirection.Ascending));

            // Connect the serial port.
            serialport = new SerialPort(value, 38400, Parity.None, 8, StopBits.One);
            serialport.Open();
        }

        // ----- PROGRESS BAR ----- //
        private void progressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            progressLabel.Text = String.Format("{0}%", progressBar.Value);
        }

        private void download_btn_Click(object sender, RoutedEventArgs e)
        {
            String filter = "";

            if (manufacturer_id == 1 && balise_checked)
            {
                filter = "Shinwoo Balise File (*.BAL.ENC)|*.BAL.ENC|Shinwoo Balise File (*.ENC)|*.ENC";
            }
            else if (manufacturer_id == 1 && !balise_checked)
            {
                filter = "Shinwoo LEU File (*.LEU.ENC)|*.LEU.ENC|Shinwoo LEU File (*.ENC)|*.ENC";
            }
            else if (manufacturer_id == 2 && balise_checked)
            {
                filter = "Bombarider Balise File (*.BAL)|*.BAL";
            }
            else if (manufacturer_id == 2 && !balise_checked)
            {
                filter = "Bombardier LEU File (*.LEU)|*.LEU";
            }
            else if (manufacturer_id == 3 && balise_checked)
            {
                filter = "Siemens Balise File (*.TLG)|*.TLG";
            }
            else
            {
                filter = "Siemens LEU File (*.SDO)|*.SDO";
            }

            byte[] data = openFile(filter);
            int size = data.Length;
            int printCount = 0;

            while (printCount < size)
            {
                // Output file data in hex format
                msgbox.AppendText(String.Format("{0:X}", data[printCount]));
                printCount++;
            }
        }

        //
        // Open Telegram file and return read data.
        //
        private byte[] openFile(String filter)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = filter;

            int size;
            byte[] data; 

            // Only read in data if user selects a file and presses "Open".
            if (openFile.ShowDialog() == true)
            {
                using (FileStream stream = new FileStream(openFile.FileName, FileMode.Open, FileAccess.Read))
                {
                    size = (int)stream.Length;
                    data = new byte[size];
                    stream.Read(data, 0, size);
                    return data;
                }
            }

            return null;
        }
    }

    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, String text, String color)
        {
            BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, bc.ConvertFromString(color));
            }
            catch (NotSupportedException) { }
        }
    }
}
