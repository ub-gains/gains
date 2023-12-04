using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace UntoldByte.GAINS.Editor
{
    public static class StableDiffusionControlNetPreprocessorSettings
    {
        private static Dictionary<string, ControlNetPreprocessorParameterSettings[]> cachedSettings;

        internal static Dictionary<string, ControlNetPreprocessorParameterSettings[]> GetControlNetPreprocessorSettings()
        {
            if(cachedSettings == null || cachedSettings.Count == 0)
                cachedSettings = JsonConvert.DeserializeObject<Dictionary<string, ControlNetPreprocessorParameterSettings[]>>(JsonString());
            return cachedSettings;
        }

        internal class ControlNetPreprocessorParameterSettings
        {
#pragma warning disable CS0649
            public string name;
            public float value;
            public float min;
            public float max;
#pragma warning restore CS0649
            public float step = 1;
        }

        static string JsonString()
        {
            string controlnetPreprocessorJsonSettings = @"
{
    ""none"": [],
    ""inpaint"": [],
    ""inpaint_only"": [],
    ""revision_clipvision"": [
        null,
        {
            ""name"": ""Noise Augmentation"",
            ""value"": 0.0,
            ""min"": 0.0,
            ""max"": 1.0
        },
    ],
    ""revision_ignore_prompt"": [
        null,
        {
            ""name"": ""Noise Augmentation"",
            ""value"": 0.0,
            ""min"": 0.0,
            ""max"": 1.0
        },
    ],
    ""canny"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""value"": 512,
            ""min"": 64,
            ""max"": 2048,
            ""step"": 8
        },
        {
            ""name"": ""Canny Low Threshold"",
            ""value"": 100,
            ""min"": 1,
            ""max"": 255
        },
        {
            ""name"": ""Canny High Threshold"",
            ""value"": 200,
            ""min"": 1,
            ""max"": 255
        },
    ],
    ""mlsd"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""min"": 64,
            ""max"": 2048,
            ""value"": 512,
            ""step"": 8
        },
        {
            ""name"": ""MLSD Value Threshold"",
            ""min"": 0.01,
            ""max"": 2.0,
            ""value"": 0.1,
            ""step"": 0.01
        },
        {
            ""name"": ""MLSD Distance Threshold"",
            ""min"": 0.01,
            ""max"": 20.0,
            ""value"": 0.1,
            ""step"": 0.01
        }
    ],
    ""hed"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""min"": 64,
            ""max"": 2048,
            ""value"": 512,
            ""step"": 8
        }
    ],
    ""scribble_pidinet"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""min"": 64,
            ""max"": 2048,
            ""value"": 512,
            ""step"": 8
        }
    ],
    ""scribble_hed"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""min"": 64,
            ""max"": 2048,
            ""value"": 512,
            ""step"": 8
        }
    ],
    ""hed_safe"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""min"": 64,
            ""max"": 2048,
            ""value"": 512,
            ""step"": 8
        }
    ],
    ""openpose"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""min"": 64,
            ""max"": 2048,
            ""value"": 512,
            ""step"": 8
        }
    ],
    ""openpose_full"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""min"": 64,
            ""max"": 2048,
            ""value"": 512,
            ""step"": 8
        }
    ],
    ""dw_openpose_full"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""min"": 64,
            ""max"": 2048,
            ""value"": 512,
            ""step"": 8
        }
    ],
    ""segmentation"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""min"": 64,
            ""max"": 2048,
            ""value"": 512,
            ""step"": 8
        }
    ],
    ""depth"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""min"": 64,
            ""max"": 2048,
            ""value"": 512,
            ""step"": 8
        }
    ],
    ""depth_leres"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""min"": 64,
            ""max"": 2048,
            ""value"": 512,
            ""step"": 8
        },
        {
            ""name"": ""Remove Near %"",
            ""min"": 0,
            ""max"": 100,
            ""value"": 0,
            ""step"": 0.1,
        },
        {
            ""name"": ""Remove Background %"",
            ""min"": 0,
            ""max"": 100,
            ""value"": 0,
            ""step"": 0.1,
        }
    ],
    ""depth_leres++"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""min"": 64,
            ""max"": 2048,
            ""value"": 512,
            ""step"": 8
        },
        {
            ""name"": ""Remove Near %"",
            ""min"": 0,
            ""max"": 100,
            ""value"": 0,
            ""step"": 0.1,
        },
        {
            ""name"": ""Remove Background %"",
            ""min"": 0,
            ""max"": 100,
            ""value"": 0,
            ""step"": 0.1,
        }
    ],
    ""normal_map"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""min"": 64,
            ""max"": 2048,
            ""value"": 512,
            ""step"": 8
        },
        {
            ""name"": ""Normal Background Threshold"",
            ""min"": 0.0,
            ""max"": 1.0,
            ""value"": 0.4,
            ""step"": 0.01
        }
    ],
    ""threshold"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""value"": 512,
            ""min"": 64,
            ""max"": 2048,
            ""step"": 8
        },
        {
            ""name"": ""Binarization Threshold"",
            ""min"": 0,
            ""max"": 255,
            ""value"": 127
        }
    ],

    ""scribble_xdog"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""value"": 512,
            ""min"": 64,
            ""max"": 2048,
            ""step"": 8
        },
        {
            ""name"": ""XDoG Threshold"",
            ""min"": 1,
            ""max"": 64,
            ""value"": 32,
        }
    ],
    ""blur_gaussian"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""value"": 512,
            ""min"": 64,
            ""max"": 2048,
            ""step"": 8
        },
        {
            ""name"": ""Sigma"",
            ""min"": 0.01,
            ""max"": 64.0,
            ""value"": 9.0,
        }
    ],
    ""tile_resample"": [
        null,
        {
            ""name"": ""Down Sampling Rate"",
            ""value"": 1.0,
            ""min"": 1.0,
            ""max"": 8.0,
            ""step"": 0.01
        }
    ],
    ""tile_colorfix"": [
        null,
        {
            ""name"": ""Variation"",
            ""value"": 8.0,
            ""min"": 3.0,
            ""max"": 32.0,
            ""step"": 1.0
        }
    ],
    ""tile_colorfix+sharp"": [
        null,
        {
            ""name"": ""Variation"",
            ""value"": 8.0,
            ""min"": 3.0,
            ""max"": 32.0,
            ""step"": 1.0
        },
        {
            ""name"": ""Sharpness"",
            ""value"": 1.0,
            ""min"": 0.0,
            ""max"": 2.0,
            ""step"": 0.01
        }
    ],
    ""reference_only"": [
        null,
        {
            ""name"": 'Style Fidelity (only for ""Balanced"" mode)',
            ""value"": 0.5,
            ""min"": 0.0,
            ""max"": 1.0,
            ""step"": 0.01
        }
    ],
    ""reference_adain"": [
        null,
        {
            ""name"": 'Style Fidelity (only for ""Balanced"" mode)',
            ""value"": 0.5,
            ""min"": 0.0,
            ""max"": 1.0,
            ""step"": 0.01
        }
    ],
    ""reference_adain+attn"": [
        null,
        {
            ""name"": 'Style Fidelity (only for ""Balanced"" mode)',
            ""value"": 0.5,
            ""min"": 0.0,
            ""max"": 1.0,
            ""step"": 0.01
        }
    ],
    ""inpaint_only+lama"": [],
    ""color"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""value"": 512,
            ""min"": 64,
            ""max"": 2048,
            ""step"": 8
        }
    ],
    ""mediapipe_face"": [
        {
            ""name"": ""Preprocessor Resolution"",
            ""value"": 512,
            ""min"": 64,
            ""max"": 2048,
            ""step"": 8
        },
        {
            ""name"": ""Max Faces"",
            ""value"": 1,
            ""min"": 1,
            ""max"": 10,
            ""step"": 1
        },
        {
            ""name"": ""Min Face Confidence"",
            ""value"": 0.5,
            ""min"": 0.01,
            ""max"": 1.0,
            ""step"": 0.01
        }
    ],
    ""recolor_luminance"": [
        null,
        {
            ""name"": ""Gamma Correction"",
            ""value"": 1.0,
            ""min"": 0.1,
            ""max"": 2.0,
            ""step"": 0.001
        }
    ],
    ""recolor_intensity"": [
        null,
        {
            ""name"": ""Gamma Correction"",
            ""value"": 1.0,
            ""min"": 0.1,
            ""max"": 2.0,
            ""step"": 0.001
        }
    ],
}";
            return controlnetPreprocessorJsonSettings;
        }
    }

}