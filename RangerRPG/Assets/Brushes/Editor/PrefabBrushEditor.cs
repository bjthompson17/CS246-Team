using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(PrefabBrush))]
public class PrefabBrushEditor : Editor 
{
    private GameObject prefab = null;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10f);
        prefab = (GameObject)EditorGUILayout.ObjectField(prefab,typeof(GameObject),true);
        PrefabBrush brush = (PrefabBrush) target;
        if(GUILayout.Button("Create Tile")) {
            string path = EditorUtility.SaveFilePanelInProject("Save Prefab Tile", "New Prefab Tile", "asset", "Save Tile", "Assets");
            if (path == "")
                return;
            Tile tileInstance = ScriptableObject.CreateInstance<Tile>();
            tileInstance.gameObject = prefab;
            AssetDatabase.CreateAsset(tileInstance, path);
            brush.prefabTile = tileInstance;
        }
    }
}
