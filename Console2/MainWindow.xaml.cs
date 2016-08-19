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

using System.Runtime.InteropServices;

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

        //[DllImport("SeparateTelegram.dll")]
        //public static extern IntPtr Separate(byte[] src, int clen);
        //public static extern int Separate(int src, int clen);

        [DllImport("TelegramSeparation.dll", EntryPoint="Separate", CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr Separate(byte[] src, int clen);

        private void upload_btn_Click(object sender, RoutedEventArgs e)
        {
            byte[] upLoadstream = new byte[] { 
                0xD7,0x20,0x62,0x57,0xF3,0x3C,0xCC,0x6B,0x1A,0xF5,0x78,0x4A,0xB4,0x20,0x76,0x8D,    //1
                0x2E,0x6F,0xC1,0xDC,0x2A,0x56,0xAF,0x69,0x3B,0xC9,0xF3,0x8F,0xD9,0xCC,0x0C,0x37,    //2
                0x22,0x3C,0xC7,0xB4,0x4F,0xF0,0xE3,0xBD,0x45,0xC9,0xE0,0xE5,0x60,0x87,0x8C,0x72,    //3
                0x2F,0x94,0xAF,0xC1,0x66,0xA2,0x6B,0xCD,0x4A,0xC7,0x74,0xA2,0xE3,0x5F,0x4D,0x93,    //4
                0x3E,0xFA,0x9D,0x47,0xE9,0xFC,0x50,0xD1,0x1A,0x1E,0xF2,0x08,0xB2,0xEC,0x17,0x8D,    //5 
                0x1F,0x3B,0x6B,0x82,0x1B,0x47,0x78,0x0B,0xE2,0xE3,0x5B,0x03,0x23,0x69,0x41,0x7F,    //6 
                0xFE,0x72,0x23,0xCC,0x7B,0x44,0xFF,0x0E,0x3B,0xD4,0x5C,0x9E,0x0E,0x56,0x08,0x78,    //7
                0xC7,0x22,0xF9,0x4A,0xFC,0x16,0x6A,0x26,0xBC,0xD4,0xAC,0x77,0x4A,0x2E,0x35,0xF4,    //8
                0xD9,0x33,0xEF,0xA9,0xD4,0x7E,0x9F,0xC5,0x0D,0x11,0xA1,0xEF,0x20,0x8B,0x2E,0xC1,    //9 
                0x78,0xD1,0xF3,0xB6,0xB8,0x21,0xB4,0x77,0x80,0xBE,0x2E,0x35,0xB0,0x32,0x36,0x94,    //10
                0x1F,0x18,0x88,0x0A,0xAD,0x46,0x11,0xB8,0x7F,0x46,0x52,0x10,0x44,0x09,0x83,0x0C,    //11
                0x2B,0x4A,0xC2,0x6E,0x62,0xE1,0x2A,0x69,0xC4,0x0F,0x90,0xA8,0x55,0x68,0x9D,0x40,    //12 
                0x43,0xBA,0xE4,0x0C,0x4A,0xFE,0x67,0x99,0x8D,0x63,0x5E,0xAF,0x09,0x56,0x84,0x0E,    //13 
                0xD1,0xA5,0xCD,0xF8,0x3B,0x85,0x4A,0xD5,0xED,0x27,0x79,0x3E,0x71,0xFB,0x39,0x81,    //14 
                0x86,0xE4,0x47,0x98,0xF6,0x89,0xFE,0x1C,0x77,0xA8,0xB9,0x3C,0x1C,0xAC,0x10,0xF1,    //15 
                0x8E,0x45,0xF2,0x95,0xF8,0x2C,0xD4,0x4D,0x79,0xA9,0x58,0xEE,0x94,0x5C,0x6B,0xE9,    //16 
                0xB2,0x67,0xDF,0x53,0xA8,0xFD,0x3F,0x8A,0x1A,0x23,0x43,0xDE,0x41,0x16,0x5D,0x82,    //17 
                0xF1,0xA3,0xE7,0x6D,0x70,0x43,0x68,0xEF,0x01,0x7C,0x5C,0x6B,0x60,0x64,0x6D,0x28,    //18 
                0x3E,0x31,0x10,0x15,0x5A,0x8C,0x23,0x70,0xFE,0x8C,0xA4,0x20,0x88,0x13,0x06,0x18,    //19 
                0x56,0x95,0x84,0xDC,0xC5,0xC2,0x54,0xD3,0x88,0x1F,0x21,0x50,0xAA,0xD1,0x3A,0x80,    //20 
                0x87,0x75,0xC8,0x18,0x95,0xFC,0xCF,0x33,0x1A,0xC6,0xBD,0x5E,0x12,0xAD,0x08,0x1D,    //21 
                0xA3,0x4B,0x9B,0xF0,0x77,0x0A,0x95,0xAB,0xDA,0x4E,0xF2,0x7C,0xE3,0xF6,0x73,0x03,    //22 
                0x0D,0xC8,0x8F,0x31,0xED,0x13,0xFC,0x38,0xEF,0x51,0x72,0x78,0x39,0x58,0x21,0xE3,    //23 
                0x1C,0x8B,0xE5,0x2B,0xF0,0x59,0xA8,0x9A,0xF3,0x52,0xB1,0xDD,0x28,0xB8,0xD7,0xD3,    //24 
                0x64,0xCF,0xBE,0xA7,0x51,0xFA,0x7F,0x14,0x34,0x46,0x87,0xBC,0x82,0x2C,0xBB,0x05,    //25 
                0xE3,0x47,0xCE,0xDA,0xE0,0x86,0xD1,0xDE,0x02,0xF8,0xB8,0xD6,0xC0,0xC8,0xDA,0x50,    //26 
                0x7C,0x62,0x20,0x2A,0xB5,0x18,0x46,0xE1,0xFD,0x19,0x48,0x41,0x10,0x26,0x0C,0x30,    //27 
                0xAD,0x2B,0x09,0xB9,0x8B,0x84,0xA9,0xA7,0x10,0x3E,0x42,0xA1,0x55,0xA2,0x75,0x01,    //28 
                0x0E,0xEB,0x90,0x31,0x2B,0xF9,0x9E,0x66,0x35,0x8D,0x7A,0xBC,0x25,0x5A,0x10,0x3B,    //29 
                0x46,0x97,0x37,0xE0,0xEE,0x15,0x2B,0x57,0xB4,0x9D,0xE4,0xF9,0xC7,0xEC,0xE6,0x06     //30 
            };
            
            /*
            int size = Marshal.SizeOf(upLoadstream[0]) * upLoadstream.Length;
            IntPtr pnt = Marshal.AllocHGlobal(size);
            try
            {
                // Copy the array to unmanaged memory.
                Marshal.Copy(upLoadstream, 0, pnt, upLoadstream.Length);
            }
            finally
            {
                // Free the unmanaged memory.
                // Marshal.FreeHGlobal(pnt);
            }
            //============================*/
            
            IntPtr ptr;
            
            ptr = Separate(upLoadstream, 480);
            String result = PtrToStringAscii(ptr);
            msgbox.AppendText(result);
            msgbox.ScrollToEnd();
        }

        private static String PtrToStringAscii(IntPtr ptr) // aPtr is nul-terminated
        {
            if (ptr == IntPtr.Zero)
                return "";
            int len = 0;
            while (Marshal.ReadByte(ptr, len) != 0)
                len++;
            if (len == 0)
                return "";
            byte[] array = new byte[len];
            Marshal.Copy(ptr, array, 0, len);
            return System.Text.Encoding.ASCII.GetString(array);
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
