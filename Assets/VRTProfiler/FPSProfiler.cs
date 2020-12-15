﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRT.Profiler
{
    public class FPSProfiler : BaseProfiler
    {
        List<Vector3> data = new List<Vector3>();
        List<string> UTCTime = new List<string>();

        public override void Flush()
        {
            data.Clear();
        }

        public override void AddFrameValues()
        {
            data.Add(new Vector3(Time.frameCount, Time.frameCount / Time.time, Time.time));
            UTCTime.Add(DateTime.UtcNow.ToString("HH:mm:ss.fff"));
        }

        public override void GetHeaders(StringBuilder sb)
        {
            sb.Append("FRAME;FPS;TimeSinceGameStart;UTCTime;");
        }

        public override void GetFramesValues(StringBuilder sb, int frame)
        {
            sb.AppendFormat("{0};{1:0.00};{2};{3};", (int)data[frame].x, data[frame].y, data[frame].z, UTCTime[frame]);
        }
    }
}