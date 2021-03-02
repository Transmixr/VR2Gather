﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTCore;

namespace VRT.UserRepresentation.Voice
{
    public class EmptyPreparer : BasePreparer
    {
        float[] circularBuffer;
        int bufferSize;
        int writePosition;
        int readPosition;


        public EmptyPreparer(WorkerType _type = WorkerType.Run) : base(_type)
        {
            Start();
        }

        public override void OnStop()
        {
            base.OnStop();
            //            if (byteArray.Length != 0) byteArray.Dispose();
            Debug.Log("EmptyPreparer Sopped");
        }
        public override void LatchFrame()
        {
        }

        public override void Synchronize()
        {
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}
