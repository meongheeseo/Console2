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
using System.Windows.Threading;
using System.Threading;

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
        int func_id = 0; // 0 - ACK, 1 - NAK, 2 - Telegram Download, 3 - Telegram Upload, 5 - Telegram Upload Request
        // 7 - Baslise Input to Output characteristics
        int myRecSeq = 0;
        int echoBack = 0;

        int rxRecSeq = 0;

        const int echoACK = 0x06;
        const int echoNAK = 0x15;
        const double JSTIMER = 0.01;    //100hz, 10ms
        const int COM_OVR_TIME = 300;    //10ms * 300 = 3sec 

        byte[] NULL_DATA = new byte[] { };

        const int SIZE_fileBuffer = 0x10000;   //64K bytes buffer
        byte[] fileBuffer = new byte[SIZE_fileBuffer];
        int xfileBuffer = 0;

        const int DATA_offset = 13;
        const int CATEGORY_offset = 9;

        public MainWindow()
        {
            InitializeComponent();
            Init_combobox();
            position_gpbox.Visibility = Visibility.Hidden;
            progressLabel.Text = String.Format("{0}%", progressBar.Value);
            serialportbox.Text = Properties.Settings.Default.ComportString;
        }

        public delegate void UpdateProgressValueCallback(int value);

        private void UpdateProgressValue(int value)
        {
            progressBar.Value = value;
        }

        private void Init_combobox()
        {
            serialportbox.DisplayMemberPath = "Text";
            serialportbox.SelectedValuePath = "Value";

            var items = new[] {
                new { Text = "COM1", Value = "COM1" }, new { Text = "COM2", Value = "COM2" },
                new { Text = "COM3", Value = "COM3" }, new { Text = "COM4", Value = "COM4" },
                new { Text = "COM5", Value = "COM5" }, new { Text = "COM6", Value = "COM6" },
                new { Text = "COM7", Value = "COM7" }, new { Text = "COM8", Value = "COM8" },
                new { Text = "COM9", Value = "COM9" }, new { Text = "COM10", Value = "COM10" },
                new { Text = "COM11", Value = "COM11" }, new { Text = "COM12", Value = "COM12" },
                new { Text = "COM13", Value = "COM13" }, new { Text = "COM14", Value = "COM14" },
                new { Text = "COM15", Value = "COM15" }, new { Text = "COM16", Value = "COM16" },
                new { Text = "COM17", Value = "COM17" }, new { Text = "COM18", Value = "COM18" },
                new { Text = "COM19", Value = "COM19" }, new { Text = "COM20", Value = "COM20" },
                new { Text = "COM21", Value = "COM21" }, new { Text = "COM22", Value = "COM22" },
                new { Text = "COM23", Value = "COM23" }, new { Text = "COM24", Value = "COM24" },
                new { Text = "COM25", Value = "COM25" }, new { Text = "COM26", Value = "COM26" },
                new { Text = "COM27", Value = "COM27" }, new { Text = "COM28", Value = "COM28" },
                new { Text = "COM29", Value = "COM29" }, new { Text = "COM30", Value = "COM30" },
                new { Text = "COM31", Value = "COM31" }, new { Text = "COM32", Value = "COM32" },
                new { Text = "COM33", Value = "COM33" }, new { Text = "COM34", Value = "COM34" },
                new { Text = "COM35", Value = "COM35" }, new { Text = "COM36", Value = "COM36" },
                new { Text = "COM37", Value = "COM37" }, new { Text = "COM38", Value = "COM38" },
                new { Text = "COM39", Value = "COM39" }, new { Text = "COM40", Value = "COM40" },
                new { Text = "COM41", Value = "COM41" }, new { Text = "COM42", Value = "COM42" },
                new { Text = "COM43", Value = "COM43" }, new { Text = "COM44", Value = "COM44" },
                new { Text = "COM45", Value = "COM45" }, new { Text = "COM46", Value = "COM46" },
                new { Text = "COM47", Value = "COM47" }, new { Text = "COM48", Value = "COM48" },
                new { Text = "COM49", Value = "COM49" }, new { Text = "COM50", Value = "COM50" }
            };

            serialportbox.ItemsSource = items;
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

        private void manu_others_Checked(object sender, RoutedEventArgs e)
        {
            position_gpbox.Visibility = Visibility.Hidden;
            manufacturer_id = 3;
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

        private void pos_others_Checked(object sender, RoutedEventArgs e)
        {
            device_id = 2;
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

            decodeWindow.decode_textBox.AppendText("HI\r\n");
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
        private Boolean serialport_open = false;

        private void serialConnect_btn_Click(object sender, RoutedEventArgs e)
        {
            // Add inserted value into serialportbox
            var value = serialportbox.Text;

            //serialportbox.Items.Remove(value);
            //serialportbox.Items.Add(value);
            //serialportbox.Items.SortDescriptions.Add(
            //    new System.ComponentModel.SortDescription("", System.ComponentModel.ListSortDirection.Ascending));

            // Connect the serial port.
            serialport = new SerialPort(value, 38400, Parity.None, 8, StopBits.One);
            serialport.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            serialport.Open();

            if (serialport.IsOpen)
            {
                serialport_open = true;
                MessageBox.Show("Connection Open", "Message");
                Properties.Settings.Default.ComportString = serialportbox.Text;
                Properties.Settings.Default.Save();
            }
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
            if (data != null) BaliseTelegramDownloadProtocol(data);

        }

        private void telRead_btn_Click(object sender, RoutedEventArgs e)
        {
            telReadProcess();
        }

        public void telReadProcess()
        {
            if (telRead_btn.Background != Brushes.DarkBlue)
            {
                telRead_btn.Background = Brushes.DarkBlue;
                func_id = 5;
            }
            else
            {
                telRead_btn.ClearValue(Button.BackgroundProperty);
                func_id = 10;       //telegram upload terminate
            }
            byte[] data = new byte[] { };
            myRecSeq = 0;
            BaliseTelegramSend(data, true);
        }

        private void tempinout_btn_Click(object sender, RoutedEventArgs e)
        {
            func_id = 7;
            int minpercent = 500; // 1 = 0.1%
            int maxpercent = 850; // 1 = 0.1%
            int numSteps = 80; // Value must be between 20-100
            int timeout = 255; // min 0 -> 10ms, max 255 -> 2.56s

            byte[] data = new byte[6];
            data[0] = (byte)(minpercent >> 8);
            data[1] = (byte)(minpercent);
            data[2] = (byte)(maxpercent >> 8);
            data[3] = (byte)(maxpercent);
            data[4] = (byte)(numSteps);
            data[5] = (byte)(timeout);

            myRecSeq = 0;
            BaliseTelegramSend(data, true);
        }

        // Sends downloaded telegram message via serial port.
        public void BaliseTelegramDownloadProtocol(byte[] data)
        {
            myRecSeq = 0;
            //---- multi records
            if (data.Length > 1024)
            {
                int sendOffset = 0;
                while (sendOffset < data.Length)
                {
                    int sendCnt = data.Length - sendOffset;
                    if (sendCnt > 1024) sendCnt = 1024;
                    byte[] subBlock = new byte[sendCnt];
                    Buffer.BlockCopy(data, sendOffset, subBlock, 0, sendCnt);
                    sendOffset += sendCnt;
                    bool last = true;
                    echoBack = 0;
                    if (sendOffset < data.Length) last = false;     //마지막 record가 아님.
                    BaliseTelegramSend(subBlock, last);
                    myRecSeq++;
                    while (echoBack != echoACK) wait(0.01);
                }
            }
            //---- single record
            else
            {
                BaliseTelegramSend(data, true);
            }
        }

        public void SinglePacketSendProtocol(byte[] data)
        {
            int retryCnt = 0;
            int waitTimer;

            myRecSeq = 0;
            while (retryCnt < 3)        //3 times retry
            {
                BaliseTelegramSend(data, true);
                echoBack = 0;
                waitTimer = 0;
                while (true)
                {
                    wait(JSTIMER);
                    if (echoBack == echoACK || echoBack == echoNAK) break;      //ACK or NAK received
                    waitTimer++;
                    if (waitTimer >= COM_OVR_TIME)      //time out
                    {
                        echoBack = 0;
                        break;
                    }
                }//while (true)

                if (echoBack == echoACK) break;
                retryCnt++;

            }//while (retryCnt > 0)
        }

        public void sendACK()
        {
            func_id = 0;        //ACK
            BaliseTelegramSend(NULL_DATA, true);
        }

        public void sendNAK()
        {
            func_id = 1;        //NAK
            BaliseTelegramSend(NULL_DATA, true);
        }

        public void BaliseTelegramSend(byte[] data, bool last)
        {
            if (!serialport_open || !serialport.IsOpen || serialport == null)
            {
                MessageBox.Show("Recheck Serial Port Connection", "Warning", MessageBoxButton.OK);
                telRead_btn.ClearValue(Button.BackgroundProperty);
                //System.Windows.Media.LinearGradientBrush;
            }

            else if (serialport.IsOpen)
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

                // manufacturer's id is only taken into consideration when func = telegram download
                if (func_id == 2)
                {
                    cat[0] = (byte)((0 << 6) + (manufacturer_id << 3) + ((func_id >> 2)));
                }
                else
                {
                    cat[0] = (byte)(func_id >> 2);
                }

                // device's id doesn't matter if func = Balise input to output characteristics measure
                if (func_id != 7)
                {
                    cat[1] = (byte)((func_id << 6) + (device_id << 3));
                }
                else
                {
                    cat[1] = (byte)(func_id << 6);
                }
                /*
                 [b18] End of Record --> cat[1], b2
                   0: Last Record
                   1: 다음 Record로 이어짐
                */
                if (last == false) cat[1] |= (byte)(1 << 2);

                // Record seq
                byte[] record = new byte[2];
                record[0] = (byte)(myRecSeq >> 8);
                record[1] = (byte)myRecSeq;

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

                if (func_id != 5)
                {
                    // If length of data is greater than 1024 byte, divide message
                    if (data.Length < 1024) { serialport.Write(data, 0, data.Length); }
                    else
                    {
                        int cnt = data.Length / 1024 + 1;
                        for (int i = 0; i < cnt; i++)
                        {
                            if (i != (cnt - 1)) { serialport.Write(data, cnt * i, 1024); }
                            else { serialport.Write(data, cnt * i, data.Length - ((cnt - 1) * 1024)); }
                        }
                    }
                }
                serialport.Write(crc32, 0, crc32.Length);
                serialport.Write(postamble, 0, postamble.Length);
            }

            //telRead_btn.ClearValue(Button.BackgroundProperty);

            //serialport.Close();
            //if (!serialport.IsOpen) { MessageBox.Show("Serial Port successfully closed.", "Message"); }
        }

        public delegate void UpdateTextCallback(String msg);

        const int PREAMBLE_WAIT = 0; const int PAYLOAD_LEN_WAIT = 1; const int PAYLOAD_WAIT = 2;
        const int CRC_WAIT = 3; const int POSTAMBLE_WAIT = 4; const int CONSOLE_FRM_RCV_DONE = 5;
        const int CONSOLE_RCV_RST = 6;
        const int MAX_CONSOLE_PAYLOAD_SIZE = 2048;

        int console_rcv_stage = CONSOLE_RCV_RST; // console_rcv_stage = CONSOLE_RCV_RST;
        int buf_cnt = -1;
        byte[] buf;

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int length;

            if (buf_cnt == -1 || buf_cnt >= buf.Length)
            {
                length = sp.BytesToRead;
                buf = new byte[length];
                sp.Read(buf, 0, length);
                buf_cnt = 0;
            }

            while (buf_cnt < buf.Length)
            {
                ConsolePktRcvAutomata();

                if (console_rcv_stage == CONSOLE_FRM_RCV_DONE)
                {
                    if (errCode != 0) MessageBox.Show("Data Received Error", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    else errCode = ConsoleCmdProcess();
                    buf_cnt = -1;
                    console_rcv_stage = CONSOLE_RCV_RST;
                    break;
                }
            }
            // Data Receive Super Loop
            //int bytes = sp.BytesToRead;
            //byte[] buffer = new byte[bytes];
            //sp.Read(buffer, 0, bytes);

            //msgbox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), System.Text.Encoding.ASCII.GetString(buffer));

        }

        private void UpdateText(String msg)
        {
            msgbox.AppendText(msg);
            msgbox.ScrollToEnd();
        }

        //
        //  DATA RECEIVE HANDLER
        //
        UInt32 sign32, yourSeq, mySeq, category, yourRecSeq, yourRecLen;
        UInt32 errCode = 0;
        byte[] ConsolePayload = new byte[2048];
        byte device, manufacturer;
        int rcvCnt, xPayLoad, crc;
        int console_payLoadCnt = 0;

        private void ConsolePktRcvAutomata()
        {
            int keep = 1;
            UInt32 t;

            while (keep != 0)
            {
                keep = 0;

                switch (console_rcv_stage)
                {
                    case PREAMBLE_WAIT:
                        if (buf_cnt >= buf.Length) break;

                        keep++;
                        sign32 <<= 8;
                        sign32 |= buf[buf_cnt];
                        buf_cnt++;

                        if (sign32 == 0xaa5555aa)
                        {
                            rcvCnt = 4;
                            console_payLoadCnt = 0;
                            console_rcv_stage = PAYLOAD_LEN_WAIT;
                        }
                        break;

                    case PAYLOAD_LEN_WAIT:
                        if (buf_cnt >= buf.Length) break;

                        keep++;
                        console_payLoadCnt <<= 8;
                        console_payLoadCnt |= buf[buf_cnt];
                        buf_cnt++;
                        if (--rcvCnt == 0)
                        {
                            xPayLoad = 0;
                            console_rcv_stage = PAYLOAD_WAIT;
                        }
                        break;

                    case PAYLOAD_WAIT:
                        if (buf_cnt >= buf.Length) break;

                        keep++;
                        ConsolePayload[xPayLoad++] = buf[buf_cnt];
                        buf_cnt++;
                        if (xPayLoad >= console_payLoadCnt)
                        {
                            rcvCnt = 4;
                            crc = 0;
                            console_rcv_stage = CRC_WAIT;
                        }
                        else if (xPayLoad >= MAX_CONSOLE_PAYLOAD_SIZE)
                        {
                            console_rcv_stage = CONSOLE_RCV_RST;
                        }
                        break;

                    case CRC_WAIT:
                        if (buf_cnt >= buf.Length) break;

                        keep++;
                        crc <<= 8;
                        crc |= buf[buf_cnt];
                        buf_cnt++;
                        if (--rcvCnt == 0)
                        {
                            rcvCnt = 4;
                            sign32 = 0;
                            console_rcv_stage = POSTAMBLE_WAIT;
                        }
                        break;

                    case POSTAMBLE_WAIT:
                        if (buf_cnt >= buf.Length) break;

                        keep++;
                        sign32 <<= 8;
                        sign32 |= buf[buf_cnt];
                        buf_cnt++;

                        if (--rcvCnt == 0)
                        {
                            if (sign32 == 0x55aaaa55)
                            {
                                yourSeq = getWord16(ConsolePayload, 0);
                                t = getWord16(ConsolePayload, 4);
                                category = getWord32(ConsolePayload, 9);
                                yourRecSeq = getWord16(ConsolePayload, 13);
                                yourRecLen = getWord16(ConsolePayload, 15);

                                if (console_payLoadCnt != getWord16(ConsolePayload, 15) + 17) errCode = 1; // length fail
                                else if (mySeq != t && t != 0) errCode = 2; // 상대 seq unknown
                                else if (ConsolePayload[8] != 0) errCode = 3; // not supported protocol version
                                else errCode = 0;
                            }
                            else errCode = 10; // packet frame form fail
                            console_rcv_stage = CONSOLE_FRM_RCV_DONE;
                        }
                        break;

                    case CONSOLE_FRM_RCV_DONE:
                        break;

                    case CONSOLE_RCV_RST:
                        sign32 = 0;
                        console_rcv_stage = PREAMBLE_WAIT;
                        break;
                }
            }
        }

        UInt32 ConsoleCmdProcess()
        {
            byte type, function;
            int recSeq, rcvCnt, percentage;
            UInt32 cat;
            UInt32 err;

            type = (byte)(category >> 30 & 0x3);
            manufacturer = (byte)(category >> 27 & 0x7);
            function = (byte)(category >> 22 & 0x1f);
            device = (byte)(category >> 19 & 0x7);
            err = 0;

            if (type != 0) return 4; // category type mismatch

            switch (function)
            {
                case 0:
                    msgbox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), "[ACK]");
                    echoBack = echoACK;
                    break;

                case 1:
                    msgbox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), "[NAK]");
                    echoBack = echoNAK;
                    break;

                case 2:
                    msgbox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), "--------Telegram Download--------\r\n");
                    //err = TelegramDownload();
                    err = 11;
                    break;

                case 3:     //---- telegram upload
                    msgbox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), "[Telegram Upload]");
                    /*
                     * ConsolePayload[(0, 1)] : record sequence#
                     * ConsolePayload[(2, 3)] : record bytes length
                     * ConsolePayload[(4,,,)] : record 
                     * 
                     */
                    cat = getWord32(ConsolePayload, CATEGORY_offset);    //get category
                    recSeq = getWord16(ConsolePayload, DATA_offset);
                    if (recSeq != rxRecSeq)     //record sequence# mismatch
                    {
                        sendNAK();
                        err = 128;
                        break;
                    }
                    rxRecSeq++;
                    rcvCnt = getWord16(ConsolePayload, DATA_offset + 2);
                    if (xfileBuffer + rcvCnt > SIZE_fileBuffer) rcvCnt = SIZE_fileBuffer - xfileBuffer;
                    if (rcvCnt > 0) Buffer.BlockCopy(ConsolePayload, DATA_offset + 4, fileBuffer, xfileBuffer, rcvCnt);
                    xfileBuffer += rcvCnt;
                    sendACK();

                    if ((cat & (UInt32)(1 << 18)) == 0) //last record
                    {
                        progressBar.Dispatcher.Invoke(new UpdateProgressValueCallback(this.UpdateProgressValue), 100);
                        LEUuploadSave();
                    }
                    err = 0;
                    break;

                case 4:     //---- progress
                    /*
                      ConsolePayload[(2, 3)] : progress data length N
                      ConsolePayload[(4, 5)] : percentage
                      ConsolePayload[(6,,,)] : comment (N - 2)
                     */
                    rcvCnt = getWord16(ConsolePayload, DATA_offset + 2);
                    percentage = getWord16(ConsolePayload, DATA_offset + 4);
                    if ((percentage & 1 << 14) == 0)  //percentage 표시를 사용함.
                    {
                        percentage &= 0x3fff;
                        progressBar.Dispatcher.Invoke(new UpdateProgressValueCallback(this.UpdateProgressValue), (int)(percentage / 10));
                    }
                    if (rcvCnt > 2)
                    {
                        byte[] comment = new byte[rcvCnt - 2];
                        Buffer.BlockCopy(ConsolePayload, DATA_offset + 6, comment, 0, rcvCnt - 2);
                        string commentStr = Encoding.Default.GetString(comment);
                        msgbox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), "\r\n" + commentStr);
                    }

                    err = 0;
                    break;

                case 5:
                    msgbox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), "--------Telegram Upload Request--------\r\n");

                    if (device != 0) err = 5; // category device mismatch
                    else if (yourRecSeq != 0) err = 6; // record seq# mismatch
                    else if (yourRecLen != 0) err = 7; // record length fail
                    //else err = TelegramUploadEnable();
                    else err = 11;
                    break;

                case 6:
                    msgbox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), "--------A4 Output Level Control--------\r\n");

                    if (yourSeq != 0) err = 6; // record Seq# mismatch
                    else if (yourRecLen != 2) err = 7; // record length fail
                    //else err = A4LevelSet(getWord16(ConsolePayload, 17)); // power level
                    else err = 0;
                    break;

                case 7:
                    msgbox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), "--------Balise Input to Output Characteristics Measure Start--------\r\n");

                    if (yourRecSeq != 0) err = 6;     //record sequence# mismatch
                    else if (yourRecLen != 6) err = 7;     //record length fail
                    else
                        err = 0;
                    //err = IOmeasureStart(getWord16(ConsolePayload, 17),   //min
                    //    getWord16(ConsolePayload, 19),   //max
                    //    ConsolePayload[21],               //step
                    //    ConsolePayload[22]);             //interval
                    break;

                case 9:
                    msgbox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), "--------Measure Data Receive--------\r\n");
                    err = 8;
                    break;

                case 10:
                    msgbox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), "--------Process Loop Break--------\r\n");

                    if (yourRecSeq != 0) err = 6; // record sequence # mismatch
                    else if (yourRecLen != 0) err = 7; // record length fail
                    //else err = HALT();
                    else err = 0;
                    break;

                default:
                    msgbox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), "--------Undefined category--------\r\n");
                    err = 9;
                    break;

            }

            return err;
        }

        UInt32 TelegramDownload()
        {
            UInt32 err = 0;

            if (device == 0) // BALISE
            {
                switch (manufacturer)
                {
                    case 0: // Shinwoo
                        //err = ShinWoo_Balise_download();
                        err = 11;
                        break;

                    case 1: // Bombardier
                        err = 11;
                        break;

                    case 2: // Simens
                        err = 11;
                        break;

                    case 3: // Thales
                        err = 11;
                        break;

                    default:
                        err = 11;
                        break;
                }
            }
            else if (device == 1) // LEU
            {
                switch (manufacturer)
                {
                    case 0: // shinwoo
                        //err = ShinWoo_LEU_download();
                        err = 11;
                        break;

                    case 1: // Bombardier
                        err = 11;
                        break;

                    case 2: // Simens
                        err = 11;
                        break;

                    case 3: // Thales
                        err = 11;
                        break;

                    default:
                        err = 11;
                        break;
                }
            }
            else err = 12;
            return err;
        }

        UInt16 getWord16(byte[] buf, int index)
        {
            UInt16 result;

            result = buf[index];
            result <<= 8;
            result += buf[index + 1];
            return result;
        }

        UInt32 getWord32(byte[] buf, int index)
        {
            UInt32 result;

            result = buf[index];
            result <<= 8;
            result += buf[index + 1];
            result <<= 8;
            result += buf[index + 2];
            result <<= 8;
            result += buf[index + 3];
            return result;
        }

        public void LEUuploadSave()
        /*
           save fileBuffer[xfileBuffer]
        */
        {
            SaveFileDialog saveFileDlg = new SaveFileDialog();
            saveFileDlg.FileOk += CheckIfFileHasCorrectExtension;
            saveFileDlg.Filter = "LEU File (*.LEU.EEP, *.LEU.TXT)|*.LEU.EEP; *.LEU.TXT";

            if (saveFileDlg.ShowDialog() == true && saveFileDlg.FileName.Length > 0)
            {
                var extension = System.IO.Path.GetExtension(saveFileDlg.FileName);

                byte[] writeBuf = new byte[xfileBuffer];
                Buffer.BlockCopy(fileBuffer, 0, writeBuf, 0, xfileBuffer);

                switch (extension.ToLower())
                {
                    case ".eep":
                        File.WriteAllBytes(saveFileDlg.FileName, writeBuf);
                        break;

                    case ".txt":
                        cvHexascForm(writeBuf);
                        byte[] writeBuf2nd = new byte[xfileBuffer];
                        Buffer.BlockCopy(fileBuffer, 0, writeBuf2nd, 0, xfileBuffer);
                        File.WriteAllBytes(saveFileDlg.FileName, writeBuf2nd);
                        break;
                }
            }
        }

        public void cvHexascForm(byte[] sourceBuf)
        {
            int sSz = sourceBuf.Length;
            int xS = 0;
            int ichr;
            int colCnt = 16;
            xfileBuffer = 0;

            while (xS < sSz)
            {
                ichr = cvHEXASC(sourceBuf[xS++]);
                fileBuffer[xfileBuffer++] = (byte)(ichr >> 8);
                fileBuffer[xfileBuffer++] = (byte)ichr;
                fileBuffer[xfileBuffer++] = (byte)'.';

                if (--colCnt == 0)
                {
                    fileBuffer[xfileBuffer++] = (byte)(0x0d);
                    fileBuffer[xfileBuffer++] = (byte)(0x0a);
                    colCnt = 16;
                }
            }
            fileBuffer[xfileBuffer++] = (byte)(0x0d);
            fileBuffer[xfileBuffer++] = (byte)(0x0a);
        }

        public int cvHEXASC(byte chr)
        {
            int i, j;

            i = (chr >> 4) & 0x0f;
            if (i > 9) i = i - 10 + 'a';
            else i += '0';

            j = chr & 0x0f;
            if (j > 9) j = j - 10 + 'a';
            else j += '0';

            return i << 8 | j;
        }


        // Check File extensions when saving
        void CheckIfFileHasCorrectExtension(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveFileDialog sv = (sender as SaveFileDialog);

            if (!(System.IO.Path.GetExtension(sv.FileName).ToLower() == ".eep" || System.IO.Path.GetExtension(sv.FileName).ToLower() == ".txt"))
            {
                e.Cancel = true;
                MessageBox.Show(".eep 또는 .txt로 끝나는 파일이름을 사용하세오", "Warning");
                //MessageBox.Show("Please use the following extensions: *.LEU.EEP or *.LEU.EEP.TXT", "Warning");
                return;
            }
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
                if (openFile.FileName == null) { return null; }

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

        [DllImport("TelegramSeparation.dll", EntryPoint = "Separate", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Separate(byte[] src, int clen);
        [DllImport("vcDecode.dll", EntryPoint = "OnDecode", CallingConvention = CallingConvention.Cdecl)]
        public static extern void OnDecode(string input, [Out]IntPtr output);
        [DllImport("MsgDecode.dll", EntryPoint = "OnMsgDecode", CallingConvention = CallingConvention.Cdecl)]
        public static extern void OnMsgDecode(string input, ref int length, [Out]IntPtr output);

        String Decode830Start(string encoded830)
        {
            int length = 0;
            IntPtr output = Marshal.AllocHGlobal(10000);
            
            //encoded830 = "<User_Tele>90 02 00 6E E6 E8 41 50 36 20 B3 46 E7 E2 82 0A 98 6E 76 28 31 04 7A 80 07 FE 30 5B E0 54 A0 57 60 5A E4 00 00 50 AA 06 64 00 04 04 41 7F 40 21 46 5F E3 68 2A 90 00 04 80 20 02 12 80 03 E8 30 00 3E 82 C0 03 25 FE 07 F8 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 </User_Tele>";
            encoded830 = Format830Encoded(encoded830);

            OnMsgDecode(encoded830, ref length, output);
            String text = PtrToStringKorean(output, length);

            if (text == "")
            {
                OnMsgDecode(encoded830, ref length, output);
                text = PtrToStringKorean(output, length);
            }
            decodeWindow.decode_textBox.AppendText(text);

            return text;
        }

        String Decode1023Start(string encoded1023)
        {
            IntPtr output = Marshal.AllocHGlobal(1000);

            encoded1023 = Format1023Encoded(encoded1023); // need to format the string so dll will accept it
            //encoded1023 = "<Transport_Tele>61 99 AB 46 10 4F 31 1A 52 C1 EE D4 B6 A7 C1 E5 52 46 11 F3 DC 6E 8A AB D5 A0 E7 3A 75 88 89 8B E6 E4 79 8A 21 E4 DE 37 5B 13 BB 6B CE C8 10 5D 1E 54 ED DB C9 CC 8E 8D 65 17 8A AF 0C D7 BF B5 17 BE 34 D3 8A B2 C0 F0 FA 19 04 AA 2C 4B 6E 2E 27 2D D3 E8 39 0F 66 42 93 28 52 85 46 ED 77 25 09 C7 A7 A4 65 B8 A9 18 28 F3 3C D0 E1 06 55 6D A1 92 90 13 9E 65 1D 53 70 30 23 71 AA 2A F7 2A </Transport_Tele>";

            OnDecode(encoded1023, output);
            String encoded830 = PtrToStringAscii(output);
            if (encoded830 == "")
            {
                OnDecode(encoded1023, output);
                encoded830 = PtrToStringAscii(output);
            }
            msgbox.AppendText(encoded830);
            msgbox.ScrollToEnd();

            return encoded830;
        }

        private void upload_btn_Click(object sender, RoutedEventArgs e)
        {

            if (device_id == 0)     //Balise
            {
                //telReadProcess();
                dummyBaliseUpload();
                return;
            }
            else if (device_id == 2)     //Not LEU
            {
                MessageBox.Show("지원되지 않는 기능입니다.");
                return;
            }
            if (manufacturer_id != 0)
            {
                MessageBox.Show("신우 LEU만 지원됩니다.");
                return;
            }
            func_id = 5;        //Telegram Upload Request
            xfileBuffer = 0;
            rxRecSeq = 0;
            SinglePacketSendProtocol(NULL_DATA);
        }


        private void dummyBaliseUpload()
        {
            byte[] A1stream = new byte[480];
            Buffer.BlockCopy(upLoadstream, 0, A1stream, 0, 480);

            IntPtr ptr;
            String result;
            int length;

            ptr = Separate(A1stream, 480);
            result = PtrToStringAscii(ptr);
            length = result.Length;

            if (length < 570)
            {
                Buffer.BlockCopy(upLoadstream, 0, A1stream, 0, 480);
                ptr = Separate(A1stream, 480);
                result = PtrToStringAscii(ptr);
                length = result.Length;
            }

            if (length >= 570)
            {
                msgbox.AppendText(result);
                msgbox.AppendText("\r\n");
                msgbox.ScrollToEnd();
                result = Decode1023Start(result);
                result = Decode830Start(result);
            }
        }

        byte[] upLoadstream = new byte[480] { 
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



        // Formats returned string of upload to format vcDecode.dll will accept.
        private String Format1023Encoded(String input)
        {
            input = input.Replace(".", " ").Replace("          ", "").Replace("\n", "");
            input = input.Substring(8, input.Length - 8 - 1);
            input = string.Concat(string.Concat("<Transport_Tele>", input.Trim()), " </Transport_Tele>");

            return input;
        }

        // Formats returned string of 1023 bit vcDecode to format MsgDecode.dll will accept.
        private string Format830Encoded(String input)
        {
            input = input.Substring(11, input.Length - 11 - 12);
            input = string.Concat(input.Trim(), " ");
            return input;
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

        private static String PtrToStringKorean(IntPtr ptr, int length)
        {
            if (ptr == IntPtr.Zero)
                return "";
            int len = 0;
            while (Marshal.ReadByte(ptr, len) != 0)
                len++;
            if (len == 0)
                return "";
            byte[] array = new byte[len];

            if (len != length)
                return "";

            Marshal.Copy(ptr, array, 0, len);

            int euckrCodepage = 51949;
            System.Text.Encoding euckr = System.Text.Encoding.GetEncoding(euckrCodepage);
            String decodedStringByEUCKR = euckr.GetString(array);

            return decodedStringByEUCKR;
        }

        public static void wait(double seconds)
        {
            var frame = new DispatcherFrame();

            new Thread((ThreadStart)(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                frame.Continue = false;
            })).Start();

            Dispatcher.PushFrame(frame);
        }

        DecodeWindow decodeWindow;
        private void msgDecode_btn_Click(object sender, RoutedEventArgs e)
        {
            //String text = Decode830Start("");
            //String text = "";
            decodeWindow = new DecodeWindow();
            decodeWindow.Show();
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
