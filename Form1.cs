using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace forms_graphs
{
    public partial class Form1 : Form
    {
        static Graphics formGraphics;
        static GraphicsPath path;
        static Pen drawingPen;
        static Brush drawingBrush;
        static Font font;
        public int dpi = 5;
        static int size = 100;

        public Form1()
        {
            InitializeComponent();
            this.SizeChanged += initialize;
            this.initialize(this, new EventArgs());
            button1.Click += DrawGraph;
            textBox1.Text = "x";
            yscalarbox.Text = "10";
            xscalarbox.Text = "10";
            this.MouseWheel += Form1_MouseWheel;
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            xscalar += e.Delta / 12;
            yscalar += e.Delta / 12;
            xscalarbox.Text = xscalar.ToString();
            yscalarbox.Text = yscalar.ToString();
            DrawGraph(this, new EventArgs());
        }

        private void initialize(object o, EventArgs e)
        {
            drawingPen = new Pen(Color.Black);
            drawingPen.Width = 2f;
            font = new Font(new FontFamily("Arial"), (int)(Math.Sqrt(Width*Width+Height*Height) / size));
            drawingBrush = new SolidBrush(Color.Black);
            formGraphics = this.CreateGraphics();
            formGraphics.Clear(Color.White);
            formGraphics.DrawLine(drawingPen, new Point(0, ygraphoffset + (Height / 2)), new Point(Width + xgraphoffset, ygraphoffset + (Height / 2)));
            formGraphics.DrawLine(drawingPen, new Point(Width / 2 + xgraphoffset, 0), new Point(Width / 2 + xgraphoffset, Height + ygraphoffset));
            DrawOrigo();
        }

        public void DrawOrigo()
        {
            for (float r = 0; r < 1; r++)
            {
                formGraphics.DrawEllipse(drawingPen, new RectangleF(Width / 2 + xgraphoffset + 5 - (float)Math.Cos(r) * 10, -5 + Height / 2 + ygraphoffset - (float)Math.Sin(r) * 10, 10, 10));
            }
        }

        public float xscalar = 5;
        public float yscalar = 5;

        public int xgraphoffset = 0;
        public int ygraphoffset = 0;

        public void DrawGraph(object o, EventArgs e)
        {
            try
            {
                BeginGraph();
            }
            catch { }
        }

        private void BeginGraph()
        {
            xscalar = float.Parse(xscalarbox.Text);
            yscalar = float.Parse(yscalarbox.Text);
            formGraphics.Clear(Color.White);
            formGraphics.DrawLine(drawingPen, new Point(-Math.Abs(xgraphoffset), ygraphoffset + (Height / 2)), new Point(Width + Math.Abs(xgraphoffset), ygraphoffset + (Height / 2)));
            formGraphics.DrawLine(drawingPen, new Point(Width / 2 + xgraphoffset, -Math.Abs(ygraphoffset)), new Point(Width / 2 + xgraphoffset, Height + Math.Abs(ygraphoffset)));
            DrawOrigo();
            path?.Dispose();
            path = new GraphicsPath();
            button1.Enabled = false;
            Thread t = new Thread(() =>
            {
                float times = calcMultiplucation(textBox1.Text);
                float plus = calcPlusMinus(textBox1.Text);
                List<PointF>[] arr = new List<PointF>[2];
                arr[0] = new List<PointF>();
                arr[1] = new List<PointF>();

                for (int i = -1; i < 2; i += 2)
                {
                    for (float x = 0; x < Int32.MaxValue; x += 1f / xscalar)
                    {
                        float tX = x;
                        tX *= i;
                        float exp = calcExponential(textBox1.Text, tX);
                        //function expression: f(x) := x;
                        //how to calculate expression based on input
                        float y = (exp * times * tX + plus) * yscalar;
                        if (tX == 0) tX = float.Epsilon;
                        y = 1f/tX * yscalar;
                        if (y < -Height-ygraphoffset && (tX > xgraphoffset || tX < xgraphoffset + Width))
                            break;
                        if (y > Height+ygraphoffset && (tX > xgraphoffset || tX< xgraphoffset + Width))
                            break;
                        float finaly = (float)Math.Round(((Height - y)) - (Height / 2)) + ygraphoffset;
                        //if graph is below or above current Y
                        //what is current Y, well let's draw
                        float finalx = Width / 2 + (tX * xscalar) + xgraphoffset;
                        
                        //if ((finaly + ygraphoffset > Height + ygraphoffset) || (finaly < 0))
                        //{
                        //    break;
                        //}

                        arr[i == -1 ? 0 : 1].Add(new PointF(finalx, finaly));
                    }
                }
                try
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        formGraphics.DrawLines(drawingPen, arr[i].ToArray());
                    }
                }
                catch { }
                for (int i = -1; i < 2; i += 2)
                {
                    for (float x = xgraphoffset; x < Width; x++)
                    {
                        float tx = x * i;
                        if (x % Math.Floor((double)(Width / 15)) == 0)
                            try
                            {
                                formGraphics.DrawString((RoundDown(tx / xscalar, 2)).ToString(), font, drawingBrush,
                                    tx + Width / 2 + xgraphoffset,
                                    Height / 2 + ygraphoffset + 10);
                                formGraphics.DrawLine(drawingPen,
                                    new PointF(x, (Height / 2) + ygraphoffset + 5),
                                    new PointF(x, (Height / 2) + ygraphoffset - 5));
                            }
                            catch { }
                    }
                    for (float x = -ygraphoffset; x < Height; x++)
                    {
                        float tx = x * i;
                        if (x % Math.Floor((double)(Width / 15)) == 0)
                            try
                            {
                                formGraphics.DrawString((RoundDown(tx / yscalar, 2)).ToString(), font, drawingBrush,
                                    Width / 2 + xgraphoffset + 10,
                                    Height/2 - tx + ygraphoffset);
                                formGraphics.DrawLine(drawingPen,
                                    new PointF((Width / 2) + xgraphoffset + 5, x),
                                    new PointF((Width / 2) + xgraphoffset - 5, x));
                            }
                            catch { }
                    }
                }
                //formGraphics.DrawPath(drawingPen, path);
                button1.Invoke((MethodInvoker)(() =>
                {
                    button1.Enabled = true;
                }));
            });
            t.IsBackground = true;
            t.Start();
        }

        public static double RoundDown(double number, int decimalPlaces)
        {
            return Math.Floor(number * Math.Pow(10, decimalPlaces)) / Math.Pow(10, decimalPlaces);
        }

        float calcPlusMinus(string input)
        {
            float sum = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '+')
                {
                    int l = 0;
                    int tmpI = 0;
                    while (true)
                    {
                        tmpI++;
                        try
                        {
                            int.Parse(input[i + tmpI].ToString());
                        }
                        catch
                        {
                            break;
                        }
                        l++;
                    }
                    try
                    {
                        sum += float.Parse(input.Substring(i + 1, l));
                    }
                    catch
                    {

                    }
                }
                if (input[i] == '-')
                {
                    int l = 0;
                    int tmpI = 0;
                    while (true)
                    {
                        tmpI++;
                        try
                        {
                            int.Parse(input[i + tmpI].ToString());
                        }
                        catch
                        {
                            break;
                        }
                        l++;
                    }
                    try
                    {
                        sum -= float.Parse(input.Substring(i + 1, l));
                    }
                    catch { }
                }
            }
            return sum;
        }

        float calcMultiplucation(string input)
        {
            float sum = 1;
            for (int i = 0; i < input.Length; i++)
            {
                try
                {
                    if (input[i] == 'x' && input[i + 1] != '^')
                    {
                        bool worked = true;
                        try { float.Parse(input[i - 1].ToString()); }
                        catch
                        {
                            worked = false;
                        }
                        finally
                        {
                            if (worked)
                            {
                                sum *= float.Parse(input[i - 1].ToString());
                            }
                        }
                    }
                }
                catch { }
            }
            return sum;
        }

        float calcExponential(string input, float x)
        {
            float result = 1;
            bool found = false;
            int iterations = 0;
            int numOfThings = 0;
            for (int i = 0; i < input.Length; i++) { if (input[i] == '^') numOfThings++; }
            int[] ints = new int[numOfThings];
            while (iterations != numOfThings)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] == '^' && input[i - 1] == 'x' && !ints.Contains(i))
                    {
                        ints[iterations++] = i;
                        if (!found)
                            result = x;
                        found = true;
                        for (int i2 = 0; i2 < int.Parse(input[i + 1].ToString()); i2++)
                        {
                            result *= x;
                        }
                        break;
                    }
                }
            }
            return result;
        }

        public static Point pressedCoords = new Point(0, 0);

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            pressedCoords = new Point(e.X, e.Y);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            xgraphoffset += e.X - pressedCoords.X;
            ygraphoffset += e.Y - pressedCoords.Y;
            DrawGraph(this, new EventArgs());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            xgraphoffset = 0;
            ygraphoffset = 0;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            initialize(this, new EventArgs());
        }
    }



    class oldstuff
    {
        //    public void yeet()
        //    {
        //        for (float i = 0; i < dpi; i += 1)
        //        {
        //            float y = (/*function param*/    (i)) * (Height / dpi);
        //            float y1 = (/*function param*/    (i + 1)) * (Height / dpi);

        //            float x = i * (Width / dpi);
        //            float x1 = (i + 1) * (Width / dpi);

        //            int inputx1 = (int)(x);
        //            int inputy1 = (int)((Height - y));

        //            int inputx2 = (int)(x1);
        //            int inputy2 = (int)(Height - y1);
        //            formGraphics.DrawLine(myPen, new Point(Width / 2 - inputx1, Height / 2 - inputy1), new Point(Width / 2 - inputx2, Height / 2 - inputy2));
        //        }

        //    }
    }
}
