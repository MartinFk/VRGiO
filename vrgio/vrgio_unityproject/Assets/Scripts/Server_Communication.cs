using Newtonsoft.Json;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using WebSocketSharp;

[Serializable]
public class Edge
{
    public string node1;
    public string node2;
    public string side;
}

[Serializable]
public class Touch
{
    public string src_ip;
    public List<int> sides;
}

[Serializable]
public class ReceivingJsonSchema
{
    public string type;
    public bool expect_answer;
    public List<string> nodes;
    public List<Edge> edges;
    public List<object> additional_data;
}

[Serializable]
public class SendingJsonSchema
{
    public string type;
    public bool expect_answer;
    public bool success;
    public bool actuate;
    public List<object> data;
}

public class Server_Communication : MonoBehaviour
{
    // URI
    private string API_URI = "http://localhost:8000"; //192.168.43.68
    private string WEBSOCKET_URI = "ws://localhost:8000";

    private WebSocket websocket;
    private float lastPingTime;
    private float pingInterval = 1f;

    public string stringData;

    public List<string> nodes;
    public List<Edge> edges;
    public List<Touch> sides_touched;

    // List of response bodies, sorted from newest(0) to oldest(n)
    // response bodies contain status about an operation
    private List<Dictionary<string, object>> responseList = new List<Dictionary<string, object>>();
    private string latestResponseBody;
    private string newResponseBody;

    // Delegate for new incoming responses.
    // Subscribe with Server_Communication.OnResponseIncoming += {YourMethod};
    // Unsubscribe with Server_Communication.OnResponseIncoming -= {YourMethod};
    public delegate void newResponseDelegate(Dictionary<string, object> response);
    public static event newResponseDelegate OnResponseIncoming;

    public String additional_data = "\"1\":\"2\"";
    /* ----------------------------------------------------------------------------------------- */
    // Update: adds new server responses to the responseDictList
    private void Update()
    {
        if (Time.time >= lastPingTime + pingInterval)
        {
            lastPingTime = Time.time;
        }


        if (newResponseBody != latestResponseBody)
        {
            Dictionary<string, object> responseDict;
            responseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(newResponseBody);
            responseList.Insert(0, responseDict);

            latestResponseBody = newResponseBody;

            // Call all methods subscribed to the responseList
            if (OnResponseIncoming != null)
            {
                OnResponseIncoming(responseDict);
            }
        }
    }

    /// <summary>
    /// Gets the response body of a request in the response body list by how many requests have been issued since.
    /// [REMINDER] For you to get the correct response it is required that all your issued requests have indeed sent a response already.
    /// </summary>
    /// <param name="howManyRequestsAgo">The amount of requests that have been sent after the request you're looking for. Defaults to 0.</param>
    /// <returns>The response that was received of the request which was sent before the last "howManyRequestsAgo" requests.</returns>
    public Dictionary<string, object> GetResponseBodyOfRequest(int howManyRequestsAgo = 0)
    {
        return responseList[howManyRequestsAgo];
    }

