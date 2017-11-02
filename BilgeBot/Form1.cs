using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Net;
using System.Security.Cryptography;


namespace Bilge_Assistant
{
    public partial class Form1 : Form
    {
        [DllImport("user32")]
        internal static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        internal static extern void ReleaseDC(IntPtr dc);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }


        public static int left;
        public static int top;
        public static Boolean Go = false;
        public static Boolean bot = false;
        Painter P = new Painter();
        public Thread painter = null;
        public Form1()
        {
            InitializeComponent();
            this.Text = "BilgeBot";
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            bot = false;
            Painter.bot = false;
            LblRunornot.Text = "Paused";
            LblRunornot.ForeColor = Color.Red;
            Go = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = true;
            btnStart.Enabled = false;
            bot = true;
            Painter.bot = true;
            LblRunornot.Text = "Running";
            LblRunornot.ForeColor = Color.Green;
            Go = true;
            painter = new Thread(new ThreadStart(P.Paint));
            painter.Start();
        }

        public void SimulateMove(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                return;
            }
            Bitmap windowBitmap = GetWindowBitmap(hwnd);
            this.GetBoxColor(windowBitmap, 3, 10, true);
            Color[,] grid = this.MakeGrid(windowBitmap);
        Label_0031:
            if (ClickJelly(hwnd, grid))
            {
                return;
            }
            if (!this.ClickFish(hwnd, grid))
            {
                Point bestMove = this.GetBestMove((Color[,])grid.Clone());
                Bitmap bmpClient = GetWindowBitmap(hwnd);
                Color[,] colorArray = this.MakeGrid(bmpClient);
                for (int i = 0; i <= 5; i++)
                {
                    for (int j = 0; j <= 11; j++)
                    {
                        if (!((colorArray[i, j]) == (grid[i, j])))
                        {
                            grid = (Color[,])colorArray.Clone();
                            goto Label_0031;
                        }
                    }
                }
                if (this.IsObject((grid[bestMove.X - 1, bestMove.Y - 1])))
                {
                    ClickBox(hwnd, bestMove.X, bestMove.Y);
                }
            }
        }

        public bool ClickJelly(IntPtr hwnd, Color[,] Grid)
        {
            for (int i = 1; i <= 12; i++)
            {
                for (int j = 1; j <= 6; j++)
                {
                    if (this.IsJelly((Grid[j - 1, i - 1])) && CanClick(Grid, j, i))
                    {
                        this.ClickBox(hwnd, j, i);
                        return true;
                    }
                }
            }
            return false;
        }

        public Bitmap GetWindowBitmap(IntPtr hwnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hwnd, out rect);
            Bitmap image = new Bitmap((rect.X2 - rect.X1) - (2 * this.PuzzlePiratesBorderWidth(hwnd)), ((rect.Y2 - rect.Y1) - this.PuzzlePiratesBorderHeight(hwnd)) - (2 * this.PuzzlePiratesBorderWidth(hwnd)));
            Graphics.FromImage(image).CopyFromScreen(new Point(rect.X1 + this.PuzzlePiratesBorderWidth(hwnd), (rect.Y1 + this.PuzzlePiratesBorderHeight(hwnd)) + this.PuzzlePiratesBorderWidth(hwnd)), new Point(), new Size((rect.X2 - rect.X1) - (2 * this.PuzzlePiratesBorderWidth(hwnd)), ((rect.Y2 - rect.Y1) - this.PuzzlePiratesBorderHeight(hwnd)) - (2 * this.PuzzlePiratesBorderWidth(hwnd))));
            return image;
        }

        public int[] FindBestMove(Color[,] Grid, int ClickX, int ClickY)
        {
            int[] array = new int[0];
            if (this.CanClick(Grid, ClickX, ClickY))
            {
                Color[,] colorArray = this.ClickGrid((Color[,])Grid.Clone(), ClickX, ClickY);
                if (((ClickX <= 3) && ((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX + 1, ClickY - 1]))) && ((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX + 2, ClickY - 1])))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 3;
                }
                if (((ClickX >= 3) && ((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 2, ClickY - 1]))) && ((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 3, ClickY - 1])))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 3;
                }
                if ((((ClickY <= 10) && (ClickY >= 3)) && (((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY])) && ((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY + 1])))) && (((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY - 2])) && ((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY - 3]))))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 5;
                }
                else if ((((ClickY <= 11) && (ClickY >= 3)) && (((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY])) && ((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY - 2])))) && ((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY - 3])))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 4;
                }
                else if ((((ClickY <= 10) && (ClickY >= 2)) && (((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY])) && ((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY + 1])))) && ((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY - 2])))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 4;
                }
                else if (((ClickY <= 11) && (ClickY >= 2)) && (((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY])) && ((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY - 2]))))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 3;
                }
                else if (((ClickY >= 3) && ((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY - 2]))) && ((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY - 3])))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 3;
                }
                else if (((ClickY <= 10) && ((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY]))) && ((colorArray[ClickX - 1, ClickY - 1]) == (colorArray[ClickX - 1, ClickY + 1])))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 3;
                }
                if ((((ClickY <= 10) && (ClickY >= 3)) && (((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY])) && ((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY + 1])))) && (((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY - 2])) && ((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY - 3]))))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 5;
                    return array;
                }
                if ((((ClickY <= 11) && (ClickY >= 3)) && (((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY])) && ((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY - 2])))) && ((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY - 3])))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 4;
                    return array;
                }
                if ((((ClickY <= 10) && (ClickY >= 2)) && (((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY])) && ((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY + 1])))) && ((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY - 2])))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 4;
                    return array;
                }
                if (((ClickY <= 11) && (ClickY >= 2)) && (((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY])) && ((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY - 2]))))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 3;
                    return array;
                }
                if (((ClickY >= 3) && ((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY - 2]))) && ((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY - 3])))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 3;
                    return array;
                }
                if (((ClickY <= 10) && ((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY]))) && ((colorArray[ClickX, ClickY - 1]) == (colorArray[ClickX, ClickY + 1])))
                {
                    Array.Resize<int>(ref array, array.Length + 1);
                    array[array.Length - 1] = 3;
                }
            }
            return array;
        }

        public Point GetBestMove(Color[,] Grid)
        {
            int[] numArray2;
            Random random = new Random();
            Point point = new Point(0, 0);
            int num = 0;
            int[] gridB = new int[0];
            bool flag = false;
            for (int i = 1; i <= 12; i++)
            {
                for (int k = 1; k <= 5; k++)
                {
                    if (this.CanClick((Color[,])Grid.Clone(), k, i))
                    {
                        numArray2 = this.FindBestMove(Grid, k, i);
                        if (this.CompareGrid(numArray2, gridB))
                        {
                            gridB = numArray2;
                            point = new Point(k, i);
                            num = 1;
                            flag = true;
                        }
                    }
                }
            }
            for (int j = 1; j <= 12; j++)
            {
                for (int m = 1; m <= 5; m++)
                {
                    for (int n = 1; n <= 12; n++)
                    {
                        for (int num7 = 1; num7 <= 5; num7++)
                        {
                            if (this.CanClick((Color[,])Grid.Clone(), m, j))
                            {
                                numArray2 = this.FindBestMove(this.ClickGrid((Color[,])Grid.Clone(), m, j), num7, n);
                                if ((gridB.Length == 0) && (numArray2.Length >= 1))
                                {
                                    gridB = numArray2;
                                    point = new Point(m, j);
                                    num = 2;
                                    flag = true;
                                }
                                else if (num == 1)
                                {
                                    if (numArray2.Length > gridB.Length)
                                    {
                                        gridB = numArray2;
                                        point = new Point(m, j);
                                        num = 2;
                                        flag = true;
                                    }
                                }
                                else if ((num == 2) && this.CompareGrid(numArray2, gridB))
                                {
                                    gridB = numArray2;
                                    point = new Point(m, j);
                                    num = 2;
                                    flag = true;
                                }
                            }
                        }
                    }
                }
            }
            if (!flag)
            {
                point = new Point(random.Next(1, 6), random.Next(1, 12));
            }
            return point;
        }

        public void ClickBox(IntPtr hwnd, int left, int top)
        {
            Form1.left = left;
            Form1.top = top;
            if (bot == true || Painter.bot == true)
            {
                SetMouse(hwnd, 0x72 + (0x2d * (left - 1)), 0x61 + (0x2d * (top - 1)));
                clickMouse();
            }
        }

        public void SetMouse(IntPtr hwnd, int x, int y)
        {
            RECT rect = new RECT();
            GetWindowRect(hwnd, out rect);
            SmoothMouse(new Point(x + rect.X1, y + rect.Y1), 300 + Random(-5, 5));
        }

        public void clickMouse()
        {
            long l = 0;
            System.Threading.Thread.Sleep(Random(0, 359));
            mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            System.Threading.Thread.Sleep(Random(0, 359));
            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
        }

        public bool ClickFish(IntPtr hwnd, Color[,] Grid)
        {
            for (int i = 1; i <= 12; i++)
            {
                for (int j = 1; j <= 6; j++)
                {
                    if (IsFish((Grid[j - 1, i - 1])) && CanClick(Grid, j, i))
                    {
                        ClickBox(hwnd, j, i);
                        return true;
                    }
                }
            }
            return false;
        }

        public Color[,] ClickGrid(Color[,] Grid, int left, int top)
        {
            Color[,] colorArray = (Color[,])Grid.Clone();
            Color color = (Grid[left - 1, top - 1]);
            Color color2 = (Grid[left, top - 1]);
            if (((!this.IsCrab(color) && !this.IsJelly(color)) && (!this.IsFish(color) && !this.IsCrab(color2))) && (!this.IsJelly(color2) && !this.IsFish(color2)))
            {
                (colorArray[left - 1, top - 1]) = color2;
                (colorArray[left, top - 1]) = color;
            }
            return colorArray;
        }

        public bool CompareGrid(int[] GridA, int[] GridB)
        {
            int num = 0;
            int num2 = 0;
            if (GridA.Length > GridB.Length)
            {
                return true;
            }
            if (GridB.Length > GridA.Length)
            {
                return false;
            }
            foreach (int num3 in GridA)
            {
                num += num3;
            }
            foreach (int num4 in GridB)
            {
                num2 += num4;
            }
            return ((num2 > num) && false);
        }

        public Color GetBoxColor(Bitmap bitmap, int left, int top)
        {
            if (((left > 7) || (top > 12)) || ((left < 0) || (top < 0)))
            {
                return new Color();
            }
            Color pixel = bitmap.GetPixel((0x75 + (0x2d * (left - 1))) - 3, (0x5b + (0x2d * (top - 1))) - 0x17);
            if (((pixel.R == 40) && (pixel.G == 0x81)) && (pixel.B == 0xc4))
            {
                return Color.FromArgb(0xff, 0x63, 0xd3, 0xf7);
            }
            if (((pixel.R == 0x4f) && (pixel.G == 0x8b)) && (pixel.B == 0xbc))
            {
                return Color.FromArgb(0xff, 0xc6, 0xeb, 0xe4);
            }
            if (((pixel.R == 8) && (pixel.G == 0x4b)) && (pixel.B == 0xb6))
            {
                return Color.FromArgb(0xff, 20, 0x4c, 0xd4);
            }
            if (((pixel.R == 0x1c) && (pixel.G == 0x71)) && (pixel.B == 0xbf))
            {
                return Color.FromArgb(0xff, 0x45, 170, 0xeb);
            }
            if (((pixel.R == 0x2c) && (pixel.G == 0x79)) && (pixel.B == 0xa5))
            {
                return Color.FromArgb(0xff, 0x6f, 0xbd, 0xab);
            }
            if (((pixel.R == 2) && (pixel.G == 0x85)) && (pixel.B == 0xb3))
            {
                return Color.FromArgb(0xff, 4, 220, 0xcc);
            }
            if (((pixel.R == 2) && (pixel.G == 0x5e)) && (pixel.B == 0xc3))
            {
                return Color.FromArgb(0xff, 4, 0x7b, 0xf4);
            }
            return pixel;
        }

        public int PuzzlePiratesBorderHeight(Bitmap bitmap)
        {
            return ((bitmap.Height - 600) - (this.PuzzlePiratesBorderWidth(bitmap) * 2));
        }

        public int PuzzlePiratesBorderHeight(IntPtr hwnd)
        {
            RECT rect;
            GetWindowRect(hwnd, out rect);
            return (((rect.Y2 - rect.Y1) - 600) - (this.PuzzlePiratesBorderWidth(hwnd) * 2));
        }

        public int PuzzlePiratesBorderWidth(Bitmap bitmap)
        {
            return ((bitmap.Width - 800) / 2);
        }

        public int PuzzlePiratesBorderWidth(IntPtr hwnd)
        {
            RECT rect;
            GetWindowRect(hwnd, out rect);
            return (((rect.X2 - rect.X1) - 800) / 2);
        }

        public bool CanClick(Color[,] Grid, int left, int top)
        {
            int num = 1;
            if (left == 6)
            {
                num = -1;
            }
            if (this.IsCrab((Grid[left - 1, top - 1])) || this.IsCrab((Grid[(left + num) - 1, top - 1])))
            {
                return false;
            }
            if (this.IsFish((Grid[left - 1, top - 1])) && this.IsFish((Grid[(left + num) - 1, top - 1])))
            {
                return false;
            }
            if (this.IsJelly((Grid[left - 1, top - 1])) && this.IsJelly((Grid[(left + num) - 1, top - 1])))
            {
                return false;
            }
            return true;
        }

        public bool IsBlack(Color color)
        {
            return (((color.R == 0) && (color.G == 0)) && (color.B == 0));
        }

        public bool IsCrab(Color color)
        {
            return (((color.R == 0x66) && (color.G == 0x93)) && (color.B == 0xc7));
        }

        public bool IsFish(Color color)
        {
            return ((((color.R == 0xf8) && (color.G == 0xf8)) && (color.B == 0x97)) || (((color.R == 0x63) && (color.G == 0x90)) && (color.B == 0x9d)));
        }

        public bool IsJelly(Color color)
        {
            return ((((color.R == 0) && (color.G == 0xff)) && (color.B == 0xe8)) || (((color.R == 0) && (color.G == 0x93)) && (color.B == 190)));
        }

        public bool IsObject(Color color)
        {
            return (((this.IsFish(color) || this.IsJelly(color)) || this.IsCrab(color)) || ((((color.R == 0x63) && (color.G == 0xd3)) && (color.B == 0xf7)) || ((((color.R == 0xc6) && (color.G == 0xeb)) && (color.B == 0xe4)) || ((((color.R == 20) && (color.G == 0x4c)) && (color.B == 0xd4)) || ((((color.R == 0x45) && (color.G == 170)) && (color.B == 0xeb)) || ((((color.R == 0x6f) && (color.G == 0xbd)) && (color.B == 0xab)) || ((((color.R == 4) && (color.G == 220)) && (color.B == 0xcc)) || (((color.R == 4) && (color.G == 0x7b)) && (color.B == 0xf4)))))))));
        }

        public Color GetBoxColor(Bitmap bitmap, int left, int top, bool spec)
        {
            if (((left > 7) || (top > 12)) || ((left < 0) || (top < 0)))
            {
                return new Color();
            }
            Color pixel = bitmap.GetPixel(((0x75 + (0x2d * (left - 1))) - PuzzlePiratesBorderWidth(bitmap)) - 3, (((0x5b + (0x2d * (top - 1))) - PuzzlePiratesBorderHeight(bitmap)) - SystemInformation.CaptionHeight) - 0x17);
            if (!spec)
            {
                if (((pixel.R == 40) && (pixel.G == 0x81)) && (pixel.B == 0xc4))
                {
                    return Color.FromArgb(0xff, 0x63, 0xd3, 0xf7);
                }
                if (((pixel.R == 0x4f) && (pixel.G == 0x8b)) && (pixel.B == 0xbc))
                {
                    return Color.FromArgb(0xff, 0xc6, 0xeb, 0xe4);
                }
                if (((pixel.R == 8) && (pixel.G == 0x4b)) && (pixel.B == 0xb6))
                {
                    return Color.FromArgb(0xff, 20, 0x4c, 0xd4);
                }
                if (((pixel.R == 0x1c) && (pixel.G == 0x71)) && (pixel.B == 0xbf))
                {
                    return Color.FromArgb(0xff, 0x45, 170, 0xeb);
                }
                if (((pixel.R == 0x2c) && (pixel.G == 0x79)) && (pixel.B == 0xa5))
                {
                    return Color.FromArgb(0xff, 0x6f, 0xbd, 0xab);
                }
                if (((pixel.R == 2) && (pixel.G == 0x85)) && (pixel.B == 0xb3))
                {
                    return Color.FromArgb(0xff, 4, 220, 0xcc);
                }
                if (((pixel.R == 2) && (pixel.G == 0x5e)) && (pixel.B == 0xc2))
                {
                    return Color.FromArgb(0xff, 4, 0x7b, 0x90);
                }
            }
            return pixel;
        }

        public int GetWaterLevel(Color[,] Grid)
        {
            for (int i = 1; i <= 12; i++)
            {
                Color color = (Grid[0, i - 1]);
                if (((color.R == 40) && (color.G == 0x81)) && (color.B == 0xc4))
                {
                    return i;
                }
                if (((color.R == 0x2c) && (color.G == 0x79)) && (color.B == 0xa5))
                {
                    return i;
                }
                if (((color.R == 0x4f) && (color.G == 0x8b)) && (color.B == 0xbc))
                {
                    return i;
                }
                if (((color.R == 8) && (color.G == 0x4b)) && (color.B == 0xb6))
                {
                    return i;
                }
                if (((color.R == 0x1c) && (color.G == 0x71)) && (color.B == 0xbf))
                {
                    return i;
                }
                if (((color.R == 2) && (color.G == 0x85)) && (color.B == 0xb3))
                {
                    return i;
                }
                if (((color.R == 2) && (color.G == 0x5e)) && (color.B == 0xc2))
                {
                    return i;
                }
                if (((color.R == 0) && (color.G == 0x93)) && (color.B == 190))
                {
                    return i;
                }
                if (((color.R == 0x66) && (color.G == 0x93)) && (color.B == 0xc7))
                {
                    return i;
                }
                if (((color.R == 0x63) && (color.G == 0x90)) && (color.B == 0x9d))
                {
                    return i;
                }
            }
            return 12;
        }

        public Color[,] MakeGrid(Bitmap bmpClient)
        {
            Color[,] colorArray = new Color[6, 12];
            for (int i = 1; i <= 12; i++)
            {
                for (int j = 1; j <= 6; j++)
                {
                    (colorArray[j - 1, i - 1]) = this.GetBoxColor(bmpClient, j, i);
                }
            }
            return colorArray;
        }

        public Color[,] MakeGrid(Bitmap bmpClient, bool spec)
        {
            Color[,] colorArray = new Color[6, 12];
            for (int i = 1; i <= 12; i++)
            {
                for (int j = 1; j <= 6; j++)
                {
                    if (!spec)
                    {
                        (colorArray[j - 1, i - 1]) = this.GetBoxColor(bmpClient, j, i);
                    }
                    if (spec)
                    {
                        (colorArray[j - 1, i - 1]) = this.GetBoxColor(bmpClient, j, i, true);
                    }
                }
            }
            return colorArray;
        }

        public IntPtr GETDC()
        {
            return GetDC(IntPtr.Zero);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = true;
            painter = new Thread(new ThreadStart(P.Paint));
            painter.Start();
        }


        public void SmoothMouse(Point newPosition, int steps)
        {
            System.Threading.Thread.Sleep(200 + Random(0, 100));
            newPosition = new Point(newPosition.X + Random(0, 45), newPosition.Y + Random(-25, 0));
            Point start = System.Windows.Forms.Cursor.Position;
            PointF iterPoint = start;
            PointF slope = new PointF(newPosition.X - start.X, newPosition.Y - start.Y);
            int closeness = 0;
            closeness = (start.X + start.Y) - (newPosition.X + newPosition.Y);
            if (closeness <= 55 && closeness >= -55)
            {
                steps -= 108 + Random(-3, 3);
            }
            slope.X = slope.X / steps;
            slope.Y = slope.Y / steps;
            for (int i = 0; i <= steps - 1; i++)
            {
                iterPoint = new PointF(iterPoint.X + slope.X, iterPoint.Y + slope.Y);
                System.Windows.Forms.Cursor.Position = (Point.Round(iterPoint));
                System.Threading.Thread.Sleep(1 + Random(0, 3));
            }
            System.Windows.Forms.Cursor.Position = newPosition;
        }

        private int Random(int min, int max)
        {
            Random rand = new Random();
            return rand.Next(min, max);
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int X1;
            public int Y1;
            public int X2;
            public int Y2;
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

    }

    class Painter
    {
        public static bool bot = false;
        public void Paint()
        {
            Form1 f = new Form1();
            Process[] processlist = Process.GetProcesses();
            String[] process = null;
            process = new string[200];
            int i = 0;
            IntPtr handle = IntPtr.Zero;
            foreach (Process theprocess in processlist)
            {
                process[i] = theprocess.MainWindowTitle;
                if (process[i].Contains("Puzzle Pirates") || process[i].Contains("Puzzle Piraten"))
                {
                    handle = theprocess.MainWindowHandle;
                    break;
                }
                i++;
            }
            Form1.RECT r;
            Form1.GetWindowRect(handle, out r);
            while (Form1.Go == true)
            {
                Thread.Sleep(200);
                f.SimulateMove(handle);
            }
        }
    };
}