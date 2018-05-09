﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Converter script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
	public class Converter : MonoBehaviour {

		[HideInInspector]
		public bool IsConverting = false; //is the unit converting its target
		[HideInInspector]
		public Unit TargetUnit; //does the player have a target to convert.
		public float MaxConvertingDistance = 3.0f; //the maximum between the converter and the target unit to convert.
		public float ConvertingTime = 15.0f; //time (in seconds) in order to complete converting the unit.
		float Timer; 

		//Automatic behavior:
		public bool AutoConvert = true; //searches for enemy units to convert and does it on its own.
		public float SearchReload = 5.0f; //timer before converter looks for enemy units.
		float SearchTimer;
		public float SearchRange = 20.0f; //the range at where the converter will search for enemy units.

		//main unit script:
		[HideInInspector]
		public Unit UnitMvt;

		//Audio clips:
		public AudioClip ConvertOrderAudio; //audio clip played when the unit is ordered to convert an enemy unit.
		public GameObject ConvertEffect; //effect spawned at target unit that got converted when the conversion is done.

		void Start () {
			//get he unit mvt script:
			UnitMvt = gameObject.GetComponent<Unit> ();

			//if the game is offline and this is a NPC character.
			if (GameManager.MultiplayerGame == false && GameManager.PlayerFactionID != UnitMvt.FactionID) {
				AutoConvert = true; //must be able to auto convert.
			}
		}

		// Update is called once per frame
		void Update () {
			if (TargetUnit == null) { //if there is no target yet.
				if (IsConverting == true) { //unit is still converting but the target unit is invalid
					UnitMvt.StopMvt (); //stop converting
					UnitMvt.CancelConverting ();
				}

				if (AutoConvert == true && UnitMvt.IsIdle()) { //if the unit can convert automatically and the unit is not doing any other task
					if (GameManager.MultiplayerGame == false || (GameManager.MultiplayerGame == true && GameManager.PlayerFactionID == UnitMvt.FactionID)) { //if this is the local player in a MP game or if this is simply an offline game
						if (SearchTimer > 0) {
							SearchTimer -= Time.deltaTime;
						} else {
							//search for units 
							int i = 0;
							while (TargetUnit == null && i < UnitMvt.FactionMgr.EnemyUnits.Count) {//loop through the faction's enemy units
								//if the unit is in the defined range:
								if (Vector3.Distance (UnitMvt.FactionMgr.EnemyUnits [i].transform.position, transform.position) < SearchRange) { //if the target unit is in the defined range.
									SetTargetUnit(UnitMvt.FactionMgr.EnemyUnits [i]);
								}
								i++;
							}
							SearchTimer = SearchReload; //reload the search timer.
						}
					}
				}
			}
			//If the player has a target unit, then send him there:
			else
			{
				if (TargetUnit.FactionID == UnitMvt.FactionID) { //If the target unit belongs to the same faction
					//stop converting
					UnitMvt.StopMvt ();
					UnitMvt.CancelConverting ();
					//reset the timer:
					Timer = 0.0f;
				} else {	
					//If the unit is in range of the unit to convert
					if (Vector3.Distance (transform.position, TargetUnit.transform.position) <= (TargetUnit.NavAgent.radius+MaxConvertingDistance)) {
						if (IsConverting == false) {
							//Stop moving:
							UnitMvt.StopMvt ();

							//Start converting:
							IsConverting = true;

							//Inform the animator that we started converting:
							UnitMvt.SetAnimState (UnitAnimState.Converting);

							//custom event:
							if (UnitMvt.GameMgr.Events)
								UnitMvt.GameMgr.Events.OnUnitStartConverting (UnitMvt, TargetUnit);

							Timer = ConvertingTime;
						}
					}
					else if(UnitMvt.Moving == false) {
						//Bring the unit back:
						UnitMvt.CheckUnitPathLocal (Vector3.zero, TargetUnit.gameObject, UnitMvt.GameMgr.MvtStoppingDistance, -1);
					}

					//converting the unit
					if (IsConverting == true) {
						//converting timer:
						if (UnitMvt.Moving == false) { //only if the converter is not moving.
							if (Timer > 0) {
								Timer -= Time.deltaTime;
							}
							if (Timer < 0) {
								//Convert unit:
								if (GameManager.MultiplayerGame == false || (GameManager.MultiplayerGame == true && GameManager.PlayerFactionID == TargetUnit.FactionID)) {
									TargetUnit.ConvertUnit (UnitMvt);
								}

								Timer = 0.0f;
							}
						} else {
							//if the unit has moved while converting.
							IsConverting = false;
							//move the unit back to its target unit
							if (TargetUnit)
								UnitMvt.CheckUnitPathLocal (Vector3.zero, TargetUnit.gameObject, UnitMvt.GameMgr.MvtStoppingDistance, -1);
						}
					}
				}
			}
		}

		//Set the target unit to convert.
		public void SetTargetUnit (Unit Target)
		{
			//if it's as single player game.
			if (GameManager.MultiplayerGame == false) {
				//directly send the unit to convert
				SetTargetUnitLocal (Target);
			} else {
				//in a case of a MP game
				//and it's the unit belongs to the local player:
				if (GameManager.PlayerFactionID == UnitMvt.FactionID) {
					//ask the server to tell all clients at once that this unit is going to convert a unit

					MFactionManager.InputActionsVars NewInputAction = new MFactionManager.InputActionsVars ();
					NewInputAction.Source = UnitMvt.netId;
					NewInputAction.Target = Target.netId;

					NewInputAction.InitialPos = transform.position;
					NewInputAction.TargetPos = Target.transform.position;

					UnitMvt.GameMgr.Factions [UnitMvt.FactionID].MFactionMgr.InputActions.Add (NewInputAction);
				}
			}
		}

		//Set the unit to convert
		public void SetTargetUnitLocal (Unit Target)
		{
			if (Target == null || TargetUnit == Target)
				return;

			//Make sure that the unit does not belong to the converter's faction
			if (Target.FactionID != UnitMvt.FactionID) {
				UnitMvt.CancelConverting (); //stop converting the last unit (if there's any)

				IsConverting = false;

				TargetUnit = Target;

				//Move the unit:
				UnitMvt.CheckUnitPathLocal (Vector3.zero, TargetUnit.gameObject, UnitMvt.GameMgr.MvtStoppingDistance, -1);

			} else if(GameManager.PlayerFactionID == UnitMvt.FactionID) { //if this the local player
				UnitMvt.UIMgr.ShowPlayerMessage ("Target unit belongs to the same faction, can not convert.", UIManager.MessageTypes.Error);
			}
		}
	}
}