using System.Collections.Generic;
using UnityEngine;
using MVoxelizer;

public class Voxelizer
{
    private MeshVoxelizer meshVoxelizer = new MeshVoxelizer();

    public GameObject VoxelizeObject(GameObject objectToVoxel, Material centerMaterial, Mesh voxelMesh)
    {
        return VoxelizeObject(
            objectToVoxel,
            GenerationType.SeparateVoxels,
            VoxelSizeType.Subdivision,
            10, 0.05f,
            Precision.VeryHigh,
            true,
            false,
            false,
            0.0f,
            UVConversion.None,
            false, FillCenterMethod.None,
            true, centerMaterial,
            false,
            voxelMesh,
            Vector3.one,
            Vector3.zero);
    }

    public GameObject VoxelizeObject(
        GameObject sourceGameObject,
        GenerationType generationType,
        VoxelSizeType voxelSizeType,
        int subdivisionLevel, float absoluteVoxelSize, // VoxelSizeType = Subdivision (left), Absolute Size (right)
        Precision precision,
        bool approximation,
        bool ignoreScaling,
        bool alphaCutout,
        float CutoffValue, // alphaCutout = true
        UVConversion uvConversion,
        bool backfaceCulling, FillCenterMethod fillCenter, // GenerationType = SingleMesh (left), Separate Voxels (right)
        bool optimization, Material centerMaterial, // GenerationType = SingleMesh (left), Separate Voxels (right)
        bool modifyVoxel,
        Mesh voxelMesh,
        Vector3 voxelScale,
        Vector3 voxelRotation)
    {
        this.meshVoxelizer = new MeshVoxelizer();
        this.meshVoxelizer.sourceGameObject = sourceGameObject;
        this.meshVoxelizer.generationType = generationType;
        this.meshVoxelizer.voxelSizeType = voxelSizeType;
        if (voxelSizeType == VoxelSizeType.Subdivision)
        {
            this.meshVoxelizer.subdivisionLevel = subdivisionLevel;
        }
        else if (voxelSizeType == VoxelSizeType.AbsoluteSize)
        {
            this.meshVoxelizer.absoluteVoxelSize = absoluteVoxelSize;
        }
        this.meshVoxelizer.precision = precision;
        this.meshVoxelizer.approximation = approximation;
        this.meshVoxelizer.ignoreScaling = ignoreScaling;
        this.meshVoxelizer.alphaCutout = alphaCutout;
        if (alphaCutout)
        {
            this.meshVoxelizer.CutoffValue = CutoffValue;
        }
        this.meshVoxelizer.uvConversion = uvConversion;
        if (generationType == GenerationType.SingleMesh)
        {
            this.meshVoxelizer.backfaceCulling = backfaceCulling;
            this.meshVoxelizer.optimization = optimization;
        }
        else if (generationType == GenerationType.SeparateVoxels)
        {
            this.meshVoxelizer.fillCenter = fillCenter;
            if (!(fillCenter == FillCenterMethod.None))
            {
                this.meshVoxelizer.centerMaterial = centerMaterial;
            }
        }
        this.meshVoxelizer.modifyVoxel = modifyVoxel;
        if (modifyVoxel)
        {
            this.meshVoxelizer.voxelMesh = voxelMesh;
            this.meshVoxelizer.voxelScale = voxelScale;
            this.meshVoxelizer.voxelRotation = voxelRotation;
        }
        GameObject voxelObject = this.meshVoxelizer.VoxelizeMesh();
        this.meshVoxelizer = new MeshVoxelizer();
        return voxelObject;
    }
}

public class building_plan : MonoBehaviour
{
    public bool operationActive;
    public Server_Communication_Manager serverComManage;
    private Server_Communication sc;

    // public TextMeshProUGUI tmptxt;
    // private string[,,] plan = new string[5, 5, 5];
    // private string[] ip_list = new string[10];

    public GameObject objectToVoxel;
    private Material _centerMaterial = null;
    private Mesh _voxelMesh = null;
    private Voxelizer voxelizer = new Voxelizer();

    public GameObject cubePrefab;
    public Transform cubeParent;
    private float CUBESIZE = 1;
    public Material newCubeToStructure;
    public Material connectedToStructure;
    public Material disconnectedFromStructure;

    public Dictionary<string, GameObject> cubes = new Dictionary<string, GameObject>();
    public List<(Edge, Edge)> edges = new List<(Edge, Edge)>();
    public string origin;

    // Start is called before the first frame update
    void Start()
    {
        // Server_Communication.OnResponseIncoming += AcceptResponsebodies;
        sc = GameObject.FindGameObjectWithTag("ServerCommunication").GetComponent<Server_Communication>();
    }

