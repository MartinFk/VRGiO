using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVoxelizer
{
    [ExecuteInEditMode]
    public class Voxel : MonoBehaviour
    {
        Mesh m_mesh = null;

        private void Awake()
        {
            m_mesh = new Mesh();
        }

        public void UpdateVoxel(Mesh mesh, Vector2 uv)
        {
            m_mesh.Clear();
            Vector2[] uvs = new Vector2[mesh.uv.Length]; 
            for (int i = 0; i < uvs.Length; ++i) uvs[i] = uv;
            m_mesh.vertices = mesh.vertices;
            m_mesh.uv = uvs;
            m_mesh.normals = mesh.normals;
            m_mesh.triangles = mesh.triangles;
            GetComponent<MeshFilter>().sharedMesh = m_mesh;
        }

        private void OnDestroy()
        {
            DestroyImmediate(m_mesh);
        }
    }
}