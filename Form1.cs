using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Diagnostics;

namespace Project_1_20202
{
    public partial class Form1 : Form
    {

        private bool runningFlag = false;

        string File_Path = "D:/Data/Data.xlsx";
        string rbuffer = "";

        public string[] data = new string[1001];

        int Time = 0;
        int Index = 0;

        double Temperature = 0.0;
        int Humidity = 0;


        public Form1()
        {
            InitializeComponent();
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] Ports = SerialPort.GetPortNames();

            COM_BOX.Items.AddRange(Ports);

            ProgBar1.Value = 100;
            ProgBar2.Value = 100;

            ProgBar1.ForeColor = Color.Red;
            ProgBar2.ForeColor = Color.Red;

            timer1.Tick += Timer1_Tick;
            timer1.Interval = 1000;

            Chart.ChartAreas[0].AxisX.Minimum = 0;
            Chart.ChartAreas[0].AxisX.Maximum = 60;

            Chart.ChartAreas[0].AxisY.Minimum = 0;
            Chart.ChartAreas[0].AxisY.Maximum = 100;

            Chart.ChartAreas[0].AxisY2.Minimum = 0;
            Chart.ChartAreas[0].AxisY2.Maximum = 100;

            Save_Path.Text = File_Path;
            Save_Path.SelectAll();
            Save_Path.SelectionAlignment = HorizontalAlignment.Center;

            Run_Time.Text = "0s";
            Run_Time.SelectAll();
            Run_Time.SelectionAlignment = HorizontalAlignment.Center;

            Cont.Checked = true;
            Selec.Checked = false;
            SampBTN.Enabled = false;

        }

        private void Connect_Click(object sender, EventArgs e)
        {
            if (!Serial_Port.IsOpen)
            {
                try
                {
                    Serial_Port.PortName = COM_BOX.Text;
                    Serial_Port.BaudRate = 9600;
                    Serial_Port.DataBits = 8;
                    Serial_Port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "One");
                    Serial_Port.Parity = (Parity)Enum.Parse(typeof(Parity), "None");

                    Serial_Port.Open();
                    ProgBar1.ForeColor = Color.Lime;
                    Conn_Disconn_Button.Text = "Disconnect";
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (runningFlag)
                {
                    timer1.Stop();
                    Run_Button.Text = "Run";
                    ProgBar2.ForeColor = Color.Red;
                    runningFlag = false;
                }
                Serial_Port.Close();
                ProgBar1.ForeColor = Color.Red;
                Conn_Disconn_Button.Text = "Connect";

            }
        }

