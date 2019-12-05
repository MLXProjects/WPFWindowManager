using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FormsDraw = System.Drawing;

namespace WPFWinMgr
{
    public partial class WinCtl : Image
    {
        FormsDraw.Rectangle WinBounds;
        string Title = "";
        public WinCtl(IntPtr hwin, WinUtils.Rect rect)
        {
            //InitializeComponent();
            this.Tag = hwin;
            WinBounds = new FormsDraw.Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            Title = GetWindowTitle(hwin);
            this.Width = WinBounds.Width;
            this.Height = WinBounds.Height;
            this.Source = GetWindowImage(hwin, WinBounds);
            DispatcherTimer dt = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Render, new EventHandler(TimerTick), this.Dispatcher);
            dt.Start();
        }

        public void TimerTick(object sender, EventArgs e)
        {            
            WinUtils.Rect rect = new WinUtils.Rect();
            WinUtils.GetWindowRect((IntPtr)this.Tag, ref rect);
            WinBounds = new FormsDraw.Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            this.Width = WinBounds.Width; 
            this.Height = WinBounds.Height;
            this.Source = GetWindowImage((IntPtr)this.Tag, WinBounds);
        }

        public string GetWindowTitle(IntPtr hWnd)
        {
            StringBuilder strbTitle = new StringBuilder(255);
            int nLength = WinUtils.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
            return strbTitle.ToString();
        }

        public ImageSource GetWindowImage(IntPtr hWnd, FormsDraw.Rectangle bounds)
        {
            using (FormsDraw.Bitmap result = new FormsDraw.Bitmap(bounds.Width, bounds.Height))
            {//creates a new Bitmap which will contain the window's image
                using (FormsDraw.Graphics MemG = FormsDraw.Graphics.FromImage(result))
                {//makes a Graphics from the Bitmap whose HDC will hold the window's RAW image
                    IntPtr dc = MemG.GetHdc();//gets the Bitmap's HDC 
                    try
                    {
                        WinUtils.PrintWindow(hWnd, dc, 0);//Writes the window's image to the HDC created above
                    }
                    finally
                    {
                        MemG.ReleaseHdc(dc);
                    }
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(result.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }
            
        }

    }
}
