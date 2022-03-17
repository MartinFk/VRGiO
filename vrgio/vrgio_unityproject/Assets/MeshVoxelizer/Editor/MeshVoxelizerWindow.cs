using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MVoxelizer
{
    public class MeshVoxelizerWindow : EditorWindow
    {
        const string tooltip_SourceGameObject = 
            "The game object to be voxelized";
        const string tooltip_GenerationType = 
            "-Single Mesh: Generated result as a single mesh. " +
            "-Separate Voxels: Generate a group of individual voxel game objects";
        const string tooltip_VoxelSizeType =
            "-Subdivision: Set voxel size by subdivision level. " +
            "-Absolute Size: Set voxel size by absolute voxel size.";
        const string tooltip_Precision = 
            "Precision level, the result will be more accurate with higher precision, while voxelization time will increase.";
        const string tooltip_Approximation = 
            "Approximate voxels around original mesh's edge/corner, make voxelization result more smooth. " +
            "This is useful when voxelizing origanic objects";
        const string tooltip_IgnoreScaling = 
            "Ignore source GameObject's local scale.";
        const string tooltip_AlphaCutout = 
            "Discard transparent voxels.";
        const string tooltip_UVConversionType = 
            "-None: Generated mesh will not have any UV." +
            "-Source Mesh: Convert UVs from the source mesh." +
            "-Voxel Mesh: Keep individual voxel's UV.";
        const string tooltip_ConvertBoneWeights =
            "Convert bone weight from the source mesh.";
        const string tooltip_BackfaceCulling =
            "Cull backface.";
        const string tooltip_Optimization = 
            "Optimize voxelization result.";
        const string tooltip_FillCenterSpace = 
            "Fill model's center space with voxels. " +
            "Try different axis if the result is incorrect.";
        const string tooltip_CenterMaterial = 
            "Material for center voxels.";
        const string tooltip_ModifyVoxel =
            "Use custom voxel instead of default cube voxel. " +
            "Enabling this will disable Backface Culling and Optimization.";
        const string tooltip_VoxelMesh = 
            "Basic mesh for voxel.";
        const string tooltip_VoxelScale = 
            "Scale individual voxel.";
        const string tooltip_VoxelRotation =
            "Rotate individual voxel.";
        const string tooltip_ShowProgressBar = 
            "Show/Hide progress bar.";
        const string tooltip_CompactOutput = 
            "Zip all generated assets in a single game object.";
        const string tooltip_ExportVoxelizedTexture = 
            "Export generated textures as png images.";

        static MeshVoxelizerEditor meshVoxelizer;
        static bool advancedOption;
        static MeshVoxelizerPreset preset;
        static string presetName;
        Vector2 scrollPos;

        [MenuItem("Window/Mesh Voxelizer")]
        static void Init()
        {
            MeshVoxelizerWindow window = (MeshVoxelizerWindow)GetWindow(typeof(MeshVoxelizerWindow), false, "Mesh Voxelizer");
            window.Show();
            presetName = "Mesh Voxelizer Preset";
        }

        private void OnDestroy()
        {
            if (preset != null)
            {
                MeshVoxelizerSetting setting = new MeshVoxelizerSetting();
                setting.SetPresetName("DefaultSetting");
                setting.RecordSetting(meshVoxelizer);
                preset.settings[0] = setting;
                EditorUtility.SetDirty(preset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                preset = null;
            }
            meshVoxelizer = null;
        }

        void OnGUI()
        {
            if (meshVoxelizer == null)
            {
                meshVoxelizer = new MeshVoxelizerEditor();
                meshVoxelizer.centerMaterial = MVHelper.GetDefaultVoxelMaterial();
                meshVoxelizer.voxelMesh = MVHelper.GetDefaultVoxelMesh();
            }

            if (preset == null)
            {
                preset = AssetDatabase.LoadAssetAtPath<MeshVoxelizerPreset>("Assets/MeshVoxelizer/Resources/MeshVoxelizerPreset.asset");
                if (preset == null)
                {
                    preset = ScriptableObject.CreateInstance<MeshVoxelizerPreset>();
                    AssetDatabase.CreateAsset(preset, "Assets/MeshVoxelizer/Resources/MeshVoxelizerPreset.asset");
                }
                if (preset.settings == null)
                {
                    preset.settings = new List<MeshVoxelizerSetting>();
                }
                if (preset.settings.Count == 0)
                {
                    MeshVoxelizerSetting setting = new MeshVoxelizerSetting();
                    setting.SetPresetName("DefaultSetting");
                    setting.RecordSetting(meshVoxelizer);
                    preset.settings.Add(setting);
                    EditorUtility.SetDirty(preset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                else
                {
                    meshVoxelizer.ApplySetting(preset.settings[0]);
                }
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            meshVoxelizer.sourceGameObject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Source GameObject", tooltip_SourceGameObject), meshVoxelizer.sourceGameObject, typeof(GameObject), true);
            Mesh sourceMesh = null;
            bool isMeshRenderer = true;
            if (meshVoxelizer.sourceGameObject != null)
            {
                if (meshVoxelizer.sourceGameObject.GetComponent<MeshRenderer>() != null)
                {
                    sourceMesh = meshVoxelizer.sourceGameObject.GetComponent<MeshFilter>().sharedMesh;
                    isMeshRenderer = true;
                }
                else if (meshVoxelizer.sourceGameObject.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    sourceMesh = meshVoxelizer.sourceGameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                    isMeshRenderer = false;
                }
            }
            MVInt3 sizeInfo = new MVInt3();
            float maxAbsoluteVoxelSize = 1.0f;
            if (sourceMesh != null)
            {
                Vector3 sourceMeshSize = sourceMesh.bounds.size;
                if (meshVoxelizer.ignoreScaling)
                {
                    sourceMeshSize.x *= meshVoxelizer.sourceGameObject.transform.localScale.x;
                    sourceMeshSize.y *= meshVoxelizer.sourceGameObject.transform.localScale.y;
                    sourceMeshSize.z *= meshVoxelizer.sourceGameObject.transform.localScale.z;
                }
                maxAbsoluteVoxelSize = MVHelper.GetMax(sourceMeshSize.x, sourceMeshSize.y, sourceMeshSize.z);
                Vector3 unit = new Vector3();
                if (meshVoxelizer.voxelSizeType == VoxelSizeType.Subdivision)
                {
                    unit.x = maxAbsoluteVoxelSize / meshVoxelizer.subdivisionLevel;
                    unit.y = maxAbsoluteVoxelSize / meshVoxelizer.subdivisionLevel;
                    unit.z = maxAbsoluteVoxelSize / meshVoxelizer.subdivisionLevel;
                }
                else
                {
                    unit.x = meshVoxelizer.absoluteVoxelSize;
                    unit.y = meshVoxelizer.absoluteVoxelSize;
                    unit.z = meshVoxelizer.absoluteVoxelSize;
                }
                unit *= 1.00001f;
                sizeInfo.x = Mathf.CeilToInt(sourceMeshSize.x / unit.x);
                sizeInfo.y = Mathf.CeilToInt(sourceMeshSize.y / unit.y);
                sizeInfo.z = Mathf.CeilToInt(sourceMeshSize.z / unit.z);
            }
            if (sizeInfo.x == 0) sizeInfo.x = 1;
            if (sizeInfo.y == 0) sizeInfo.y = 1;
            if (sizeInfo.z == 0) sizeInfo.z = 1;

            meshVoxelizer.generationType = (GenerationType)EditorGUILayout.EnumPopup(new GUIContent("Generation Type", tooltip_GenerationType), meshVoxelizer.generationType);
            meshVoxelizer.voxelSizeType = (VoxelSizeType)EditorGUILayout.EnumPopup(new GUIContent("Voxel Size Type", tooltip_VoxelSizeType), meshVoxelizer.voxelSizeType);

            string info = "Info: ";
            if (meshVoxelizer.voxelSizeType == VoxelSizeType.Subdivision)
            {
                meshVoxelizer.subdivisionLevel = EditorGUILayout.IntSlider("Subdivision Level", meshVoxelizer.subdivisionLevel, 1, MeshVoxelizer.MAX_SUBDIVISION);
                info += "  W:" + sizeInfo.x + "  L:" + sizeInfo.z + "  H:" + sizeInfo.y;
                if (meshVoxelizer.subdivisionLevel > 300)
                {
                    info += "\nExtremely high subdivision level may cause unstable result and take very long time to process";
                }
                else if (meshVoxelizer.subdivisionLevel > 100)
                {
                    info += "\nHigh subdivision level will take longer time to process";
                }
            }
            else
            {
                meshVoxelizer.absoluteVoxelSize = EditorGUILayout.Slider("Absolute Voxel Size", meshVoxelizer.absoluteVoxelSize, maxAbsoluteVoxelSize / MeshVoxelizer.MAX_SUBDIVISION, maxAbsoluteVoxelSize);
                info += "W:" + sizeInfo.x + " L:" + sizeInfo.z + " H:" + sizeInfo.y;
                if (meshVoxelizer.absoluteVoxelSize < maxAbsoluteVoxelSize / 300.0f)
                {
                    info += "\nExtremely small voxel size may cause unstable result and take very long time to process";
                }
                else if (meshVoxelizer.absoluteVoxelSize < maxAbsoluteVoxelSize / 100.0f)
                {
                    info += "\nSmall voxel size will take longer time to process";
                }
            }
            if (sourceMesh == null) info = "Please select a valid source GameObject";
            EditorGUILayout.HelpBox(info, MessageType.None);

            meshVoxelizer.precision = (Precision)EditorGUILayout.EnumPopup(new GUIContent("Precision", tooltip_Precision), meshVoxelizer.precision);
            meshVoxelizer.approximation = EditorGUILayout.Toggle(new GUIContent("Approximation", tooltip_Approximation), meshVoxelizer.approximation);
            meshVoxelizer.ignoreScaling = EditorGUILayout.Toggle(new GUIContent("Ignore Scaling", tooltip_IgnoreScaling), meshVoxelizer.ignoreScaling);
            meshVoxelizer.alphaCutout = EditorGUILayout.Toggle(new GUIContent("Alpha Cutout", tooltip_AlphaCutout), meshVoxelizer.alphaCutout);
            if (meshVoxelizer.alphaCutout)
            {
                meshVoxelizer.CutoffValue = EditorGUILayout.Slider("Cutoff Value", meshVoxelizer.CutoffValue, 0.0f, 1.0f);
            }
            
            meshVoxelizer.uvConversion = (UVConversion)EditorGUILayout.EnumPopup(new GUIContent("UV Conversion Type", tooltip_UVConversionType), meshVoxelizer.uvConversion);
            if (meshVoxelizer.generationType == GenerationType.SingleMesh)
            {
                if (sourceMesh != null && sourceMesh.vertices.Length == sourceMesh.boneWeights.Length)
                {
                    EditorGUI.BeginDisabledGroup(meshVoxelizer.optimization);
                    meshVoxelizer.boneWeightConversion = EditorGUILayout.Toggle(new GUIContent("Bone Weight Conversion", tooltip_ConvertBoneWeights), meshVoxelizer.boneWeightConversion);
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.BeginDisabledGroup(meshVoxelizer.modifyVoxel);
                meshVoxelizer.backfaceCulling = EditorGUILayout.Toggle(new GUIContent("Backface Culling", tooltip_BackfaceCulling), meshVoxelizer.backfaceCulling);
                if (isMeshRenderer) meshVoxelizer.optimization = EditorGUILayout.Toggle(new GUIContent("Optimization", tooltip_Optimization), meshVoxelizer.optimization);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                meshVoxelizer.fillCenter = (FillCenterMethod)EditorGUILayout.EnumPopup(new GUIContent("Fill Center Space", tooltip_FillCenterSpace), meshVoxelizer.fillCenter);
                EditorGUI.BeginDisabledGroup(meshVoxelizer.fillCenter == FillCenterMethod.None);
                meshVoxelizer.centerMaterial = (Material)EditorGUILayout.ObjectField(new GUIContent("Center Material", tooltip_CenterMaterial), meshVoxelizer.centerMaterial, typeof(Material), true);
                EditorGUI.EndDisabledGroup();
            }

            if (!isMeshRenderer)
            {
                info = "For animated object, it's recommended to enable \"Modify Voxel\" option.";
                EditorGUILayout.HelpBox(info, MessageType.None);
            }
            meshVoxelizer.modifyVoxel = EditorGUILayout.BeginToggleGroup(new GUIContent("Modify Voxel", tooltip_ModifyVoxel), meshVoxelizer.modifyVoxel);
            meshVoxelizer.voxelMesh = (Mesh)EditorGUILayout.ObjectField(new GUIContent("Voxel Mesh", tooltip_VoxelMesh), meshVoxelizer.voxelMesh, typeof(Mesh), true);
            meshVoxelizer.voxelScale = EditorGUILayout.Vector3Field(new GUIContent("Voxel Scale", tooltip_VoxelScale), meshVoxelizer.voxelScale);
            meshVoxelizer.voxelRotation = EditorGUILayout.Vector3Field(new GUIContent("Voxel Rotation", tooltip_VoxelRotation), meshVoxelizer.voxelRotation);
            EditorGUILayout.EndToggleGroup();

            EditorGUI.BeginDisabledGroup(sourceMesh == null);
            if (GUILayout.Button("Voxelize Mesh"))
            {
                meshVoxelizer.VoxelizeMesh();
                EditorGUIUtility.ExitGUI();
            }
            EditorGUI.EndDisabledGroup();

            advancedOption = EditorGUILayout.Foldout(advancedOption, "Advanced Options");
            if (advancedOption)
            {
                meshVoxelizer.showProgressBar = EditorGUILayout.Toggle(new GUIContent("Show Progress Bar", tooltip_ShowProgressBar), meshVoxelizer.showProgressBar);
                meshVoxelizer.compactOutput = EditorGUILayout.Toggle(new GUIContent("Compact Output", tooltip_CompactOutput), meshVoxelizer.compactOutput);
                EditorGUI.BeginDisabledGroup(meshVoxelizer.compactOutput);
                meshVoxelizer.exportVoxelizedTexture = EditorGUILayout.Toggle(new GUIContent("Export Voxelized Texture", tooltip_ExportVoxelizedTexture), meshVoxelizer.exportVoxelizedTexture);
                EditorGUI.EndDisabledGroup();
                if (preset != null)
                {
                    if (preset.settings == null) preset.settings = new List<MeshVoxelizerSetting>();
                    EditorGUILayout.LabelField("Mesh voxelizer presets");
                    presetName = EditorGUILayout.TextField("Preset name", presetName);
                    if (GUILayout.Button("Add preset"))
                    {
                        MeshVoxelizerSetting setting = new MeshVoxelizerSetting();
                        setting.SetPresetName(presetName);
                        setting.RecordSetting(meshVoxelizer);
                        preset.settings.Add(setting);
                        EditorUtility.SetDirty(preset);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    List<int> toDelete = new List<int>();
                    for (int i = 1; i < preset.settings.Count; ++i)
                    {
                        EditorGUILayout.LabelField(preset.settings[i].presetName);
                        if (GUILayout.Button("Load preset"))
                        {
                            meshVoxelizer.ApplySetting(preset.settings[i]);
                        }
                        if (GUILayout.Button("Overwrite preset"))
                        {
                            MeshVoxelizerSetting setting = new MeshVoxelizerSetting();
                            setting.SetPresetName(presetName);
                            setting.RecordSetting(meshVoxelizer);
                            preset.settings[i] = setting;
                            EditorUtility.SetDirty(preset);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        if (GUILayout.Button("Delete preset"))
                        {
                            toDelete.Add(i);
                        }
                    }
                    if (toDelete.Count > 0)
                    {
                        for (int i = 0; i < toDelete.Count; ++i)
                        {
                            preset.settings.RemoveAt(i);
                        }
                        EditorUtility.SetDirty(preset);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
    }

    [CustomEditor(typeof(VoxelGroup))]
    public class VoxelGroupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            VoxelGroup voxelGroup = (VoxelGroup)target;

            if (GUILayout.Button("Rebuild Voxels"))
            {
                voxelGroup.RebuildVoxels();
            }

            if (GUILayout.Button("Reset Voxels"))
            {
                voxelGroup.ResetVoxels();
            }
        }
    }
}