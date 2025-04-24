using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Lab6App
{
    class ComPort
    {
        SerialPort comPort = null;
        FlowDocument fd;
        object Obj;
        string Response;
        System.Timers.Timer timResponse;
        int responseCnt;

        public static string[] Ports { get { return SerialPort.GetPortNames(); } }

        public ComPort(FlowDocument fd)
        {
            this.fd = fd;
            comPort = new SerialPort();
            comPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            Obj = new object();
            Response = "";
            timResponse = new System.Timers.Timer();
            timResponse.Interval = 10;
            timResponse.Elapsed += TimResponse_Elapsed;
        }

        protected void DisplayData(System.Windows.Media.SolidColorBrush color, string msg)
        {
            if (msg == null) return;
            if (msg == "") return;
            fd.Dispatcher.BeginInvoke(
            new System.Action(() =>
            {
                if (fd.Blocks.Count > 256)
                    fd.Blocks.Remove(fd.Blocks.FirstBlock);
                Paragraph pg = new Paragraph();
                pg.Foreground = color;

                pg.Inlines.Add(msg);
                fd.Blocks.Add(pg);
                ((ScrollViewer)(((FlowDocumentScrollViewer)(fd.Parent)).Parent)).ScrollToBottom();
            }));
        }

        public void Config(string portName)
        {
            if (comPort.IsOpen == true)
            {
                Close();
            }
            try
            {
                if (string.IsNullOrEmpty(portName))
                {
                    comPort = new SerialPort();
                    comPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                    return;
                }
                comPort.PortName = portName;
            }
            catch { }
        }

        public void Config(string portName, string baudRate, int parity, string stopBits)
        {
            if (comPort.IsOpen == true)
            {
                Close();
            }
            try
            {
                comPort.PortName = portName;
                comPort.BaudRate = Int32.Parse(baudRate);
                comPort.Parity = (Parity)parity;
                comPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
                comPort.DataBits = 8;
            }
            catch { }
        }

        public bool Open()
        {
            try
            {
                comPort.Open();
                timResponse.Start();
                string s = "Порт " + comPort.PortName + " открыт " + DateTime.Now;
                DisplayData(App.brushBlack, s);
                return true;
            }
            catch
            {
                comPort.Close();
                timResponse.Stop();
                string s = "Порт " + comPort.PortName + " закрыт " + DateTime.Now;
                DisplayData(App.brushBlack, s);
                return false;
            }
        }

        public bool Close()
        {
            try
            {
                DisplayData(App.brushIndigo, Response);
                comPort.Close();
                Response = "";
                string s = "Порт " + comPort.PortName + " закрыт " + DateTime.Now;
                DisplayData(App.brushBlack, s);
                timResponse.Stop();
                return true;
            }
            catch { return false; }
        }

        public bool IsOpened {  get { return comPort.IsOpen; } }

        public bool Send(string msg)
        {
            if (comPort.IsOpen == false)
                if (Open() == false)
                    return false;
            try
            {
                lock (Obj)
                {
                    byte[] newMsg = HexToByte(msg);
                    comPort.DiscardInBuffer();
                    comPort.DiscardOutBuffer();
                    ClearResponse();
                    comPort.Write(newMsg, 0, newMsg.Length);
                    string s = ByteToHex(newMsg);
                    DisplayData(App.brushGray, s);
                }
                return true;
            }
            catch { return false; }
        }



        public static string ByteToHex(byte[] comByte)
        {
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            foreach (byte data in comByte)
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').
                PadRight(3, ' '));
            return builder.ToString().ToUpper();
        }

        public static byte[] HexToByte(string msg)
        {
            msg = msg.Replace(" ", "");
            byte[] comBuffer = new byte[msg.Length / 2];
            for (int i = 0; i < msg.Length / 2; i++)
                comBuffer[i] = Convert.ToByte(msg.Substring(i * 2, 2), 16);
            return comBuffer;
        }

        public void ClearResponse()
        {
            Response = "";
            responseCnt = 0;
        }

        private void TimResponse_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (Obj)
            {
                responseCnt++;
                if (isResponse == true)
                {
                    DisplayResponse();
                }
            }
        }

        public void DisplayResponse()
        {
            string[] h = null;
            if (Response.Length > 0)
            {
                h = Response.Split(' ');
            }
            if ((h != null) && (h.Length >= 2))
            {
                DisplayData(App.brushIndigo, Response);
            }
            Response = "";
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (Obj)
            {
                int bytes = comPort.BytesToRead;
                byte[] comBuffer = new byte[bytes];
                comPort.Read(comBuffer, 0, bytes);
                Response += ByteToHex(comBuffer);
                responseCnt = 0;
            }
        }

        public bool isResponse
        {
            get
            {
                if (responseCnt < 10)
                    return false;
                else return true;
            }
        }
    }
}
