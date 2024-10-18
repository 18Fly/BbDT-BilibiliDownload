using HtmlAgilityPack;
using Microsoft.UI.Xaml.Controls;
using QRCoder;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static BbDT.RespModels;

namespace BbDT
{
    /// <summary>
    /// 哔哩哔哩视频下载客户端
    /// </summary>
    internal class BilibiliDownloadClient
    {
        public BilibiliDownloadClient(string ReqUrl, HttpClient httpClient, Microsoft.UI.Xaml.Controls.Image image, ContentDialog ContentDialog, Action changeDialog, Action<long, long> downloadReport, Action<long, long> downloadAudioReport, Action controlDownload, ProgressBar progressBar)
        {
            url.requestUrl = ReqUrl;
            string gourl = WebUtility.UrlEncode(ReqUrl);
            url.loginUrl = $"https://passport.bilibili.com/x/passport-login/web/qrcode/generate?source=main-fe-header&go_url={gourl}";

            client = httpClient;

            imageSource = image;
            contentDialog = ContentDialog;
            callback = changeDialog;
            callbackDownload = downloadReport;
            callbackAudioDownload = downloadAudioReport;
            callbackControlDownload = controlDownload;
            this.progressBar = progressBar;
        }

        struct NecessaryHeaderAndCookies
        {
            string buvid3; //VedioMainPage
            string b_nut; //VedioMainPage
            string SESSDATA; //VedioMainPage

            string Referer; //VedioDownload

            public string Buvid3 { get => buvid3; set => buvid3 = value; }
            public string B_Nut { get => b_nut; set => b_nut = value; }
            public string SessData { get => SESSDATA; set => SESSDATA = value; }
            public string ReFerer { get => Referer; set => Referer = value; }
        }

        struct NecessaryUriParamter
        {
            string source; //generate
            string go_url; //generate

            string spm_id_from; //VedioMainPage
            string vd_source; //VedioMainPage

            string qrcode_key; //poll
            string qrcode_url; //poll
            string source_poll; //poll

            string bvid; //playurl
            string cid; //playurl

            public string Source { get => source; set => source = value; }
            public string Go_Url { get => go_url; set => go_url = value; }
            public string Spm_Id_From { get => spm_id_from; set => spm_id_from = value; }
            public string Vd_Source { get => vd_source; set => vd_source = value; }
            public string QrCode_Key { get => qrcode_key; set => qrcode_key = value; }
            public string QrCode_Url { get => qrcode_url; set => qrcode_url = value; }
            public string Source_Poll { get => source_poll; set => source_poll = value; }
            public string Bvid { get => bvid; set => bvid = value; }
            public string Cid { get => cid; set => cid = value; }
        }

        struct NecessaryRequestUrl
        {
            public string loginUrl;
            public string pollUrl;
            public string requestUrl;
            public string vedioDownloadUrl;
            public string audioDownloadUrl;
        }

        NecessaryRequestUrl url;
        NecessaryHeaderAndCookies header;
        NecessaryUriParamter uriParamter;

        Timer timer;

        private readonly HttpClient client;
        private Microsoft.UI.Xaml.Controls.Image imageSource;
        private ContentDialog contentDialog;
        private ProgressBar progressBar;

        Action callback;
        Action<long, long> callbackDownload;
        Action<long, long> callbackAudioDownload;
        Action callbackControlDownload;

        /// <summary>
        /// 进行登陆操作,获取关键用户鉴权信息
        /// </summary>
        /// <returns>用于await等待执行</returns>
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public async Task<bool> GetLoginInformation()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url.requestUrl);

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36 Edg/129.0.0.0");

            var response = await client.SendAsync(request);
            if (response.Headers.TryGetValues("set-cookie", out var cookieValues))
            {
                foreach (var item in cookieValues)
                {
                    string[] strings = item.Split(';')
                   .GetValue(0)
                   .ToString()
                   .Split('=');
                    switch (strings[0])
                    {
                        case "buvid3":
                            header.Buvid3 = strings[1];
                            break;
                        case "b_nut":
                            header.B_Nut = strings[1];
                            break;
                        default:
                            break;
                    }
                }
            }

            request.Dispose();
            response.Dispose();

            request = new HttpRequestMessage(HttpMethod.Get, url.loginUrl);

            response = await client.SendAsync(request);
            LoginResp loginResp = await response.Content.ReadFromJsonAsync<LoginResp>();
            uriParamter.QrCode_Url = loginResp.data.url;
            uriParamter.QrCode_Key = loginResp.data.qrcode_key;
            url.pollUrl = $"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={loginResp.data.qrcode_key}&source=main-fe-header";

            request.Dispose();
            response.Dispose();

            GenerateQrCodeLogin();

