using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace BMP2JPG_Tool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }
        private string _path = "";
        private bool isHave = false;
        private ArrayList files = new ArrayList();
        private int Zi = 1;
        private void Form1_Load(object sender, EventArgs e)
        {
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;

            // Set up the ToolTip text for the Button and Checkbox.
            toolTip1.SetToolTip(this.listBox1, "将要转换的文件拖放到这里");
            SetCtrlDragEvent(this.listBox1);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!isHave)
            {
                listBox1.Items.Clear();
                listBox2.Items.Clear();
                listBox1.Items.Add("拖动");
                listBox1.Items.Add("文件");
                listBox1.Items.Add("到此");
                listBox1.Items.Add("处");
                return;
            }
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + DateTime.Now.ToString("hhmmss_MMdd");
           
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                other2jpg(_path + "\\" + listBox1.Items[i].ToString().ToLower(), folder);
                listBox2.Items.Add(string.Format("{0:000}", i + 1).ToString() + ".jpg");
            }
            isHave = false;
        }
        public void bmp2jpg(string st1, string st2)
        {
            ImageCodecInfo[] vImageCodecInfos = ImageCodecInfo.GetImageEncoders();
            Bitmap vBitmap = new Bitmap(st1);

            foreach (ImageCodecInfo vImageCodecInfo in vImageCodecInfos)
            {
                if (vImageCodecInfo.FormatDescription.ToLower() == "jpeg")
                {
                    EncoderParameters vEncoderParameters = new EncoderParameters(1);
                    vEncoderParameters.Param[0] = new EncoderParameter(
                        System.Drawing.Imaging.Encoder.Quality, 75L);
                    vBitmap.Save(st2, vImageCodecInfo, vEncoderParameters);
                    break;
                }
            }
        }
        private void jpg2jpg(string str1, string folder, string filename)
        {

            if (!Directory.Exists(folder))
            {
                try { Directory.CreateDirectory(folder); }
                catch(Exception ex)
                {
                    MessageBox.Show("error:"+ex.Message);
                }

            }
            FileInfo fileInfo = new FileInfo(str1);
            double size = (double)fileInfo.Length;
            int rat = Convert.ToInt32(1972864.0 / ((double)size) * 100);
            // MessageBox.Show(rat.ToString());
            if (rat < 100)
            {
                GetPicThumbnail(str1, folder + "\\" + filename, rat);
            }
            if (rat > 100) { File.Copy(str1, folder + "\\" + filename); }
        }
        private void other2jpg(string str1, string folder)
        {

            switch (str1.Substring(str1.Length - 4, 4))
            {
                case ".jpg":
                    jpg2jpg(str1, folder, string.Format("{0:000}", Zi) + ".jpg");
                    //jpg2jpg(str1, folder, Path.GetFileName(str1));
                    Zi++;
                    break;
                case ".bmp":
                    string tmp = Path.GetTempFileName() + ".jpg";
                    bmp2jpg(str1, tmp);
                    jpg2jpg(tmp, folder, string.Format("{0:000}", Zi) + ".jpg");
                    // jpg2jpg(tmp, folder, Path.GetFileNameWithoutExtension(str1) + ".jpg");
                    Zi++;
                    break;
            }

        }
        public void SetCtrlDragEvent(Control ctrl)
        {
            if (ctrl is ListBox)
            {
                ListBox tb = ctrl as ListBox;
                tb.AllowDrop = true;
                tb.DragEnter += (sender, e) =>
                {
                    e.Effect = DragDropEffects.Link;//拖动时的图标
                };
                tb.DragDrop += (sender, e) =>
                {

                    listBox1.Items.Clear();
                    files.Clear();
                    isHave = true;
                    String[] fileNames = (String[])e.Data.GetData(DataFormats.FileDrop);
                    // int j = ((ListBox)sender).Items.Count + 1;
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        if (Path.GetExtension(fileNames[i].ToLower()) == ".jpg" || Path.GetExtension(fileNames[i].ToLower()) == ".bmp")
                        {
                            files.Add(fileNames[i]);
                            // ((ListBox)sender).Items.Add(System.IO.Path.GetFileName(fileNames[i]));
                        }
                    }
                    IComparer dateCompare = new DateCompare();
                    files.Sort(dateCompare);
                    foreach (string s in files)
                    {
                        listBox1.Items.Add(Path.GetFileName(s));
                    }

                    _path = System.IO.Path.GetDirectoryName(fileNames[0]);
                };
            }
        }
        public bool GetPicThumbnail(string sFile, string dFile, int flag)
        {

            System.Drawing.Image iSource = System.Drawing.Image.FromFile(sFile);
            ImageFormat tFormat = iSource.RawFormat;
            int sW = 0, sH = 0;
            //按比例缩放
            int dHeight = iSource.Height;
            int dWidth = iSource.Width;
            Size tem_size = new Size(iSource.Width, iSource.Height);
            if (tem_size.Width > dHeight || tem_size.Width > dWidth) //将**改成c#中的或者操作符号
            {
                if ((tem_size.Width * dHeight) > (tem_size.Height * dWidth))
                {
                    sW = dWidth;
                    sH = (dWidth * tem_size.Height) / tem_size.Width;
                }
                else
                {
                    sH = dHeight;
                    sW = (tem_size.Width * dHeight) / tem_size.Height;
                }
            }

            else
            {
                sW = tem_size.Width;
                sH = tem_size.Height;
            }
            Bitmap ob = new Bitmap(dWidth, dHeight);
            Graphics g = Graphics.FromImage(ob);
            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);
            g.Dispose();
            //以下代码为保存图片时，设置压缩质量
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    ob.Save(dFile, jpegICIinfo, ep);//dFile是压缩后的新路径
                }
                else
                {
                    ob.Save(dFile, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                iSource.Dispose();
                ob.Dispose();
            }
        }
    }
    public class DateCompare : IComparer
    {
        int IComparer.Compare(Object x, Object y)
        {
            FileInfo fx = new FileInfo((string)x);
            FileInfo fy = new FileInfo((string)y);
            return fx.CreationTime.CompareTo(fy.CreationTime);
        }
    }
}
