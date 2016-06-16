
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;//Dısarıya yazma işlemlerinde kullanacağımız kütüphane metin belgesine yaz keydet gibi
using Emgu.CV;//Kamerayaı kontrol etme  kütüphanesi
using Emgu.CV.CvEnum;//yüz tanıma için kullanacağımız kütphane 
using Emgu.CV.Structure;//yüz tanıma için kullanacağımız diğer kütüphane
using System.Data.SqlClient;//Sql için kullanıcağımız kütüphane

namespace YuzTanima
{
    public partial class Form1 : Form
    {
        Image<Bgr, Byte> SuankiGoruntu;//anlık görüntüleri tutacağımız generic ımage listesi kamereda sürekli hareket halinde olduğumuzdan bu liste ile hareketlerimiz takip edecek frame olarak bu listede tutacak
        Capture Goruntu;//Ekran görünteleme methodu
        HaarCascade Yuz;//yüzü tanımak için kullancağımız method 
        HaarCascade Goz;//Gözümüzü tanımak için kullancağımız method
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);//Kamerda yüzümüzü tanıdığında bir kare cıktığında yüzümüzü içine alan bir kare o karenin üstünde bilgi yazıyo kimolduğunu işte onu bu methodla yapıcaz
        Image<Gray, byte> Sonuc, SonucYuz = null;//yüzümüzü grileştircek
        Image<Gray, byte> Gri = null; //aynı şekilde grileştirme için kullanıcaz
        List<Image<Gray, byte>> BulunanYuzler = new List<Image<Gray, byte>>();
        List<string> Etiket = new List<string>();
        List<string> KisiListesi = new List<string>();
        int GoruntuSayisi, t;
        string isim;



        string Names = null;

        public Form1()
        {
            InitializeComponent();

            Yuz = new HaarCascade("haarcascade_frontalface_default.xml");//yukadarda tanımladığımız Yuz methoduna burda bir xml dosyası veriyoruz bu xml de bir görüntüde yüzü tanıyabilmek için bilgiler vardır bu xml dosyası projenin debug kısmında yani programın çalıştığı dizinde 
            Goz = new HaarCascade("haarcascade_eye.xml");//bu da aynı şekilde goz methoduna bir xml veriyoruz yüz ile aynı


        }

        private void button2_Click(object sender, EventArgs e)
        {
            GoruntuSayisi++;//kac yüzümüzün olduğunu tutyor bu değşken

            Gri = Goruntu.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);//görüntüyü gri olarak alıyoruz Gri 'yr atıyoruz burda 

