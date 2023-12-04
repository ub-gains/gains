using System.Linq;
using UnityEngine;

namespace UntoldByte.GAINS.Editor
{
    public static class MeshUtilities
    {
        internal static void PrepareUVsForProjectiveInterpolation(Vector4[] UVs)
        {
            for (int i = 0; i < UVs.Length; i++)
            {
                UVs[i] = new Vector4(UVs[i].x / UVs[i].z, UVs[i].y / UVs[i].z, 1 / UVs[i].z, UVs[i].w);
            }
        }

        internal static Vector4[] GenerateSceneProjectionUVs(Camera camera, GameObject gameObject, bool isSimpleProjection = true)
        {
            Mesh mesh = GetSharedMesh(gameObject);
            if (mesh == null) return null;

            Vector3 cameraPosition = camera.transform.position;

            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;
            Transform transform = gameObject.transform;

            Vector4[] uvs = new Vector4[vertices.Length];

            if(!isSimpleProjection)
                for (int i = 0; i < uvs.Length; i++)
                {
                    uvs[i] = new Vector4(-10, -10, 100, 0);
                }

            bool isFrontFacing;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int index0 = triangles[i];
                int index1 = triangles[i + 1];
                int index2 = triangles[i + 2];

                Vector3 vertex0 = transform.TransformPoint(vertices[index0]);
                Vector3 vertex1 = transform.TransformPoint(vertices[index1]);
                Vector3 vertex2 = transform.TransformPoint(vertices[index2]);

                isFrontFacing = IsFrontFacing(vertex0, vertex1, vertex2, cameraPosition);

                if (isSimpleProjection || isFrontFacing)
                {
                    Vector3 vertex0VP = camera.WorldToViewportPoint(vertex0);
                    Vector3 vertex1VP = camera.WorldToViewportPoint(vertex1);
                    Vector3 vertex2VP = camera.WorldToViewportPoint(vertex2);
                    uvs[index0] = new Vector4(vertex0VP.x, vertex0VP.y, vertex0VP.z, isFrontFacing ? 1 : 0);
                    uvs[index1] = new Vector4(vertex1VP.x, vertex1VP.y, vertex1VP.z, isFrontFacing ? 1 : 0);
                    uvs[index2] = new Vector4(vertex2VP.x, vertex2VP.y, vertex2VP.z, isFrontFacing ? 1 : 0);
                }
            }

            return uvs;
        }

        internal static bool IsFrontFacing(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 cameraPosition)
        {
            Vector3 a = vertex1 - vertex0;
            Vector3 b = vertex2 - vertex0;
            Vector3 triangleNormal = Vector3.Cross(a, b);
            Vector3 cameraToTriangle = vertex0 - cameraPosition;

            bool isFrontFacing = Vector3.Dot(cameraToTriangle, triangleNormal) < 0;

            return isFrontFacing;
        }

        internal static Vector2 NearFarDepth(Vector4[] projectionUVs)
        {
            float near = projectionUVs.AsParallel().Min(p => p.z);
            float far = projectionUVs.AsParallel().Max(p => p.z);
            return new Vector2(near, far);
        }

        internal static Vector3 WorldToViewportPosition(Camera camera, Vector3 vertex)
        {
            Vector3 viewportPosition = camera.WorldToViewportPoint(vertex);
            return viewportPosition;
        }

        internal static Mesh GetSharedMesh(GameObject gameObject)
        {
            if (gameObject == null) return null;

            Mesh mesh = null;

            if (IsMesh(gameObject))
            {
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                if (meshFilter == null) return null;

                mesh = meshFilter.sharedMesh;
            }
            else if (IsSkinnedMesh(gameObject))
            {
                SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer == null) return null;

                mesh = skinnedMeshRenderer.sharedMesh;
            }

            return mesh;
        }

        internal static bool IsMesh(GameObject gameObject)
        {
            if (gameObject.TryGetComponent<MeshRenderer>(out _) && gameObject.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh != null)
            {
                return true;
            }

            return false;
        }

        internal static bool IsSkinnedMesh(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out SkinnedMeshRenderer skinnedMeshRenderer) && skinnedMeshRenderer.sharedMesh != null)
            {
                return true;
            }
            return false;
        }

        internal static bool CheckGameObjectRequirements(GameObject gameObject)
        {
            if (gameObject == null)
                return false;

            if (IsMesh(gameObject))
            {
                return true;
            }
            else if (IsSkinnedMesh(gameObject))
            {
                return true;
            }
            return false;
        }

        internal static Material[] GetSharedGameObjectMaterials(GameObject gameObject)
        {
            if (gameObject == null) return null;

            if (IsMesh(gameObject) && gameObject.TryGetComponent(out MeshRenderer meshRenderer))
            {
                return meshRenderer.sharedMaterials;
            }
            else if (IsSkinnedMesh(gameObject) && gameObject.TryGetComponent(out SkinnedMeshRenderer skinnedMeshRenderer))
            {
                return skinnedMeshRenderer.sharedMaterials;
            }

            return null;
        }

        internal static void SetSharedGameObjectMaterials(GameObject gameObject, Material[] materials)
        {
            if (IsMesh(gameObject) && gameObject.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.sharedMaterials = materials;
            }
            else if (IsSkinnedMesh(gameObject) && gameObject.TryGetComponent(out SkinnedMeshRenderer skinnedMeshRenderer))
            {
                skinnedMeshRenderer.materials = materials;
            }
        }

    }

}
