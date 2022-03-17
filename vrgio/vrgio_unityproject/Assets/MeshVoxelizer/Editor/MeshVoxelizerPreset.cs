using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVoxelizer
{
    [System.Serializable]
    public struct MeshVoxelizerSetting
    {
        public string presetName;
        public bool showProgressBar;

        //public GameObject sourceGameObject = null;
        public GenerationType generationType;
        public VoxelSizeType voxelSizeType;
        public int subdivisionLevel;
        public float absoluteVoxelSize;
        public Precision precision;
        public UVConversion uvConversion;
        public bool approximation;
        public bool ignoreScaling;
        public bool alphaCutout;
        public float CutoffValue;

        public bool modifyVoxel;
        public Mesh voxelMesh;
        public Vector3 voxelScale;
        public Vector3 voxelRotation;

        //single mesh
        public bool boneWeightConversion;
        public bool backfaceCulling;
        public bool optimization;
        public bool compactOutput;

        //separate voxels
        public FillCenterMethod fillCenter;
        public Material centerMaterial;

        public void RecordSetting(MeshVoxelizerEditor meshVoxelizer)
        {
            generationType      = meshVoxelizer.generationType;
            voxelSizeType       = meshVoxelizer.voxelSizeType;
            subdivisionLevel    = meshVoxelizer.subdivisionLevel;
            absoluteVoxelSize   = meshVoxelizer.absoluteVoxelSize;
            precision           = meshVoxelizer.precision;
            uvConversion        = meshVoxelizer.uvConversion;
            approximation       = meshVoxelizer.approximation;
            ignoreScaling       = meshVoxelizer.ignoreScaling;
            alphaCutout         = meshVoxelizer.alphaCutout;
            CutoffValue         = meshVoxelizer.CutoffValue;
            modifyVoxel         = meshVoxelizer.modifyVoxel;
            voxelMesh           = meshVoxelizer.voxelMesh;
            voxelScale          = meshVoxelizer.voxelScale;
            boneWeightConversion= meshVoxelizer.boneWeightConversion;
            backfaceCulling     = meshVoxelizer.backfaceCulling;
            optimization        = meshVoxelizer.optimization;
            fillCenter          = meshVoxelizer.fillCenter;
            centerMaterial      = meshVoxelizer.centerMaterial;
            compactOutput       = meshVoxelizer.compactOutput;
            showProgressBar     = meshVoxelizer.showProgressBar;
        }

        public void SetPresetName(string name)
        {
            presetName = name;
        }
    }

    //[CreateAssetMenu(fileName = "MeshVoxelizerPreset", menuName = "MeshVoxelizerWindow.meshVoxelizer/MeshVoxelizerPreset", order = 10)]
    public class MeshVoxelizerPreset : ScriptableObject
    {
        public List<MeshVoxelizerSetting> settings = new List<MeshVoxelizerSetting>();
    }
}