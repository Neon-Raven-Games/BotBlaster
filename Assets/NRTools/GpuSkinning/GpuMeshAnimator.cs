using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class GpuMeshAnimator : MonoBehaviour
    {
        public Mesh mesh; // The mesh to be animated
        public Material material; // Material with the shader that applies the deltas
        public int numFrames = 100; // Number of frames in the animation
        public float animationSpeed = 1.0f; // Animation playback speed
        public bool useInterpolation = true; // Enable interpolation between frames

        private ComputeBuffer vertexIDBuffer; // Buffer for vertex IDs
        private ComputeBuffer deltaBuffer; // Buffer for vertex deltas
        private Vector3[] baseVertices; // Base vertex positions
        private Vector3[][] deltaFrames; // Delta data for each frame (coming from external tool)
        private int[] vertexIDs; // Mapping of vertex IDs
        private float currentFrame = 0; // Current frame of the animation

        private static readonly int _SVertexIDs = Shader.PropertyToID("_VertexIDs");
        private static readonly int _SDeltas = Shader.PropertyToID("_Deltas");


        public List<MeshDeltaData> deltaData = new();

        void LoadDeltaData(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                while (fs.Position < fs.Length)
                {
                    // Read the mesh name length and then the name itself
                    int meshNameLength = br.ReadInt32();
                    string meshName = new string(br.ReadChars(meshNameLength));

                    var meshDeltaData = new MeshDeltaData
                    {
                        MeshName = meshName,
                        Frames = new List<FrameDelta>()
                    };

                    // Read the number of frames
                    int numFrames = br.ReadInt32();

                    for (int frameIndex = 0; frameIndex < numFrames; frameIndex++)
                    {
                        var frameDelta = new FrameDelta();

                        // Read each delta for the frame
                        for (int vertexIndex = 0;
                             vertexIndex < numFrames;
                             vertexIndex++) // Adjust according to actual data
                        {
                            var delta = new VertexDelta
                            {
                                X = br.ReadSingle(),
                                Y = br.ReadSingle(),
                                Z = br.ReadSingle()
                            };

                            frameDelta.Deltas.Add(delta);
                        }

                        meshDeltaData.Frames.Add(frameDelta);
                    }

                    deltaData.Add(meshDeltaData);
                }
            }
        }

        private void Start()
        {
            baseVertices = mesh.vertices;

            // Initialize the vertex ID buffer
            vertexIDs = new int[baseVertices.Length];
            for (int i = 0; i < baseVertices.Length; i++)
            {
                vertexIDs[i] = i; // Each vertex gets an index
            }

            // Set up the ComputeBuffer for vertex IDs
            vertexIDBuffer = new ComputeBuffer(baseVertices.Length, sizeof(int));
            vertexIDBuffer.SetData(vertexIDs);

            // todo, update path
            LoadDeltaData("path/to/your/binary/file.bin");

            // Initially set the first frame's deltas
            deltaBuffer = new ComputeBuffer(baseVertices.Length, sizeof(float) * 3);
            UpdateDeltas(deltaFrames[0]);
        }

        // Method to load delta data (from WinForms or any source)
        private void LoadDeltasFromWinForms()
        {
            // Example placeholder: Assume deltaFrames is a 2D array where each row is a frame and each element is a delta for a vertex
            deltaFrames = new Vector3[numFrames][];
            for (int frame = 0; frame < numFrames; frame++)
            {
                // Replace this with actual delta loading from your WinForms program
                deltaFrames[frame] = new Vector3[baseVertices.Length];
                for (int v = 0; v < baseVertices.Length; v++)
                {
                    // Example: Just filling with placeholder values for now
                    deltaFrames[frame][v] = Vector3.zero; // Replace with actual delta data from WinForms
                }
            }
        }

        private void Update()
        {
            // Increment the frame based on the animation speed and frame count
            currentFrame += Time.deltaTime * animationSpeed;
            if (currentFrame >= numFrames)
                currentFrame = 0;

            // Apply frame interpolation if enabled
            if (useInterpolation)
            {
                int frame0 = Mathf.FloorToInt(currentFrame); // Base frame
                int frame1 = (frame0 + 1) % numFrames; // Next frame for interpolation
                float t = currentFrame - frame0; // Interpolation factor

                // Interpolate between frame0 and frame1 deltas
                Vector3[] interpolatedDeltas = new Vector3[baseVertices.Length];
                for (int i = 0; i < baseVertices.Length; i++)
                {
                    interpolatedDeltas[i] = Vector3.Lerp(deltaFrames[frame0][i], deltaFrames[frame1][i], t);
                }

                UpdateDeltas(interpolatedDeltas);
            }
            else
            {
                // No interpolation, just apply the current frame's deltas
                int frameIndex = Mathf.FloorToInt(currentFrame);
                UpdateDeltas(deltaFrames[frameIndex]);
            }

            // Pass the buffers to the shader
            material.SetBuffer(_SVertexIDs, vertexIDBuffer);
            material.SetBuffer(_SDeltas, deltaBuffer);
        }

        // Method to update the delta buffer with new deltas
        private void UpdateDeltas(Vector3[] deltas)
        {
            deltaBuffer.SetData(deltas);
        }

        private void OnDestroy()
        {
            if (vertexIDBuffer != null)
            {
                vertexIDBuffer.Release();
            }

            if (deltaBuffer != null)
            {
                deltaBuffer.Release();
            }
        }
    }
}

public class VertexDelta
{
    public float X;
    public float Y;
    public float Z;
}

public class FrameDelta
{
    public List<VertexDelta> Deltas = new List<VertexDelta>();
}

public class MeshDeltaData
{
    public string MeshName;
    public List<FrameDelta> Frames = new List<FrameDelta>();
}