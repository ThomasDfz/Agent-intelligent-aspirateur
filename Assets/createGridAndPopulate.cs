using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class createGridAndPopulate : MonoBehaviour
{

    public GameObject[,,] myTable = new GameObject[10, 10, 2];
    public int durationFactor = 20;
    public int apparitionFactor = 10;
    public Boolean pausedGame = false;

    private System.Random rnd = new System.Random();
    private ArrayList toBeAdded = new ArrayList();
    private static Mutex mut = new Mutex(); // the mutex to change the arrylist
    int count0 = 0;
    int count1 = 0;

    void populate()
    {
        Debug.Log("thread start");
        while (!pausedGame)
        {
            // while the game isn't paused, the environment generates in it's own thread the list of objects to create
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                {
                    if (!myTable[i, j, 0])
                    {
                        if (rnd.Next(0, 100) < 1)
                        {
                            count0++;
                            mut.WaitOne();
                            int[] newElem = { i, j, 0 };
                            toBeAdded.Add(newElem);
                            mut.ReleaseMutex();
                        }
                    }
                    if (!myTable[i, j, 1])
                    {
                        if (rnd.Next(0, 1000) < 5)
                        {
                            count1++;
                            mut.WaitOne();
                            int[] newElem = { i, j, 1 };
                            toBeAdded.Add(newElem);
                            mut.ReleaseMutex();
                        }
                    }
                }
            Thread.Sleep(apparitionFactor * durationFactor);
        }
        Debug.Log("thread ends");
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("start");

        Thread workerThread = new Thread(populate);
        workerThread.Start();

    }

    // Update is called once per frame
    void Update()
    {
        mut.WaitOne();
        foreach (int[] newElement in toBeAdded)
        {
            if (newElement[2] == 0)
            {
                UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath("Assets/Ressources/Dust.prefab", typeof(GameObject));
                Vector3 position = new Vector3(newElement[0], newElement[1], 1);
                GameObject clone = Instantiate(prefab, position, Quaternion.identity) as GameObject;
                myTable[newElement[0], newElement[1], 0] = clone;
            }
            else
            {
                UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath("Assets/Ressources/Jewelry.prefab", typeof(GameObject));
                Vector3 position = new Vector3(newElement[0], newElement[1], 1);
                GameObject clone = Instantiate(prefab, position, Quaternion.identity) as GameObject;
                myTable[newElement[0], newElement[1], 1] = clone;
            }

        }
        toBeAdded.Clear(); //elements have been placed, the table is cleared;
        mut.ReleaseMutex();
        String s = "counts :" + count0 + " " + count1;
        Debug.Log(s);

    }
}