    /* ----------------------------------------------------------------------------------------- */
    // WEBSOCKET
    public IEnumerator InitWebSocket()
    {
        using (WebSocket ws = new WebSocket(WEBSOCKET_URI + "/unity_ws"))
        {
            ws.OnOpen += (sender, e) =>
            {
                Debug.Log("connect to: " + sender.ToString());
                ws.SendAsync("{\"type\":\"json\", \"expect_answer\": true, \"data\": \"Connected\"}", SendComplete);
            };
            ws.OnError += (sender, e) =>
            {
                Debug.Log(e.Exception);
            };
            ws.OnMessage += (sender, e) =>
            {
                if (e.IsText)
                {
                    Debug.Log("Received: " + e.Data);
                    stringData = e.Data;
                    try
                    {
                        var data = JsonConvert.DeserializeObject<ReceivingJsonSchema>(stringData);
                        AreEqual(data.type, "json");
                        nodes = data.nodes;
                        edges = data.edges;
                        if (data.additional_data != null)
                        {
                            foreach (object item in data.additional_data)
                            {
                                if (item.GetType() == typeof(List<Touch>))
                                {
                                    sides_touched = (List<Touch>)item;
                                }
                            }
                        }
                        if (data.expect_answer)
                        {
                            bool actuate;
                            List<object> data = new List<object>();
                            string message = JsonConvert.SerializeObject(new SendingJsonSchema()
                            {
                                type = "json",
                                expect_answer = true,
                                success = true,
                                actuate = actuate,
                                data = data
                            });
                            ws.SendAsync(message, SendComplete);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                    }
                    ws.SendAsync("{\"type\":\"json\", \"expect_answer\": true, \"data\": {" + additional_data + "}}", SendComplete);
                }
                else if (e.IsBinary)
                {
                    Debug.Log(e.RawData);
                }
            };
            ws.OnClose += (sender, e) =>
            {
                Debug.Log("closed connection.");
            };

            ws.ConnectAsync();
            websocket = ws;
            while (true)
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public void SendComplete(bool success)
    {
        //Debug.Log("Send completed with: " + success);
    }

    void OnApplicationQuit()
    {
        Debug.Log("Quitting. Disconnecting websocket.");
        websocket.SendAsync("disconnect", SendComplete);
        websocket.CloseAsync();
    }

    /* ----------------------------------------------------------------------------------------- */
    // GetRequest: Performs a Get-Request on the server, given the request URI (and additional arguments)
    IEnumerator GetRequest(string uri, params Tuple<string, string>[] args)
    {
        foreach (Tuple<string, string> arg in args)
        {
            if (arg == args[0])
            {
                uri += "?";
            }
            else
            {
                uri += "&";
            }
            uri += arg.Item1 + "=" + arg.Item2;
        }

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    newResponseBody = webRequest.downloadHandler.text;
                    yield break;
            }
        }
    }

    /* ----------------------------------------------------------------------------------------- */
    // PostRequest: Performs a Post-Request on the server, given the request URI and a json string
    IEnumerator PostRequest(string uri, string json, params Tuple<string, string>[] args)
    {
        foreach (Tuple<string, string> arg in args)
        {
            if (arg == args[0])
            {
                uri += "?";
            }
            else
            {
                uri += "&";
            }
            uri += arg.Item1 + "=" + arg.Item2;
        }

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }


    /* ----------------------------------------------------------------------------------------- */
    /// <summary>
    /// Adds new node i.e., cube to Graph, which gets connected in the shape structure physically. 
    /// The returned response can be received by subscribing to the Event OnResponseIncoming.
    /// Alternatively it can be read through the method GetResponseBody().
    /// </summary>
    /// <param name="src_ip">[description]</param>
    /// <param name="type">[description]. Defaults to "main".</param>
    /// <param name="component_class">[description]. Defaults to "cube".</param>    
    public void RegisterComponent(string src_ip, string type = "main", string component_class = "cube")
    {
        string uri = API_URI + "/register/component";
        Tuple<string, string>[] argArray = new Tuple<string, string>[]
        {
            new Tuple<string, string>("src_ip", src_ip),
            new Tuple<string, string>("type", type),
            new Tuple<string, string>("component_class", component_class)
        };
        Coroutine coroutine = StartCoroutine(GetRequest(uri, argArray));
    }

    /* ----------------------------------------------------------------------------------------- */
    /// <summary>
    /// Etasblishes bi-directional connection in Graph between two nodes representing the physical components that got attached to each other.
    /// The returned response can be received by subscribing to the Event OnResponseIncoming.
    /// Alternatively it can be read through the method GetResponseBody().
    /// </summary>
    /// <param name="node_ip"></param>
    /// <param name="side"></param>
    public void ConnectComponents(string node_ip, string side)
    {
        string uri = API_URI + "/connect/components";
        Tuple<string, string>[] argArray = new Tuple<string, string>[]
        {
            new Tuple<string, string>("node_ip", node_ip),
            new Tuple<string, string>("side", side)
        };
        Coroutine coroutine = StartCoroutine(GetRequest(uri, argArray));
    }

    public void ConnectComponents(string node_one_ip, string node_two_ip, string side_one, string side_two)
    {
        ConnectComponents(node_one_ip, side_one);
        ConnectComponents(node_two_ip, side_two);
    }

    /* ----------------------------------------------------------------------------------------- */
    /// <summary>
    /// Removes bi-directional connection in Graph between two nodes representing the physical components that got attached to each other.
    /// The returned response can be received by subscribing to the Event OnResponseIncoming.
    /// Alternatively it can be read through the method GetResponseBody().
    /// </summary>
    /// <param name="node_one_ip">Node's IP which got a new node attached to it.</param>
    /// <param name="node_two_ip">The new node's IP that got itself attached.</param>
    public void DisconnectComponents(string node_one_ip, string node_two_ip)
    {
        string uri = API_URI + "/disconnect/components";
        Tuple<string, string>[] argArray = new Tuple<string, string>[]
        {
            new Tuple<string, string>("node_one_ip", node_one_ip),
            new Tuple<string, string>("node_two_ip", node_two_ip)
        };
        Coroutine coroutine = StartCoroutine(GetRequest(uri, argArray));
    }

    /* ----------------------------------------------------------------------------------------- */
    /// <summary>
    /// Should return the total count of components.
    /// The returned response can be received by subscribing to the Event OnResponseIncoming.
    /// Alternatively it can be read through the method GetResponseBody().
    /// </summary>
    public void ComponentCount()
    {
        string uri = API_URI + "/component/count";
        Coroutine coroutine = StartCoroutine(GetRequest(uri));
    }

    /* ----------------------------------------------------------------------------------------- */
    /// <summary>
    /// Should return metadata about any given node using its IP address as the identifier. For inspecting any node for its neighbors and its own type.
    /// The returned response can be received by subscribing to the Event OnResponseIncoming.
    /// Alternatively it can be read through the method GetResponseBody().
    /// </summary>
    /// <param name="src_ip">[description]</param>
    public void ComponentInfo(string src_ip)
    {
        string uri = API_URI + "/component/info";
        Tuple<string, string>[] argArray = new Tuple<string, string>[]
        {
            new Tuple<string, string>("src_ip", src_ip)
        };
        Coroutine coroutine = StartCoroutine(GetRequest(uri, argArray));
    }

    /* ----------------------------------------------------------------------------------------- */
    /// <summary>
    /// Actuates the physical component or performs message passing from one component to another.
    /// </summary>
    /// <param name="src_ip">IP address of the component to be actuated.</param>
    /// <param name="json">[description]</param>
    public void ComponentActuate(string src_ip, string json)
    {
        string uri = API_URI + "/component/actuate";
        Tuple<string, string>[] argArray = new Tuple<string, string>[]
        {
            new Tuple<string, string>("src_ip", src_ip)
        };
        StartCoroutine(PostRequest(uri, json, argArray));
    }

    /* ----------------------------------------------------------------------------------------- */
    /// <summary>
    /// Visualizes the entire Graph with all components shown.
    /// The returned response can be received by subscribing to the Event OnResponseIncoming.
    /// Alternatively it can be read through the method GetResponseBody().
    /// </summary>
    public void VisualizeGraph()
    {
        string uri = API_URI + "/visualize/graph";
        StartCoroutine(GetRequest(uri));
    }//viel erfolg :)

    public void GetIpInfo()
    {
        string uri = API_URI + "/component/ip_info";
        StartCoroutine(GetRequest(uri));
    }

    private void Start()
    {
        StartCoroutine(InitWebSocket());
        // RegisterComponent("ip1");
        // RegisterComponent("ip2");
        // RegisterComponent("ip3");
        // RegisterComponent("ip4");
        // ConnectComponents("ip4", "left");
        // ConnectComponents("ip2", "top");
        // ConnectComponents("ip4", "ip3", "right", "bottom");
        // DisconnectComponents("ip4", "ip2");
        // ConnectComponents("ip3", "ip2", "front", "back");
        // ComponentCount();
        // ComponentInfo("ip3");
        // ComponentActuate("ip3", "{}");
        // VisualizeGraph();
    }
}