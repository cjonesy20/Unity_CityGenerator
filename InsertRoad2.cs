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
        if (GameObject.Find("RoadArchitectSystem"))
        {
            tRoadSystemObj = GameObject.Find("RoadArchitectSystem");
        }
        else
        {
            tRoadSystemObj = new GameObject("RoadArchitectSystem");
        }

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

    private bool ProcessRoad(MapRoad road)
    {
        foreach (OSM_Tag tags in road.way.tags)
        {
            //Debug.Log("Key: " + tags.key + " Value: " + tags.value);
            if ( (tags.key == "service") || ((tags.key == "highway") && (tags.value == "service")) || (tags.key == "rail") )
            {
                Debug.Log("FALSE: Key: " + tags.key + " Value: " + tags.value);
                return false;
            }
        }
        Debug.Log("TRUE: ");
        return true;
    }


    public void InsertRoads()
    {
        
        Debug.Log("InsertRoads");
        foreach (MapRoad road in Map.Instance.mapRoads.GetMapRoads())
        {
            /*
               service: driveway
               service: alley
               service: parking_aisle
               highway: service
               service: siding
               railway: rail
           */

            if (ProcessRoad(road) == false)
            {
                continue;
            }

            List<List<Vector3>> positions = road.GetPositions();
            //List<List<Vector3>> laneWaypoints = road.GetLanes();

            List<Vector3> roadPoints = new List<Vector3>();

            // positions for each road. path.Length >= 2
            foreach (List<Vector3> path in positions)
            {

                int count = path.Count;

                // Can't create an intersection with a road that only has 2 points. There has to be at least 3. So we'll add a third point midway between the two points provided.
                if (count == 2)
                {
                    Vector3 pos1 = path[0];
                    Vector3 pos2 = path[1];
                    // path[0].y is ignored, so we don't bother to get the average. 
                    Vector3 posMid = new Vector3(((path[0].x + path[1].x) / 2), path[0].y, ((path[0].z + path[1].z) / 2));
                    float terrainHeight1 = Terrain.activeTerrain.SampleHeight(pos1);
                    float terrainHeight2 = Terrain.activeTerrain.SampleHeight(pos2);
                    float terrainHeightMid = Terrain.activeTerrain.SampleHeight(posMid);
                    roadPoints.Add(new Vector3(pos1.x, terrainHeight1, pos1.z));
                    roadPoints.Add(new Vector3(pos2.x, terrainHeight2, pos2.z));
                    roadPoints.Add(new Vector3(posMid.x, terrainHeightMid, posMid.z));
                }

                for (int i = 0; i < count; i++)
                {
                    Vector3 pos = path[i];
                    float terrainHeight = Terrain.activeTerrain.SampleHeight(pos);
                    roadPoints.Add(new Vector3(pos.x, terrainHeight, pos.z));
                    //Debug.Log(pos);

                }
            }
            Debug.Log("Create Road");
            GSDRoad _gsdRoad = CreateNewRoad(roadPoints, road.lanes);
            createdRoads.Add(_gsdRoad);
        }
            Debug.Log(createdRoads.Count + " roads created.");

                //GSDRoadAutomation.CreateIntersections_ProgrammaticallyForRoad(createdRoads[0], GSDRoadIntersection.iStopTypeEnum.TrafficLight1, GSDRoadIntersection.RoadTypeEnum.NoTurnLane);
            roadSystem.opt_bAllowRoadUpdates = true;
            roadSystem.UpdateAllRoads();
        
        /*for (int i = 0; i < 10; i++)
        {
            GSDRoadAutomation.CreateIntersections_ProgrammaticallyForRoad(createdRoads[i], GSDRoadIntersection.iStopTypeEnum.TrafficLight1, GSDRoadIntersection.RoadTypeEnum.NoTurnLane);
        }*/

        /*foreach (GSDRoad _road in createdRoads)
        {

            GSDRoadAutomation.CreateIntersections_ProgrammaticallyForRoad(_road, GSDRoadIntersection.iStopTypeEnum.TrafficLight1, GSDRoadIntersection.RoadTypeEnum.NoTurnLane);

        }*/
        //roadSystem.UpdateAllRoads();


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