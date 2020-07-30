using GSD.Roads;
using System.Collections.Generic;
using UnityEngine;
using CityGen3D;
using UnityEditor;

public class MenuTest : MonoBehaviour
{
    // Add a menu item named "Do Something" to MyMenu in the menu bar.
    [MenuItem("Window/Road Architect/Add CityGen Roads")]
    [ExecuteInEditMode]
    static void RunRACityGen()
    {
        Debug.Log("Doing Something...");
        RACityGen newRoad = new RACityGen();
        newRoad.InsertRoads();
    }

    [MenuItem("Window/Road Architect/OSM Tags")]
    [ExecuteInEditMode]
    static void Tags()
    {
        RACityGen getTags = new RACityGen();
        getTags.GetTags();
    }
    
}

public class RACityGen
{
    GSDRoadSystem roadSystem;
    GameObject tRoadSystemObj;
    List<GSDRoad> createdRoads;

    public RACityGen()
    {
        Debug.Log("RACityGen Constructor called");
        tRoadSystemObj = new GameObject("RoadArchitectSystem");
        roadSystem = tRoadSystemObj.AddComponent<GSDRoadSystem>();
        roadSystem.opt_bAllowRoadUpdates = false;
        roadSystem.opt_bMultithreading = true; // This is key for runtime generation
        createdRoads = new List<GSDRoad>();
        
    }

    public void GetTags()
    {
        foreach (MapRoad road in Map.Instance.mapRoads.GetMapRoads())
        {
            List<List<Vector3>> positions = road.GetPositions();
            //List<List<Vector3>> laneWaypoints = road.GetLanes();

            List<Vector3> roadPoints = new List<Vector3>();


            foreach (OSM_Tag tags in road.way.tags)
            {
                Debug.Log("Key: " + tags.key + " Value: " + tags.value);
            }
        }
    }


    public void InsertRoads()
    {
        
        Debug.Log("InsertRoads");
        foreach (MapRoad road in Map.Instance.mapRoads.GetMapRoads())
        {
            List<List<Vector3>> positions = road.GetPositions();
            //List<List<Vector3>> laneWaypoints = road.GetLanes();
            
            List<Vector3> roadPoints = new List<Vector3>();

            
            foreach (OSM_Tag tags in road.way.tags)
            {
                Debug.Log(tags.key);
                Debug.Log(tags.value);
            }

            // positions for each road. path.Length >= 2
            foreach (List<Vector3> path in positions)
            {
                
                int count = path.Count;
                for (int i = 0; i < count; i++)
                {
                    Vector3 pos = path[i];
                    float terrainHeight = Terrain.activeTerrain.SampleHeight(pos);
                    roadPoints.Add(new Vector3(pos.x, terrainHeight, pos.z));
                    Debug.Log(pos);
                   
                }
            }
            Debug.Log("Create Road");
            GSDRoad _gsdRoad = CreateNewRoad(roadPoints, road.lanes);
            createdRoads.Add(_gsdRoad);
        }
        roadSystem.opt_bAllowRoadUpdates = true;
        roadSystem.UpdateAllRoads();
    }

    private GSDRoad CreateNewRoad(List<Vector3> nodeList, int numLanes)
    {
        GSDRoad road = GSDRoadAutomation.CreateRoad_Programmatically(roadSystem, ref nodeList);
        //Debug.Log("CreateNewRoad: " + nodeList[0].y);
        /*for (int i = 0; i < road.GSDSpline.mNodes.Count - 1; i++)
        {
            //road.GSDSpline.mNodes[i].LoadWizardObjectsFromLibrary("GSDGroup-KRailLights-6L", true, false);
            //road.GSDSpline.mNodes[i].LoadWizardObjectsFromLibrary("GSDGroup-Fancy1-6L", true, false);
        }*/

        //road.opt_bIsLightmapped = true; // These are just a quick way to edit the road
        //road.opt_bIsStatic = true;
        road.opt_Lanes = numLanes;
        //road.opt_ClearDetailsDistance = 36;

        roadSystem.opt_bAllowRoadUpdates = false;
        //roadSystem.UpdateAllRoads();

        return road;
    }

}