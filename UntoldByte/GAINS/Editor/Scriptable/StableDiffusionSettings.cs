using System;
using UnityEngine;

namespace UntoldByte.GAINS.Editor
{
    [Serializable]
    [CreateAssetMenu(fileName = "SDSettings", menuName = "ScriptableObjects/UntoldByte GAINS/SD Settings", order = 1)]
    public class StableDiffusionSettings : ScriptableObject
    {
        [Header("Stable Diffusion WebUI settings")]
        public string serverAddress;

        public bool lowVRAM;

        public string upscaler = "";

        public void OnValidate()
        {
            CommonUtilities.LoadSettings();
        }
    }
}
