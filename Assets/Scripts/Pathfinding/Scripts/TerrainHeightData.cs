// Most of this code came from joshrs926 at https://answers.unity.com/questions/1642900/how-is-terrainsampleheight-implemented.html
// I modified it to be an IComponentData and converted the height map from an unsafe-list to a blob-array.
// I made it store terrain as 16 bit ushorts instead of 32 bit floats to save memory.
// I changed some division operations to shifts to make sample operations run faster.
//
// To place it on a terrain during a conversion, use:
//    Terrain terrain = GetComponent<Terrain>();
//    dstManager.AddComponentData(entity, new TerrainHeightData(terrain));
//
// To read height data in a job:
//    TerrainHeightData terrainMap = GetSingleton<TerrainHeightData>()
//    float height = terrainMap.SampleHeight(pos);

using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;

public struct TerrainHeightData : IComponentData
{
    public AABB AABB { get; private set; }
    public bool IsValid => heightMapRef.IsCreated;

    public BlobAssetReference<BlobArray<ushort>> heightMapRef;

    int resolution;
    float2 sampleSize;
    int QuadCount => resolution - 1;

    float minVal; // used to convert to/from 16 bit
    float maxVal; // used to convert to/from 16 bit
     
    // Constructor that fills all the variables (including height-map blob ref), given the specified terrain object.
    public TerrainHeightData(Terrain terrain)
    {
        resolution = terrain.terrainData.heightmapResolution;
        sampleSize = new float2(terrain.terrainData.heightmapScale.x, terrain.terrainData.heightmapScale.z);
        AABB = GetTerrrainAABB(terrain);

        using BlobBuilder builder = new BlobBuilder(Allocator.Temp);
        ref BlobArray<ushort> root = ref builder.ConstructRoot<BlobArray<ushort>>();
        BlobBuilderArray<ushort> arrayBuilder = builder.Allocate(ref root, resolution * resolution);
        
        var map = terrain.terrainData.GetHeights(0, 0, resolution, resolution);

        // First the min an max values need to be calculated and stored (for later use when decoding a height).
        // This allows the height map to be of type ushort, which is half the size of float.
        minVal = float.MaxValue;
        maxVal = float.MinValue;
        for (int y = 0; y < resolution; y++) {
            for (int x = 0; x < resolution; x++) {
                if (map[y, x] < minVal) { minVal = map[y,x]; }
                if (map[y, x] > maxVal) { maxVal = map[y,x]; }
            }
        }

        // now store each point, scaling it via the min/max calculated above in the process
        for (int y = 0; y < resolution; y++) {
            for (int x = 0; x < resolution; x++) {
                int i = y * resolution + x;
                arrayBuilder[i] = (ushort)MathUtils.Scale(map[y, x], minVal, maxVal, 0, ushort.MaxValue);
            }
        }
        heightMapRef = builder.CreateBlobAssetReference<BlobArray<ushort>>(Allocator.Persistent);
    }

    // Returns world height of terrain at x and z position values.
    public float SampleHeight(float3 worldPosition)
    {
        GetTriAtPosition(worldPosition, out Triangle tri);
        return tri.SampleHeight(worldPosition);
    }
     
    // Returns world height of terrain at x and z position values. Also outputs normalized normal vector of terrain at position.
    public float SampleHeight(float3 worldPosition, out float3 normal)
    {
        GetTriAtPosition(worldPosition, out Triangle tri);
        normal = tri.Normal;
        return tri.SampleHeight(worldPosition);
    }
     
    // fetches the triangle at the specified position
    void GetTriAtPosition(float3 worldPosition, out Triangle tri)
    {
        if (!IsWithinBounds(worldPosition)) {
            throw new System.ArgumentException($"Position given {worldPosition} is outside of terrain x or z bounds.");
        }
        float2 localPos = new float2(worldPosition.x - AABB.Min.x, worldPosition.z - AABB.Min.z);
        float2 samplePos = localPos / sampleSize;
        int2 sampleFloor = (int2)math.floor(samplePos);
        float2 sampleDecimal = samplePos - sampleFloor;
        bool upperLeftTri = sampleDecimal.y > sampleDecimal.x;
        int2 v1Offset = upperLeftTri ? new int2(0, 1) : new int2(1, 1);
        int2 v2Offset = upperLeftTri ? new int2(1, 1) : new int2(1, 0); 
        float3 v0 = GetWorldVertex(sampleFloor);
        float3 v1 = GetWorldVertex(sampleFloor + v1Offset);
        float3 v2 = GetWorldVertex(sampleFloor + v2Offset);
        tri = new Triangle(v0, v1, v2);
    }
     
    //public void Dispose()
    //{
    //    heightMap.Dispose();
    //}
     
    // returns true if the specified point is within the terrain bounds (on the x/z plane)
    bool IsWithinBounds(float3 worldPos)
    {
        return worldPos.x >= AABB.Min.x && worldPos.z >= AABB.Min.z && worldPos.x <= AABB.Max.x && worldPos.z <= AABB.Max.z;
    }
     
    // return the vertex at the specified hight map coordinates.
    float3 GetWorldVertex(int2 heightMapCrds)
    {
        int i = heightMapCrds.x + heightMapCrds.y * resolution;
        // scale the height (which is a ushort) back to a float using the precalculated min/max vals
        // Since val is being divided by 65536, which is a power of 2, a fast divide can be used.
        float scaledHeight = MathUtils.FastDivideByPow2(heightMapRef.Value[i], 65536) * (maxVal - minVal) + minVal;
        float3 vertexPercentages = new float3(MathUtils.FastDivideByPow2(heightMapCrds.x, (uint)QuadCount), scaledHeight, MathUtils.FastDivideByPow2(heightMapCrds.y, (uint)QuadCount));
        return AABB.Min + AABB.Size * vertexPercentages;
    }
     
    // return the AABB box for the given terrain
    static AABB GetTerrrainAABB(Terrain terrain)
    {
        float3 min = terrain.transform.position;
        float3 max = min + (float3)terrain.terrainData.size;
        float3 extents = (max - min) * 0.5f;
        return new AABB() { Center = min + extents, Extents = extents };
    }
}

public readonly struct Triangle
{
    public float3 V0 { get; }
    public float3 V1 { get; }
    public float3 V2 { get; }
    // this is already normalized
    public float3 Normal { get; }
     
    public Triangle(float3 v0, float3 v1, float3 v2)
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Normal = math.normalize(math.cross(V1 - V0, V2 - V0));
    }
    public float SampleHeight(float3 position)
    {
        // plane formula: a(x - x0) + b(y - y0) + c(z - z0) = 0
        // <a,b,c> is a normal vector for the plane
        // (x,y,z) and (x0,y0,z0) are any points on the plane            
        return (-Normal.x * (position.x - V0.x) - Normal.z * (position.z - V0.z)) / Normal.y + V0.y;
    }
}