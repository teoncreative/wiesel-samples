using System;
using WieselEngine;

public class NetworkMenuScript : MonoBehavior
{
    UIDocumentComponent doc;

    string serverPort = "25000";
    string connectIp = "127.0.0.1";
    string connectPort = "25000";

    public override void OnStart()
    {
        doc = GetComponent<UIDocumentComponent>();
        if (doc == null)
        {
            return;
        }

        doc.SetString("server_port", serverPort);
        doc.SetString("connect_ip", connectIp);
        doc.SetString("connect_port", connectPort);
        doc.SetInt("role", 0);
        doc.SetString("status_text", "Disconnected");
    }

    public override void OnUpdate(float deltaTime)
    {
        if (doc == null)
        {
            return;
        }

        int role = (int)Network.Role;
        doc.SetInt("role", role);

        string status;
        switch (Network.Role)
        {
            case NetworkRole.Server:
                status = "Hosting (Server)";
                break;
            case NetworkRole.Client:
                status = Network.IsConnected ? "Connected (Client)" : "Connecting...";
                break;
            case NetworkRole.ListenServer:
                status = "Hosting (Listen Server)";
                break;
            default:
                status = "Disconnected";
                break;
        }
        doc.SetString("status_text", status);
    }

    public override void OnUIEvent(string eventName)
    {
        if (doc == null)
        {
            return;
        }

        switch (eventName)
        {
            case "host":
            {
                string portStr = doc.GetString("server_port");
                int port = 25000;
                int.TryParse(portStr, out port);
                Network.StartServer("0.0.0.0", port);
                NetworkSceneManager.LoadScene("sponza", LoadSceneMode.Single);
                break;
            }
            case "stop_server":
            {
                Network.StopServer();
                break;
            }
            case "connect":
            {
                string ip = doc.GetString("connect_ip");
                string portStr = doc.GetString("connect_port");
                int port = 25000;
                int.TryParse(portStr, out port);
                Network.ConnectToServer(ip, port);
                break;
            }
            case "disconnect":
            {
                Network.Disconnect();
                break;
            }
        }
    }

    public override void OnConnectedToServer()
    {
        Debug.Log("Connected to server");
    }

    public override void OnUIDataChanged(string variableName)
    {
        if (doc == null)
        {
            return;
        }

        switch (variableName)
        {
            case "server_port":
                serverPort = doc.GetString("server_port");
                break;
            case "connect_ip":
                connectIp = doc.GetString("connect_ip");
                break;
            case "connect_port":
                connectPort = doc.GetString("connect_port");
                break;
        }
    }
}
