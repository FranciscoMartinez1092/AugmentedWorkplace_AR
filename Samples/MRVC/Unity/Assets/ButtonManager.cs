using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine;
using System.Collections.Generic;

public class ButtonManager : MonoBehaviour, IInputClickHandler, IInputHandler
{
    public GameObject SpatialMappingObject;
    public GameObject Canvas;
    public CustomNetworkManager networkManager;
    private LogStates logger;

    private bool Observing = true;
    void Start ()
    {
        // Add debug text logger
        logger = Canvas.GetComponent<LogStates>();
    }
    public void OnInputClicked(InputClickedEventData eventData)
    {
        // AirTap code goes here
        List<Mesh> meshes = SpatialMappingObject.GetComponent<SpatialMappingManager>().GetMeshes();
        logger.AppendMeshDebugText(meshes.Count.ToString() + " meshes.");
        foreach (Mesh mesh in meshes)
        {
            networkManager.sendSpatialMesh(mesh);
        }
        //if (!Observing)
        //{
        //    SpatialMappingObject.GetComponent<SpatialMappingManager>().StartObserver();
        //    Observing = !Observing;
        //} else
        //{
        //    SpatialMappingObject.GetComponent<SpatialMappingManager>().StopObserver();
        //    Observing = !Observing;
        //}
    }
    public void OnInputDown(InputEventData eventData)
    { }
    public void OnInputUp(InputEventData eventData)
    { }
}