using UnityEditor;

namespace UntoldByte.GAINS.Editor
{
    [CustomEditor(typeof(EntityMaterialManager))]
    public class EntityMaterialManagerEditor : UnityEditor.Editor, IEntityMaterialManagerHost
    {
        private bool initialized;
        private EntityMaterialManagerModule entityMaterialManagerModule;

        private void InitializeIfNeeded()
        {
            if (initialized) return;
            initialized = true;

            SetupEntityMaterilManagerModule();
        }

        private void SetupEntityMaterilManagerModule()
        {
            entityMaterialManagerModule = new EntityMaterialManagerModule();
            entityMaterialManagerModule.SetHost(this);
            entityMaterialManagerModule.SetTarget((EntityMaterialManager)target);
        }

        private void OnValidate()
        {
            SetupEntityMaterilManagerModule();
        }

        public override void OnInspectorGUI()
        {
            InitializeIfNeeded();

            if(entityMaterialManagerModule != null)
                entityMaterialManagerModule.OnGUI();
        }
    }
}
