using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using FunctionDll;
using OpenCvSharp;
using System.IO;


namespace RolongoCapture
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("HisFX3Platform64D.dll", EntryPoint = "HisFX3OpenDevice")]
        //! 打开工装dev, 编号为boxIndex, 编号不能超过_RMAX_BOXS-1
        /*!
        \param[in] boxIndex 工装编号
        \param[in] dev 工装唯一序列号, 如果为NULL，则不比对序列号，随机打开一个工装
        \return 0:成功  非0:失败
        \sa HisFX3CloseDevice
        */
        public extern static int HisFX3OpenDevice(int boxIndex, string dev);
        //! 关闭工装
        /*!
        \param[in] boxIndex 工装编号, 如何为-1则关闭所有工装
        \return 0:成功  非0:失败
        \sa
        */
        [DllImport("HisFX3Platform64D.dll", EntryPoint = "HisFX3CloseDevice")]
        public extern static int HisFX3CloseDevice(int boxIndex);

        //! 获取工装数量
        /*! 
        \return 工装个数
        \sa HisFX3EnumDev
        */
        [DllImport("HisFX3Platform64D.dll", EntryPoint = "HisFX3GetDevCount")]
        public extern static int HisFX3GetDevCount();
        //! 枚举在线的工装设备，作用和HisFX3EnumDev一样，只是返回的格式不同
        /*! 
        \param[out] dev 工装唯一序列号列表，多个序列号之间用";"隔开；内存需要用户自己申请，建议不少于256Bytes;
        \return 0:成功  非0:失败
        \sa
        */
        [DllImport("HisFX3Platform64D.dll", EntryPoint = "HisFX3EnumDevStr")]
        public extern static int HisFX3EnumDevStr(IntPtr dev);

        //! 开始出图
        /*!
        \param[in] iniPath INI参数文件路径，路径名不能有中文字符
        \param[in] cam 模组编号
        \return 0:成功  非0:失败
        \sa 
        */
        [DllImport("HisFX3Platform64D.dll", EntryPoint = "HisFX3StartPreview_INI")]
        public extern static int HisFX3StartPreview_INI(string iniPath, int cam = 0);

        //! 停止出图
        /*!
        \param[in] sequence 下电时序，设置为NULL时，则采用默认下电时序
        \param[in] cam 模组编号
        \return 0:成功  非0:失败
        \sa
        */
        [DllImport("HisFX3Platform64D.dll", EntryPoint = "HisFX3StopPreview")]
        public extern static int HisFX3StopPreview(string sequence, int cam);

        //! 抓取一帧图像
        /*!
        \param[in] imageBuf 存取图像数据的内存指针
        \param[in] bufSize imageBuf大小， 单位：Byte
        \param[out] frameIndex 帧索引
        \param[out] errorFrame 是否为错误帧
        \param[out] recSize 收到的数据大小，单位：Byte
        \param[in] imageFormat 图像数据格式，如果和SENSOR格式不一致，会自动转化为imageFormat格式; 0表示和输入保持一致
        \param[in] timeout 等待时间
        \param[in] cam 模组编号
        \return 0:成功  非0:失败
        \sa
        */
        [DllImport("HisFX3Platform64D.dll", EntryPoint = "HisFX3GrabFrame")]
        public extern static int HisFX3GrabFrame(IntPtr imageBuf, int bufSize, ref uint frameIndex, ref bool errorFrame, ref uint recSize, uint imageFormat = 0, uint timeout = 2000, int cam = 0);

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                if (button1.Text == "打开设备")
                {
                    int result = HisFX3OpenDevice(listBox1.SelectedIndex, listBox1.SelectedItem.ToString());
                    if (result == 0)
                    {
                        MessageBox.Show("设备已开启");
                        button1.Text = "关闭设备";
                    }
                }
                else
                {
                    int result = HisFX3CloseDevice(listBox1.SelectedIndex);
                    if (result == 0)
                    {
                        MessageBox.Show("设备已关闭");
                        button1.Text = "打开设备";
                    }
                }
            }
        }

        public bool OpenOrCloseDevice(int boxIndex, string dev, bool kaiguan)
        {
            if (kaiguan)
            {
                int result = HisFX3OpenDevice(boxIndex, dev);
                if (result == 0)
                    return true;
                else
                    return false;
            }
            else
            {
                int result = HisFX3CloseDevice(boxIndex);
                if (result == 0)
                    return true;
                else
                    return false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //声明一个至少256bytes的内存空间
            IntPtr x = Marshal.AllocHGlobal(4096);
            int n = 0;
            if (HisFX3EnumDevStr(x) == 0)
            {
                string xx = Marshal.PtrToStringAnsi(x);
                Marshal.FreeHGlobal(x);
                listBox1.Items.AddRange(xx.Split(','));
            }
        }

        public string GetDevices()
        { 
            //声明一个至少256bytes的内存空间
            IntPtr x = Marshal.AllocHGlobal(4096);
            int n = 0;
            if (HisFX3EnumDevStr(x) == 0)
            {
                string xx = Marshal.PtrToStringAnsi(x);
                Marshal.FreeHGlobal(x);
                return xx;
            }
            return "";
        }

        public unsafe IntPtr AlignedPointer(int size, uint align)
        {
            void* raw_malloc_ptr;
            void* aligned_ptr;
            IntPtr ptr = Marshal.AllocHGlobal((int)(size + align));
            raw_malloc_ptr = ptr.ToPointer();
            aligned_ptr = (void*)(((UInt64)raw_malloc_ptr + align) & ~(align - 1));
            ((void**)aligned_ptr)[-1] = raw_malloc_ptr;
            return (IntPtr)aligned_ptr;
        }
        public unsafe void AlignedPointer2(int size, uint align,ref IntPtr buff)
        {
            void* raw_malloc_ptr;
            void* aligned_ptr;
            IntPtr ptr = Marshal.AllocHGlobal((int)(size + align));
            raw_malloc_ptr = ptr.ToPointer();
            aligned_ptr = (void*)(((UInt64)raw_malloc_ptr + align) & ~(align - 1));
            ((void**)aligned_ptr)[-1] = raw_malloc_ptr;
            buff = (IntPtr)aligned_ptr;
        }

        #region 图像格式
        private const uint HisBaylor8_BGGR = 0x01;
        private const uint HisBaylor8_RGGB = 0x02;
        private const uint HisBaylor8_GRBG = 0x03;
        private const uint HisBaylor8_GBRG = 0x04;
        private const uint HisBaylor8_MONO = 0x07;
        private const uint HisBaylor8_END = 0x0F;
        private const uint HisBaylor10_BGGR = 0x11;
        private const uint HisBaylor10_RGGB = 0x12;
        private const uint HisBaylor10_GRBG = 0x13;
        private const uint HisBaylor10_GBRG = 0x14;
        private const uint HisBaylor10_MONO = 0x17;
        private const uint HisBaylor10_END = 0x1F;
        private const uint HisBaylor12_BGGR = 0x41;
        private const uint HisBaylor12_RGGB = 0x42;
        private const uint HisBaylor12_GRBG = 0x43;
        private const uint HisBaylor12_GBRG = 0x44;
        private const uint HisBaylor12_MONO = 0x47;
        private const uint HisBaylor12_END = 0x4F;
        private const uint HisBaylor14_BGGR = 0x51;
        private const uint HisBaylor14_RGGB = 0x52;
        private const uint HisBaylor14_GRBG = 0x53;
        private const uint HisBaylor14_GBRG = 0x54;
        private const uint HisBaylor14_MONO = 0x57;
        private const uint HisBaylor14_END = 0x5F;
        private const uint HisBaylor16_BGGR = 0x61;
        private const uint HisBaylor16_RGGB = 0x62;
        private const uint HisBaylor16_GRBG = 0x63;
        private const uint HisBaylor16_GBRG = 0x64;
        private const uint HisBaylor16_MONO = 0x67;
        private const uint HisBaylor16_END = 0x6F;
        private const uint HisBaylor_Compact = 0x80000000; /*! 数据排列变成高位在前，低位合并为1个Byte */
        private const uint HisYUV8_422_YUYV = 0x21; //YUYV
        private const uint HisYUV8_422_UYVY = 0x22;
        private const uint HisYUV8_422_YVYU = 0x23;
        private const uint HisYUV8_422_VYUY = 0x24;
        private const uint HisYUV8_422P = 0x25;
        private const uint HisYUV8_420P = 0x26; // 不支持 裁边，ROI上传
        private const uint HisRGB_RGB24 = 0x30;
        private const uint HisRGB_BGR24 = 0x31;
        private const uint HisGray_Y8 = 0x71;
        private const uint HisRGB_Bilinear = 0x10000000; /*! 双线性差值算法，和private const uint HisRGB_RGB24; private const uint HisRGB_BGR24一起使用，eg: private const uint HisRGB_RGB24 | private const uint HisRGB_Bilinear*/
        private const uint HisOutputAddInfo = 0x20000000;
        #endregion

        private object obj = new object();

        private object timerlock = new object();
        Thread oThread;
        private void button3_Click(object sender, EventArgs e)
        {
            if (button3.Text == "开始出图")
            {
                lock (obj)
                {
                    int result = HisFX3StartPreview_INI("E:\\NIO_max9296_max96717_1442.ini", 0);
                    if (result == 0)
                    {
                        MessageBox.Show("出图成功！");
                        button3.Text = "关闭出图";
                        oThread = new Thread(new ThreadStart(Display));
                        oThread.IsBackground = true;
                        oThread.Start();
                        //IntPtr Imagebuf = AlignedPointer(1280 * 800 * 3, 64);                //使用图像格式：插值后的RGB24 1Pixel占3Byte
                        //uint frameIndex = 0;
                        //bool errorFrame = true;
                        //uint recSize = 0;
                        //int res = HisFX3GrabFrame(Imagebuf, 1280 * 800 * 3, ref frameIndex, ref errorFrame, ref recSize, HisRGB_RGB24 | HisRGB_Bilinear);
                        //byte[] image = new byte[1280 * 800 * 3];
                        //Marshal.Copy(Imagebuf, image, 0, 1280 * 800 * 3);
                        //Marshal.FreeHGlobal(Imagebuf);
                        //pictureBox1.Image = RGBTOBITMAP(image, 1280, 800);
                    }
                }
            }
            else
            {
                int result = HisFX3StopPreview(null, 0);
                if (result == 0)
                {
                    oThread.Abort();
                    timer1.Enabled = false;
                    MessageBox.Show("出图关闭！");
                    button3.Text = "开始出图";
                }
            }
        }

        public Image RGBTOBITMAP(byte[] buffer, int width, int height)
        {
            /*****下面的就是将RGB数据转换为BITMAP对象******/
            int stride = width * 3;
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            int scan0 = (int)handle.AddrOfPinnedObject();
            scan0 += (height - 1) * stride;
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height, -stride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, (IntPtr)scan0);
            handle.Free();
            return Image.FromHbitmap(bitmap.GetHbitmap());
        }

        private void Display()
        {
            IntPtr Imagebuf = AlignedPointer(1280 * 800 * 3, 64);                //使用图像格式：插值后的RGB24 1Pixel占3Byte
            uint frameIndex = 0;
            bool errorFrame = true;
            uint recSize = 0;
            while (true)
            {
                lock (timerlock)
                {

                    int res = HisFX3GrabFrame(Imagebuf, 1280 * 800 * 3, ref frameIndex, ref errorFrame, ref recSize, HisRGB_RGB24 | HisRGB_Bilinear);
                    if (res == 0)
                    {
                        byte[] image = new byte[1280 * 800 * 3];
                        Marshal.Copy(Imagebuf, image, 0, 1280 * 800 * 3);
                        pictureBox1.Image = RGBTOBITMAP(image, 1280, 800);
                        this.Invoke(new Action(() =>
                        {
                            pictureBox1.Show();
                            pictureBox1.Refresh();
                        }));
                        //Marshal.FreeHGlobal(Imagebuf);
                    }
                }
                Thread.Sleep(1);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int result = HisFX3StartPreview_INI("E:\\NIO_max9296_max96717_1442.ini", 0);
            if (result == 0)
            {
                IntPtr Imagebuf = AlignedPointer(1280 * 800 * 3, 64);                //使用图像格式：插值后的RGB24 1Pixel占3Byte
                uint frameIndex = 0;
                bool errorFrame = true;
                uint recSize = 0;

                int res = HisFX3GrabFrame(Imagebuf, 1280 * 800 * 3, ref frameIndex, ref errorFrame, ref recSize, HisRGB_RGB24 | HisRGB_Bilinear);
                if (res == 0 && errorFrame == false)
                {
                    byte[] image = new byte[1280 * 800 * 3];
                    Marshal.Copy(Imagebuf, image, 0, 1280 * 800 * 3);
                    pictureBox1.Image = RGBTOBITMAP(image, 1280, 800);
                    pictureBox1.Show();
                    pictureBox1.Refresh();
                    MessageBox.Show("出图成功！");
                    Bitmap bmp = (Bitmap)pictureBox1.Image;
                    string path = Application.StartupPath + "\\Reference.bmp";
                    if (File.Exists(path))
                        File.Delete(path);
                    else
                        bmp.Save(Application.StartupPath + "\\Reference.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                }
            }
        }

        public Bitmap CaptureImage(string iniPath, int boxIndex)
        {
            int width = int.Parse(optionFunction.ReadIniData("Image_Sensor", "Image_Width", "", iniPath));
            int height = int.Parse(optionFunction.ReadIniData("Image_Sensor", "Image_Height", "", iniPath));
            int result = HisFX3StartPreview_INI(iniPath, boxIndex);
            if (result == 0)
            {
                IntPtr Imagebuf = AlignedPointer(width * height * 3, 64);//使用图像格式：插值后的RGB24 1Pixel占3Byte
                uint frameIndex = 0;
                bool errorFrame = true;
                uint recSize = 0;

                int res = HisFX3GrabFrame(Imagebuf, width * height * 3, ref frameIndex, ref errorFrame, ref recSize, HisRGB_RGB24 | HisRGB_Bilinear);
                if (res == 0 && errorFrame == false)
                {
                    byte[] image = new byte[width * height * 3];
                    Marshal.Copy(Imagebuf, image, 0, width * height * 3);
                    return (Bitmap)RGBTOBITMAP(image, width, height);
                }
            }
            return null;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            pictureBox2.Load(Application.StartupPath + "\\Reference - 副本.bmp");
        }

        public static SSIMResult getMSSIM(Bitmap b1, Bitmap b2)
        {
            Mat i1 = OpenCvSharp.Extensions.BitmapConverter.ToMat(b1);//用bitmap转换为mat
            Mat i2 = OpenCvSharp.Extensions.BitmapConverter.ToMat(b2);//用bitmap转换为mat
            const double C1 = 6.5025, C2 = 58.5225;
            /***************************** INITS **********************************/
            MatType d = MatType.CV_32F;

            Mat I1 = new Mat(), I2 = new Mat();
            i1.ConvertTo(I1, d);           // cannot calculate on one byte large values
            i2.ConvertTo(I2, d);

            Mat I2_2 = I2.Mul(I2);        // I2^2
            Mat I1_2 = I1.Mul(I1);        // I1^2
            Mat I1_I2 = I1.Mul(I2);        // I1 * I2
            /***********************PRELIMINARY COMPUTING ******************************/
            Mat mu1 = new Mat(), mu2 = new Mat();   //
            Cv2.GaussianBlur(I1, mu1, new OpenCvSharp.Size(11, 11), 1.5);
            Cv2.GaussianBlur(I2, mu2, new OpenCvSharp.Size(11, 11), 1.5);

            Mat mu1_2 = mu1.Mul(mu1);
            Mat mu2_2 = mu2.Mul(mu2);
            Mat mu1_mu2 = mu1.Mul(mu2);

            Mat sigma1_2 = new Mat(), sigma2_2 = new Mat(), sigma12 = new Mat();

            Cv2.GaussianBlur(I1_2, sigma1_2, new OpenCvSharp.Size(11, 11), 1.5);
            sigma1_2 -= mu1_2;

            Cv2.GaussianBlur(I2_2, sigma2_2, new OpenCvSharp.Size(11, 11), 1.5);
            sigma2_2 -= mu2_2;

            Cv2.GaussianBlur(I1_I2, sigma12, new OpenCvSharp.Size(11, 11), 1.5);
            sigma12 -= mu1_mu2;
            // FORMULA
            Mat t1, t2, t3;

            t1 = 2 * mu1_mu2 + C1;
            t2 = 2 * sigma12 + C2;
            t3 = t1.Mul(t2);              // t3 = ((2*mu1_mu2 + C1).*(2*sigma12 + C2))

            t1 = mu1_2 + mu2_2 + C1;
            t2 = sigma1_2 + sigma2_2 + C2;
            t1 = t1.Mul(t2);               // t1 =((mu1_2 + mu2_2 + C1).*(sigma1_2 + sigma2_2 + C2))

            Mat ssim_map = new Mat();
            Cv2.Divide(t3, t1, ssim_map);      // ssim_map =  t3./t1;
            Scalar mssim = Cv2.Mean(ssim_map);// mssim = average of ssim map

            SSIMResult result = new SSIMResult();
            result.diff = ssim_map;
            result.mssim = mssim;
            return result;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
                lblSSIM.Text = getMSSIM((Bitmap)pictureBox1.Image, (Bitmap)pictureBox2.Image).score.ToString();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            pictureBox1.Load(Application.StartupPath + "\\Reference2.bmp");
        }
    }
}
