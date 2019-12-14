using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge;
using System.Drawing.Imaging;
using AForge.Neuro;
using AForge.Neuro.Learning;

namespace Identificado_Visual
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;

        VideoCaptureDevice videoSource;

        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            // Enumera os dispositivos
            videoDevices = new FilterInfoCollection(
                    FilterCategory.VideoInputDevice);
            // Escolhe a fonte de video
            videoSource = new VideoCaptureDevice(
                    videoDevices[0].MonikerString);
            // Começa a pegar os frames
            videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
            videoSource.SnapshotFrame += new NewFrameEventHandler(videoDevice_SnapshotFrame);
            // set NewFrame event handler
            videoSource.Start();

            timer1.Enabled = true;
            timer1.Start();


        }
        int imag = 0;
        Bitmap bitmap;
        Bitmap btmp;
        private void videoDevice_SnapshotFrame(object sender, NewFrameEventArgs eventArgs)
        {
            bitmap = eventArgs.Frame;
        }
        int first = 0;
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (first == 1)
            {
                pictureBox1.Image.Dispose();
            }
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();// clone the bitmap
            
            if(first == 0)
            {
                
            }
            first = 1;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoSource.IsRunning == true) videoSource.Stop();
        }

        Color[] RGB = new Color[3];
        Color Red = new Color();
        private void Button3_Click(object sender, EventArgs e)
        {
            
            
        }

        public static Bitmap MakeGrayscale3(Bitmap original,float R, float G, float B)
        {
            
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
                    new float[] {R, R, R, 0, 0},
                    new float[] {G, G, G, 0, 0},
                    new float[] {B, B, B, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
               });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        private void Button2_Click(object sender, EventArgs e)
        {

        }
        int first2 = 0;
        Bitmap bmp;
        Bitmap bmpGray;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {


                if (first2 == 0)
                {
                    timer1.Interval = 100;
                }
                if (first2 == 1)
                {
                    pictureBox2.Image.Dispose();
                    bmp.Dispose();

                }
                bmp = new Bitmap(pictureBox1.Image);
                bmpGray = MakeGrayscale3(bmp, float.Parse(textBox1.Text), float.Parse(textBox2.Text), float.Parse(textBox3.Text));
                pictureBox2.Image = bmpGray;
                if (first2 == 0)
                {
                    //autobias(10);
                    timer2.Enabled = true;
                    timer2.Start();
                }
                first2 = 1;
                
            }
            catch
            {

            }
        }
        int bias = 0;
        int nblobs = 100;
        int firstP = 0;
        public int autobias(int num)
        {
            int ok = 0;
            

                Bitmap c = new Bitmap(pictureBox2.Image);
                Bitmap d = new Bitmap(c.Width, c.Height);
                if (firstP == 0)
                {
                    int max = 0;
                    for (int i = 0; i < c.Width; i++)
                    {
                        for (int x = 0; x < c.Height; x++)
                        {
                            Color oc = c.GetPixel(i, x);
                            int grayScale = (int)((oc.R) + (oc.G) + (oc.B));
                            if (grayScale > max)
                            {
                                max = grayScale;
                            }
                        }
                    }
                    bias = Convert.ToInt32(Convert.ToDouble(0.3) * max);
                    firstP = 1;
                }
                Color nc = new Color();
                for (int i = 0; i < c.Width; i++)
                {
                    for (int x = 0; x < c.Height; x++)
                    {
                        Color oc = c.GetPixel(i, x);
                        int grayScale = (int)((oc.R) + (oc.G) + (oc.B));
                        if (grayScale > bias)
                        {
                            nc = Color.FromArgb(oc.A, 255, 255, 255);
                        }
                        else
                        {
                            nc = Color.FromArgb(oc.A, 0, 0, 0);
                        }
                        d.SetPixel(i, x, nc);
                    }
                }
                pictureBox3.Image = d;

                BlobCounter bc = new BlobCounter();
                bc.ProcessImage(d);
            Blob[] blobs = bc.GetObjectsInformation();
            nblobs = blobs.Count();
                if (nblobs > 10)
                {
                    bias = bias + num;
                }
           
            return bias;
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                Bitmap r = new Bitmap(pictureBox2.Image);
                Bitmap g = new Bitmap(r.Width, r.Height);
                Color nc = new Color();
                for (int i = 0; i < r.Width; i++)
                {
                    for (int x = 0; x < r.Height; x++)
                    {
                        Color oc = r.GetPixel(i, x);
                        int grayScale = (int)((oc.R) + (oc.G) + (oc.B));
                        if (grayScale > bias)
                        {
                            nc = Color.FromArgb(oc.A, 255, 255, 255);
                        }
                        else
                        {
                            nc = Color.FromArgb(oc.A, 0, 0, 0);
                        }
                        g.SetPixel(i, x, nc);
                    }
                }
                pictureBox3.Image = g;

                BlobCounter bc = new BlobCounter();
                bc.ProcessImage(g);
                Blob[] blobs = bc.GetObjectsInformation();
                int nblobs = blobs.Count();
                int objetos = 0;
                foreach(var bloob in blobs)
                {
                    if(bloob.Area > 500)
                    {
                        objetos++;
                        if(bloob.Area > 50000)
                        {
                            if (checkBox1.Checked)
                            {
                                autobias(100);
                            }
                        }
                        if(nblobs > 100)
                        {
                            if (checkBox1.Checked)
                            {
                                autobias(10);
                            }
                        }
                        if (nblobs < 20)
                        {
                            //autobias(-1);
                        }
                    }
                }
                label5.Text = objetos.ToString();
                
                label7.Text = nblobs.ToString();
            }
            catch
            {

            }
        }

        int first_try = 0;
        private void Button4_Click(object sender, EventArgs e)
        {
            
            int neural_start = 0;
            while (neural_start == 0)
            {
                if (first_try == 0)
                {
                    button1.PerformClick();
                }
                try
                {
                    pictureBox4.Image = pictureBox3.Image;
                    neural_start = 1;
                }
                catch
                {

                }
            }
            Bitmap d = new Bitmap(pictureBox4.Image);
            BlobCounter bc = new BlobCounter();
            bc.ProcessImage(d);
            Blob[] blobs = bc.GetObjectsInformation();
            foreach (var bloob in blobs)
            {
                if (bloob.Area > 500)
                {
                    double[][] treinamento =
                    {
                        new double[]{bloob.Area, bloob.Rectangle.Height, bloob.Rectangle.Width, bloob.Fullness }
                    };
                    label9.Text = bloob.Area.ToString();
                    label11.Text = bloob.Rectangle.Height.ToString();
                    label13.Text = bloob.Rectangle.Width.ToString();
                    label15.Text = bloob.Fullness.ToString();
                }

            }
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Button5_Click(object sender, EventArgs e)
        {
            Bitmap d = new Bitmap(pictureBox4.Image);
            BlobCounter bc = new BlobCounter();
            bc.ProcessImage(d);
            Blob[] blobs = bc.GetObjectsInformation();
            foreach (var bloob in blobs)
            {
                if (bloob.Area > 500)
                {
                    this.dataGridView1.Rows.Add(bloob.Area, bloob.Fullness, textBox4.Text);

                }

            }
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            double[] area = new double[dataGridView1.Rows.Count-1];
            double[] height = new double[dataGridView1.Rows.Count-1];
            double[] width = new double[dataGridView1.Rows.Count-1];
            double[] fullness = new double[dataGridView1.Rows.Count-1];
            double[] output = new double[dataGridView1.Rows.Count-1];
            for (int rows = 0; rows < dataGridView1.Rows.Count-1; rows++)
            {
                for (int col = 0; col < dataGridView1.Rows[rows].Cells.Count; col++)
                {
                    string value = dataGridView1.Rows[rows].Cells[col].Value.ToString();
                    if(col == 0)
                    {
                        area[rows]= Convert.ToDouble(value)/5000;
                    }
                    if (col == 1)
                    {
                        fullness[rows] = Convert.ToDouble(value);
                    }
                    if (col == 2)
                    {
                        if(value == "batom")
                        {
                            output[rows] = 0.8;
                        }
                        if (value == "batom2")
                        {
                            output[rows] = 0.1;
                        }

                    }
                }

            }
            double[][] inputs = new double[dataGridView1.Rows.Count-1][];
            double[][] outputs = new double[dataGridView1.Rows.Count-1][];
            for (int i = 0; i < dataGridView1.Rows.Count-1; i++)
            {
                inputs[i] = new double[] { area[i], fullness[i] };
                outputs[i] = new double[] { output[i] };
            }

            ActivationNetwork network = new ActivationNetwork(new SigmoidFunction(), 2, 3, 1);

            BackPropagationLearning back = new BackPropagationLearning(network);
            int iteracao = 0;
            double erro;
            while (true)
            {
                erro = back.RunEpoch(inputs, outputs);

                if (erro < 0.000001)
                {
                    break;
                }
                if (iteracao > 5000000)
                {
                    break;
                }
                iteracao++;
            }
            network.Save("neural.dat");
            MessageBox.Show("ACABOU!");
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            int neural_start = 0;
            while (neural_start == 0)
            {
                if (first_try == 0)
                {
                    button1.PerformClick();
                }
                try
                {
                    pictureBox5.Image = pictureBox3.Image;
                    neural_start = 1;
                }
                catch
                {

                }
            }

            Bitmap d = new Bitmap(pictureBox5.Image);
            BlobCounter bc = new BlobCounter();
            bc.ProcessImage(d);
            Blob[] blobs = bc.GetObjectsInformation();
            foreach (var bloob in blobs)
            {
                if (bloob.Area > 500)
                {
                    double[][] inputs =
                    {
                         new double[]{ bloob.Area/5000, bloob.Fullness }
                    };
                    Network network = Network.Load("neural.dat");
                    double[] saida = network.Compute(inputs[0]);
                    double dist1 = Math.Abs(saida[0] - 0.5);
                    double dist2 = Math.Abs(saida[0] - 0.1);
                    if(dist1 > dist2)
                    {
                        MessageBox.Show("batom deitado");
                    }
                    else
                    {
                        MessageBox.Show("batom em pe");
                    }
                    
                }

            }
        }
    }

}
