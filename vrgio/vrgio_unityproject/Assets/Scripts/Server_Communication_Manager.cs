using UnityEngine;

public class Server_Communication_Manager : MonoBehaviour
{
    public Server_Communication sc;

    [Header("RegisterComponent")]
    public string reg_src_ip;
    public string reg_type = "main";
    public string reg_component_class = "cube";

    public void RegisterComponent()
    {
        sc.RegisterComponent(reg_src_ip, reg_type, reg_component_class);
        Debug.Log("GetRequest sent: RegisterComponent");
    }

    [Header("ConnectComponents")]
    public string con_node_one_ip;
    public string con_side_one;
    public string con_node_two_ip;
    public string con_side_two;

    public void ConnectComponents()
    {
        sc.ConnectComponents(con_node_one_ip, con_node_two_ip, con_side_one, con_side_two);
        Debug.Log("GetRequest sent: ConnectComponents");
    }

    [Header("DisconnectComponents")]
    public string dis_node_one_ip;
    public string dis_node_two_ip;

    public void DisconnectComponents()
    {
        sc.DisconnectComponents(dis_node_one_ip, dis_node_two_ip);
        Debug.Log("GetRequest sent: DisconnectComponents");
    }

    public void ComponentCount()
    {
        sc.ComponentCount();
        Debug.Log("GetRequest sent: ComponentCount");
    }

    [Header("ComponentInfo")]
    public string inf_src_ip;

    public void ComponentInfo()
    {
        sc.ComponentInfo(inf_src_ip);
        Debug.Log("GetRequest sent: ComponentInfo");
    }

    [Header("ComponentActuate")]
    public string act_src_ip;
    public string act_json = "{}";

    public void ComponentActuate()
    {
        sc.ComponentActuate(act_src_ip, act_json);
        Debug.Log("PostRequest sent: ComponentActuate");
    }

    public void VisualizeGraph()
    {
        sc.VisualizeGraph();
        Debug.Log("GetRequest sent: VisualizeGraph");
    }

    public void GetIpInfo()
    {
        sc.GetIpInfo();
        Debug.Log("GetRequest sent: GetIpInfo");
    }
}
