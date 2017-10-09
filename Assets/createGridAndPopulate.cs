using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class createGridAndPopulate : MonoBehaviour {

    int myTable = 0;

    void increment()
    {
        Debug.Log("thread start");
        while (myTable<100)
        {
            myTable++;
            Thread.Sleep(10);
        }
        Debug.Log("thread ends");
    }

    // Use this for initialization
    void Start () {
        Debug.Log("start");

        Thread workerThread = new Thread(increment);
        workerThread.Start();
    }
	
	// Update is called once per frame
	void Update () {
        Debug.Log("up");


    }
}
