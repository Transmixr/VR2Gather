﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ProfilerManager : MonoBehaviour {
    public string fileName = "Profiler";
    public float SamplingRate = 1/30f;
    public bool FPSActive = true;
    public bool HMDActive = Config.Instance.pilot3NavigationLogs;
    public bool TVMActive = false;
    public GameObject[] TVMs;

    private float timeToNext = 0.0f;
    private uint lineCount = 0;
    private Transform HMD;

    public static ProfilerManager Instance { get; private set; }

    private void Awake() {
        Instance = this;
        //Moved to update for pilot 3
        //HMD = FindObjectOfType<Camera>().gameObject.transform;
        
    }       

    List<BaseProfiler> profiles = new List<BaseProfiler>();

    public void AddProfiler(BaseProfiler profiler) {
        profiles.Add(profiler);
        foreach (var profile in profiles)
            profile.Flush();
        timeToNext = 0;
        lineCount = 0;
    }

    // Use this for initialization
    void Start () {
        //if (FPSActive) AddProfiler(new FPSProfiler());
        //Moved to update for pilot 3
        //if (HMDActive) AddProfiler(new HMDProfiler(HMD));
        if (TVMActive) AddProfiler(new TVMProfiler(TVMs));
    }

    // Update is called once per frame
    void Update () {
        if (HMDActive == true)
        {
            var cam = FindObjectOfType<Camera>().gameObject;
            if (cam!=null)
            {
                HMD = cam.transform;
                AddProfiler(new FPSProfiler());
                AddProfiler(new HMDProfiler(HMD));
                HMDActive = false;
            }
        }
        if (Time.time > 0) {
            timeToNext -= Time.deltaTime;
            if (timeToNext < 0.0f) {
                timeToNext += SamplingRate;
                foreach (var profile in profiles)                
                    profile.AddFrameValues();
                lineCount++;
            }
        }
    }
    
    private void OnApplicationQuit() {
        //UnityEngine.Debug.Log("<color=red>XXXShishir: </color> Writing nav logs to " + string.Format("{0}/../{1}.csv", Application.persistentDataPath, fileName));
        StringBuilder sb = new StringBuilder();
        if (profiles.Count > 0) {
            foreach (var profile in profiles)
                profile.GetHeaders(sb);
            sb.Length--;
            sb.AppendLine();
            for (int i = 0; i < lineCount; i++) {
                foreach (var profile in profiles)
                    profile.GetFramesValues(sb, i);
                sb.Length--;
                sb.AppendLine();
            }
            //string time = System.DateTime.Now.ToString("yyyyMMddHmm");
            System.IO.File.WriteAllText(string.Format("{0}/../{1}.csv", Application.persistentDataPath, fileName), sb.ToString());
        }
    }
}
