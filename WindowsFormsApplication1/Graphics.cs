using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
namespace WindowsFormsApplication1
{
    class ColorImage
    {
        public UInt32[] buf;
        public int w;
        public int h;
        public ColorImage(Bitmap bi)
        {
            this.w = bi.Width;
            this.h = bi.Height;
            BitmapData bd = bi.LockBits(new Rectangle(0, 0, bi.Width, bi.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            this.buf = new UInt32[bi.Width * bi.Height];
            for (int i = 0; i < this.buf.Length; i++)
            {
                this.buf[i] = (UInt32)System.Runtime.InteropServices.Marshal.ReadInt32(bd.Scan0, i * 4);
            }
            bi.UnlockBits(bd);
            return;
        }
    }
    class GsImage
    {
        public UInt32[] buf;
        public int w;
        public int h;
        public GsImage(Bitmap bi)
        {
            this.w = bi.Width;
            this.h = bi.Height;
            BitmapData bd = bi.LockBits(new Rectangle(0, 0, bi.Width, bi.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            this.buf = new UInt32[bi.Width * bi.Height];
            for (int i = 0; i < this.buf.Length; i++)
            {
                int t=System.Runtime.InteropServices.Marshal.ReadInt32(bd.Scan0, i * 4);
                this.buf[i] = (UInt32)((((t >> 16) & 0xff) + ((t >> 8) & 0xff) + ((t >> 0) & 0xff))/3);
            }
            bi.UnlockBits(bd);
            return;
        }
    }


    class PatchMatcher
    {
 
        private ColorImage _patch;
        public PatchMatcher(Bitmap i_patch)
        {
            this._patch = new ColorImage(i_patch);
        }
        public PatchMatcher(String i_fname):this(new Bitmap(i_fname))
        {
        }
        private static int getScore(ColorImage i_s, int tx, int ty, ColorImage i_patch)
        {
            int sum = 0;
            for (int y = 0; y < i_patch.h; y++)
            {
                for (int x = 0; x < i_patch.w; x++)
                {
                    UInt32 ps = i_s.buf[i_s.w * (y+ty) + x+tx];
                    UInt32 pp = i_patch.buf[i_patch.w * y + x];
                    int rs = (int)(((ps >> 0) & 0xff) - ((pp >> 0) & 0xff));
                    int gs = (int)(((ps >> 8) & 0xff) - ((pp >> 8) & 0xff));
                    int bs = (int)(((ps >>16) & 0xff) - ((pp >>16) & 0xff));
                    sum += (int)Math.Sqrt(rs * rs + gs * gs + bs * bs);
                }
            }
            return sum;
        }
        public class MatchResult
        {
            public int x;
            public int y;
            public int score;
            public int score_max;
            public MatchResult(int i_x, int i_y, int i_s,int i_m)
            {
                this.set(i_x, i_y, i_s);
                this.score_max = i_m;
            }
            public void set(int i_x, int i_y, int i_s)
            {
                this.x=i_x;
                this.y=i_y;
                this.score=i_s;
            }
        }
        public MatchResult matching(ColorImage i_src)
        {
            int cw = i_src.w - this._patch.w;
            int ch = i_src.h - this._patch.h;
            MatchResult ret = new MatchResult(0, 0, int.MaxValue, (int)(this._patch.w * this._patch.h * Math.Sqrt(255 * 255 * 3)));
            for (int sy = 0; sy <= ch; sy++)
            {
                for (int sx = 0; sx <= cw; sx++)
                {
                    int r = getScore(i_src, sx, sy, this._patch);
                    if (r < ret.score)
                    {
                        ret.set(sx, sy, r);
                    }
                }
            }
            return ret;

        }
        public MatchResult matching(Bitmap i_src)
        {
            return this.matching(new ColorImage(i_src));            
        }
    }
}
