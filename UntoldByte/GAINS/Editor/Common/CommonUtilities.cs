using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Unity.SharpZipLib.Zip.Compression;
using Unity.SharpZipLib.Zip.Compression.Streams;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UntoldByte.GAINS.Editor
{
    public static class CommonUtilities
    {
        static StableDiffusionSettings stableDiffusionSettings;
#pragma warning disable IDE1006 // Naming Styles
        static string serverAddress => stableDiffusionSettings.serverAddress;
        static bool lowVRAM => stableDiffusionSettings.lowVRAM;
        static string upscaler => stableDiffusionSettings.upscaler;
#pragma warning restore IDE1006 // Naming Styles
        static HttpClient shortHttpClient;
        static HttpClient httpClient;

        internal static HttpClient GetShortHttpClient()
        {
            ReloadSettingsIfNecessary();

            if (shortHttpClient != null)
                return shortHttpClient;

            CreateShortHttpClient();

            return shortHttpClient;
        }

        private static void CreateShortHttpClient()
        {
#pragma warning disable IDE0017 // Simplify object initialization
            shortHttpClient = new HttpClient();
#pragma warning restore IDE0017 // Simplify object initialization
            shortHttpClient.BaseAddress = new Uri(serverAddress);
            shortHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            shortHttpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        internal static HttpClient GetHttpClient()
        {
            ReloadSettingsIfNecessary();

            if (httpClient != null)
                return httpClient;

            CreateHttpClient();

            return httpClient;
        }

        private static void CreateHttpClient()
        {
#pragma warning disable IDE0017 // Simplify object initialization
            httpClient = new HttpClient();
#pragma warning restore IDE0017 // Simplify object initialization
            httpClient.BaseAddress = new Uri(serverAddress);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.Timeout = TimeSpan.FromSeconds(1000);
        }

        internal static async Task<bool> StableDiffusionWebUILoaded()
        {
            //HttpClient client = GetShortHttpClient();
            string response = await RepeatGetStringAsync(() => GetShortHttpClient(), "/app_id");
            if (response.Contains("app_id"))
                return true;
            return false;
        }

        internal static async Task<IEnumerable<Texture2D>> UpscaleImages(IEnumerable<Texture2D> images, string upscaler, int width = 512, int height = 512)
        {
            List<Texture2D> upscaledTextures = new List<Texture2D>();

            foreach (Texture2D image in images)
            {
                string encodedImage = Convert.ToBase64String(image.EncodeToPNG());

#pragma warning disable IDE0017 // Simplify object initialization
                var request = new SDUpscaleRequest();
#pragma warning restore IDE0017 // Simplify object initialization
                request.resize_mode = 1;
                request.upscaling_resize_w = width;
                request.upscaling_resize_h = height;
                request.upscaling_crop = false;
                request.upscaler_1 = upscaler;
                request.upscaler_2 = upscaler;
                request.image = encodedImage;

                HttpClient client = GetHttpClient();

                var content = new StringContent(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync("/sdapi/v1/extra-single-image", content);

                string jsonString;
                if (response.IsSuccessStatusCode)
                    jsonString = await response.Content.ReadAsStringAsync();
                else
                {
                    Debug.LogError("Failed to upscale image.");
                    continue;
                }

                var sdResponse = JsonConvert.DeserializeObject<SDUpscaleResponse>(jsonString);

                var decoded = Convert.FromBase64String(sdResponse.image);

                Texture2D upscaledTexture = new Texture2D(1, 1);
                upscaledTexture.LoadImage(decoded);
                upscaledTextures.Add(upscaledTexture);
            }

            return upscaledTextures.ToArray();
        }

        internal static async Task<string> RepeatGetStringAsync(Func<HttpClient> httpClientAction, string path)
        {
            while (true)
            {
                string content;

                try
                {
                    content = await httpClientAction().GetStringAsync(path);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch
                {
                    continue;
                }
#pragma warning restore CA1031 // Do not catch general exception types

                return content;
            }
        }

        internal static bool LowVRAM => lowVRAM;

        internal static string Upscaler { get { return upscaler; } set { stableDiffusionSettings.upscaler = value; } }

        private static void ReloadSettingsIfNecessary()
        {
            if (serverAddress == null)
                LoadSettings();
        }

        private static StableDiffusionSettings ReadSettings()
        {
            StableDiffusionSettings settingsFile = AssetDatabase.LoadAssetAtPath<StableDiffusionSettings>("Assets/UntoldByte/GAINS/Settings/SDSettings.asset");
            if (settingsFile == null)
            {
                settingsFile = ScriptableObject.CreateInstance<StableDiffusionSettings>();
                settingsFile.serverAddress = "http://127.0.0.1:7860";
                settingsFile.upscaler = "SwinIR_4x";
            }
            return settingsFile;
        }

        internal static void LoadSettings()
        {
            stableDiffusionSettings = ReadSettings();
            CreateShortHttpClient();
            CreateHttpClient();
        }

        internal static void SaveSettings()
        {
            var settingsFile = ReadSettings();

            settingsFile.upscaler = upscaler;

            EditorUtility.SetDirty(settingsFile);
        }

        internal static bool IsOpenGLLike()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
                return true;

#if UNITY_2023_1_OR_NEWER
#else
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2)
                return true;
#endif

            return false;
        }

#region Upscalers
        
        internal static async Task<string[]> RefreshLatentUpscalersAsync()
        {
            string jsonUpscalersResponse = await RepeatGetStringAsync(() => GetShortHttpClient(), "/sdapi/v1/latent-upscale-modes");
            SDLatentUpscaler[] upscalersResponse = JsonConvert.DeserializeObject<SDLatentUpscaler[]>(jsonUpscalersResponse);
            string[] upscalers = upscalersResponse.Select(us => us.name).ToArray();
            return upscalers;
        }

        internal static async Task<string[]> RefreshUpscalersAsync()
        {
            string jsonUpscalersResponse = await RepeatGetStringAsync(() => GetShortHttpClient(), "/sdapi/v1/upscalers");
            SDUpscaler[] upscalersResponse = JsonConvert.DeserializeObject<SDUpscaler[]>(jsonUpscalersResponse);
            string[] upscalers = upscalersResponse.Select(us => us.name).ToArray();
            return upscalers;
        }

#endregion Upscalers

#region zip
        public static byte[] Compress(byte[] uncompressed)
        {
            MemoryStream memoryStream = new MemoryStream();
            Deflater deflater = new Deflater(Deflater.BEST_COMPRESSION);
            using (DeflaterOutputStream outputStream = new DeflaterOutputStream(memoryStream, deflater, 131072))
            {
                outputStream.Write(uncompressed, 0, uncompressed.Length);
            }

            return memoryStream.ToArray();
        }

        public static byte[] Decompress(byte[] compressed)
        {
            MemoryStream inputMemoryStream = new MemoryStream(compressed);
            Inflater inflater = new Inflater();
            MemoryStream outputMemoryStream = new MemoryStream();
            byte[] buffer = new byte[8192];
            using (InflaterInputStream mStream = new InflaterInputStream(inputMemoryStream, inflater, 131072))
            {
                int size;
                do
                {
                    size = mStream.Read(buffer, 0, buffer.Length);
                    outputMemoryStream.Write(buffer, 0, size);
                } while (size == buffer.Length);
            }

            return outputMemoryStream.ToArray();
        }
#endregion zip
    }
}