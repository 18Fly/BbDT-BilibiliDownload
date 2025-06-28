using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.IO;
using System.Net.Http;
using Windows.Graphics;
using Windows.Storage;

// 了解更多关于 WinUI、WinUI 项目结构和项目模板的信息，请访问: http://aka.ms/winui-project-info。

namespace BbDT
{
    /// <summary>
    /// 主窗口类，可单独使用或在 Frame 中导航使用。
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        // 静态 HttpClient 实例，用于网络请求
        private static readonly HttpClient httpClient = new();

        // 视频和音频的下载进度
        private double videoProgress, audioProgress;

        // 控制是否已登录并允许下载
        private bool controlDownload = false;

        // Bilibili 下载客户端实例
        private BilibiliDownloadClient downloadClient;

        /// <summary>
        /// 主窗口构造函数，初始化窗口和下载客户端
        /// </summary>
        public MainWindow()
        {
            // 初始化 XAML 组件
            InitializeComponent();
            // 设置窗口图标
            AppWindow.SetIcon("Assets/Logo.ico");
            // 设置窗口初始位置和大小
            AppWindow.MoveAndResize(new RectInt32(960 - 650 / 2, 540 - 500 / 2, 650, 500));
            // 初始化 Bilibili 下载客户端，传递相关 UI 控件和回调
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
        /// 下载按钮点击事件处理方法
        /// </summary>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // 未登录时，先进行登录操作
            if (!controlDownload)
            {
                _ = await downloadClient.GetLoginInformation(DispatcherQueue);
            }
            // 检查输入框是否为空
            else if (TB.Text.Length == 0)
            {
                LoginDialog.Title = "提示";
                LoginDialog.Content = "请输入下载链接！";
                LoginDialog.PrimaryButtonText = "确认";
                _ = LoginDialog.ShowAsync();
            }
            else
            {
                // 设置进度条颜色和初始值
                PB.Foreground = new SolidColorBrush(Colors.SkyBlue);
                PB.Value = 0;

                // 如果已存在旧的下载文件，则删除
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

                // 调用下载客户端获取下载链接并开始下载
                _ = await downloadClient.GetUrlSource(TB.Text);
            }
        }

        /// <summary>
        /// 更改登录对话框主按钮的文本和可用状态
        /// </summary>
        private void ChangePrimaryButton()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                LoginDialog.PrimaryButtonText = "点击返回";
                LoginDialog.IsPrimaryButtonEnabled = true;
            });
        }

        /// <summary>
        /// 更新视频下载进度的回调方法
        /// </summary>
        /// <param name="byteread">已读取的字节数</param>
        /// <param name="totalbyte">总字节数</param>
        private void ReportVideoProgress(long byteread, long totalbyte)
        {
            videoProgress = (double)byteread / totalbyte * 34;
            PB.Value = audioProgress + videoProgress;
        }

        /// <summary>
        /// 更新音频下载进度的回调方法
        /// </summary>
        /// <param name="byteread">已读取的字节数</param>
        /// <param name="totalbyte">总字节数</param>
        private void ReportAudioProgress(long byteread, long totalbyte)
        {
            audioProgress = (double)byteread / totalbyte * 34;
            PB.Value = audioProgress + videoProgress;
        }

        /// <summary>
        /// 下载状态改变时的回调方法，允许下载
        /// </summary>
        private void ChangeDownloadStatus()
        {
            controlDownload = true;
        }
    }
}
