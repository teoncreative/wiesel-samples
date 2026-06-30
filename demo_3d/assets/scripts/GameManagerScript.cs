using WieselEngine;

public class GameManagerScript : MonoBehavior
{
    public Prefab playerPrefab = new Prefab();

    public override void OnStart()
    {
        if (Network.IsServer)
        {
            SpawnPlayer(0);
        }
    }

    public override void OnClientConnected(ulong sessionId)
    {
        Debug.Log("Client connected: " + sessionId);
        SpawnPlayer(sessionId);
    }

    public override void OnConnectedToServer()
    {
        Debug.Log("Connected to server");
    }

    public override void OnClientDisconnected(ulong sessionId)
    {
        Debug.Log("Client disconnected: " + sessionId);
    }

    void SpawnPlayer(ulong ownerSessionId)
    {
        if (!playerPrefab.IsValid())
        {
            Debug.Log("Player prefab not set!");
            return;
        }

        Entity player = playerPrefab.Instantiate(Entity.Scene);
        if (player != null)
        {
            if (!player.HasComponent<NetworkIdentityComponent>())
            {
                player.AddComponent<NetworkIdentityComponent>();
            }
            string playerName = "Player " + ownerSessionId;
            player.GetComponent<TagComponent>().Name = playerName;
            var netId = player.GetComponent<NetworkIdentityComponent>();
            netId.OwnerSessionId = ownerSessionId;
            netId.Authority = ownerSessionId == 0 ? NetworkAuthority.Server : NetworkAuthority.Client;

            // The entity name is included in the spawn packet, so remote clients
            // will see this name on their copy of the entity. PlayerScript reads
            // it via Entity.GetComponent<TagComponent>().Name to set the nametag.

            Debug.Log("Spawned player for session " + ownerSessionId);
        }
    }
}