            MCvAvgComp[][] YuzBul = Gri.DetectHaarCascade(Yuz, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));//burda kaydetme işlemi yapılcağı için McvAvfComp methodu tekrar bir tarama yaparak kaydedilecek bir yüz var mı bakıyo yoksa kaydetmiyecek hata vericektir 

            foreach (MCvAvgComp item in YuzBul[0])//bu döngü eğer McvAvgComp methodu içinde dönerek bir yüz bulmussa onu SonucYuz'e atıyor
            {
                SonucYuz = SuankiGoruntu.Copy(item.rect).Convert<Gray, byte>();//burda atama işlemi yapılıyor bulunan yüzü SonucYuz'e atıyor
                break;//Dongu Duruyor zaten 1 yüz atadık başak yüze gerek yok
            }

            SonucYuz = Sonuc.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);//burda bulunan yüzümüzü 100 e 100 yapıyoruz
            BulunanYuzler.Add(SonucYuz);//ve Yüzümüzü diğer yüzlerimizin yanına ekliyoruz
            Etiket.Add(textBox1.Text);//texboxtaki yüze karşılık olarak yazılan ismide alıyoruz

            ımageBox2.Image = SonucYuz;//aldığımız yüzü diğer ikinci kücük ekranda gösteriyoruz bak bunu aldık haa diye

            File.WriteAllText(Application.StartupPath + "/Kisiler/Kisiler.txt", BulunanYuzler.ToArray().Length.ToString() + "%");//bulduğumuz yüzün ismini yukarda belirtilen kisiler klasöründeki kisiler txt dosyasına atıyoruz aslında gerek yok buna istersek veritanını tekrar cağırabilirizde zaten bu yüzü birazdan aşağıda veritabanına da kaydediyoruz 

            for (int i = 1; i < BulunanYuzler.ToArray().Length + 1; i++)//burda yüzlerimizi ve isimlerini gerkeli klsörlere atıyoruz tabi elimizde bulunan yüz kadar dönüyoruz 
            {
                if (i >= BulunanYuzler.ToArray().Length)//burda eğer dedim çünkü elimizde bulunan yüzleri tekrar tekrar oluştırmasın Resim klasöründe diye son yüz alıyoruz burda bu if son yüzü alıyor veritabanında kayıtlı olmayan yüzü alıyor
                {
                    BulunanYuzler.ToArray()[i - 1].Save(Application.StartupPath + "/Resim/face" + Etiket.ToArray()[i - 1] + ".bmp");//son yüzü burda Resim klasörüne atıyor
                }


                File.AppendAllText(Application.StartupPath + "/Kisiler/Kisiler.txt", Etiket.ToArray()[i - 1] + "%");//resmin ismini kisiler klasöründeki kisiler txt'de dosyasına yazıyor
            }

            MessageBox.Show(textBox1.Text + "´nın yüzü tesbit edildi", "Tamamdır", MessageBoxButtons.OK, MessageBoxIcon.Information);//kullanıcıya uyarı veriyorz yüz tesbit edildi diye

            string adi = textBox1.Text, ResimYolu = "Resim/face" + adi + ".bmp";//burda resimizin yolunu ve ismini değişkenlere atıyoz birazdan kaydedicez aslında bunu direk SqlCommand içinede yazabiliriz ama ben böyle yazdım daha net olsun diye

            SqlCommand Komut = new SqlCommand("insert into Kullanicilar(Adi,ResimYol) values('" + adi + "','" + ResimYolu + "')", Baglan);//SqlCommanda yüzümüzün resmin yolunu kaydetme kodu
            int Eklendimi = Komut.ExecuteNonQuery();//kaydetme kodunu çalıştırıyoruz
            if (Eklendimi > 0)//eğer ekleme işlemi başarılı olduysa messageBox açıyoruz 
            {
                MessageBox.Show("kişi Veritabanına Eklendi");//MessageBox ile kullanıcaya uyarı veriyoruz
            }

        }
        SqlConnection Baglan = new SqlConnection("Server=.;Database=YuzTanimaDB;Integrated Security=true");//Veritabanına hangi Database bağlanacağımızı burda belirtiyoruz

        private void Form1_Load(object sender, EventArgs e)//Formun load'ında veritabanındaki diğer resimleri ve isimleri cekiyoruz o yüzler tekrar cameraya geldiğinde tanımak için
        {
            if (Baglan.State == ConnectionState.Closed)//bağlantımızı kontrol ediyoz eğer kapalı ise açıyoruz
            {
                Baglan.Open();//burda bağlantıyı acıyoruz

            }
            SqlCommand Komut = new SqlCommand("select * from kullanicilar", Baglan);//burdaki SqlCommanda komutu veritabanındaki kullanıcılarımızı cekme kodu
            SqlDataReader oku = Komut.ExecuteReader();//burda komutu çalıştırrıyoruz okuyoruz bu bir veri getirme kodudur

            while (oku.Read())//verileri okdukça true dönüyor kaç satır varsa o kadar dönecek bu döngü
            {

                Goruntu = new Capture(oku["ResimYol"].ToString());//Goruntu methodumuza veritabanındaki görüntüleri sırayla veriyoruz 

                GoruntuSayisi = GoruntuSayisi + 1;//Görüntü Sayısını bir arttırıyoruz

                SonucYuz = Goruntu.QueryFrame().Convert<Gray, byte>();//şuanki görüntüyü SonucYuz'Atıyoruz

                BulunanYuzler.Add(SonucYuz);//burda SonucYuz'deki görüntüyü diğer yüzlerinin yanına koyuyoruz

                Etiket.Add(oku["Adi"].ToString());//burda da isimleri etiket dizisine atıyoruz

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Goruntu = new Emgu.CV.Capture(2);//burda kamera aygıtımızda görüntü alıyoruz ordaki 1 cihaz numaramız bende 2 kamera var ilk kamera 0 dır ikinci kamera 1 dir benim ilk kamera calışmadı emgucv ile uyumsuz cıktı ama 2 inci kamera calıştı bende oraya 1 yazdım



            Goruntu.QueryFrame();//aldığımız görüntülerin framleri bizim için önemli bu frame ile xml dosyasını kullanarak yüzü ve ya gözü bulucaz
            Application.Idle += TanimlamaYap;//buraya kadar kamerayı actık xml dosyamızı hazırladık görüntümüz aldık asıl bu eventile aşağıda bir TanimlamaYap methodunu çalıştırıyoruz bu method yüzümüzü ve ya gözümü buluyor kare içine alıyor tanımlama yapıyorumz
            button1.Enabled = false;//butonun durumunu false yapıyoz ki üstüne tıklanmasınn bir daha bu hataya sebeb olabilir
        }
        private void TanimlamaYap(object sender, EventArgs e)
        {
            KisiListesi.Add("");

            SuankiGoruntu = Goruntu.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);//Suanki görüntünün framelerini alıyor 320 240 boyutlarında 

            Gri = SuankiGoruntu.Convert<Gray, Byte>();//Suanki Görüntüyü Griye dönüştürüyor

            MCvAvgComp[][] YuzuBul = Gri.DetectHaarCascade(Yuz, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));//Görüntüleri  diziye aktarıyor birazdan içinden dönücez yüzlerimizi belirticez tabi bu gri olarak alıyor biz normal görüntüyü görcez 

            foreach (MCvAvgComp item in YuzuBul[0])
            {
                t++;
                Sonuc = SuankiGoruntu.Copy(item.rect).Convert<Gray, Byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                SuankiGoruntu.Draw(item.rect, new Bgr(Color.Red), 2);//suanki görüntüde bir yüz bulmussa onu etrafında kırmızı bir kare çiziyor

                if (BulunanYuzler.ToArray().Length != 0)// eğer sıfır değilse bir yüz tanimlanmiştir kaydedilmiştir  ve onu cevreleyen karenin üstünde isim beliriyor kim olduğuna dair
                {
                    MCvTermCriteria TermCrit = new MCvTermCriteria(GoruntuSayisi, 0.001);
                    EigenObjectRecognizer Taniyici = new EigenObjectRecognizer(
                    BulunanYuzler.ToArray(), Etiket.ToArray(), 3000, ref TermCrit);

                    isim = Taniyici.Recognize(Sonuc);//sonuc içindeki kişiyi isim değişkenine atıyo

                    SuankiGoruntu.Draw(isim, ref font, new Point(item.rect.X - 2, item.rect.Y - 2), new Bgr(Color.LightGreen));//burada yüzün etrafındaki karenin üstünde ismi yazdırıyoz

                    label2.Text = isim;
                }

                KisiListesi[t - 1] = isim;
                KisiListesi.Add("");

                //buradan sonra göz tanima işlemi yapılıyor. Göz tanıma yüz gibi aynı şeyleri tekrar ediyo aslında

                Gri.ROI = item.rect;
                MCvAvgComp[][] GozBul = Gri.DetectHaarCascade(Goz, 1.1, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
                Gri.ROI = Rectangle.Empty;

                foreach (MCvAvgComp itemGoz in GozBul[0])
                {
                    Rectangle GozDortgen = itemGoz.rect;
                    GozDortgen.Offset(item.rect.X, item.rect.Y);
                    SuankiGoruntu.Draw(GozDortgen, new Bgr(Color.Blue), 2);
                }
                //burda göz tanıma bitiyor-------------------


            }
            t = 0;

            ımageBox1.Image = SuankiGoruntu;

        }
    }
}
