using UnityEngine;
using UnityEditor;

public class TerrainDataExporter
{
    [MenuItem("Tools/Save TerrainData Asset")]
    static void SaveTerrainData()
    {
        // Select your terrain in the scene
        Terrain terrain = Selection.activeGameObject?.GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("Select a Terrain in the scene first.");
            return;
        }

        // Create a new TerrainData
        TerrainData newData = new TerrainData();
        newData.heightmapResolution = terrain.terrainData.heightmapResolution;
        newData.size = terrain.terrainData.size;

        // Copy heights
        newData.SetHeights(0, 0, terrain.terrainData.GetHeights(0, 0,
            terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution));

        // Copy Terrain Layers (textures)
        TerrainLayer[] layers = terrain.terrainData.terrainLayers;
        if (layers != null && layers.Length > 0)
        {
            TerrainLayer[] newLayers = new TerrainLayer[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                // Duplicate the layer to keep textures safe
                TerrainLayer layerCopy = Object.Instantiate(layers[i]);
                string path = "Assets/" + layers[i].name + "_copy.terrainlayer.asset";
                AssetDatabase.CreateAsset(layerCopy, path);
                newLayers[i] = layerCopy;
            }
            newData.terrainLayers = newLayers;
        }

        // Copy trees
        newData.treePrototypes = terrain.terrainData.treePrototypes;
        newData.SetTreeInstances(terrain.terrainData.treeInstances, true);

        // Copy details (grass, etc.)
        for (int i = 0; i < terrain.terrainData.detailPrototypes.Length; i++)
        {
            int[,] details = terrain.terrainData.GetDetailLayer(0, 0,
                terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, i);
            newData.SetDetailLayer(0, 0, i, details);
        }

        // Save TerrainData asset
        string assetPath = "Assets/" + terrain.name + "_TerrainData.asset";
        AssetDatabase.CreateAsset(newData, assetPath);
        AssetDatabase.SaveAssets();

        // Assign the new data to the terrain
        terrain.terrainData = newData;

        Debug.Log("TerrainData saved as asset: " + assetPath);
    }
}
