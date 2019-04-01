using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class DrawLine : MonoBehaviour, IInputHandler, IInputClickHandler, IHoldHandler, ISourcePositionHandler, ISourceStateHandler {

    public GameObject lineGeneratorPrefab;
    public GameObject linePointPrefab;
    public GameObject SpatialMapping;
    private Vector3 lastPoint;
    private List<GameObject> lineGeneratorList;
    private bool firstPoint = true;
    private bool mouseHeld = false;
    //private float currentDepth = 0.0f;
    private HoloToolkit.Unity.SpatialMapping.SpatialMappingManager SpatialMappingManager;
    private bool Holding = false;
    
    // Use this for initialization
    void Start() {
        // Setup fallback handler for hand gestures
        InputManager.Instance.PushFallbackInputHandler(gameObject);

        SpatialMappingManager = SpatialMapping.GetComponent<HoloToolkit.Unity.SpatialMapping.SpatialMappingManager>();
        // Store list of lines
        lineGeneratorList = new List<GameObject>();
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("Input Clicked");
        Vector3 position = Vector3.zero;
        //if (eventData.InputSource.TryGetPointerPosition(eventData.SourceId, out position))
        //{
        //    Debug.Log("Pointer: " + position);
        //}
        if (eventData.InputSource.TryGetGripPosition(eventData.SourceId, out position))
        {
            Debug.Log("Grip: " + position);
        }
        //eventData.Use();
    }

    public void OnInputDown(InputEventData eventData)
    {
        Debug.Log("Input Down");
        InteractionSourceInfo source;
        eventData.InputSource.TryGetSourceKind(eventData.SourceId, out source);
        Debug.Log("Source: " + source);
        Holding = true;
        //Vector3 handPosition;
        //eventData.InputSource.TryGetGripPosition(eventData.SourceId, out handPosition);
        //Vector3 screenHandPosition = Camera.main.WorldToScreenPoint(handPosition);
        //RaycastHit hitInfo;
        //if (Physics.Raycast(Camera.main.ScreenPointToRay(screenHandPosition), out hitInfo, SpatialMappingManager.LayerMask)) {
        //    currentDepth = hitInfo.point.z;
        //}
        //else
        //{
        //    currentDepth = handPosition.z;
        //}
    }

    public void OnInputUp(InputEventData eventData)
    {
        Debug.Log("Input Up");
        if (Holding)
        {
            Debug.Log("Stopped holding");
            Holding = false;

            // Create line if a line was being drawn
            // mouseHeld = false;
            firstPoint = true;
            GenerateNewLine();
            ClearAllPoints();
            //eventData.Use();
        }
    }

    public void OnHoldStarted(HoldEventData eventData)
    {
        Debug.Log("Holding started");
        //Holding = true;
        //eventData.Use();
    }
    public void OnHoldCompleted(HoldEventData eventData)
    {
        Debug.Log("Holding completed without movement");
        //Holding = false;

        //// Create line if a line was being drawn
        //// mouseHeld = false;
        //firstPoint = true;
        //GenerateNewLine();
        //ClearAllPoints();
        ////eventData.Use();
    }

    public void OnHoldCanceled(HoldEventData eventData)
    {
        //Debug.Log("Canceled holding");
        //Holding = false;

        //// Create line if a line was being drawn
        //// mouseHeld = false;
        //firstPoint = true;
        //GenerateNewLine();
        //ClearAllPoints();
        //eventData.Use();
    }

    public void OnSourceDetected(SourceStateEventData eventData)
    {   
        Debug.Log(gameObject.transform.GetChild(0));
        Debug.Log("Before: " + gameObject.transform.GetChild(0).gameObject.activeInHierarchy);
        Debug.Log("Hand detected");
        Vector3 position = Vector3.zero;
        uint sourceId = eventData.SourceId;
        eventData.InputSource.TryGetGripPosition(sourceId, out position);
        gameObject.transform.GetChild(0).transform.position = position;
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        Debug.Log("After: " + gameObject.transform.GetChild(0).gameObject.activeInHierarchy);
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {
        Debug.Log("Hand lost");
        Holding = false;

        firstPoint = true;
        GenerateNewLine();
        ClearAllPoints();

        gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void OnPositionChanged(SourcePositionEventData eventData)
    {
        Vector3 newPos = eventData.GripPosition;
        // Update hand cursor position
        gameObject.transform.GetChild(0).transform.position = newPos;
        gameObject.transform.GetChild(0).rotation = Camera.main.transform.rotation;
        // Set z to raycast collision if hit
        //Ray cameraToPoint = new Ray(Camera.main.transform.position, newPos - Camera.main.transform.position);
        //RaycastHit hitInfo;
        //if (Physics.Raycast(cameraToPoint, out hitInfo)) //, SpatialMappingManager.LayerMask))
        //{
        //    Debug.Log("Hit: " + hitInfo.collider.gameObject.name);
        //    newPos.z = hitInfo.point.z;
        //}

        //Debug.DrawRay(eventData.GripPosition, newPos - eventData.GripPosition, color: Color.red, duration:0);
        // Debug.Log("Position Changed: " + eventData.GripPosition);
        if (Holding)
        {
            // Debug.Log("Gripping: " + eventData.GripPosition);
            if (firstPoint || !newPos.Equals(lastPoint))
            {

                CreateLinePoint(newPos);
                lastPoint = newPos;
            }
            firstPoint = false;
        }
    }
    //void Update() {
    //    // Check left mouseclick
    //    if (Input.GetMouseButton(0)) {
    //        mouseHeld = true;

    //        // Take raycast and create line point at a depth of z + 2 or at the nearest object hit
    //        RaycastHit hit;
    //        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    //        Vector3 newPos = ray.GetPoint(2);

    //        if (Physics.Raycast(ray, out hit)) {
    //            Debug.Log(transform.gameObject);
    //            Vector3 objectHit = hit.point;
    //            float distanceHit = hit.distance;
    //            if (distanceHit <= 2) {
    //                newPos.z = hit.point.z;
    //            }
    //        }
    //        // Create line points if it is different from the last point
    //        if (firstPoint || !newPos.Equals(lastPoint)) {
    //            CreateLinePoint(newPos);    
    //            lastPoint = newPos;
    //        }
    //        firstPoint = false;
    //    } else if (mouseHeld) { // Create line if a line was being drawn
    //        mouseHeld = false;
    //        firstPoint = true;
    //        GenerateNewLine();
    //        ClearAllPoints();
    //    }

    //    // Clear current points with right click (manual)
    //    if (Input.GetMouseButtonDown(1)) {
    //        ClearAllPoints();
    //    }

    //    // Clear last line drawn
    //    if (Input.GetKeyDown("q")) {
    //        if (lineGeneratorList.Count > 0) {
    //            DeleteLastLine();
    //        }
    //    }

    //    // Create line with current points (manual)
    //    if (Input.GetKeyDown("e")) {
    //        GenerateNewLine();
    //        ClearAllPoints();
    //    }
    //}


    private void CreateLinePoint(Vector3 pointPosition) {
        Debug.Log("Creating line point");
        Instantiate(linePointPrefab, pointPosition, Quaternion.identity);
    }
    private void ClearAllPoints() {
        GameObject[] points = GameObject.FindGameObjectsWithTag("LinePoint");

        foreach (GameObject point in points) {
            Destroy(point);
        }
    }
    public void DeleteLastLine() {
        Debug.Log("Deleting last line");
        GameObject lastLineGenerator = lineGeneratorList[lineGeneratorList.Count - 1];
        lineGeneratorList.RemoveAt(lineGeneratorList.Count - 1);
        Destroy(lastLineGenerator);
    }
    private void GenerateNewLine() {
        Debug.Log("Generating line");
        GameObject[] points = GameObject.FindGameObjectsWithTag("LinePoint");
        Vector3[] pointPositions;

        if (points.Length >= 2)
        {
            pointPositions = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                pointPositions[i] = points[i].transform.position;
            }
            SpawnLineGenerator(pointPositions);
        }
            //} else if (points.Length == 1) {
            //    pointPositions = new Vector3[2];
            //    pointPositions[0] = points[0].transform.position;
            //    pointPositions[1] = points[0].transform.position;
            //} else {
            //    return;
            //}

        }
	void SpawnLineGenerator(Vector3[] pointPositions)
    {
        Debug.Log("Spawning line generator");
        Debug.Log("Points: " + pointPositions);
        Debug.Log("Length: " + pointPositions.GetLength(0));
        Debug.Log("Other length: " + pointPositions.Length);
        GameObject lineGeneratorClone = Instantiate(lineGeneratorPrefab);
        LineRenderer lineRenderer = lineGeneratorClone.GetComponent<LineRenderer>();

        lineRenderer.positionCount = pointPositions.Length;
        lineRenderer.SetPositions(pointPositions);
        
        lineGeneratorList.Add(lineGeneratorClone);
    }
}
