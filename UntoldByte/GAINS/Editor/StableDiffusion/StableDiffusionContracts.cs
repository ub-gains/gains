using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UntoldByte.GAINS.Editor
{
    public interface IStableDiffusionModule
    {
        void WindowSetup(IHostEditorWindow hostEditorWindow);
        void SetHost(IHostEditorWindow hostEditorWindow);
        void OnGUI();
        Task MakeRequest();
        Task Cancel();
        void SetMode(StableDiffusionMode stableDiffusionMode);
        void SetControlnetPicture(Texture2D controlnetPicture);
        Task<IEnumerable<Texture2D>> RemBg(IEnumerable<Texture2D> image);
    }

    public enum StableDiffusionMode
    {
        None,
        Sketch,
        Color,
        Depth,
        ColorDepth
    }

    public interface IHostEditorWindow
    {
        bool GeneratingInitialized { get; }
        void RefreshUI();
        void UpdateProgress(float generateProgress, string generateProgressText);
        void UpdatePreview(bool generating, Texture2D previewTexture = null);
        void Update(bool generating, List<StableDiffusionResult> generatedTextures = null);
        Texture2D PackControlnetImage();
    }

    internal class SDRequest
    {
        public SDRequestOverrideSettings override_settings;
        public bool override_settings_restore_afterwards;

        public string prompt;
        public string negative_prompt;
        public string seed;
        public string sampler_name;
        public int batch_size;
        public int steps;
        public float cfg_scale;
        public int width;
        public int height;
        public string sampler_index;

        public bool enable_hr;
        public string hr_upscaler;
        public int hr_second_pass_steps;
        public float hr_scale;
        public float denoising_strength;
    }

    internal class SDRequestOverrideSettings
    {
        public string sd_model_checkpoint;
    }

    internal class SDControlnetRequest : SDRequest
    {
        public SDControlnetScriptRequest alwayson_scripts;
    }

    internal class SDControlnetScriptRequest
    {
        public ControlNetArgs ControlNet;
        [JsonProperty("tiled vae", NullValueHandling = NullValueHandling.Ignore)]
        public TiledVAEArgs TiledVAE;
    }

    internal class ControlNetArgs{
        public SDControlnetRequestControlnet[] args;
    }

    internal class TiledVAEArgs
    {
        public bool enabled;
        public int encoder_tile_size;
        public int decoder_tile_size;
        public bool vae_to_gpu;
        public bool fast_decoder;
        public bool fast_encoder;
        public bool color_fix;
    }

    internal class SDControlnetPreprocessorRequest
    {
        public string controlnet_module;
        public string[] controlnet_input_images;
        public float controlnet_processor_res;
        public float controlnet_threshold_a;
        public float controlnet_threshold_b;
    }

    internal class SDControlnetRequestControlnet
    {
        public bool enabled;
        public string input_image;

        public string module;
        public string model;

        public float weight;
        public float guidance;
        public float guidance_start;
        public float guidance_end;
        public bool lowvram;
        public float processor_res;
        public float threshold_a;
        public float threshold_b;
    }

    internal class SDColorDepthRequest : SDControlnetRequest
    {
        public string[] init_images;
        public bool include_init_images;
        //public float denoising_strength;
    }

    internal class SDResponse
    {
        public string[] images = default;
        public string info = default;
    }

    internal class SDResponseInfo
    {
#pragma warning disable CS0649
        public string seed;
#pragma warning restore CS0649
    }

    internal class SDUpscaleRequest
    {
        public int resize_mode;
        public int upscaling_resize_w;
        public int upscaling_resize_h;
        public bool upscaling_crop;
        public string upscaler_1;
        public string upscaler_2;
        public string image;
    }

    internal class SDUpscaleResponse
    {
        public string image = default;
        public string html_info = default;
    }

    internal class RembgRequest
    {
        public string input_image;
        public string model;
        public bool return_mask;
        public bool alpha_matting;
        public int alpha_matting_foreground_threshold;
        public int alpha_matting_background_threshold;
        public int alpha_matting_erode_size;
    }

    internal class RembgResponse
    {
        public string image = default;
    }

    //  "progress": 0.41000000000000003,
    //"eta_relative": 12.670998230213069,
    //"state": {
    //  "skipped": false,
    //  "interrupted": false,
    //  "job": "",
    //  "job_count": 1,
    //  "job_timestamp": "20230310201931",
    //  "job_no": 0,
    //  "sampling_step": 9,
    //  "sampling_steps": 20
    //},
    //"current_image": "

    internal class SDProgressResponse
    {
        public float progress = default;
        public float eta_relative = default;
        public SDProgressResponseState state = default;
        public string current_image = default;
        public string textinfo = default;
    }

    internal class SDProgressResponseState
    {
        public bool skipped = default;
        public bool interrupted = default;
        public string job = default;
        public int job_count = default;
        public string job_timestamp = default;
        public int job_no = default;
        public int sampling_step = default;
        public int sampling_steps = default;
    }

    [System.Serializable]
    public class SDModels
    {
        public SDModel[] sDModels;
    }

    [System.Serializable]
    public class SDModel
    {
        public string title = default;
    }

    internal class SDOptions
    {
        public string sd_model_checkpoint = default;
    }

    internal class SDSampler
    {
        public string name = default;
    }

    internal class SDLatentUpscaler
    {
        public string name = default;
    }

    internal class SDUpscaler
    {
        public string name = default;
        public float scale = default;
    }

    internal class SDControlnetModules
    {
        public string[] module_list = default;
    }

    internal class SDControlnetModels
    {
        public string[] model_list = default;
    }

    internal class SDScripts
    {
        public string[] txt2img;
        public string[] img2img;
    }
}