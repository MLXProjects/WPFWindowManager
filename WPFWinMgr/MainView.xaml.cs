using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FormsDraw = System.Drawing;

namespace WPFWinMgr
{
    public partial class MainView : Window
    {
        public bool dragMode = false;
        public double FirstXPos, FirstYPos, FirstArrowXPos, FirstArrowYPos;
        IntPtr thishWnd;
        bool fullScreen = false;
        public MainView()
        {
            InitializeComponent();
        }

        public void MakeWindow(IntPtr hWnd)
        {
            Image window = new Image();
            window.Width = 240;
            window.Height = 240;
            window.Stretch = Stretch.Fill;
            WinUtils.Rect rect = new WinUtils.Rect();//makes a new Rect which will hold the target window size
            WinUtils.GetWindowRect(hWnd, ref rect);//gets the target window size
            FormsDraw.Rectangle bounds = new FormsDraw.Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);//converts the rect into something usable 
            FormsDraw.Bitmap result = new FormsDraw.Bitmap(bounds.Width, bounds.Height);//creates a new Bitmap which will contain the window's image
            FormsDraw.Graphics MemG = FormsDraw.Graphics.FromImage(result);//makes a Graphics from the Bitmap whose HDC will hold the window's RAW image
            IntPtr dc = MemG.GetHdc();//gets the Bitmap's HDC 
            WinUtils.PrintWindow(hWnd, dc, 0);//Writes the window's image to the HDC created above
            MemG.ReleaseHdc(dc);
            window.Source = WinUtils.ImageSourceFromBitmap(result);
            WinMgr.Children.Add(window);//adds the PB to the FlowLayoutPanel
        }

        public void ListWindows(bool addWins)
        {
            winList.Items.Clear();
            cv.Children.Clear();
            var hWnds = new List<IntPtr>();
            WinUtils.EnumDelegate filter = delegate(IntPtr hWnd, int lParam)
            {
                string[] m_astrFilter = new string[] { "Start", "Program Manager" };
                string title = WinUtils.GetWindowTitle(hWnd);
                if (WinUtils.IsWindowVisible(hWnd) && !string.IsNullOrEmpty(title) && !m_astrFilter.Contains(title) && hWnd != thishWnd)
                    hWnds.Add(hWnd);
                return true;
            };

            if (WinUtils.EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero))
            {
                foreach (var hWnd in hWnds)
                {
                    ComboBoxItem win = new ComboBoxItem();
                    win.Tag = hWnd;
                    win.Content = WinUtils.GetWindowTitle(hWnd);
                    winList.Items.Add(win);
                    if (addWins)
                        AddWindow(hWnd);
                }
                winList.SelectedIndex = 0;
            }
        }

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox1.Text))
                return;
            AddWindow((IntPtr)Int32.Parse(textBox1.Text));
        }

        public void AddWindow(IntPtr hWnd)
        {
            WinUtils.Rect rect = new WinUtils.Rect();
            WinUtils.GetWindowRect(hWnd, ref rect);
            WinCtl newWindow = new WinCtl(hWnd, rect);
            newWindow.MouseDown += newWindow_MouseDown;
            newWindow.MouseUp += newWindow_MouseUp;
            newWindow.MouseMove += newWindow_MouseMove;
            cv.Children.Add(newWindow);
        }

        void newWindow_MouseDown(object sender, MouseButtonEventArgs e)
        { 
            WinCtl win = sender as WinCtl;
            Point ex = Mouse.GetPosition(win);
            if (ex.X < (win.Width - 103) && ex.Y < 26 && e.LeftButton == MouseButtonState.Pressed)
            {
                dragMode = true;
                win.CaptureMouse();
                var cont = VisualTreeHelper.GetParent(win) as UIElement;
                FirstXPos = e.GetPosition(win).X;
                FirstYPos = e.GetPosition(win).Y;
                FirstArrowXPos = e.GetPosition(cont).X - FirstXPos;
                FirstArrowYPos = e.GetPosition(cont).Y - FirstYPos;
            }
        }

        void newWindow_MouseMove(object sender, MouseEventArgs e)
        {
            WinCtl win = sender as WinCtl;
            var cont = VisualTreeHelper.GetParent(win) as UIElement;
            if (dragMode)
            {
                win.SetValue(Canvas.LeftProperty,
                    e.GetPosition(cont).X - FirstXPos);

                win.SetValue(Canvas.TopProperty,
                     e.GetPosition(cont).Y - FirstYPos);
            }
        }

        void newWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            WinCtl win = sender as WinCtl;
            if (dragMode)
            {
                win.ReleaseMouseCapture();
                dragMode = false;
            }
            Point ex = Mouse.GetPosition(win);
            if (ex.Y <= 32)
            {
                if (ex.Y >= 6 && ex.Y <= 23)
                {
                    if (ex.X >= (win.Width - 103) && ex.X <= (win.Width - 73))
                    {
                        if (win.Width > 160 && win.Height > 26)
                            WinUtils.ShowWindow((IntPtr)win.Tag, 6);
                        else WinUtils.ShowWindow((IntPtr)win.Tag, 9);
                    }
                    else if (ex.X >= (win.Width - 70) && ex.X <= (win.Width - 40))
                    {
                    }
                    else if (ex.X >= (win.Width - 37) && ex.X <= (win.Width - 7))
                    {
                        cv.Children.Remove(win);
                    }
                }
            }
        }

        private void winMain_Loaded(object sender, RoutedEventArgs e)
        {
            thishWnd = new WindowInteropHelper(this).Handle;
            ListWindows(false);
            ToggleFullscreen();
        }

        private void bin2_Click(object sender, RoutedEventArgs e)
        {
            ListWindows(true);
        }

        private void winList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (winList.SelectedItem as ComboBoxItem != null)
            textBox1.Text = (winList.SelectedItem as ComboBoxItem).Tag.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ToggleFullscreen();
        }

        public void ToggleFullscreen()
        {
            if (fullScreen)
            {
                this.WindowState = System.Windows.WindowState.Normal;
                this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                fullScreen = false;
            }
            else
            {
                this.WindowStyle = System.Windows.WindowStyle.None;
                this.WindowState = System.Windows.WindowState.Maximized;
                fullScreen = true;
            }
        }
    }

    public class WinUtils
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        public struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public static ImageSource ImageSourceFromBitmap(FormsDraw.Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        public static string GetWindowTitle(IntPtr hWnd)
        {
            StringBuilder strbTitle = new StringBuilder(255);
            int nLength = WinUtils.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
            string strTitle = strbTitle.ToString();
            return strTitle;
        }

    }
}
