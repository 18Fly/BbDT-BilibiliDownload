
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Net.Http;
using Windows.Graphics;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BbDT
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        private static readonly HttpClient httpClient = new();

        private double vedioProgress, audioProgress;

        private bool controlDownload = false;

        BilibiliDownloadClient downloadClient;

        public MainWindow()
        {
            this.InitializeComponent();
            this.AppWindow.SetIcon("Assets/Logo.ico");

            this.AppWindow.MoveAndResize(new RectInt32(960 - 650 / 2, 540 - 500 / 2, 650, 500));

            downloadClient = new("https://www.bilibili.com/video/BV1pL4y1e7kQ/?spm_id_from=333.337.search-card.all.click", httpClient, QRCodeImage, LoginDialog, changePrimaryButton, ReportVedioProgress, ReportAudioProgress, changeDownloadStatus, PB);
        }



        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!controlDownload)
            {
                _ = await downloadClient.GetLoginInformation();
            }
            else if (TB.Text.Length == 0)
            {
                LoginDialog.Title = "提示";
                LoginDialog.Content = "请输入下载链接！";
                LoginDialog.PrimaryButtonText = "确认";
                _ = LoginDialog.ShowAsync();
            }
            else
            {
                PB.Foreground = new SolidColorBrush(Colors.SkyBlue);
                _ = await downloadClient.GetUrlSource(TB.Text);
            }
        }

        private void changePrimaryButton()
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                LoginDialog.PrimaryButtonText = "点击返回";
                LoginDialog.IsPrimaryButtonEnabled = true;
            });
        }

        private void ReportVedioProgress(long byteread, long totalbyte)
        {
            vedioProgress = byteread / totalbyte * 34;
            PB.Value = audioProgress + vedioProgress;
        }

        private void ReportAudioProgress(long byteread, long totalbyte)
        {
            audioProgress = byteread / totalbyte * 34;
            PB.Value = audioProgress + vedioProgress;
        }

        private void changeDownloadStatus()
        {
            this.controlDownload = true;
        }
    }
}
