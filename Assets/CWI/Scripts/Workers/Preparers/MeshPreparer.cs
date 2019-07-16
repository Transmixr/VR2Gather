﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Workers {
    public class MeshPreparer : BaseWorker
    {
        bool isReady = false;
        Unity.Collections.NativeArray<PointCouldVertex> vertexArray;
        System.IntPtr                       currentBuffer;
        int                                 PointCouldVertexSize;
        Vector3[]   points;
        int[]       indices;
        Color32[]   colors;

        public MeshPreparer() : base(WorkerType.End)
        {
            PointCouldVertexSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(PointCouldVertex));
            Start();
        }

        public override void OnStop()
        {
            base.OnStop();
            if (vertexArray.Length != 0) vertexArray.Dispose();
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)] // Also tried with Pack=1
        public struct PointCouldVertex
        {
            public Vector3 vertex;
            public Color32 color;
        }
        protected override void Update()
        {
            base.Update();
            if (token != null && !isReady)
            {
                unsafe {
                    int bufferSize = (int)API_cwipc_util.cwipc_get_uncompressed_size(token.currentBuffer);
                    int size = bufferSize / PointCouldVertexSize;
                    int dampedSize = (int)(size * Config.Instance.memoryDamping);
                    if (vertexArray.Length < dampedSize) {
                        vertexArray = new Unity.Collections.NativeArray<PointCouldVertex>(dampedSize, Unity.Collections.Allocator.TempJob);
                        currentBuffer = (System.IntPtr)Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(vertexArray);
                    }
                    int ret = API_cwipc_util.cwipc_copy_uncompressed(token.currentBuffer, currentBuffer, (System.IntPtr)bufferSize);

                    points = new Vector3[size];
                    indices = new int[size];
                    colors = new Color32[size];
                    for (int i = 0; i < size; i++) {
                        points[i]  = vertexArray[i].vertex;
                        indices[i] = i;
                        colors[i]  = vertexArray[i].color;
                    }
                    isReady = true;
                    Next();
                }
            }
        }

        public bool GetMesh(ref Mesh mesh) {
            if (isReady) {
                if (mesh != null) {
                    mesh.Clear();
                    mesh.vertices = points;
                    mesh.colors32 = colors;
                    mesh.SetIndices(indices, MeshTopology.Points, 0);
                }
                isReady = false;
                return true;
            }
            return false;
        }
    }
}