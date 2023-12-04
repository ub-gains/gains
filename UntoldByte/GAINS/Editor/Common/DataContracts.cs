using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UntoldByte.GAINS.Editor
{
    public enum PrototypeType
    {
        Sketch,
        ColorSnap,
        DepthSnap,
        ColorDepthSnap
    }

    public interface IPrototype
    {
        int Id { get; set; }
        PrototypeType Type { get; set; }
        Texture2D Texture { get; set; }
        Texture2D SecondTexture { get; set; }
    }

    [System.Serializable]
    public class Prototype : IPrototype
    {
        protected int id;
        protected PrototypeType type;
        protected byte[] textureBytes;
        protected byte[] secondTextureBytes;

        protected TextureFormat textureFormat;
        protected TextureFormat secondTextureFormat;

#pragma warning disable CA2235 // Mark all non-serializable fields
        protected Vector2 textureDimensions;
        protected Vector2 secondTextureDimensions;
#pragma warning restore CA2235 // Mark all non-serializable fields

        [System.NonSerialized]
        protected Texture2D texture;
        [System.NonSerialized]
        protected Texture2D secondTexture;

        public int Id { get => id; set => id = value; }
        public PrototypeType Type { get => type; set => type = value; }
        public virtual Texture2D Texture 
        { 
            get 
            {
                if (texture != null) return texture;
                if (textureBytes == null || textureBytes.Length == 0) return null;
                texture = TextureUtilities.ToTexture2D(textureBytes, textureFormat);
                return texture;
            }
            set
            {
                if (value == null) return;
                textureFormat = value.format;
                textureDimensions = new Vector2(value.width, value.height);

                byte[] textureBytes = TextureUtilities.ToByteArray(value);
                Object.DestroyImmediate(texture);
                this.textureBytes = textureBytes;
            }
        }
        public virtual Texture2D SecondTexture
        {
            get
            {
                if (secondTexture != null) return secondTexture;
                if (secondTextureBytes == null || secondTextureBytes.Length == 0) return null;
                secondTexture = TextureUtilities.ToTexture2D(secondTextureBytes, secondTextureFormat);
                return secondTexture;
            }
            set
            {
                if (value == null) return;
                secondTextureFormat = value.format;
                secondTextureDimensions = new Vector2(value.width, value.height);

                byte[] secondTextureBytes = TextureUtilities.ToByteArray(value);
                Object.DestroyImmediate(secondTexture);
                this.secondTextureBytes = secondTextureBytes;
            }
        }

        internal void SetTextureBytes(byte[] textureBytes)
        {
            this.textureBytes = textureBytes;
        }

        internal void SetSecondTextureBytes(byte[] secondTextureBytes)
        {
            this.secondTextureBytes = secondTextureBytes;
        }
    }

    [System.Serializable]
    public class EntityPrototype: Prototype
    {
#pragma warning disable CA2235 // Mark all non-serializable fields
#pragma warning disable CA1819 // Properties should not return arrays
        public Vector4[] UVs { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning restore CA2235 // Mark all non-serializable fields
    }

    public interface IPrototypeUpdater
    {
        void UpdatePrototype(Prototype prototype);
    }

    public interface IPrototypeEditor
    {
        void SetParent(IPrototypeUpdater prototypeUpdater);
        void SetPrototype(IPrototype prototype);
    }

    public interface IEntityMaterialManagerHost 
    {
        void Repaint();
    }

    public interface IEntityMaterialManagerModule
    {
        void OnGUI();
    }
}