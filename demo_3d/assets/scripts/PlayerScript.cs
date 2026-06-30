using WieselEngine;

public class PlayerScript : MonoBehavior
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float jumpForce = 8f;
    public Font nameTagFont = new Font();

    TransformComponent transform;
    TransformComponent cameraTransform;
    RigidBodyComponent rigidBody;
    UIDocumentComponent hud;
    UIDocumentComponent settings;
    BillboardTextComponent nameTag;

    // Networked variables - auto-synced across server and clients
    public NetworkVariable<int> health = new NetworkVariable<int>(100);
    public float camRotX = 0;
    public float camRotY = 0;
    bool grounded = false;
    bool settingsOpen = true;
    bool isLocalPlayer = false;

    // Stamina system: 4 charges, recharges one at a time (lowest empty first)
    const int maxCharges = 4;
    const float rechargeTime = 3.0f;
    int readyCharges = 4;
    float rechargingTimer = 0;

    public override void OnStart()
    {
        transform = GetComponent<TransformComponent>();
        rigidBody = GetComponent<RigidBodyComponent>();

        var netId = GetComponent<NetworkIdentityComponent>();
        isLocalPlayer = netId != null && netId.IsOwnedByUs();
        Debug.Log("isLocalPlayer: " + isLocalPlayer);

        if (isLocalPlayer)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupRemotePlayer();
        }

        health.OnValueChanged += (oldVal, newVal) =>
        {
            Debug.Log("Health: " + oldVal + " -> " + newVal);
        };
    }

    void SetupRemotePlayer()
    {
        // Create nametag as child entity so it follows the player
        Entity tagEntity = CreateEntity("NameTag");
        tagEntity.SetParent(Entity);
        tagEntity.AddComponent<BillboardTextComponent>();
        nameTag = tagEntity.GetComponent<BillboardTextComponent>();
        if (nameTag != null)
        {
            if (nameTagFont.IsValid())
            {
                nameTag.Font = nameTagFont;
            }
            var tag = Entity.GetComponent<TagComponent>();
            nameTag.Text = tag != null ? tag.Name : "Player";
            nameTag.FontSize = 32.0f;
            nameTag.Alignment = TextAlignment.Center;
            nameTag.Occlusion = BillboardOcclusion.Faded;
        }

        // Offset the tag upward above the player
        var tagTransform = tagEntity.GetComponent<TransformComponent>();
        if (tagTransform != null)
        {
            tagTransform.Position = new Vector3f(0.0f, 2.0f, 0.0f);
        }
    }

    void SetupLocalPlayer()
    {
        // Create camera as child entity
        Entity camEntity = CreateEntity("PlayerCamera");
        camEntity.SetParent(Entity);
        camEntity.AddComponent<CameraComponent>();
        cameraTransform = camEntity.GetComponent<TransformComponent>();
        cameraTransform.Position.X = 0.0f;
		cameraTransform.Position.Y = 1.35f;
        cameraTransform.Position.Z = 0.35f;
        
        SceneManager.LoadScene("ui", LoadSceneMode.Additive);
        Scene uiScene = SceneManager.FindScene("ui");
        if (uiScene != null)
        {
            Entity hudEntity = uiScene.FindEntity("HUD");
            if (hudEntity != null)
            {
                hud = hudEntity.GetComponent<UIDocumentComponent>();
            }

            Entity settingsEntity = uiScene.FindEntity("Settings");
            if (settingsEntity != null)
            {
                settings = settingsEntity.GetComponent<UIDocumentComponent>();
                settings.Visible = false;
            }
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (!settingsOpen)
        {
            Move(deltaTime);
            Look();
        }
        UpdateStamina(deltaTime);
    }

    void ToggleSettings()
    {
        settingsOpen = !settingsOpen;
        if (settings != null)
        {
            settings.Visible = settingsOpen;
        }
        if (settingsOpen)
        {
            Input.SetCursorMode(CursorMode.Normal);
        }
        else
        {
            Input.SetCursorMode(CursorMode.Relative);
        }
    }

    void UpdateStamina(float deltaTime)
    {
        if (readyCharges < maxCharges)
        {
            rechargingTimer += deltaTime;
            if (rechargingTimer >= rechargeTime)
            {
                readyCharges++;
                rechargingTimer = 0;
            }
        }

        if (hud != null)
        {
            for (int i = 0; i < maxCharges; i++)
            {
                if (i < readyCharges)
                {
                    hud.SetInt("stamina_" + i, 100);
                }
                else if (i == readyCharges)
                {
                    int fill = (int)(rechargingTimer / rechargeTime * 100);
                    hud.SetInt("stamina_" + i, fill);
                }
                else
                {
                    hud.SetInt("stamina_" + i, 0);
                }
            }
        }
    }

    bool ConsumeCharge()
    {
        if (readyCharges > 0)
        {
            readyCharges--;
            if (readyCharges < maxCharges && rechargingTimer == 0)
            {
                rechargingTimer = 0;
            }
            return true;
        }
        return false;
    }

    void Move(float deltaTime)
    {
        float x = 0;
        float z = 0;

        if (Input.GetKey("Left")) x = -1;
        if (Input.GetKey("Right")) x = 1;
        if (Input.GetKey("Up")) z = 1;
        if (Input.GetKey("Down")) z = -1;

        Vector3f move = new Vector3f(x, 0, z);
        Vector3f finalMove = move * moveSpeed * deltaTime;
        transform.Translate(finalMove);

        if (Input.GetKey("Jump") && grounded)
        {
            if (ConsumeCharge())
            {
                rigidBody.AddImpulse(new Vector3f(0, jumpForce, 0));
                grounded = false;
            }
        }
    }

    public override void OnCollisionEnter(Entity other)
    {
        grounded = true;
    }

    void Look()
    {
        if (cameraTransform == null)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        camRotY -= mouseX * mouseSensitivity;
        camRotX -= mouseY * mouseSensitivity;
        camRotX = Mathf.Clamp(camRotX, -89f, 89f);

        transform.Rotation = new Vector3f(0, camRotY, 0);
        cameraTransform.Rotation = new Vector3f(camRotX, 0, 0);
    }

    public override bool OnKeyPressed(KeyCode keyCode, bool repeat)
    {
        if (!isLocalPlayer)
        {
            return false;
        }

        if (keyCode == KeyCode.Escape)
        {
            ToggleSettings();
            return true;
        }
        if (keyCode == KeyCode.F && !repeat)
        {
            SendServerRpc("Interact", 10, "sword");
            return true;
        }
        return false;
    }

    // -- Network RPCs --

    [ServerRpc]
    public void Interact(int damage, string weapon)
    {
        Debug.Log("Player interacted (server-side) damage=" + damage + " weapon=" + weapon);
        health.Value -= damage;
        SendClientRpc("OnInteracted", damage, weapon);
    }

    [ClientRpc]
    public void OnInteracted(int damage, string weapon)
    {
        Debug.Log("Player hit with " + weapon + " for " + damage + "! Health: " + health.Value);
    }

}
