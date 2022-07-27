using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO.Ports;
using System.Globalization;
using System;
using TMPro;

public class serialCom : MonoBehaviour
{

    private static serialCom _instance;
    public static serialCom Instance
    {
        get { return _instance; }
    }

    [SerializeField] private TextMeshProUGUI txt_bat;   

    private Thread serialThread;
    SerialPort sp = new SerialPort();
    private bool comOpen = false;
    private Queue<string> dataQueue;
    private bool frameFlag = false;

    public Quaternion q;
    private float batLvl;
    private bool[] touchReg = { false, false, false, false, false, false };

    [SerializeField] string comPort = "COM3";
    public int baudrate = 230400;


    void Start(){

    }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        print("START");
        q = new Quaternion();
        serialThread = new Thread(StartThread);
        dataQueue = new Queue<string>();
        ConnectCOM();

    }


    public void StartThread(){
        string data;
        while (true){
            if (frameFlag){
                if (comOpen)
                {
                    try{
                        data = sp.ReadLine();
                        dataQueue.Enqueue(data);
                    }
                    catch{
                        print("Something went wrong! Data could not be read or enqueued");
                        comOpen = false;
                    }
                }
                frameFlag = false;
            }   
        }
    }


    public void ConnectCOM(){
        if (!sp.IsOpen){       
            sp.PortName = comPort;
            sp.BaudRate = baudrate;
            sp.Open();
            comOpen = true;

            if (serialThread.ThreadState == ThreadState.Unstarted){
                serialThread.Start();
            }
            print(comPort + " open - Listining SerialPort.");
        }
        else{
            comOpen = false;
            sp.Close();
            print(comPort + " closed.");
        }
    }

    // Update is called once per frame
    void Update(){
        frameFlag = true; // trigger serialRead thread once a frame
        if (dataQueue.Count > 0){
            lock (dataQueue){
                if (dataQueue.Count > 0)
                ParseData(dataQueue.Dequeue());
            }
        }
    }


    private void ParseData(string msg){
        if (msg.Length < 1) return;
        string[] token = msg.Split("/");

        switch (token[0]){
            case "q":
                handleQuat(token);
                break;

            case "b":
                handleBat(token);
                break;

            case "t":
                handleTouch(token);
                break;

            case "r":
                handleReleaase(token);
                break;

            default:
                //print(msg);
                break;
        }
    }


    private void handleQuat(string[] values){

        Quaternion qt = new Quaternion(
            float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(values[3], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(values[4], CultureInfo.InvariantCulture.NumberFormat)
            );
        
        q.w = qt.w;
        q.x = qt.y;
        q.y = qt.x;
        q.z = qt.z;

    }

    public void sendReset()
    {
        sendCommand("c", 4);
    }

    public void sendVibration()
    {
        sendCommand("v",17); 
        
    }

    private void sendCommand(string id, int val, int opt = 1)
    {
        sp.Write(id);
        sp.Write(((char)val).ToString());
        sp.Write(((char)opt).ToString());
        sp.Write("$");

    }

    private void handleBat(string[] values){

        batLvl = (float.Parse(values[1]) + 200) / 100;
        txt_bat.text = batLvl.ToString("0.00") + " V";
        //print(batLvl);
    }

    private void handleTouch(string[] values)
    {
        print("T/" + values[1]);

    }

    private void handleReleaase(string[] values)
    {
        print("R/" + values[1]);

    }

    public void sendColorSide(int side, int colorID)
    {
        sendCommand(side.ToString(), colorID);
    }

    public Quaternion getCubeQuat(){
        return q;
    }

    public void sendWhiteFlash(){
        sendCommand("b", 127);

    }

    public void sendFullColor(int colorId)
    {
        sendCommand("F", colorId);

    }

}
