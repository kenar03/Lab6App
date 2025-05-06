using LiveCharts.Wpf;
using LiveCharts;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Lab6App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HostMessage message;
        ComPort cp;
        SaveFileDialog sfd;
        OpenFileDialog ofd;
        StreamWriter sw = null;
        System.Timers.Timer timer;
        Union uni1;
        Union uni2;
        string filename;
        string sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        public Func<double, string> YFormatter { get; set; }
        public SeriesCollection SeriesCollection { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            message = new HostMessage();
            getPorts(cbPort);
            sfd = new SaveFileDialog();
            ofd = new OpenFileDialog();
            sfd.Filter = "Текстовый файл | *.txt | Все файлы | *.*";
            ofd.Filter = "Текстовый файл|*.txt|Все файлы|*.*";
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            uni1 = new Union();
            uni2 = new Union();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (cp == null)
            {
                ShowMessage("Не выбран COM-порт");
                timer.Stop();
                return;
            }
            getResponse();
            cp.Send("05 04 00 02 00 04 51 8D");
        }

        private void getResponse()
        {
            string z = cp.ResponseString;
            if (string.IsNullOrEmpty(z))
                return;
            byte[] data = ComPort.HexToByte(z);
            int crc = getCRC(data, data.Length - 2);
            int rcrc = (data[data.Length - 1] << 8) | data[data.Length - 2];
            if (crc != rcrc) return;

            uni1.byte1 = data[4];
            uni1.byte2 = data[3];
            uni1.byte3 = data[6];
            uni1.byte4 = data[5];

            uni2.byte1 = data[8];
            uni2.byte2 = data[7];
            uni2.byte3 = data[10];
            uni2.byte4 = data[9];

            Dispatcher.Invoke(() =>
            {
                channel1.Text = uni1.f.ToString("0.000000");
                channel2.Text = uni2.f.ToString("0.000000");
            });

            if(sw != null)
            {
                sw.Write(uni1.f.ToString("0.000000"));
                sw.WriteLine("\t" + uni2.f.ToString("0.000000"));
            }
        }

        int getCRC(byte[] array, int length)
        {
            int CRC = 0xFFFF;
            for (int p = 0; p < length; p++)
            {
                CRC ^= (int)array[p]; // XOR byte into least sig. byte of crc
                for (int i = 8; i != 0; i--) // Loop over each bit
                {
                    if ((CRC & 0x0001) != 0) // If the LSB is set
                    {
                        CRC >>= 1; // Shift right and XOR 0xA001
                        CRC ^= 0xA001;
                    }
                    else // Else LSB is not set
                        CRC >>= 1; // Just shift right
                }
            }
            return CRC;
        }

        public void ShowMessage(string s)
        {
            message.SetText(s);
            DialogHost.CloseDialogCommand.Execute(null, null);
            hostMessage.ShowDialog(message);
            return;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Привет");
        }

        public static string[] RemoveDuplicates(string[] s)
        {
            HashSet<string> set = new HashSet<string>(s);
            string[] result = new string[set.Count];
            set.CopyTo(result);
            return result;
        }

        private void getPorts(ComboBox cb)
        {
            string name = cb.Text;
            cb.Items.Clear();
            string[] st = RemoveDuplicates(ComPort.Ports);
            Array.Sort(st);
            Array.Sort(st, (x, y) => x.Length.CompareTo(y.Length));
            for (int i = 0; i < st.Length; i++)
            {
                cb.Items.Add(st[i]);
                if (name == st[i])
                    cb.SelectedIndex = i;
            }
            if (cb.SelectedIndex < 0)
            {
                if (cb.Items.Count > 0)
                {
                    cbPort.SelectedIndex = 0;
                }
                else
                {
                    cbPort.SelectedIndex = -1;
                }
            }
            cb.Items.Refresh();
        }

        private void cbPort_DropDown(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            getPorts(cb);
        }

        private void UsrEdit_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string txt = tb.Text.Replace(" ", "").ToUpper();
            int pos = tb.CaretIndex;
            bool isEnd = false;
            if ((tb.Text.Length == 0) || ((pos == tb.Text.Length) && (tb.Text[tb.Text.Length - 1] != ' ')))
                isEnd = true;
            string o = "";
            for (int i = 0; i < txt.Length; i += 2)
            {
                try
                {
                    o += txt.Substring(i, 2) + " ";
                }
                catch
                {
                    try
                    {
                        o += txt.Substring(i, 1);
                    }
                    catch
                    {

                    }
                }
            }
            tb.Text = o;
            if (isEnd == true)
                pos = tb.Text.Length;
            if (pos > tb.Text.Length)
                pos = tb.Text.Length;
            if (pos < 0) pos = 0;
            tb.CaretIndex = pos;
        }

        private bool isHexDigit(string text)
        {
            char sym = text[text.Length - 1];
            if (char.IsDigit(sym))
                return true;
            if (sym == ' ') return true;
            if ((sym >= 'A') && (sym <= 'F')) return true;
            return false;
        }

        private void UsrEdit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isHexDigit(e.Text.ToUpper());
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (cp == null)
            {
                ShowMessage("Не выбран COM-порт");
                return;
            }
            if (tbCmd.Text.Length < 2)
            {
                ShowMessage("Команда не введена!");
                return;
            }
            cp.Send(tbCmd.Text);
        }

        private void FlowDocumentScrollViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Fd.Blocks.Clear();
        }

        private void btnCrc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tbCmd.Text = AddCRC(tbCmd.Text);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }

        internal static string AddCRC(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new Exception("Команда не введена!");
            }
            byte[] cmd = ComPort.HexToByte(text);
            if (cmd.Length == 0) return text;

            int CRC = 0xFFFF;
            for (int p = 0; p < cmd.Length; p++)
            {
                CRC ^= (int)cmd[p];          // XOR byte into least sig. byte of crc
                for (int i = 8; i != 0; i--)      // Loop over each bit
                {
                    if ((CRC & 0x0001) != 0)        // If the LSB is set
                    {
                        CRC >>= 1;                    // Shift right and XOR 0xA001
                        CRC ^= 0xA001;
                    }
                    else                            // Else LSB is not set
                        CRC >>= 1;                    // Just shift right
                }
            }
            byte[] crc = new byte[2];
            crc[0] = (byte)(CRC & 0xFF);
            crc[1] = (byte)((CRC & 0xFF00) >> 8);
            string s = ComPort.ByteToHex(cmd);
            s += Convert.ToString(crc[0], 16).ToUpper().PadLeft(2, '0');
            s += " " + Convert.ToString(crc[1], 16).ToUpper().PadLeft(2, '0');
            return s;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (cp != null)
            {
                if(cp.IsOpened)
                    cp.Close();
                //cp.Dispose();
                cbPort.Background = Brushes.White;
            }
            cp = new ComPort(Fd);
            cp.Config(cbPort.Text, cbBaud.Text, cbParity.SelectedIndex, cbStop.Text);
            cp.Open();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (cp != null)
            {
                cp.Close();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(sfd.ShowDialog() == true)
            {
                filename = sfd.FileName;
            }

        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (timer.Enabled == false)
            {
                timer.Start();
                if (string.IsNullOrEmpty(filename))
                {
                    ShowMessage("Не выбрал файл");
                    return;
                }
                sw = new StreamWriter(filename);
            }    
            else
            {
                timer.Stop();
                if(sw != null)
                {
                    sw.Close();
                }
                sw = null;
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (ofd.ShowDialog() == true)
            {
                StreamReader sr = new StreamReader(ofd.FileName);
                ChartValues<double> cv1 = new ChartValues<double>();
                ChartValues<double> cv2 = new ChartValues<double>();
                do
                {
                    try
                    {
                        string ss = sr.ReadLine();
                        if (string.IsNullOrEmpty(ss)) break;
                        string[] parts = ss.Split('\t');
                        if (parts.Length == 2)
                        {
                            //double val1 = double.Parse(parts[0].Replace(",", sep).Replace(".", sep), CultureInfo.InvariantCulture);
                            //double val2 = double.Parse(parts[1].Replace(",", sep).Replace(".", sep), CultureInfo.InvariantCulture);
                            
                        }
                        cv1.Add(double.Parse(parts[0].Replace(",", sep).Replace(".", sep)));
                        cv2.Add(double.Parse(parts[1].Replace(",", sep).Replace(".", sep)));
                        // cv.Add(double.Parse(ss.Replace(",", sep).Replace(".", sep)));
                    }
                    catch
                    {
                        break;
                    }
                } while (true);
                sr.Close();
                SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        LineSmoothness = 0.1,
                        PointGeometry = null,
                        Fill = Brushes.Transparent,
                        Stroke = Brushes.Indigo,
                        Values = cv1,
                    },
                    new LineSeries
                    {
                        LineSmoothness = 0.1,
                        PointGeometry = null,
                        Fill = Brushes.Transparent,
                        Stroke = Brushes.Red,
                        Values = cv2,
                    }
                };

                YFormatter = value => ((double)value).ToString("0.00");

                DataContext = this;
                Diag.AxisX[0].MaxValue = cv1.Count;
                Diag.AxisY[0].MaxValue = 3.0;
                Diag.AxisX[0].SetRange(0, cv1.Count);
                Diag.AxisY[0].SetRange(0, 3.01);
            }
        }
    }
}