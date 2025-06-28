using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.IO;
using System.Net.Http;
using Windows.Graphics;
using Windows.Storage;

// �˽������� WinUI��WinUI ��Ŀ�ṹ����Ŀģ�����Ϣ�������: http://aka.ms/winui-project-info��

namespace BbDT
{
    /// <summary>
    /// �������࣬�ɵ���ʹ�û��� Frame �е���ʹ�á�
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        // ��̬ HttpClient ʵ����������������
        private static readonly HttpClient httpClient = new();

        // ��Ƶ����Ƶ�����ؽ���
        private double videoProgress, audioProgress;

        // �����Ƿ��ѵ�¼����������
        private bool controlDownload = false;

        // Bilibili ���ؿͻ���ʵ��
        private BilibiliDownloadClient downloadClient;

        /// <summary>
        /// �����ڹ��캯������ʼ�����ں����ؿͻ���
        /// </summary>
        public MainWindow()
        {
            // ��ʼ�� XAML ���
            InitializeComponent();
            // ���ô���ͼ��
            AppWindow.SetIcon("Assets/Logo.ico");
            // ���ô��ڳ�ʼλ�úʹ�С
            AppWindow.MoveAndResize(new RectInt32(960 - 650 / 2, 540 - 500 / 2, 650, 500));
            // ��ʼ�� Bilibili ���ؿͻ��ˣ�������� UI �ؼ��ͻص�
            downloadClient = new(
                "https://www.bilibili.com/video/BV1pL4y1e7kQ/?spm_id_from=333.337.search-card.all.click",
                httpClient,
                QRCodeImage,
                LoginDialog,
                ChangePrimaryButton,
                ReportVideoProgress,
                ReportAudioProgress,
                ChangeDownloadStatus,
                PB
            );
        }

        /// <summary>
        /// ���ذ�ť����¼�������
        /// </summary>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // δ��¼ʱ���Ƚ��е�¼����
            if (!controlDownload)
            {
                _ = await downloadClient.GetLoginInformation(DispatcherQueue);
            }
            // ���������Ƿ�Ϊ��
            else if (TB.Text.Length == 0)
            {
                LoginDialog.Title = "��ʾ";
                LoginDialog.Content = "�������������ӣ�";
                LoginDialog.PrimaryButtonText = "ȷ��";
                _ = LoginDialog.ShowAsync();
            }
            else
            {
                // ���ý�������ɫ�ͳ�ʼֵ
                PB.Foreground = new SolidColorBrush(Colors.SkyBlue);
                PB.Value = 0;

                // ����Ѵ��ھɵ������ļ�����ɾ��
                if (File.Exists("D://output.mp4"))
                {
                    File.Delete("D://output.mp4");
                }

                if (File.Exists("D://tmp//Audio.m4s"))
                {
                    File.Delete("D://tmp//Audio.m4s");
                }

                if (File.Exists("D://tmp//Video.m4s"))
                {
                    File.Delete("D://tmp//Video.m4s");
                }

                // �������ؿͻ��˻�ȡ�������Ӳ���ʼ����
                _ = await downloadClient.GetUrlSource(TB.Text);
            }
        }

        /// <summary>
        /// ���ĵ�¼�Ի�������ť���ı��Ϳ���״̬
        /// </summary>
        private void ChangePrimaryButton()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                LoginDialog.PrimaryButtonText = "�������";
                LoginDialog.IsPrimaryButtonEnabled = true;
            });
        }

        /// <summary>
        /// ������Ƶ���ؽ��ȵĻص�����
        /// </summary>
        /// <param name="byteread">�Ѷ�ȡ���ֽ���</param>
        /// <param name="totalbyte">���ֽ���</param>
        private void ReportVideoProgress(long byteread, long totalbyte)
        {
            videoProgress = (double)byteread / totalbyte * 34;
            PB.Value = audioProgress + videoProgress;
        }

        /// <summary>
        /// ������Ƶ���ؽ��ȵĻص�����
        /// </summary>
        /// <param name="byteread">�Ѷ�ȡ���ֽ���</param>
        /// <param name="totalbyte">���ֽ���</param>
        private void ReportAudioProgress(long byteread, long totalbyte)
        {
            audioProgress = (double)byteread / totalbyte * 34;
            PB.Value = audioProgress + videoProgress;
        }

        /// <summary>
        /// ����״̬�ı�ʱ�Ļص���������������
        /// </summary>
        private void ChangeDownloadStatus()
        {
            controlDownload = true;
        }
    }
}
