using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

[CustomGridBrush(false, false, false, "Prefab Brush")]
public class PrefabBrush : GridBrushBase {
    public Tile prefabTile;
    public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position)
    {
        brushTarget.GetComponent<Tilemap>().SetTile(position,prefabTile);
    }

    public override void Erase(GridLayout grid, GameObject brushTarget, Vector3Int position)
    {
        brushTarget.GetComponent<Tilemap>().SetTile(position,null);
    }
    [MenuItem("Assets/Create/Prefab Brush")]
    public static void CreateBrush()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Prefab Brush", "New Prefab Brush", "Asset", "Save Prefab Brush", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PrefabBrush>(), path);
    }
}
