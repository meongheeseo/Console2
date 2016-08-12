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
        int manufacturer_id = 0; // 0 - Shinwoo, 1 - Bombardier, 2 - Siemens
        int device_id = 0; // 0 - Balise, 2 - LEU
        int func_id = 0; // 0 - ACK, 1 - NAK, 2 - Telegram Download, 3 - Telegram Upload

        public MainWindow()
        {
            InitializeComponent();
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
                manufacturer_id = 0;
            }
            catch (Exception) { }
        }

        private void manu_bombardier_Checked(object sender, RoutedEventArgs e)
        {
            position_gpbox.Visibility = Visibility.Visible;
            manufacturer_id = 1;
        }

        private void manu_siemens_Checked(object sender, RoutedEventArgs e)
        {
            position_gpbox.Visibility = Visibility.Hidden;
            manufacturer_id = 2;
        }
        
        
        // ----- CHECKS IF BALISE IS SELECTED ----- //
        private void pos_balise_Checked(object sender, RoutedEventArgs e)
        {
            device_id = 0;
        }

        private void pos_leu_Checked(object sender, RoutedEventArgs e)
        {
            device_id = 1;
        }

        // ----- READ, WRITE, RESET BUTTON CONTROL ----- //
        private void posread_btn_Click(object sender, RoutedEventArgs e)
        {
            if (device_id == 0)
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
            if (device_id == 0)
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

            if (serialport.IsOpen) { MessageBox.Show("Connection Open", "Message"); }
            else { MessageBox.Show("Recheck Connection", "Warning"); }
        }

        // ----- PROGRESS BAR ----- //
        private void progressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            progressLabel.Text = String.Format("{0}%", progressBar.Value);
        }

        private void download_btn_Click(object sender, RoutedEventArgs e)
        {
            func_id = 2;
            String filter = "";

            if (manufacturer_id == 0 && device_id == 0)
            {
                filter = "Shinwoo Balise File (*.BAL.ENC)|*.BAL.ENC|Shinwoo Balise File (*.ENC)|*.ENC";
            }
            else if (manufacturer_id == 0 && device_id == 1)
            {
                filter = "Shinwoo LEU File (*.LEU.ENC)|*.LEU.ENC|Shinwoo LEU File (*.ENC)|*.ENC";
            }
            else if (manufacturer_id == 1 && device_id == 0)
            {
                filter = "Bombarider Balise File (*.BAL)|*.BAL";
            }
            else if (manufacturer_id == 1 && device_id == 1)
            {
                filter = "Bombardier LEU File (*.LEU)|*.LEU";
            }
            else if (manufacturer_id == 2 && device_id == 0)
            {
                filter = "Siemens Balise File (*.TLG)|*.TLG";
            }
            else
            {
                filter = "Siemens LEU File (*.SDO)|*.SDO";
            }

            byte[] data = openFile(filter);

            SerialPortSend(data);
        }

        // Sends downloaded telegram message via serial port.
        public void SerialPortSend(byte[] data)
        { 
            // Preamble
            byte[] preamble = new byte[] { 0xAA, 0x55, 0x55, 0xAA };
            
            // Payload length
            int payload_length = 4 + 4 + 1 + 4 + 2 + 2 + data.Length;
            byte[] length = new byte[4];
            length[0] = (byte)(payload_length >> 24);
            length[1] = (byte)(payload_length >> 16);
            length[2] = (byte)(payload_length >> 8);
            length[3] = (byte)payload_length;

            // Source Sequence Number
            byte[] seq = new byte[] { 0x00, 0x01, 0x02, 0x03 };

            // Destination Sequence Number
            byte[] dest = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            
            // Protocol version
            byte[] version = new byte[] { 0x00 };

            // Category
            byte[] cat = new byte[4];
            cat[0] = (byte)((1 << 6) + (manufacturer_id << 3) + ((func_id >> 2)));
            cat[1] = (byte)((func_id << 6) + (device_id << 3));

            // Record seq
            byte[] record;
            // if length of the message exceeds 1024 byte, send 0x00, 0x01
            if (payload_length <= 1024) { record = new byte[] { 0x00, 0x00 }; }
            else { record = new byte[] { 0x00, 0x01 }; }

            // Data byte length
            byte[] data_length = new byte[2];
            data_length[0] = (byte)(data.Length >> 8);
            data_length[1] = (byte)(data.Length);

            // CRC32
            byte[] crc32 = new byte[] { 0x00, 0x00, 0x00, 0x00 };

            // postamble
            byte[] postamble = new byte[] { 0x55, 0xAA, 0xAA, 0x55 };

            serialport.Write(preamble, 0, preamble.Length);
            serialport.Write(length, 0, length.Length);
            serialport.Write(seq, 0, seq.Length);
            serialport.Write(dest, 0, dest.Length);
            serialport.Write(version, 0, version.Length);
            serialport.Write(cat, 0, cat.Length);
            serialport.Write(record, 0, record.Length); 
            serialport.Write(data_length, 0, data_length.Length);
            serialport.Write(data, 0, data.Length);
            serialport.Write(crc32, 0, crc32.Length);
            serialport.Write(postamble, 0, postamble.Length);

            serialport.Close();
            if (!serialport.IsOpen) { MessageBox.Show("Serial Port successfully closed.", "Message"); }
        }

        //
        // Open Telegram file and return read data.
        //
        public byte[] openFile(String filter)
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
