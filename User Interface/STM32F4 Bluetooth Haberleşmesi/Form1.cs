using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace STM32F4_Bluetooth_Haberleşmesi
{
    public partial class Form1 : Form
    {
        
        string yazi;
        int indis_accX, indis_accY, indis_gyroX, indis_gyroY, indis_gyroZ, indis_comX, indis_comY, indis_comZ, indis_kalmanX, indis_kalmanY, indis_kalmanZ, indis_pusulaZ, indis_LowPassZ, indis_um7Roll, indis_um7Pitch, indis_um7Yaw;
        double accX, accY, gyroX, gyroY, gyroZ, comX, comY, comZ, kalmanX, kalmanY, kalmanZ, pusulaZ, lowPassZ, um7Roll, um7Pitch, um7Yaw;
        Thread Kanal1, Kanal2, Kanal3, Kanal4, Kanal5, Kanal6, Kanal7, Kanal8, Kanal9, Kanal10, Kanal11, Kanal12, Kanal13, Kanal14, Kanal15, Kanal16, Kanal17;
        Cube dortgen1, dortgen2, dortgen3, dortgen4;
        int baslangic = 0;

        TimeSpan sure = new TimeSpan(0, 0, 0, 0, 100);

        public Form1()
        {
            InitializeComponent();
        }

        delegate void SetTextCallback(string Metin, TextBox Metin_Kutusu);
        delegate void SetTextCallback2(string Metin, RichTextBox Metin_Kutusu);

        private void Form1_Load(object sender, EventArgs e)
        {
            CenterToScreen();
            Baglanti.PortName = "COM4";
            Baglanti.BaudRate = 9600;

            dortgen1 = new Cube(50, 25, 100);
            dortgen2 = new Cube(50, 25, 100);
            dortgen3 = new Cube(50, 25, 100);
            dortgen4 = new Cube(50, 25, 100);
            render();
        }

        private void dugme_baglan_Click(object sender, EventArgs e)
        {
            Baglanti.Open();
            durum.Text = "Bağlantı başarıyla kuruldu!";
            Kanal1 = new Thread(new ThreadStart(VeriAl));
            //Kanal2 = new Thread(new ParameterizedThreadStart(worker_ComX));
            //Kanal3 = new Thread(new ParameterizedThreadStart(worker_KalmanX));
            //Kanal4 = new Thread(new ParameterizedThreadStart(worker_AccX));
            //Kanal5 = new Thread(new ParameterizedThreadStart(worker_ComY));
            //Kanal6 = new Thread(new ParameterizedThreadStart(worker_KalmanY));
            //Kanal7 = new Thread(new ParameterizedThreadStart(worker_AccY));
            //Kanal8 = new Thread(new ParameterizedThreadStart(worker_GyroX));
            //Kanal9 = new Thread(new ParameterizedThreadStart(worker_GyroY));
            //Kanal10 = new Thread(new ParameterizedThreadStart(worker_PusulaZ));
            //Kanal11 = new Thread(new ParameterizedThreadStart(worker_GyroZ));
            //Kanal12 = new Thread(new ParameterizedThreadStart(worker_ComZ));
            //Kanal13 = new Thread(new ParameterizedThreadStart(worker_KalmanZ));
            //Kanal14 = new Thread(new ParameterizedThreadStart(worker_LowPassZ));
            Kanal15 = new Thread(new ParameterizedThreadStart(worker_Um7Roll));
            Kanal16 = new Thread(new ParameterizedThreadStart(worker_Um7Pitch));
            Kanal17 = new Thread(new ParameterizedThreadStart(worker_Um7Yaw));

            Kanal1.Start();
            //Kanal2.Start(new Action<myData>(this.AddDataPoint_ComX));
            //Kanal3.Start(new Action<myData>(this.AddDataPoint_KalmanX));
            //Kanal4.Start(new Action<myData>(this.AddDataPoint_AccX));
            //Kanal5.Start(new Action<myData>(this.AddDataPoint_ComY));
            //Kanal6.Start(new Action<myData>(this.AddDataPoint_KalmanY));
            //Kanal7.Start(new Action<myData>(this.AddDataPoint_AccY));
            //Kanal8.Start(new Action<myData>(this.AddDataPoint_GyroX));
            //Kanal9.Start(new Action<myData>(this.AddDataPoint_GyroY));
            //Kanal10.Start(new Action<myData>(this.AddDataPoint_PusulaZ));
            //Kanal11.Start(new Action<myData>(this.AddDataPoint_GyroZ));
            //Kanal12.Start(new Action<myData>(this.AddDataPoint_ComZ));
            //Kanal13.Start(new Action<myData>(this.AddDataPoint_KalmanZ));
            //Kanal14.Start(new Action<myData>(this.AddDataPoint_lowPassZ));
            Kanal15.Start(new Action<myData>(this.AddDataPoint_Um7Roll));
            Kanal16.Start(new Action<myData>(this.AddDataPoint_Um7Pitch));
            Kanal17.Start(new Action<myData>(this.AddDataPoint_Um7Yaw));

        }

        private void dugme_baglantiKes_Click(object sender, EventArgs e)
        {
            BaglantiKes();
        }

        private void Baglanti_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            VeriAl();
        }

        private void Metin_Girisi(string metin, TextBox metin_kutusu)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (metin_kutusu.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(Metin_Girisi);
                this.Invoke(d, new object[] { metin, metin_kutusu });
            }
            else
            {
                metin_kutusu.Text = metin;
            }
        }

        private void Metin_Girisi2(string metin, RichTextBox metin_kutusu)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (metin_kutusu.InvokeRequired)
            {
                SetTextCallback2 d = new SetTextCallback2(Metin_Girisi2);
                this.Invoke(d, new object[] { metin, metin_kutusu });
            }
            else
            {
                metin_kutusu.Text = metin_kutusu.Text + "\n" + metin;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            BaglantiKes();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            BaglantiKes();
        }

        private void BaglantiKes()
        {
            if (Baglanti.IsOpen)
            {
                Baglanti.Close();
                Kanal1.Interrupt();
                //Kanal2.Interrupt();
                //Kanal3.Interrupt();
                //Kanal4.Interrupt();
                //Kanal5.Interrupt();
                //Kanal6.Interrupt();
                //Kanal7.Interrupt();
                //Kanal8.Interrupt();
                //Kanal9.Interrupt();
                //Kanal10.Interrupt();
                //Kanal11.Interrupt();
                //Kanal12.Interrupt();
                //Kanal13.Interrupt();
                //Kanal14.Interrupt();
                Kanal15.Interrupt();
                Kanal16.Interrupt();
                Kanal17.Interrupt();
                durum.Text = "Bağlantı başarıyla kapatıldı!";
            }
        }

        private void VeriAl()
        {
            yazi = Baglanti.ReadLine();
            Metin_Girisi2(yazi, richTextBox1);
            //if (yazi.IndexOf("Pol")==0)
            //{
            //    yazi = yazi.Replace('.', ',');
            //    indis_um7Roll = yazi.IndexOf("R:");
            //    indis_um7Pitch = yazi.IndexOf("P:");
            //    indis_um7Yaw = yazi.IndexOf("Y:");

            //    um7Roll = Convert.ToDouble(yazi.Substring(indis_um7Roll + 2, indis_um7Pitch - indis_um7Roll - 2).Trim());
            //    um7Pitch = Convert.ToDouble(yazi.Substring(indis_um7Pitch + 2, indis_um7Yaw - indis_um7Pitch - 2).Trim());
            //    um7Yaw = Convert.ToDouble(yazi.Substring(indis_um7Yaw + 2, yazi.Length - indis_um7Yaw - 2 - 1).Trim());
   
            //}
            
            //if (yazi[0] == 'R')
            //{
                
            //}
            //if (yazi[0] == 'X')
            //{
            //    Metin_Girisi("", gelenKutusu);
            //    Metin_Girisi(yazi.ToString(), gelenKutusu);

            //    yazi = yazi.Replace('.', ',');

            //    indis_accX = yazi.IndexOf("X0:");
            //    indis_accY = yazi.IndexOf("Y0:");
            //    indis_gyroX = yazi.IndexOf("X1:");
            //    indis_gyroY = yazi.IndexOf("Y1:");
            //    indis_comX = yazi.IndexOf("X2:");
            //    indis_comY = yazi.IndexOf("Y2:");
            //    indis_kalmanX = yazi.IndexOf("X3:");
            //    indis_kalmanY = yazi.IndexOf("Y3:");
            //    indis_pusulaZ = yazi.IndexOf("Z0:");
            //    indis_gyroZ = yazi.IndexOf("Z1:");
            //    indis_comZ = yazi.IndexOf("Z2:");
            //    indis_kalmanZ = yazi.IndexOf("Z3:");
            //    indis_LowPassZ = yazi.IndexOf("Z4:");

            //    //accX = Convert.ToDouble(yazi.Substring(indis_accX + 3, indis_accY - indis_accX - 3).Trim());
            //    //accY = Convert.ToDouble(yazi.Substring(indis_accY + 3, indis_gyroX - indis_accY - 3).Trim());
            //    //gyroX = Convert.ToDouble(yazi.Substring(indis_gyroX + 3, indis_gyroY - indis_gyroX - 3).Trim());
            //    //gyroY = Convert.ToDouble(yazi.Substring(indis_gyroY + 3, indis_comX - indis_gyroY - 3).Trim());
            //    //comX = Convert.ToDouble(yazi.Substring(indis_comX + 3, indis_comY - indis_comX - 3).Trim());
            //    //comY = Convert.ToDouble(yazi.Substring(indis_comY + 3, indis_kalmanX - indis_comY - 3).Trim());
            //    //kalmanX = Convert.ToDouble(yazi.Substring(indis_kalmanX + 3, indis_kalmanY - indis_kalmanX - 3).Trim());
            //    //kalmanY = Convert.ToDouble(yazi.Substring(indis_kalmanY + 3, indis_pusulaZ - indis_kalmanY - 3).Trim());
            //    //pusulaZ = Convert.ToDouble(yazi.Substring(indis_pusulaZ + 3, indis_gyroZ - indis_pusulaZ - 3).Trim());
            //    //gyroZ = Convert.ToDouble(yazi.Substring(indis_gyroZ + 3, indis_comZ - indis_gyroZ - 3).Trim());
            //    //comZ = Convert.ToDouble(yazi.Substring(indis_comZ + 3, indis_kalmanZ - indis_comZ - 3).Trim());
            //    //kalmanZ = Convert.ToDouble(yazi.Substring(indis_kalmanZ + 3, indis_LowPassZ - indis_kalmanZ - 3).Trim());
            //    //lowPassZ = Convert.ToDouble(yazi.Substring(indis_LowPassZ + 3, yazi.Length - indis_LowPassZ - 3 - 1).Trim());

            //    Metin_Girisi(accX.ToString(), metin_accX);
            //    Metin_Girisi(accY.ToString(), metin_accY);
            //    Metin_Girisi(pusulaZ.ToString(), metin_pusulaZ);
            //    Metin_Girisi(gyroX.ToString(), metin_gyroX);
            //    Metin_Girisi(gyroY.ToString(), metin_gyroY);
            //    Metin_Girisi(gyroZ.ToString(), metin_gyroZ);
            //    Metin_Girisi(comX.ToString(), metin_comX);
            //    Metin_Girisi(comY.ToString(), metin_comY);
            //    Metin_Girisi(comZ.ToString(), metin_comZ);
            //    Metin_Girisi(kalmanX.ToString(), metin_kalmanX);
            //    Metin_Girisi(kalmanY.ToString(), metin_kalmanY);
            //    Metin_Girisi(kalmanZ.ToString(), metin_kalmanZ);

            //    render();

            //}
               
        }

        public void AddDataPoint_Um7Roll(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_Um7Roll), new object[] { data });
            }
            else
            {
                um7_grafik.Series["Roll"].Points.AddXY(data.t, data.um7_roll);
            }
        }

        public void AddDataPoint_Um7Pitch(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_Um7Pitch), new object[] { data });
            }
            else
            {
                um7_grafik.Series["Pitch"].Points.AddXY(data.t, data.um7_pitch);
            }
        }

        public void AddDataPoint_Um7Yaw(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_Um7Yaw), new object[] { data });
            }
            else
            {
                um7_grafik.Series["Yaw"].Points.AddXY(data.t, data.um7_yaw);
            }
        }

        public void AddDataPoint_AccX(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_AccX), new object[] { data });
            }
            else
            {
                veri_roll.Series["AccX"].Points.AddXY(data.t, data.y_accX);
            }
        }

        public void AddDataPoint_GyroX(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_GyroX), new object[] { data });
            }
            else
            {
                veri_roll.Series["GyroX"].Points.AddXY(data.t, data.y_gyroX);
            }
        }

        public void AddDataPoint_ComX(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_ComX), new object[] { data });
            }
            else
            {
                veri_roll.Series["ComX"].Points.AddXY(data.t, data.y_comX);
            }
        }

        public void AddDataPoint_KalmanX(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_KalmanX), new object[] { data });
            }
            else
            {
                veri_roll.Series["KalmanX"].Points.AddXY(data.t, data.y_kalmanX);
            }
        }

        public void AddDataPoint_AccY(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_AccY), new object[] { data });
            }
            else
            {
                veri_pitch.Series["AccY"].Points.AddXY(data.t, data.y_accY);
            }
        }

        public void AddDataPoint_GyroY(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_GyroY), new object[] { data });
            }
            else
            {
                veri_pitch.Series["GyroY"].Points.AddXY(data.t, data.y_gyroY);
            }
        }

        public void AddDataPoint_ComY(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_ComY), new object[] { data });
            }
            else
            {
                veri_pitch.Series["ComY"].Points.AddXY(data.t, data.y_comY);
            }
        }

        public void AddDataPoint_KalmanY(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_KalmanY), new object[] { data });
            }
            else
            {
                veri_pitch.Series["KalmanY"].Points.AddXY(data.t, data.y_kalmanY);
            }
        }

        public void AddDataPoint_PusulaZ(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_PusulaZ), new object[] { data });
            }
            else
            {
                yaw_grafik.Series["PusulaZ"].Points.AddXY(data.t, data.y_pusulaZ);
            }
        }

        public void AddDataPoint_GyroZ(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_GyroZ), new object[] { data });
            }
            else
            {
                yaw_grafik.Series["GyroZ"].Points.AddXY(data.t, data.y_gyroZ);
            }
        }

        public void AddDataPoint_ComZ(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_ComZ), new object[] { data });
            }
            else
            {
                yaw_grafik.Series["ComZ"].Points.AddXY(data.t, data.y_comZ);
            }
        }


        public void AddDataPoint_KalmanZ(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_KalmanZ), new object[] { data });
            }
            else
            {
                yaw_grafik.Series["KalmanZ"].Points.AddXY(data.t, data.y_kalmanZ);
            }
        }

        public void AddDataPoint_lowPassZ(myData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<myData>(this.AddDataPoint_lowPassZ), new object[] { data });
            }
            else
            {
                yaw_grafik.Series["LowPassZ"].Points.AddXY(data.t, data.y_LowPassZ);
            }
        }

        private void worker_Um7Roll(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, um7_roll = um7Roll });
                Thread.Sleep(sure);
            }
        }

        private void worker_Um7Pitch(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, um7_pitch = um7Pitch });
                Thread.Sleep(sure);
            }
        }

        private void worker_Um7Yaw(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, um7_yaw = um7Yaw });
                Thread.Sleep(sure);
            }
        }

        private void worker_AccX(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, y_accX = accX });
                Thread.Sleep(sure);
            }
        }

        private void worker_ComX(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, y_comX = comX });
                Thread.Sleep(sure);
            }
        }

        private void worker_KalmanX(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, y_kalmanX = kalmanX });
                Thread.Sleep(sure);
            }
        }

        private void worker_GyroX(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, y_gyroX = gyroX });
                Thread.Sleep(sure);
            }
        }

        private void worker_AccY(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, y_accY = accY });
                Thread.Sleep(sure);
            }
        }

        private void worker_ComY(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, y_comY = comY });
                Thread.Sleep(sure);
            }
        }

        private void worker_KalmanY(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, y_kalmanY = kalmanY });
                Thread.Sleep(sure);
            }
        }

        private void worker_GyroY(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, y_gyroY = gyroY });
                Thread.Sleep(sure);
            }
        }

        private void worker_PusulaZ(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, y_pusulaZ = pusulaZ });
                Thread.Sleep(sure);
            }
        }

        private void worker_GyroZ(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, y_gyroZ = gyroZ });
                Thread.Sleep(sure);
            }
        }

        private void worker_ComZ(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, y_comZ = comZ });
                Thread.Sleep(sure);
            }
        }

        private void worker_KalmanZ(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, y_kalmanZ = kalmanZ });
                Thread.Sleep(sure);
            }
        }

        private void worker_LowPassZ(object obj)
        {
            var _delegate = (Action<myData>)obj;

            for (double i = 0; i < 500; i += 0.20)
            {
                _delegate(new myData { t = i, y_LowPassZ = lowPassZ });
                Thread.Sleep(sure);
            }
        }

        public class myData
        {
            public double t;
            public double y_accX;
            public double y_gyroX;
            public double y_comX;
            public double y_kalmanX;
            public double y_accY;
            public double y_gyroY;
            public double y_comY;
            public double y_kalmanY;
            public double y_pusulaZ;
            public double y_gyroZ;
            public double y_comZ;
            public double y_kalmanZ;
            public double y_LowPassZ;
            public double um7_roll;
            public double um7_pitch;
            public double um7_yaw;
        }

        private void render()
        {
            //Set the rotation values
            dortgen1.RotateX = accX;
            dortgen1.RotateY = pusulaZ;
            dortgen1.RotateZ = accY;

            dortgen2.RotateX = gyroX;
            dortgen2.RotateY = gyroZ;
            dortgen2.RotateZ = gyroY;

            dortgen3.RotateX = comX;
            dortgen3.RotateY = comZ;
            dortgen3.RotateZ = comY;

            dortgen4.RotateX = kalmanX;
            dortgen4.RotateY = kalmanZ;
            dortgen4.RotateZ = kalmanY;

            //Cube is positioned based on center
            Point origin1 = new Point(accModel.Width / 2, accModel.Height / 2);
            Point origin2 = new Point(gyroModel.Width / 2, gyroModel.Height / 2);
            Point origin3 = new Point(comModel.Width / 2, comModel.Height / 2);
            Point origin4 = new Point(kalmanModel.Width / 2, kalmanModel.Height / 2);

            accModel.Image = dortgen1.drawCube(origin1);
            gyroModel.Image = dortgen2.drawCube(origin2);
            comModel.Image = dortgen3.drawCube(origin3);
            kalmanModel.Image = dortgen4.drawCube(origin4);

        }

        
    }

    internal class Math3D
    {
        public class Point3D
        {
            //The Point3D class is rather simple, just keeps track of X Y and Z values,
            //and being a class it can be adjusted to be comparable
            public double X;
            public double Y;
            public double Z;

            public Point3D(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public Point3D(float x, float y, float z)
            {
                X = (double)x;
                Y = (double)y;
                Z = (double)z;
            }

            public Point3D(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public Point3D()
            {
            }

            public override string ToString()
            {
                return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
            }
        }

        public class Camera
        {
            //For 3D drawing we need a point of perspective, thus the Camera
            public Point3D Position = new Point3D();
        }

        public static Point3D RotateX(Point3D point3D, double degrees)
        {
            //Here we use Euler's matrix formula for rotating a 3D point x degrees around the x-axis

            //[ a  b  c ] [ x ]   [ x*a + y*b + z*c ]
            //[ d  e  f ] [ y ] = [ x*d + y*e + z*f ]
            //[ g  h  i ] [ z ]   [ x*g + y*h + z*i ]

            //[ 1    0        0   ]
            //[ 0   cos(x)  sin(x)]
            //[ 0   -sin(x) cos(x)]

            double cDegrees = (Math.PI * degrees) / 180.0f; //Convert degrees to radian for .Net Cos/Sin functions
            double cosDegrees = Math.Cos(cDegrees);
            double sinDegrees = Math.Sin(cDegrees);

            double y = (point3D.Y * cosDegrees) + (point3D.Z * sinDegrees);
            double z = (point3D.Y * -sinDegrees) + (point3D.Z * cosDegrees);

            return new Point3D(point3D.X, y, z);
        }

        public static Point3D RotateY(Point3D point3D, double degrees)
        {
            //Y-axis

            //[ cos(x)   0    sin(x)]
            //[   0      1      0   ]
            //[-sin(x)   0    cos(x)]

            double cDegrees = (Math.PI * degrees) / 180.0; //Radians
            double cosDegrees = Math.Cos(cDegrees);
            double sinDegrees = Math.Sin(cDegrees);

            double x = (point3D.X * cosDegrees) + (point3D.Z * sinDegrees);
            double z = (point3D.X * -sinDegrees) + (point3D.Z * cosDegrees);

            return new Point3D(x, point3D.Y, z);
        }

        public static Point3D RotateZ(Point3D point3D, double degrees)
        {
            //Z-axis

            //[ cos(x)  sin(x) 0]
            //[ -sin(x) cos(x) 0]
            //[    0     0     1]

            double cDegrees = (Math.PI * degrees) / 180.0; //Radians
            double cosDegrees = Math.Cos(cDegrees);
            double sinDegrees = Math.Sin(cDegrees);

            double x = (point3D.X * cosDegrees) + (point3D.Y * sinDegrees);
            double y = (point3D.X * -sinDegrees) + (point3D.Y * cosDegrees);

            return new Point3D(x, y, point3D.Z);
        }

        public static Point3D Translate(Point3D points3D, Point3D oldOrigin, Point3D newOrigin)
        {
            //Moves a 3D point based on a moved reference point
            Point3D difference = new Point3D(newOrigin.X - oldOrigin.X, newOrigin.Y - oldOrigin.Y, newOrigin.Z - oldOrigin.Z);
            points3D.X += difference.X;
            points3D.Y += difference.Y;
            points3D.Z += difference.Z;
            return points3D;
        }

        //These are to make the above functions workable with arrays of 3D points
        public static Point3D[] RotateX(Point3D[] points3D, double degrees)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateX(points3D[i], degrees);
            }
            return points3D;
        }

        public static Point3D[] RotateY(Point3D[] points3D, double degrees)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateY(points3D[i], degrees);
            }
            return points3D;
        }

        public static Point3D[] RotateZ(Point3D[] points3D, double degrees)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateZ(points3D[i], degrees);
            }
            return points3D;
        }

        public static Point3D[] Translate(Point3D[] points3D, Point3D oldOrigin, Point3D newOrigin)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = Translate(points3D[i], oldOrigin, newOrigin);
            }
            return points3D;
        }

    }

    internal class Cube
    {
        //Example cube class to demonstrate the use of 3D points and 3D rotation

        public int width = 0;
        public int height = 0;
        public int depth = 0;

        double xRotation = 0.0;
        double yRotation = 0.0;
        double zRotation = 0.0;

        Math3D.Camera camera1 = new Math3D.Camera();
        Math3D.Point3D cubeOrigin;

        public double RotateX
        {
            get { return xRotation; }
            set { xRotation = value; }
        }

        public double RotateY
        {
            get { return yRotation; }
            set { yRotation = value; }
        }

        public double RotateZ
        {
            get { return zRotation; }
            set { zRotation = value; }
        }

        public Cube(int W, int H, int D)
        {
            width = W;
            height = H;
            depth = D;
            cubeOrigin = new Math3D.Point3D(width / 2, height / 2, depth / 2);
        }

        public Cube(int side, Math3D.Point3D origin)
        {
            width = side;
            height = side;
            depth = side;
            cubeOrigin = origin;
        }

        public Cube(int Side)
        {
            width = Side;
            height = Side;
            depth = Side;
            cubeOrigin = new Math3D.Point3D(width / 2, height / 2, depth / 2);
        }

        public Cube(int Width, int Height, int Depth, Math3D.Point3D origin)
        {
            width = Width;
            height = Height;
            depth = Depth;
            cubeOrigin = origin;
        }

        //Finds the othermost points. Used so when the cube is drawn on a bitmap,
        //the bitmap will be the correct size
        public static Rectangle getBounds(PointF[] points)
        {
            double left = points[0].X;
            double right = points[0].X;
            double top = points[0].Y;
            double bottom = points[0].Y;
            for (int i = 1; i < points.Length; i++)
            {
                if (points[i].X < left)
                    left = points[i].X;
                if (points[i].X > right)
                    right = points[i].X;
                if (points[i].Y < top)
                    top = points[i].Y;
                if (points[i].Y > bottom)
                    bottom = points[i].Y;
            }

            return new Rectangle(0, 0, (int)Math.Round(right - left), (int)Math.Round(bottom - top));
        }

        public Bitmap drawCube(Point drawOrigin)
        {
            //FRONT FACE
            //Top Left - 7
            //Top Right - 4
            //Bottom Left - 6
            //Bottom Right - 5

            //Vars
            PointF[] point3D = new PointF[24]; //Will be actual 2D drawing points
            Point tmpOrigin = new Point(0, 0);

            Math3D.Point3D point0 = new Math3D.Point3D(0, 0, 0); //Used for reference

            //Zoom factor is set with the monitor width to keep the cube from being distorted
            double zoom = (double)Screen.PrimaryScreen.Bounds.Width / 1.5;

            //Set up the cube
            Math3D.Point3D[] cubePoints = fillCubeVertices(width, height, depth);

            //Calculate the camera Z position to stay constant despite rotation            
            Math3D.Point3D anchorPoint = (Math3D.Point3D)cubePoints[4]; //anchor point
            double cameraZ = -(((anchorPoint.X - cubeOrigin.X) * zoom) / cubeOrigin.X) + anchorPoint.Z;
            camera1.Position = new Math3D.Point3D(cubeOrigin.X, cubeOrigin.Y, cameraZ);

            //Apply Rotations, moving the cube to a corner then back to middle
            cubePoints = Math3D.Translate(cubePoints, cubeOrigin, point0);
            cubePoints = Math3D.RotateX(cubePoints, xRotation); //The order of these
            cubePoints = Math3D.RotateY(cubePoints, yRotation); //rotations is the source
            cubePoints = Math3D.RotateZ(cubePoints, zRotation); //of Gimbal Lock
            cubePoints = Math3D.Translate(cubePoints, point0, cubeOrigin);

            //Convert 3D Points to 2D
            Math3D.Point3D vec;
            for (int i = 0; i < point3D.Length; i++)
            {
                vec = cubePoints[i];
                if (vec.Z - camera1.Position.Z >= 0)
                {
                    point3D[i].X = (int)((double)-(vec.X - camera1.Position.X) / (-0.1f) * zoom) + drawOrigin.X;
                    point3D[i].Y = (int)((double)(vec.Y - camera1.Position.Y) / (-0.1f) * zoom) + drawOrigin.Y;
                }
                else
                {
                    tmpOrigin.X = (int)((double)(cubeOrigin.X - camera1.Position.X) / (double)(cubeOrigin.Z - camera1.Position.Z) * zoom) + drawOrigin.X;
                    tmpOrigin.Y = (int)((double)-(cubeOrigin.Y - camera1.Position.Y) / (double)(cubeOrigin.Z - camera1.Position.Z) * zoom) + drawOrigin.Y;

                    point3D[i].X = (float)((vec.X - camera1.Position.X) / (vec.Z - camera1.Position.Z) * zoom + drawOrigin.X);
                    point3D[i].Y = (float)(-(vec.Y - camera1.Position.Y) / (vec.Z - camera1.Position.Z) * zoom + drawOrigin.Y);

                    point3D[i].X = (int)point3D[i].X;
                    point3D[i].Y = (int)point3D[i].Y;
                }
            }

            //Now to plot out the points
            Rectangle bounds = getBounds(point3D);
            bounds.Width += drawOrigin.X;
            bounds.Height += drawOrigin.Y;

            Bitmap tmpBmp = new Bitmap(bounds.Width, bounds.Height);
            Graphics g = Graphics.FromImage(tmpBmp);

            //Back Face
            g.DrawLine(Pens.Black, point3D[0], point3D[1]);
            g.DrawLine(Pens.Black, point3D[1], point3D[2]);
            g.DrawLine(Pens.Black, point3D[2], point3D[3]);
            g.DrawLine(Pens.Black, point3D[3], point3D[0]);

            //Front Face
            g.DrawLine(Pens.Black, point3D[4], point3D[5]);
            g.DrawLine(Pens.Black, point3D[5], point3D[6]);
            g.DrawLine(Pens.Black, point3D[6], point3D[7]);
            g.DrawLine(Pens.Black, point3D[7], point3D[4]);

            //Right Face
            g.DrawLine(Pens.Black, point3D[8], point3D[9]);
            g.DrawLine(Pens.Black, point3D[9], point3D[10]);
            g.DrawLine(Pens.Black, point3D[10], point3D[11]);
            g.DrawLine(Pens.Black, point3D[11], point3D[8]);

            //Left Face
            g.DrawLine(Pens.Black, point3D[12], point3D[13]);
            g.DrawLine(Pens.Black, point3D[13], point3D[14]);
            g.DrawLine(Pens.Black, point3D[14], point3D[15]);
            g.DrawLine(Pens.Black, point3D[15], point3D[12]);

            //Bottom Face
            g.DrawLine(Pens.Black, point3D[16], point3D[17]);
            g.DrawLine(Pens.Black, point3D[17], point3D[18]);
            g.DrawLine(Pens.Black, point3D[18], point3D[19]);
            g.DrawLine(Pens.Black, point3D[19], point3D[16]);

            //Top Face
            g.DrawLine(Pens.Black, point3D[20], point3D[21]);
            g.DrawLine(Pens.Black, point3D[21], point3D[22]);
            g.DrawLine(Pens.Black, point3D[22], point3D[23]);
            g.DrawLine(Pens.Black, point3D[23], point3D[20]);

            g.Dispose(); //Clean-up

            return tmpBmp;
        }

        public static Math3D.Point3D[] fillCubeVertices(int width, int height, int depth)
        {
            Math3D.Point3D[] verts = new Math3D.Point3D[24];

            //front face
            verts[0] = new Math3D.Point3D(0, 0, 0);
            verts[1] = new Math3D.Point3D(0, height, 0);
            verts[2] = new Math3D.Point3D(width, height, 0);
            verts[3] = new Math3D.Point3D(width, 0, 0);

            //back face
            verts[4] = new Math3D.Point3D(0, 0, depth);
            verts[5] = new Math3D.Point3D(0, height, depth);
            verts[6] = new Math3D.Point3D(width, height, depth);
            verts[7] = new Math3D.Point3D(width, 0, depth);

            //left face
            verts[8] = new Math3D.Point3D(0, 0, 0);
            verts[9] = new Math3D.Point3D(0, 0, depth);
            verts[10] = new Math3D.Point3D(0, height, depth);
            verts[11] = new Math3D.Point3D(0, height, 0);

            //right face
            verts[12] = new Math3D.Point3D(width, 0, 0);
            verts[13] = new Math3D.Point3D(width, 0, depth);
            verts[14] = new Math3D.Point3D(width, height, depth);
            verts[15] = new Math3D.Point3D(width, height, 0);

            //top face
            verts[16] = new Math3D.Point3D(0, height, 0);
            verts[17] = new Math3D.Point3D(0, height, depth);
            verts[18] = new Math3D.Point3D(width, height, depth);
            verts[19] = new Math3D.Point3D(width, height, 0);

            //bottom face
            verts[20] = new Math3D.Point3D(0, 0, 0);
            verts[21] = new Math3D.Point3D(0, 0, depth);
            verts[22] = new Math3D.Point3D(width, 0, depth);
            verts[23] = new Math3D.Point3D(width, 0, 0);

            return verts;
        }  
    }
}