            timer = new Timer(ContinueCheckLogin, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));


            //while (!controlRequest) ;
            //request = new HttpRequestMessage(HttpMethod.Get, url.requestUrl);
            //request.Headers.Add("set-cookie", $"SESSDATA={header.SessData};");
            //response = await client.SendAsync(request);

            return true;
        }

        /// <summary>
        /// 轮询是否扫描登陆成功的回调函数
        /// </summary>
        /// <param name="oj">用不到，Timer的回调函数强制要求参数如此</param>
        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
        public async void ContinueCheckLogin(object oj)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url.pollUrl);

            var response = await client.SendAsync(request);
            string tmp = await response.Content.ReadAsStringAsync();
            QRCodeCheck qRCodeCheck = JsonSerializer.Deserialize<QRCodeCheck>(tmp);

            if (!qRCodeCheck.data.refresh_token.Equals(""))
            {
                timer.Dispose();
                if (response.Headers.TryGetValues("set-cookie", out var cookieValues))
                {
                    foreach (var item in cookieValues)
                    {
                        string[] strings = item.Split(';')
                       .GetValue(0)
                       .ToString()
                       .Split('=');
                        switch (strings[0])
                        {
                            case "SESSDATA":
                                header.SessData = strings[1];
                                break;
                            default:
                                break;
                        }
                    }
                }
                callback();
                callbackControlDownload();
            }

            request.Dispose();
            response.Dispose();
            return;
        }

        /// <summary>
        /// 生成登录二维码
        /// </summary>
        public void GenerateQrCodeLogin()
        {
            using (QRCodeGenerator qRCodeGenerator = new())
            using (QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(uriParamter.QrCode_Url, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qRCode = new(qRCodeData))
            {
                byte[] bytes = qRCode.GetGraphic(5, Color.FromArgb(0x19, 0x6a, 0xa7), Color.FromArgb(0xff, 0xff, 0xff), false); //#196aa7 #ffffff
                _ = File.WriteAllBytesAsync("D://tmp/QRLoginCode.png", bytes);
                _ = contentDialog.ShowAsync();
            }
        }

        /// <summary>
        /// 获取视频下载链接
        /// </summary>
        /// <param name="downUrl">将要下载的视频链接</param>
        /// <returns>用户鉴权是否正常</returns>
        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
        public async Task<bool> GetUrlSource(string downUrl)
        {
            string[] tmpString = downUrl.Split('/','?');
            uriParamter.Bvid = tmpString[4];

            var request = new HttpRequestMessage(HttpMethod.Get, downUrl);

            var response = await client.SendAsync(request);

            var document = new HtmlDocument();
            var UnEncodeStream = await response.Content.ReadAsStreamAsync();
            using GZipStream gZipStream = new(UnEncodeStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gZipStream, Encoding.UTF8);
            var EncodeString = await reader.ReadToEndAsync();

            document.LoadHtml(EncodeString);

            request.Dispose();
            response.Dispose();

            HtmlNodeCollection htmlNodeCollection = document.DocumentNode.SelectNodes("//script");

            foreach (var item in htmlNodeCollection)
            {
                string innerText = item.InnerText;
                if (innerText.Contains("window.__playinfo__={"))
                {
                    int v = innerText.IndexOf("backup_url");
                    int v1 = innerText.IndexOf("bandwidth");

                    string v2 = innerText.Substring(v, v1);
                    string[] strings = v2.Split('/');
                    uriParamter.Cid = strings[6];
                }
            }

            request = new HttpRequestMessage(HttpMethod.Get, $"https://api.bilibili.com/x/player/wbi/playurl?fnval=4048&bvid={uriParamter.Bvid}&cid={uriParamter.Cid}");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36 Edg/129.0.0.0");
            request.Version = new(2, 0);
            response = await client.SendAsync(request);

            VedioAndAudioUrl.Root vedioAndAudioUrl = JsonSerializer.Deserialize<VedioAndAudioUrl.Root>(await response.Content.ReadAsStringAsync());
            url.vedioDownloadUrl = vedioAndAudioUrl.data.dash.video[0].backup_url[0];
            url.audioDownloadUrl = vedioAndAudioUrl.data.dash.audio[0].backup_url[0];

            request.Dispose();
            response.Dispose();

            _ = GetAudioSource();
            _ = GetVedioSource();

            return true;
        }

        /// <summary>
        /// 获取音频数据
        /// </summary>
        /// <returns>用于await等待</returns>
        public async Task<bool> GetAudioSource()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url.audioDownloadUrl);

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36 Edg/129.0.0.0");
            request.Headers.Add("referer", url.requestUrl);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            long? totalBytes = response.Content.Headers.ContentLength;
            using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
              fileStream = new FileStream("D://tmp/Audio.m4s", FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                var buffer = new byte[8192];  // 设置读取缓冲区大小
                long totalBytesRead = 0;      // 累计已读取字节数
                int bytesRead;

                // 循环读取响应流
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    // 将读取的字节写入本地文件
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;

                    // 如果知道文件总大小，则计算并报告下载进度
                    if (totalBytes.HasValue)
                    {
                        callbackAudioDownload(totalBytesRead, totalBytes.Value);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 获取视频数据
        /// </summary>
        /// <returns>用于await等待操作</returns>
        public async Task<bool> GetVedioSource()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url.vedioDownloadUrl);

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36 Edg/129.0.0.0");
            request.Headers.Add("referer", url.requestUrl);


            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            long? totalBytes = response.Content.Headers.ContentLength;
            using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
              fileStream = new FileStream("D://tmp/Vedio.m4s", FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                var buffer = new byte[8192];  // 设置读取缓冲区大小
                long totalBytesRead = 0;      // 累计已读取字节数
                int bytesRead;

                // 循环读取响应流
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    // 将读取的字节写入本地文件
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;

                    // 如果知道文件总大小，则计算并报告下载进度
                    if (totalBytes.HasValue)
                    {
                        callbackDownload(totalBytesRead, totalBytes.Value);
                    }
                }
            }

            FFmpegExecuteClient.IntegrateSource(progressBar, contentDialog);

            return true;
        }
    }
}
