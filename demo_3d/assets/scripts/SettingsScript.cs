using WieselEngine;

public class SettingsScript : MonoBehavior
{
    UIDocumentComponent doc;
    PlayerScript playerScript;
    CameraComponent playerCamera;

    // Buffered settings
    bool vsync;
    bool ssao;
    bool bloom;
    bool motionBlur;
    bool shadows;
    bool rtShadows;
    int aaMode;
    int masterVolume;
    int musicVolume;
    int sfxVolume;
    int sensitivity;
    int shadowQuality;
    int aniso;
    int textureQuality;
    int fov;

    public override void OnStart()
    {
        doc = GetComponent<UIDocumentComponent>();

        Entity player = FindEntity("Player");
        if (player != null)
        {
            playerScript = player.GetComponent<PlayerScript>();
            Entity cameraEntity = player.GetChild(0);
            if (cameraEntity != null)
            {
                playerCamera = cameraEntity.GetComponent<CameraComponent>();
            }
        }

        LoadFromEngine();
        PushToUI();
    }

    void LoadFromEngine()
    {
        vsync = Settings.VSync;
        ssao = Settings.SSAO;
        bloom = Settings.Bloom;
        motionBlur = Settings.MotionBlur;
        shadows = Settings.Shadows;
        rtShadows = Settings.RTShadows;
        aaMode = (int)Settings.AAMode;
        masterVolume = (int)(Settings.MasterVolume * 100);
        musicVolume = (int)(Settings.MusicVolume * 100);
        sfxVolume = (int)(Settings.SFXVolume * 100);
        shadowQuality = Settings.ShadowQuality;
        aniso = Settings.AnisotropicFiltering;
        textureQuality = Settings.TextureQuality;
        if (playerScript != null)
        {
            sensitivity = (int)(playerScript.mouseSensitivity * 25);
        }
        if (playerCamera != null)
        {
            fov = (int)playerCamera.FieldOfView;
        }
    }

    void PushToUI()
    {
        if (doc == null)
        {
            return;
        }

        doc.SetBool("vsync", vsync);
        doc.SetBool("ssao", ssao);
        doc.SetBool("bloom", bloom);
        doc.SetBool("motion_blur", motionBlur);
        doc.SetBool("shadows", shadows);
        doc.SetBool("rt_supported", Settings.IsRTSupported);
        doc.SetBool("rt_shadows", rtShadows);
        doc.SetInt("aa_mode", aaMode);
        doc.SetInt("master_volume", masterVolume);
        doc.SetInt("music_volume", musicVolume);
        doc.SetInt("sfx_volume", sfxVolume);
        doc.SetInt("sensitivity", sensitivity);
        doc.SetInt("shadow_quality", shadowQuality);
        doc.SetInt("aniso", aniso);
        doc.SetInt("texture_quality", textureQuality);
        doc.SetInt("fov", fov);
    }

    void ApplyToEngine()
    {
        Settings.VSync = vsync;
        Settings.SSAO = ssao;
        Settings.Bloom = bloom;
        Settings.MotionBlur = motionBlur;
        Settings.Shadows = shadows;
        Settings.RTShadows = rtShadows;
        Settings.AAMode = (AntiAliasingMode)aaMode;
        Settings.MasterVolume = masterVolume / 100f;
        Settings.MusicVolume = musicVolume / 100f;
        Settings.SFXVolume = sfxVolume / 100f;
        Settings.ShadowQuality = shadowQuality;
        Settings.AnisotropicFiltering = aniso;
        Settings.TextureQuality = textureQuality;
        if (playerScript != null)
        {
            playerScript.mouseSensitivity = sensitivity / 25f;
        }
        if (playerCamera != null)
        {
            playerCamera.FieldOfView = fov;
        }
    }

    public override void OnUIEvent(string eventName)
    {
        switch (eventName)
        {
            case "toggle_vsync":
                vsync = !vsync;
                PushToUI();
                break;
            case "toggle_ssao":
                ssao = !ssao;
                PushToUI();
                break;
            case "toggle_bloom":
                bloom = !bloom;
                PushToUI();
                break;
            case "toggle_motion_blur":
                motionBlur = !motionBlur;
                PushToUI();
                break;
            case "toggle_shadows":
                shadows = !shadows;
                PushToUI();
                break;
            case "toggle_rt_shadows":
                rtShadows = !rtShadows;
                PushToUI();
                break;
            case "apply":
                ApplyToEngine();
                break;
            case "cancel":
                LoadFromEngine();
                PushToUI();
                break;
        }
    }

    public override void OnUIDataChanged(string variableName)
    {
        if (doc == null)
        {
            return;
        }

        switch (variableName)
        {
            case "aa_mode":
                aaMode = doc.GetInt("aa_mode");
                break;
            case "master_volume":
                masterVolume = doc.GetInt("master_volume");
                break;
            case "music_volume":
                musicVolume = doc.GetInt("music_volume");
                break;
            case "sfx_volume":
                sfxVolume = doc.GetInt("sfx_volume");
                break;
            case "sensitivity":
                sensitivity = doc.GetInt("sensitivity");
                break;
            case "shadow_quality":
                shadowQuality = doc.GetInt("shadow_quality");
                break;
            case "aniso":
                aniso = doc.GetInt("aniso");
                break;
            case "texture_quality":
                textureQuality = doc.GetInt("texture_quality");
                break;
            case "fov":
                fov = doc.GetInt("fov");
                break;
        }
    }
}
