using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Xabe.FFmpeg;

namespace BbDT
{
    /// <summary>
    /// 用于执行FFmpeg.exe命令行命令的客户端
    /// </summary>
    internal class FFmpegExecuteClient
    {
        /// <summary>
        /// 后台调用FFmpeg.exe来合并音视频数据，需要配置系统变量
        /// 可以在写安装包的时候，用脚本注册一个系统变量
        /// UI上让用户自定义保存路径，同时默认保存路径在C:\Users\16567\Videos\BbDT
        /// </summary>
        public static async void IntegrateSource(ProgressBar progress, ContentDialog dialog)
        {
            FFmpeg.SetExecutablesPath(Environment.GetEnvironmentVariable("FFMPEG"));
            var paramter = $"-hwaccel auto -i D://tmp/Audio.m4s -i D://tmp/Vedio.m4s -c:v hevc_amf -c:a aac -b:v 1000k D://output.mp4";

            await FFmpeg.Conversions.New().Start(paramter);

            progress.Value += 34;
            progress.Foreground = new SolidColorBrush(Colors.PaleGreen);
            dialog.Title = "提示";
            dialog.PrimaryButtonText = "确定";
            dialog.Content = "下载完成！";
            _ = dialog.ShowAsync();
        }

    }
}
