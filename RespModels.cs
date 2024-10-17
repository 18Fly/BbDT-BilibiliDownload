#nullable enable

using System.Collections.Generic;

namespace BbDT
{
    /// <summary>
    /// JSON模板类
    /// </summary>
    internal class RespModels
    {
        /// <summary>
        /// 登录信息的JSON模板
        /// </summary>
        public class LoginResp
        {
            public int code { get; set; }
            public string? message { get; set; }
            public int ttl { get; set; }
            public Data? data { get; set; }

            public class Data
            {
                public string? url { get; set; }
                public string? qrcode_key { get; set; }
            }
        }

        /// <summary>
        /// 二维码登录的JSON模板
        /// </summary>
        public class QRCodeCheck
        {
            public int code { get; set; }
            public string? message { get; set; }
            public int ttl { get; set; }
            public Data? data { get; set; }

            public class Data
            {
                public string? url { get; set; }
                public string? refresh_token { get; set; }
                public long timestamp { get; set; }
                public int code { get; set; }
                public string? message { get; set; }
            }
        }

        /// <summary>
        /// 音视频下载链接的JSON模板
        /// </summary>
        public class VedioAndAudioUrl
        {
            public class Audio
            {
                public int id { get; set; }
                public string? baseUrl { get; set; }
                public string? base_url { get; set; }
                public List<string>? backupUrl { get; set; }
                public List<string>? backup_url { get; set; }
                public int bandwidth { get; set; }
                public string? mimeType { get; set; }
                public string? mime_type { get; set; }
                public string? codecs { get; set; }
                public int width { get; set; }
                public int height { get; set; }
                public string? frameRate { get; set; }
                public string? frame_rate { get; set; }
                public string? sar { get; set; }
                public int startWithSap { get; set; }
                public int start_with_sap { get; set; }
                public SegmentBase? SegmentBase { get; set; }
                public SegmentBase? segment_base { get; set; }
                public int codecid { get; set; }
            }

            public class Dash
            {
                public int duration { get; set; }
                public double minBufferTime { get; set; }
                public double min_buffer_time { get; set; }
                public List<Video>? video { get; set; }
                public List<Audio>? audio { get; set; }
                public Dolby? dolby { get; set; }
                public object? flac { get; set; }
            }

            public class Data
            {
                public string? from { get; set; }
                public string? result { get; set; }
                public string? message { get; set; }
                public int quality { get; set; }
                public string? format { get; set; }
                public int timelength { get; set; }
                public string? accept_format { get; set; }
                public List<string>? accept_description { get; set; }
                public List<int>? accept_quality { get; set; }
                public int video_codecid { get; set; }
                public string? seek_param { get; set; }
                public string? seek_type { get; set; }
                public Dash? dash { get; set; }
                public List<SupportFormat>? support_formats { get; set; }
                public object? high_format { get; set; }
                public int last_play_time { get; set; }
                public long last_play_cid { get; set; }
                public object? view_info { get; set; }
            }

            public class Dolby
            {
                public int type { get; set; }
                public object? audio { get; set; }
            }

            /// <summary>
            /// 以此类为根路径,例如:VedioAndAudioUrl.Root
            /// </summary>
            public class Root
            {
                public int code { get; set; }
                public string? message { get; set; }
                public int ttl { get; set; }
                public Data? data { get; set; }
            }

            public class SegmentBase
            {
                public string? Initialization { get; set; }
                public string? indexRange { get; set; }
            }

            public class SegmentBase2
            {
                public string? initialization { get; set; }
                public string? index_range { get; set; }
            }

            public class SupportFormat
            {
                public int quality { get; set; }
                public string? format { get; set; }
                public string? new_description { get; set; }
                public string? display_desc { get; set; }
                public string? superscript { get; set; }
                public List<string>? codecs { get; set; }
            }

            public class Video
            {
                public int id { get; set; }
                public string? baseUrl { get; set; }
                public string? base_url { get; set; }
                public List<string>? backupUrl { get; set; }
                public List<string>? backup_url { get; set; }
                public int bandwidth { get; set; }
                public string? mimeType { get; set; }
                public string? mime_type { get; set; }
                public string? codecs { get; set; }
                public int width { get; set; }
                public int height { get; set; }
                public string? frameRate { get; set; }
                public string? frame_rate { get; set; }
                public string? sar { get; set; }
                public int startWithSap { get; set; }
                public int start_with_sap { get; set; }
                public SegmentBase? SegmentBase { get; set; }
                public SegmentBase? segment_base { get; set; }
                public int codecid { get; set; }
            }
        }

    }
}
