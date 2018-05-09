using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RTSEngine;

/* Attack Editor script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

[CustomEditor(typeof(Attack))]
public class AttackEditor : Editor {

	public SerializedProperty AttackCategories;
	public SerializedProperty AttackSources;
	public SerializedProperty AreaDamage;

	public override void OnInspectorGUI ()
	{
		Attack Target = (Attack)target;

		GUIStyle TitleGUIStyle = new GUIStyle ();
		TitleGUIStyle.fontSize = 20;
		TitleGUIStyle.alignment = TextAnchor.MiddleCenter;
		TitleGUIStyle.fontStyle = FontStyle.Bold;

		EditorGUILayout.LabelField ("Attack Component:", TitleGUIStyle);
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

        Target.Attacker = (Attack.Attackers)EditorGUILayout.EnumPopup("Attacker", Target.Attacker);

        EditorGUILayout.LabelField ("Attack Icon:");
		Target.AttackIcon = EditorGUILayout.ObjectField (Target.AttackIcon, typeof(Sprite), true) as Sprite;

		Target.AttackAllTypes = EditorGUILayout.Toggle ("Attack All Types?", Target.AttackAllTypes);
        Target.AttackUnits = EditorGUILayout.Toggle("Attack Units?", Target.AttackUnits);
        Target.AttackBuildings = EditorGUILayout.Toggle("Attack Buildings?", Target.AttackBuildings);
        if (Target.AttackAllTypes == false) {
			AttackCategories = serializedObject.FindProperty("AttackCategoriesList");
			EditorGUILayout.PropertyField (AttackCategories, true);
			serializedObject.ApplyModifiedProperties();
		}

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		Target.MinUnitStoppingDistance = EditorGUILayout.FloatField ("Min Units Stopping Distance: ", Target.MinUnitStoppingDistance);
		Target.MaxUnitStoppingDistance = EditorGUILayout.FloatField ("Max Units Stopping Distance: ", Target.MaxUnitStoppingDistance);

		Target.MinBuildingStoppingDistance = EditorGUILayout.FloatField ("Min Buildings Stopping Distance: ", Target.MinBuildingStoppingDistance);
		Target.MaxBuildingStoppingDistance = EditorGUILayout.FloatField ("Max Buildings Stopping Distance: ", Target.MaxBuildingStoppingDistance);

        Target.ChangeMvtDistance = EditorGUILayout.FloatField("Change Mvt Distance:", Target.ChangeMvtDistance);

        EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		Target.AttackOnAssign = EditorGUILayout.Toggle ("Attack On Assign?", Target.AttackOnAssign);
		Target.AttackWhenAttacked = EditorGUILayout.Toggle ("Attack When Attacked?", Target.AttackWhenAttacked);
		Target.AttackInRange = EditorGUILayout.Toggle ("Attack In Range?", Target.AttackInRange);
		Target.AttackRange = EditorGUILayout.FloatField ("Attack Range:", Target.AttackRange);
		Target.FollowRange = EditorGUILayout.FloatField ("Follow Distance:", Target.FollowRange);
		Target.SearchReload = EditorGUILayout.FloatField ("Search Reload:", Target.SearchReload);

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		Target.AreaDamage = EditorGUILayout.Toggle ("Area Damage?", Target.AreaDamage);
		if (Target.AreaDamage == true) {
			AreaDamage = serializedObject.FindProperty("AttackRanges");
			EditorGUILayout.PropertyField (AreaDamage, true);
			serializedObject.ApplyModifiedProperties();
		} else {
			Target.BuildingDamage = EditorGUILayout.FloatField ("Building Damage:", Target.BuildingDamage);
			Target.UnitDamage = EditorGUILayout.FloatField ("Unit Damage:", Target.UnitDamage);
		}

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

        Target.DelayTime = EditorGUILayout.FloatField("Attack Delay Time:", Target.DelayTime);
        Target.UseDelayTrigger = EditorGUILayout.Toggle("Use Delay Trigger?", Target.UseDelayTrigger);
        Target.PlayAnimInDelay = EditorGUILayout.Toggle("Play Animation In Delay?", Target.PlayAnimInDelay);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        //Damage over time:
        Target.DoT.Enabled = EditorGUILayout.Toggle("Damage Over Time?", Target.DoT.Enabled);
        if (Target.DoT.Enabled == true)
        {
            Target.DoT.Infinite = EditorGUILayout.Toggle("Infinite DoT?", Target.DoT.Infinite);
            Target.DoT.Duration = EditorGUILayout.FloatField("DoT Duration", Target.DoT.Duration);
            Target.DoT.Cycle = EditorGUILayout.FloatField("DoT Cycle", Target.DoT.Cycle);
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        Target.AttackEffect = EditorGUILayout.ObjectField(Target.AttackEffect, typeof(EffectObj), true) as EffectObj;
        Target.AttackEffectTime = EditorGUILayout.FloatField("Attack Effect Duration:", Target.AttackEffectTime);

        EditorGUILayout.Space();
        EditorGUILayout.Space();


        Target.DirectAttack = EditorGUILayout.Toggle ("Direct Attack?", Target.DirectAttack);
		Target.MoveOnAttack = EditorGUILayout.Toggle ("Move On Attack?", Target.MoveOnAttack);
		Target.AttackReload = EditorGUILayout.FloatField ("Attack Reload", Target.AttackReload);
		Target.AttackType = (Attack.AttackTypes) EditorGUILayout.EnumPopup ("Attack Type:", Target.AttackType);

		EditorGUILayout.Space ();

		AttackSources = serializedObject.FindProperty("AttackSources");
		EditorGUILayout.PropertyField (AttackSources, true);
		serializedObject.ApplyModifiedProperties();

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

        EditorGUILayout.LabelField("Weapon Object:");
        Target.WeaponObj = EditorGUILayout.ObjectField (Target.WeaponObj, typeof(GameObject), true) as GameObject;
        Target.FreezeRotX = EditorGUILayout.Toggle("Freeze Rotation on X?", Target.FreezeRotX);
        Target.FreezeRotY = EditorGUILayout.Toggle("Freeze Rotation on Y?", Target.FreezeRotY);
        Target.FreezeRotZ = EditorGUILayout.Toggle("Freeze Rotation on Z?", Target.FreezeRotZ);
        Target.SmoothRotation = EditorGUILayout.Toggle("Smooth Rotation?", Target.SmoothRotation);
        if (Target.SmoothRotation == true)
        {
            Target.RotationDamping = EditorGUILayout.FloatField("Rotation Damping: ", Target.RotationDamping);
        }
        Target.WeaponIdleAngles = EditorGUILayout.Vector3Field("Weapon Idle Angle: ", Target.WeaponIdleAngles);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        Target.AttackAnimTime = EditorGUILayout.FloatField ("Attack Animation Duration:", Target.AttackAnimTime);

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		EditorGUILayout.LabelField ("Attack Order Audio:");
		Target.AttackOrderSound = EditorGUILayout.ObjectField (Target.AttackOrderSound, typeof(AudioClip), true) as AudioClip;
		EditorGUILayout.LabelField ("Attack Audio:");
		Target.AttackSound = EditorGUILayout.ObjectField (Target.AttackSound, typeof(AudioClip), true) as AudioClip;

		EditorUtility.SetDirty (Target);
	}
}
