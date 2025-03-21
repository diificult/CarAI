using UnityEditor;
using UnityEngine;

public class MapEditorTool : EditorWindow
{
    private GameObject selectedObject;
    private GameObject Parent;

    [MenuItem("Tools/Map Editor Tool")]
    public static void ShowWindow()
    {
        GetWindow<MapEditorTool>("Map Editor Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Map Editor Tool", EditorStyles.boldLabel);

        selectedObject = (GameObject)EditorGUILayout.ObjectField("Selected Object", selectedObject, typeof(GameObject), true);
        Parent = (GameObject) EditorGUILayout.ObjectField("Parent", Parent, typeof(GameObject), true);
        
        if (selectedObject == null)
        {
            EditorGUILayout.HelpBox("Please select an object to duplicate.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("Duplicate Forward"))
        {
            DuplicateObject(Vector3.forward);
        }

        if (GUILayout.Button("Duplicate Backward"))
        {
            DuplicateObject(Vector3.back);
        }

        if (GUILayout.Button("Duplicate Left"))
        {
            DuplicateObject(Vector3.left);
        }

        if (GUILayout.Button("Duplicate Right"))
        {
            DuplicateObject(Vector3.right);
        }
    }

    private void DuplicateObject(Vector3 direction)
    {
        if (selectedObject != null)
        {
            GameObject duplicate = Instantiate(selectedObject, Parent.transform);
            Undo.RegisterCreatedObjectUndo(duplicate, "Duplicate Object");
            duplicate.transform.position = selectedObject.transform.position + direction * 20;
            duplicate.transform.rotation = selectedObject.transform.rotation;
            duplicate.GetComponent<Node>().GridX = selectedObject.GetComponent<Node>().GridX + (int)direction.x;
            duplicate.GetComponent<Node>().GridY = selectedObject.GetComponent<Node>().GridY + (int)direction.z;
            duplicate.name =  "Road " + duplicate.GetComponent<Node>().GridX + " " + duplicate.GetComponent<Node>().GridY;
            selectedObject = duplicate;
            Selection.activeGameObject = duplicate;
        }
        else
        {
            Debug.LogWarning("No object selected for duplication.");
        }
    }
}