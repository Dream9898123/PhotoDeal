using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            filter = "图像文件(JPeg, Gif, Bmp, etc.)|*.jpg;*.jpeg;*.gif;*.bmp;*.tif; *.tiff; *.png| JPeg 图像文件(*.jpg;*.jpeg)"
              + "|*.jpg;*.jpeg |GIF 图像文件(*.gif)|*.gif |BMP图像文件(*.bmp)|*.bmp|Tiff图像文件(*.tif;*.tiff)|*.tif;*.tiff|Png图像文件(*.png)"
              + "| *.png |所有文件(*.*)|*.*"
              + "|*.mp4";
            this.comboBox1.SelectedIndex = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string str = SelectImg();
            ChangePhotoNameByPhotoInfo(str);
            //var lines = from directory in directories
            //            from tag in directory.Tags
            //            select $"{directory.Name}: {tag.TagName} = {tag.Description}";

            //foreach (var line in lines)
            //    Console.WriteLine(line);
        }

        private static void ChangePhotoNameByPhotoInfo(string str)
        {
            if (Path.GetExtension(str).Equals(".mp4"))
            {

            };
            if (filter.Contains(Path.GetExtension(str)))
            {
                var directories = ImageMetadataReader.ReadMetadata(str);

                var lines = from directory in directories
                            from tag in directory.Tags
                                //select $"{directory.Name}: {tag.TagName} = {tag.Description}";
                            select new { tag.Name, tag.Description };
                //var dd = directories.OfType<ExifIfd0Directory>().FirstOrDefault()?.Tags?.FirstOrDefault(t => (t.Name.Equals("Date/Time")))?.Description;
                if (lines != null)
                {
                    string dd = "";
                    DateTime dt = DateTime.MinValue;
                    if (Path.GetExtension(str).Equals(".mp4"))
                    {
                        dd = lines.FirstOrDefault(t => t.Name.Equals("Created"))?.Description;
                        Match match = Regex.Match(dd, @"\d{1}月 \d{2} \d{2}:\d{2}:\d{2}:\d{2} \d{4}");
                        dd = dd.Substring(2);
                        //dd = match.Value;
                        if (!string.IsNullOrEmpty(dd))
                        dt = DateTime.ParseExact(dd, " M月 dd HH:mm:ss yyyy", CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        dd = lines.FirstOrDefault(t => t.Name.Equals("Date/Time"))?.Description;
                        if(!string.IsNullOrEmpty(dd))
                        dt = DateTime.ParseExact(dd, "yyyy:MM:dd HH:mm:ss", CultureInfo.CurrentCulture);
                        else
                        {

                        }
                    }
                    if (dt != DateTime.MinValue)
                    {
                        CreatePhoto(dt, str);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //this.listBox1.Items.Clear();
            //string str = SelectImg();
            //if (str != "")
            //{
            //    this.listBox1.Items.Add(str);
            //    Image image = GetImage(str);
            //    this.pictureBox1.BackgroundImage = image;
            //    List<string> list = GetImageInfo(str, image);
            //    foreach (var item in list)
            //    {
            //        listBox1.Items.Add(item);
            //    }

            //}
        }

        private void btnSelectImg1_Click(object sender, EventArgs e)
        {
            //string str = SelectImg();
            //if (str != "")
            //{
            //    txtImg1.Text = str;
            //    pbImg1.BackgroundImage = GetImage(str);
            //    List<string> list = GetImageInfo(str, pbImg1.BackgroundImage);
            //    txtMsg.Clear();
            //    foreach (var item in list)
            //    {
            //        txtMsg.AppendText("\n" + item);
            //    }

            //}
        }

        private List<string> GetImageInfo(string path, Image image)
        {
            List<string> list = new List<string>();

            PropertyItem[] pt = image.PropertyItems;
            for (int i = 0; i < pt.Length; i++)
            {

                PropertyItem p = pt[i];
                switch (pt[i].Id)

                {  // 设备制造商 20.  

                    case 0x010F:
                        {
                            string str = System.Text.ASCIIEncoding.ASCII.GetString(pt[i].Value);
                            list.Add("设备制造商：" + str);
                        }
                        break;

                    case 0x0110: // 设备型号 25.  

                        {
                            string str = GetValueOfType2(p.Value);
                            list.Add("设备型号：" + str);
                        }
                        break;

                    case 0x0132: // 拍照时间 30.
                        {
                            string str = GetValueOfType2(p.Value);
                            list.Add("拍照时间：" + str);
                        }
                        break;

                    case 0x829A: // .曝光时间  
                        {
                            string str = GetValueOfType5(p.Value);
                            list.Add("曝光时间：" + str);
                        }
                        break;

                    case 0x8827: // ISO 40.   
                        {
                            string str = GetValueOfType3(p.Value);
                            list.Add("ISO：" + str);
                        }

                        break;

                    case 0x010E: // 图像说明info.description
                        {
                            string str = GetValueOfType2(p.Value);
                            list.Add("图像说明：" + str);
                        }
                        break;

                    case 0x920a: //相片的焦距

                        {
                            string str = GetValueOfType5A(p.Value) + " mm";
                            list.Add("焦距值：" + str);
                        }
                        break;

                    case 0x829D: //相片的光圈值
                        {
                            string str = GetValueOfType5A(p.Value);
                            list.Add("光圈值：" + str);
                        }
                        break;

                    default:
                        {

                        }
                        break;

                }

            }
            return list;
        }

        public string GetValueOfType2(byte[] b)// 对type=2 的value值进行读取
        {
            return System.Text.Encoding.ASCII.GetString(b);
        }

        private static string GetValueOfType3(byte[] b) //对type=3 的value值进行读取
        {
            if (b.Length != 2) return "unknow";
            return Convert.ToUInt16(b[1] << 8 | b[0]).ToString();
        }

        private static string GetValueOfType5(byte[] b) //对type=5 的value值进行读取
        {
            if (b.Length != 8) return "unknow";
            UInt32 fm, fz;
            fm = 0;
            fz = 0;
            fz = Convert.ToUInt32(b[7] << 24 | b[6] << 16 | b[5] << 8 | b[4]);
            fm = Convert.ToUInt32(b[3] << 24 | b[2] << 16 | b[1] << 8 | b[0]);
            return fm.ToString() + "/" + fz.ToString() + " sec";

        }

        private static string GetValueOfType5A(byte[] b)//获取光圈的值
        {
            if (b.Length != 8) return "unknow";
            UInt32 fm, fz;
            fm = 0;
            fz = 0;
            fz = Convert.ToUInt32(b[7] << 24 | b[6] << 16 | b[5] << 8 | b[4]);
            fm = Convert.ToUInt32(b[3] << 24 | b[2] << 16 | b[1] << 8 | b[0]);
            double temp = (double)fm / fz;
            return (temp).ToString();
        }
        static string filter = "";
        public string SelectImg()
        {
            OpenFileDialog openFi = new OpenFileDialog();
            openFi.Filter = filter;
            if (openFi.ShowDialog() == DialogResult.OK)
            {
                return openFi.FileName;
            }
            return "";
        }

        /// <summary>
        /// 用内存流来读取图片
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Image GetImage(string filePath)
        {
            Image image = null;
            try
            {
                //实例化一个文件流
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    //把文件读取到字节数组
                    byte[] data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
                    image = Image.FromStream(new MemoryStream(data));
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return image;
        }

        private void btnPhotoPath_Click(object sender, EventArgs e)
        {
            string s = this.txtPhotoPath.Text; 
            if(string.IsNullOrEmpty(s))
                s = Directory.GetCurrentDirectory();
            this.folderBrowserDialog1.SelectedPath = s;
            
            if(this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.txtPhotoPath.Text = this.folderBrowserDialog1.SelectedPath;
            }
        }

        System.IO.FileInfo[] GetAllFileInfo2(System.IO.DirectoryInfo dir)
        {
            return dir.GetFiles(".", System.IO.SearchOption.AllDirectories);

        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(this.txtPhotoPath.Text);
            //#region 调用端（类库的方法）
            System.IO.FileInfo[] allFiles = GetAllFileInfo2(dir);
            foreach (var item in allFiles)
            {
                ChangePhotoNameByPhotoInfo(item.FullName);
            }
            //foreach (System.IO.FileInfo file in allFiles)
            //{
            //    //MessageBox.Show(file.Name);
            //    Match match = Regex.Match(file.Name, @"\d{4}_\d{2}_\d{2}_\d{2}_\d{2}");
            //    string date = match.Value;

            //    //Regex reg = new Regex("<span class='functiontuData'>(?<date>.*?)</span>", RegexOptions.CultureInvariant);

            //    //foreach (Match m in reg.Matches(file.Name))
            //    //{
            //    //    string sdate = m.Groups["date"].Value;
            //    //    sdate = sdate.Replace("年", "-").Replace("月", "-").Replace("日", "").Replace(".", "-");
            //    //    DateTime dt = System.DateTime.Now;


            //    //    //符合日期规范
            //    //    if (DateTime.TryParse(sdate, out dt)) { }

            //    //}
            //    //MessageBox.Show(date.ToString());
            //    DateTime dt = DateTime.ParseExact(date, "yyyy_MM_dd_HH_mm", CultureInfo.CurrentCulture);
            //    string dealFullName = file.FullName;
            //    CreatePhoto(dt, dealFullName);
            //}
            //#endregion

        }

        private static void CreatePhoto(DateTime dt, string dealFullName)
        {
            string s = dt.ToString("yyyyMMdd - HH_mm");
            string sPath = /*Path.Combine(*/Path.GetDirectoryName(dealFullName)/*, "处理好的图片")*/;
            if (!Directory.Exists(sPath))
                Directory.CreateDirectory(sPath);
            string sFullName = Path.Combine(sPath, s + Path.GetExtension(dealFullName));
            int num = 1;
            bool addSS = true;
            while (File.Exists(sFullName))
            {
                if (addSS)
                {
                    sFullName = Path.Combine(sPath, s + "_" + dt.Second.ToString("00") + Path.GetExtension(dealFullName));
                    addSS = false;
                }
                else
                {
                    sFullName = Path.Combine(sPath, s + "_" + (num++).ToString("00") + Path.GetExtension(dealFullName));
                }
            }
            File.Move(dealFullName, sFullName);
            File.SetCreationTime(sFullName, dt);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            button1_Click(sender,e);
        }

        private void comboBox1_Validated(object sender, EventArgs e)
        {
            try
            {
                label4.Text = DateTime.Now.ToString(this.comboBox1.Text);
            }
            catch
            {

            }
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                label4.Text = DateTime.Now.ToString(this.comboBox1.Text);
            }
            catch
            {

            }
        }
    }
}
