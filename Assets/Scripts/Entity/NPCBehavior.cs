using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NPCBehavior : NetworkBehaviour
{
    public NetworkVariable<bool> ShowPlayers = new NetworkVariable<bool>();

    [SerializeField] bool showPlayersInitialValue;
    [SerializeField] float transparentAlphaValue = 0.5f;
    [SerializeField] List<GameObject> childObjectsToHide = new List<GameObject>();

    public delegate void ShowPlayersChanged(bool showPlayers);
    public ShowPlayersChanged OnShowPlayersChanged;

    Material opaqueMaterial;
    Material transparentMaterial;
    MeshRenderer meshRenderer;


    private void Awake()
    {
        ShowPlayers.OnValueChanged += (oldValue, newValue) =>
        {
            OnShowPlayersChanged?.Invoke(newValue);
        };
    }
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        //make two copies of the material
        opaqueMaterial = new Material(meshRenderer.material);
        transparentMaterial = new Material(meshRenderer.material);

        ConfigureTransparentMaterial(transparentMaterial);
    }

    public override void OnNetworkSpawn() { 
        base.OnNetworkSpawn();

        if (IsServer)
        {
            ToggleShowPlayers(showPlayersInitialValue);
        }
        else
        {
            SetClientVisibility(ShowPlayers.Value);
        }
    }

    public void ToggleShowPlayers(bool newValue)
    {
        if (IsServer)
        {
            ShowPlayers.Value = newValue;
            SwitchMaterial(!newValue);
            toggleShowPlayersClientRpc(newValue);
        }
        else
        {
            toggleShowPlayersServerRpc(newValue);
        }
    }
    [ServerRpc]
    private void toggleShowPlayersServerRpc(bool newValue)
    {
        ShowPlayers.Value = newValue;
        SwitchMaterial(!newValue);
        toggleShowPlayersClientRpc(newValue);
    }
    [ClientRpc]
    private void toggleShowPlayersClientRpc(bool newValue)
    {
        if (!IsHost)
        {
            SetClientVisibility(newValue);
        }
    }


    private void SwitchMaterial(bool useTransparentMaterial)
    {
        if (useTransparentMaterial)
        {
            meshRenderer.material = transparentMaterial;
        }
        else
        {
            meshRenderer.material = opaqueMaterial;
        }
    }

    // Configure a copy of the material to be transparent
    private void ConfigureTransparentMaterial(Material material)
    {
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.SetInt("_Surface", 1);

        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.SetShaderPassEnabled("DepthOnly", false);
        material.SetShaderPassEnabled("SHADOWCASTER", false);
        material.SetOverrideTag("RenderType", "Transparent");
        material.EnableKeyword("SURFACE_TYPE_TRANSPARENT");
        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

        if (material.HasProperty("_Color"))
        {
            material.color = new Color(material.color.r, material.color.g, material.color.b, transparentAlphaValue);
        }

    }

    public void SetClientVisibility(bool isShown)
    {
        meshRenderer.enabled = isShown;
        foreach (GameObject obj in childObjectsToHide)
        {
            obj.SetActive(isShown);
        }
    }
}
