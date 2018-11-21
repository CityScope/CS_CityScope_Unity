using UnityEditor;
using UnityEngine;


//[CustomEditor(typeof(cityIO)), CanEditMultipleObjects]
//public class cityIOEditor : Editor {
//
//	public SerializedProperty 
//	dataSource_Prop,
//	tableName_Prop;
//
//	void OnEnable () {
//		// Setup the SerializedProperties
//		dataSource_Prop = serializedObject.FindProperty ("_dataSource");
//		tableName_Prop = serializedObject.FindProperty ("_tableName");        
//	}
//
//	public override void OnInspectorGUI() {
//		serializedObject.Update ();
//
//		EditorGUILayout.PropertyField( dataSource_Prop );
//
//		cityIO.DataSource src = (cityIO.DataSource)dataSource_Prop.enumValueIndex;
//
//		if (src != cityIO.DataSource.INTERNAL) {
//			EditorGUILayout.PropertyField( tableName_Prop, new GUIContent("_tableName") );            
//		}
//
//
//		serializedObject.ApplyModifiedProperties ();
//	}
//}