        private void Run_Click(object sender, EventArgs e)
        {
            if ((Serial_Port.IsOpen) && (!runningFlag))
            {
                timer1.Start();
                Run_Button.Text = "Stop";
                ProgBar2.ForeColor = Color.Lime;
                Cont.Enabled = false;
                Selec.Enabled = false;
                runningFlag = true;
                if (Selec.Checked)
                    SampBTN.Enabled = true;

            }
            else if ((Serial_Port.IsOpen) && runningFlag)
            {
                timer1.Stop();
                Run_Button.Text = "Run";
                ProgBar2.ForeColor = Color.Red;
                SampBTN.Enabled = false;
                runningFlag = false;
            }
            else if (!Serial_Port.IsOpen)
            {
                MessageBox.Show("COM not open", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Serial_Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
                rbuffer += Serial_Port.ReadExisting();
        }
            
        private void Timer1_Tick(object sender, EventArgs e)
        {
            /*
            check = false;
            do
            {
                Serial_Port.Write("$");
                for(int i = 0; i < 50000000; i++)
                {

                }    
                if(!rbuffer.Equals("") && (rbuffer.Length == 8))
                {
                    check = true;
                }
                else
                {
                    rbuffer = "";    //reset buffer
                }
            }
            while(!check);
            */

            Serial_Port.Write("$");

            for (int i = 0; i < 1000000; i++);

            if (rbuffer.Equals("") || (rbuffer.Length > 9))
                rbuffer += Convert.ToString(Temperature) + ',' + Convert.ToString(Humidity);

                //Typical response: "xxx.x,yyy"
            string[] temp = new string[2];
            temp = rbuffer.Split(',');
            rbuffer = "";    //reset buffer


            try
            {
                if (Cont.Checked)
                {
                    Temperature = Convert.ToDouble(temp[0]);
                    Humidity = Convert.ToInt16(temp[1]);
                    data[Index] = Convert.ToString(Time) + " " + Convert.ToString(Temperature) + " " + Convert.ToString(Humidity);
                    Temp_Box.Text = Convert.ToString(Temperature) + " C";
                    Humd_Box.Text = Convert.ToString(Humidity) + " %";
                    Run_Time.Text = Convert.ToString(Time) + "s";
                    Chart.Series["Temperature"].Points.AddXY(Time, Temperature);
                    Chart.Series["Humidity"].Points.AddXY(Time, Humidity);
                    Data_Table.Rows.Insert(0, Convert.ToString(Time), Convert.ToString(Temperature), Convert.ToString(Humidity));
                    Index++;
                }
                else if (Selec.Checked)
                {
                    Temperature = Convert.ToDouble(temp[0]);
                    Humidity = Convert.ToInt16(Convert.ToDouble(temp[1]));
                    Temp_Box.Text = Convert.ToString(Temperature) + " C";
                    Humd_Box.Text = Convert.ToString(Humidity) + " %";
                    Run_Time.Text = Convert.ToString(Time) + "s";
                    Chart.Series["Temperature"].Points.AddXY(Time, Temperature);
                    Chart.Series["Humidity"].Points.AddXY(Time, Humidity);
                }
                
            }
            catch(Exception err)
            {
                Time = 0;
                this.Run_Click(sender, e);
                Chart.Series.Clear();
                MessageBox.Show(err.Message + "\nNo data read!", "ERROR", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }


            Temp_Box.SelectAll();
            Temp_Box.SelectionAlignment = HorizontalAlignment.Center;
            Humd_Box.SelectAll();
            Humd_Box.SelectionAlignment = HorizontalAlignment.Center;
            Run_Time.SelectAll();
            Run_Time.SelectionAlignment = HorizontalAlignment.Center;


            if (Time >= 60)
            {
                Chart.ChartAreas[0].AxisX.Maximum = Time + 1;
                Chart.ChartAreas[0].AxisX.Minimum = Time - 60;
            }

            if(Index == 1001)
            {
                for(int i = 0; i < 1000; i++)
                {
                    data[i] = data[i + 1];
                }
                Index = 1000;
            }

            
            Time++;
        }

        private async void SaveToFile()
        {

            FileInfo file;

            if (!Save_Path.Text.Equals(""))
                file = new FileInfo(Save_Path.Text);
            else
                file = new FileInfo(File_Path);
            try
            {
                await SaveExcelFile(file);
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message + "\nFile not save!", "ERROR", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
        }

        private async Task SaveExcelFile(FileInfo file)
        {
            if (file.Exists)
                file.Delete();

            var package = new ExcelPackage(file);

            var ws = package.Workbook.Worksheets.Add("Main Report");

            ws.Cells["A1"].Value = "Last Report!";
            ws.Cells["A1:D1"].Merge = true;
            ws.Row(1).Style.Font.Size = 24;
            ws.Row(1).Style.Font.Color.SetColor(Color.Cyan);

            ws.Cells[2, 1].Value = "ID";
            ws.Cells[2, 2].Value = "Time (s)";
            ws.Cells[2, 3].Value = "Temperature (C)";
            ws.Cells[2, 4].Value = "Humidity (%)";
            ws.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Row(2).Style.Font.Bold = true;

            string[] SubStrg = new string[3];

            if (Cont.Checked)
            {
                for (int i = 0; i < Index; i++)
                {
                    ws.Cells[i + 3, 1].Value = i + 1;
                    SubStrg = data[i].Split(' ');
                    ws.Cells[i + 3, 2].Value = SubStrg[0];
                    ws.Cells[i + 3, 3].Value = SubStrg[1];
                    ws.Cells[i + 3, 4].Value = SubStrg[2];

                    ws.Row(i + 3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Row(i + 3).Height = 20;
                }
            }
            else if (Selec.Checked)
            {

                for (int i = 0; i < Index; i++)
                {
                    ws.Cells[i + 3, 1].Value = i + 1;
                    SubStrg = data[i].Split(' ');
                    ws.Cells[i + 3, 2].Value = SubStrg[0];
                    ws.Cells[i + 3, 3].Value = SubStrg[1];
                    ws.Cells[i + 3, 4].Value = SubStrg[2];

                    ws.Row(i + 3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Row(i + 3).Height = 20;
                }
            }
            

            ws.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Column(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Column(1).Width = 5;
            ws.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Column(2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Column(2).Width = 10;
            ws.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Column(3).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Column(3).Width = 20;
            ws.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Column(4).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Column(4).Width = 20;


            await package.SaveAsync();

            package.Dispose();

        }

        private void Exit_Button_Click(object sender, EventArgs e)
        {
            if (Serial_Port.IsOpen)
                Serial_Port.Close();
            this.Close();
        }

        private void Save_File_Button_Click(object sender, EventArgs e)
        {
            File_Path = Save_Path.Text;
            SaveToFile();
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            Data_Table.Rows.Clear();
            data = new string[1000];
            Index = 0;
            if(!runningFlag)
            {
                Cont.Enabled = true;
                Selec.Enabled = true;
            }
            
        }

        private void SampBTN_Click(object sender, EventArgs e)
        {
            data[Index] = Convert.ToString(Time) + " " + Convert.ToString(Temperature) + " " + Convert.ToString(Humidity);

            Data_Table.Rows.Insert(0, Convert.ToString(Time), Convert.ToString(Temperature), Convert.ToString(Humidity));

            Index++;

            if (Index == 1001)
            {
                for (int i = 0; i < 1000; i++)
                {
                    data[i] = data[i + 1];
                }
                Index = 1000;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] Ports = SerialPort.GetPortNames();

            COM_BOX.Items.Clear();

            COM_BOX.Items.AddRange(Ports);
        }
    }
}
