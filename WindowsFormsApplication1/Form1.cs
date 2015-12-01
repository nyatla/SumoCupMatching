using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {
        private System.Timers.Timer _keyTimer;
        private System.Timers.Timer _timer;
        public Form1()
        {
            InitializeComponent();
            this.Opacity = 0.8;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _keyTimer = new System.Timers.Timer();
            _keyTimer.Elapsed += new System.Timers.ElapsedEventHandler(onKey);
            _keyTimer.Interval = 16;
            // タイマーの生成
            _timer = new System.Timers.Timer();
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(onCapture);
            _timer.Interval = 33;

            return;
        }
        private void button2_Click(object sender, EventArgs e)
        {
        }
        int _state = 0;
        Point _capture_pos;

        private void button1_Click(object sender, EventArgs e)
        {
            this.label1.Visible = false;
            this.Opacity = 1;
            Rectangle t = this.label1.RectangleToScreen(this.label1.ClientRectangle);
            _capture_pos = new Point(t.Left,t.Top);
            tmpbmp = new Bitmap(t.Width,t.Height);



            this._state++;
            _timer.Start();
            _keyTimer.Start();
        }
        //Bitmapの作成
        Bitmap tmpbmp;
        Bitmap sampling = new Bitmap(40, 70);
        Bitmap ui_sampling = new Bitmap(40, 70);
        public void onCapture(object sender, ElapsedEventArgs e)
        {
            if (_state < 1)
            {
                return;
            }
            Bitmap bmp = this.tmpbmp;
            lock(this){
                if (bmp == null)
                {
                    return;
                }
                //Graphicsの作成
                Graphics g = Graphics.FromImage(bmp);
                //画面全体をコピーする
                g.CopyFromScreen(_capture_pos, new Point(0, 0), bmp.Size);
                g.Dispose();
                try
                {
                    //Formクローズ時に落ちる。めんどくさいkら握りつぶす
                    Invoke(new updateImg(method), new Object[] { bmp });
                }
                catch (Exception ee)
                {
                    this._timer.Stop();
                }
 
            }
        }
        
        public void method(Bitmap bmp){
            if (bmp == null)
            {
                return;
            }
            Graphics g = Graphics.FromImage(sampling);
            g.DrawImage(bmp, 0, 0, sampling.Width, sampling.Height);
            test(sampling);
            g.Dispose();

            this.pictureBox2.Image = sampling;
        }
        delegate void updateImg(Bitmap b);
        public void onKey(object sender, ElapsedEventArgs e)
        {
            switch (st.rep)
            {
                case PadState.Repeat.Blue:
                    for (int i = 0; i < 2; i++)
                    {
                        if (this.st.repeat_counter > 0) { this.st.repeat_counter--; SendKeys.SendWait("{Up}"); }

                    }
                    break;
                case PadState.Repeat.Green:
                    for (int i = 0; i < 2; i++)
                    {
                        if (this.st.repeat_counter > 0) { this.st.repeat_counter--; SendKeys.SendWait("{Down}"); }
                    }
                    break;
                case PadState.Repeat.Dual:
                    if (this.st.repeat_counter > 0) { this.st.repeat_counter--; SendKeys.SendWait("{Down}");}
                    if (this.st.repeat_counter > 0) { this.st.repeat_counter--; SendKeys.SendWait("{Up}"); }
                    break;

            }
        }
        public class PadState
        {
            public enum Repeat
            {
                None,
                Blue,   //青連打
                Green,  //緑連打
                Dual    //青緑交互
            };
            public bool b1 = false;
            public bool b2 = false;
            public bool g1 = false;
            public bool g2 = false;
            public bool r2 = false;
            public Repeat rep = Repeat.None;
            public int up_idx = 0;
            public int down_idx = 0;
            public int ud_idx=0;
            public int repeat_counter = 0;

        }



        public PadState st=new PadState();
        //連打パターン
        public static int[] rep_tbl = { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100};
        //単発条件
        public static bool isEnable(int ti)
        {
            return true;
        }
        public void test(Bitmap b)
        {
            ColorImage img = new ColorImage(b);
            PatchMatcher.MatchResult b1 = b1patt.matching(img);
            PatchMatcher.MatchResult b2 = b2patt.matching(img);
            PatchMatcher.MatchResult g1 = g1patt.matching(img);
            PatchMatcher.MatchResult g2 = g2patt.matching(img);
            PatchMatcher.MatchResult r1 = r2patt.matching(img);
            bool mkb1 = false, mkb2 = false, mkg1 = false, mkr2 = false,mkg2=false;
            if (b2.score < 9000)
            {
                mkb2 = true;    //B2検出
            }
            else if (b1.score < 8000)
            {
                mkb1 = true;    //B1検出
            }
            if (g2.score < 10000)
            {
                mkg2 = true;    //G2検出
            }
            else if (g1.score < 8000)
            {
                mkg1 = true;    //G1検出
            }
            if (r1.score < 8000)
            {
                mkr2 = true;    //R2検出
            }
//            System.Console.WriteLine(sw.ElapsedMilliseconds+"[ms]");            
            //アクションの決定
            if (!this.st.b1 && mkb1)
            {
                //新規B1
                System.Console.WriteLine("B1検出" + b1.score + ":" + b2.score + ":" + g1.score + ":" + g2.score + ":" + r1.score);
                int ti = (this.st.down_idx + this.st.up_idx);
                if (isEnable(ti))

                {
                     SendKeys.SendWait("{UP}");
               }
                this.st.rep = PadState.Repeat.None;
                this.st.up_idx++;
            }
            if (!this.st.b2 && mkb2)
            {
                //新規B2
                System.Console.WriteLine("B2検出" + b1.score + ":" + b2.score + ":" + g1.score + ":" + g2.score + ":" + r1.score);
                if (this.st.rep == PadState.Repeat.None)
                {
                    this.st.rep = PadState.Repeat.Blue;
                    this.st.repeat_counter = rep_tbl[this.st.ud_idx];
                    this.st.ud_idx++;
                }
                else
                {
                    this.st.rep = PadState.Repeat.None;
                }

            }
            if (!this.st.g1 && mkg1)
            {
                //新規G1
                System.Console.WriteLine("G1検出" + b1.score + ":" + b2.score + ":" + g1.score + ":" + g2.score + ":" + r1.score);
                int ti=(this.st.down_idx+this.st.up_idx);
                if (isEnable(ti))
                {
                    SendKeys.SendWait("{DOWN}");
                }
                this.st.rep = PadState.Repeat.None;
                this.st.down_idx++;
            }
            if (!this.st.g2 && mkg2)
            {
                //新規G2
                System.Console.WriteLine("G2検出" + b1.score + ":" + b2.score + ":" + g1.score + ":" + g2.score + ":" + r1.score);
                if (this.st.rep == PadState.Repeat.None)
                {
                    this.st.rep = PadState.Repeat.Green;
                    this.st.repeat_counter = rep_tbl[this.st.ud_idx];
                    this.st.ud_idx++;
                }
                else
                {
                    this.st.rep = PadState.Repeat.None;
                }
            }
            if (!this.st.r2 && mkr2)
            {
                //新規R2
                System.Console.WriteLine("R2検出");
                if (this.st.rep == PadState.Repeat.None)
                {
                    this.st.rep = PadState.Repeat.Dual;
                    this.st.repeat_counter = rep_tbl[this.st.ud_idx];
                    this.st.ud_idx++;
                }
                else
                {
                    this.st.rep = PadState.Repeat.None;
                }
            }

            //ステータスの決定
            this.st.b1 = mkb1;
            this.st.b2 = mkb2;
            this.st.g1 = mkg1;
            this.st.g2 = mkg2;
            this.st.r2 = mkr2;
        }
        static int m=0;
        private void savePatch(Bitmap b,String p)
        {
                b.Save(".\\test2\\"+p+m+".png");
                m++;
        }
        PatchMatcher b1patt = new PatchMatcher("../../patt/blue_single.png");
        PatchMatcher b2patt = new PatchMatcher("../../patt/blue_ren.png");
        PatchMatcher g1patt = new PatchMatcher("../../patt/green_single.png");
        PatchMatcher g2patt = new PatchMatcher("../../patt/green_ren.png");
        PatchMatcher r2patt = new PatchMatcher("../../patt/red.png");
        bool is_force_closed = false;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this._timer.Stop();
            return;

        }

       


    }
}