    void Update()
    {
        List<GameObject> cubesNotChecked = new List<GameObject>(cubes.Values);
        foreach (string node in sc.nodes)
        {
            if (cubes.ContainsKey(node))
            {
                cubesNotChecked.Remove(cubes[node]);
            }
            else
            {
                GenerateCube(node);
            }
        }
        foreach (GameObject go in cubesNotChecked)
        {
            go.SetActive(false);
        }

        List<Edge> edgesAlreadyChecked = new List<Edge>();
        foreach (Edge edge in sc.edges)
        {
            foreach (Edge checkedEdge in edgesAlreadyChecked)
            {
                foreach ((Edge, Edge) edgepair in this.edges)
                {
                    if (edgepair.Item1 == edge || edgepair.Item2 == edge) continue;
                }
                if (edge.node1 == checkedEdge.node2 && edge.node2 == checkedEdge.node1)
                {
                    GameObject cube1 = cubes[edge.node1];
                    GameObject cube2 = cubes[edge.node2];
                    ChangeMaterial(cube1, connectedToStructure);
                    ChangeMaterial(cube2, connectedToStructure);
                    Place(edge, checkedEdge, cube1, cube2);

                    this.edges.Add((edge, checkedEdge));
                }
            }
            edgesAlreadyChecked.Add(edge);
        }
        foreach ((Edge, Edge) edgepair in this.edges.ToArray())
        {
            if (!(sc.edges.Contains(edgepair.Item1) || sc.edges.Contains(edgepair.Item2)))
            {
                this.edges.Remove(edgepair);

                if (!CanNodeAReachNodeB(edgepair.Item1.node1, edgepair.Item1.node2, ""))
                {
                    ChangeMaterial(cubes[edgepair.Item1.node1], disconnectedFromStructure);
                    foreach (string node in cubes.Keys)
                    {
                        if (CanNodeAReachNodeB(edgepair.Item1.node1, node, "")) ChangeMaterial(cubes[node], disconnectedFromStructure);
                    }
                }
            }
        }

        if (cubePrefab == null)
        {
            CUBESIZE = 1;
        }
        else
        {
            MeshFilter mf = cubePrefab.GetComponentInChildren<MeshFilter>();
            Vector3 meshSize = (mf != null && mf.sharedMesh != null && mf.sharedMesh.bounds != null && mf.sharedMesh.bounds.size != null) ? mf.sharedMesh.bounds.size : Vector3.one;
            CUBESIZE = meshSize.x;
        }

        List<string> alreadyPlaced = new List<string>();
        cubes[origin].transform.position = Vector3.zero;
        alreadyPlaced.Add(origin);
        foreach (string node in new List<string>(cubes.Keys))
        {
            if (node == origin) continue;
            foreach ((Edge, Edge) edgepair in this.edges)
            {
                if (edgepair.Item1.node1 == node)
                {
                    Place(edgepair.Item2, edgepair.Item1, cubes[edgepair.Item1.node2], cubes[node]);
                }
                else if (edgepair.Item1.node2 == node)
                {
                    Place(edgepair.Item1, edgepair.Item2, cubes[node], cubes[edgepair.Item1.node1]);
                }
            }
        }
    }

    void GenerateCube(string name)
    {
        GameObject newCube;
        if (cubePrefab == null)
        {
            newCube = cubes[name] = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }
        else
        {
            newCube = cubes[name] = Instantiate(cubePrefab, Vector3.zero, Quaternion.identity, cubeParent);
        }
        newCube.name = name;
        ChangeMaterial(newCube, newCubeToStructure);
        if (origin == null || origin == "") origin = name;
    }

    GameObject VoxelizeObject()
    {
        return voxelizer.VoxelizeObject(objectToVoxel, _centerMaterial, _voxelMesh);
    }

    bool CanNodeAReachNodeB(string nodeA, string nodeB, string previousNode)
    {
        foreach (Edge e in sc.edges)
        {
            if (e.node1 == nodeA)
            {
                if (e.node2 != nodeB && e.node2 != previousNode) CanNodeAReachNodeB(e.node2, nodeB, e.node1);
                else return true;
            }
        }
        return false;
    }

    void ChangeMaterial(GameObject obj, Material mat)
    {
        if (obj.TryGetComponent<MeshRenderer>(out MeshRenderer mr))
        {
            mr.material = mat;
        }
        else
        {
            mr = obj.GetComponentInChildren<MeshRenderer>();
            if (mr == null) return;
            mr.material = mat;
        }
    }

    void Place(Edge edge1, Edge edge2, GameObject cube1, GameObject cube2)
    {
        switch (edge1.side)
        {
            case "right":
                cube2.transform.SetPositionAndRotation(cube1.transform.position + cube1.transform.right * CUBESIZE, Quaternion.LookRotation(cube1.transform.right * -1));
                break;
            case "left":
                cube2.transform.SetPositionAndRotation(cube1.transform.position + cube1.transform.right * -CUBESIZE, Quaternion.LookRotation(cube1.transform.right));
                break;
            case "top":
                cube2.transform.SetPositionAndRotation(cube1.transform.position + cube1.transform.up * CUBESIZE, Quaternion.LookRotation(cube1.transform.up * -1));
                break;
            case "bottom":
                cube2.transform.SetPositionAndRotation(cube1.transform.position + cube1.transform.up * -CUBESIZE, Quaternion.LookRotation(cube1.transform.up));
                break;
            case "front":
                cube2.transform.SetPositionAndRotation(cube1.transform.position + cube1.transform.forward * CUBESIZE, Quaternion.LookRotation(cube1.transform.forward * -1));
                break;
            case "back":
                cube2.transform.SetPositionAndRotation(cube1.transform.position + cube1.transform.forward * -CUBESIZE, Quaternion.LookRotation(cube1.transform.forward));
                break;
        }
        switch (edge2.side)
        {
            case "right":
                cube2.transform.SetPositionAndRotation(cube2.transform.position, Quaternion.LookRotation(cube2.transform.right * -1));
                break;
            case "left":
                cube2.transform.SetPositionAndRotation(cube2.transform.position, Quaternion.LookRotation(cube2.transform.right));
                break;
            case "top":
                cube2.transform.SetPositionAndRotation(cube2.transform.position, Quaternion.LookRotation(cube2.transform.up * -1));
                break;
            case "bottom":
                cube2.transform.SetPositionAndRotation(cube2.transform.position, Quaternion.LookRotation(cube2.transform.up));
                break;
            case "front":
                cube2.transform.SetPositionAndRotation(cube2.transform.position, cube2.transform.rotation);
                break;
            case "back":
                cube2.transform.SetPositionAndRotation(cube2.transform.position, Quaternion.LookRotation(cube2.transform.forward * -1));
                break;
        }
    }
}
