using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;



namespace ArchitectZad4
{


    struct MyColor
    {
        public double R; //0.0 - 1.0
        public double G;
        public double B;



        public MyColor(Color x)
        {
            R = x.R / 255.0;
            G = x.G / 255.0;
            B = x.B / 255.0;
        }
        public MyColor(double r, double g, double b)
        {
            R = r;
            G = g;
            B = b;
        }


        public Color GetColor()
        {
            int r = (int)Math.Round((R * 255.0));
            int g = (int)Math.Round((G * 255.0));
            int b = (int)Math.Round((B * 255.0));

            if (r > 255)
                r = 255;
            else if (r < 0)
                r = 0;
            if (g > 255)
                g = 255;
            else if (g < 0)
                g = 0;
            if (b > 255)
                b = 255;
            else if (b < 0)
                b = 0;


            Color res = Color.FromArgb(255, r, g, b);
            return res;
        }

        public Color GetColor(int bit = 8)
        {
            int m = (int)(Math.Pow(2, bit) - 1);
            double Rt = (double)((int)Math.Round((R * m))) / (double)m;
            double Gt = (double)((int)Math.Round((G * m))) / (double)m;
            double Bt = (double)((int)Math.Round((B * m))) / (double)m;

            int r = (int)Math.Round((Rt * 255.0));
            int g = (int)Math.Round((Gt * 255.0));
            int b = (int)Math.Round((Bt * 255.0));

            if (r > 255)
                r = 255;
            else if (r < 0)
                r = 0;
            if (g > 255)
                g = 255;
            else if (g < 0)
                g = 0;
            if (b > 255)
                b = 255;
            else if (b < 0)
                b = 0;


            Color res = Color.FromArgb(255, r, g, b);
            return res;
        }

        public static MyColor operator +(MyColor a, MyColor b)
        {
            return new MyColor(a.R + b.R, a.G + b.G, a.B + b.B);
        }
        public static MyColor operator *(MyColor a, double m)
        {
            return new MyColor(a.R*m, a.G*m, a.B*m);
        }
        public static MyColor operator *(MyColor a, MyColor b)
        {
            return new MyColor(a.R * b.R, a.G * b.G, a.B * b.B);
        }
        public static MyColor operator -(MyColor a, MyColor b)
        {
            return new MyColor(a.R - b.R, a.G - b.G, a.B - b.B);
        }

    }


    class MyImage
    {
        private Bitmap source;

        public void Save(string name)
        {
            source.Save(name + ".bmp");
        }

        public Bitmap GetSource()
        {
            return source;
        }

        public MyImage(Image image)
        {
            source = new Bitmap(image);
            width = source.Width;
            height = source.Height;
        }


        public Image GetImage()
        {
            Image im = source;
            return im;
        }

        private int width;
        public int Width { get => width;}

        private int height;
        public int Height { get => height;}
        

        public MyColor GetPixel(int i, int j)
        {
            if (i < 0)
                i = -i;
            else if (i >= source.Width)
                i = 2 * source.Width - i - 1;

            if (j < 0)
                j = -j;
            else if (j >= source.Height)
                j = 2 * source.Height - j - 1;

            Color t = source.GetPixel(i, j);
            MyColor r = new MyColor(t);
            return r;
        }
        public void SetPixel(int i, int j, MyColor color)
        {
            source.SetPixel(i, j, color.GetColor());
        }


        public MyColor this[int i, int j]
        {
            get
            {
                return GetPixel(i, j);
            }

            set
            {
                SetPixel(i, j, value);
            }
        }



    }




    
}
