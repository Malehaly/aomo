using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/* Attack script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
	public class Attack : MonoBehaviour {

		public Sprite AttackIcon; //an icon to represent this attack type in the task panel.
        [HideInInspector]
        public int AttackID = -1; //in case there are a lot of other attack components on this object.

        public bool DirectAttack = false; //If set to true, the unit will affect damage when in range with the target, if not the damage will be affected within an object released by the unit (like a particle effect).
		public bool MoveOnAttack = false; //Is the unit allowed to move while attacking?

		//Attack type:
		public bool AttackAllTypes = true;
        public bool AttackUnits = true;
        public bool AttackBuildings = true;
		public string AttackCategories; //enter here the names the categories that the faction can attack, categories must be seperated with a comma.
		//[HideInInspector]
		public List<string> AttackCategoriesList = new List<string>();

		public float AttackReload = 2.0f; //Time between two successive attacks
		float AttackTimer;

		//Distance interval between the unit and its target unit to launch an attack.
		public float MinUnitStoppingDistance = 2.5f; //Make sure it's not 0.0f because it will lead to weird behaviour!
		public float MaxUnitStoppingDistance = 4.0f;

		//How far the unit must stand from the building to launch an attack?
		public float MinBuildingStoppingDistance = 0.5f; //Make sure it's not 0.0f because it will lead to weird behaviour!
		public float MaxBuildingStoppingDistance = 2.0f;
		[HideInInspector]
		public float LastBuildingDistance;
		//Attack range:
		public bool AttackOnAssign = true; //can attack when the player assigns a target?
		public bool AttackWhenAttacked = false; //is the unit allowed to defend itself when attacked? 
		public bool AttackInRange = false; //when an enemy unit enter in range of this unit, can the unit attack it automatically?
		public float AttackRange = 10.0f;

		//AI related settings;
		[HideInInspector]
		public Building AttackRangeCenter; //which building center the units should protect?
		[HideInInspector]
		public bool AttackRangeFromCenter = false; //should the unit protect the building center:

		public float SearchReload = 1.0f; //Search for enemy units every ..
		float SearchTimer;
		//Target:
		public bool RequireAttackTarget = true; //if set to false then the player can attack anywhere in the map (suits areal attacks).
		[HideInInspector]
		public Vector3 AttackPosition;

		[HideInInspector]
		public GameObject AttackTarget; //the target (unit or building) that the unit is attacking.
		public float FollowRange = 15.0f; //If the target leaves this range then the unit will stop following/attacking it.
		bool WasInTargetRange = false;

        //Attack target effect:
        public EffectObj AttackEffect; //this is the prefab of the attack effect that will be shown on the attak target's body when the attack is launched
        public float AttackEffectTime; //how long will the attack effect be shown for?

        //when the target changes position, then the attacker must change position
        public float ChangeMvtDistance = 2.0f;
        public Vector3 LastTargetPos;

        //Attack damage:
        [System.Serializable]
		public class DamageVars
		{
			public string Code; //the code of the unit/building that will get the below damage
			public float Damage; //the amount of damage specifically for the building/unit with the above code.
		}
		public DamageVars[] CustomDamage; //if the target unit/building code is in the list then it will be given the matching damage, if not then the default damage
		public float UnitDamage = 10.0f; //damage points when this unit attacks another unit.
		public float BuildingDamage = 10.0f; //damage points when this unit attacks a building.

		//Area attack:
		public bool AreaDamage = false;
		[System.Serializable]
		public class AttackRangesVars
		{
			public float Range = 10.0f;
			public float UnitDamage = 5.0f;
			public float BuildingDamage = 4.0f;
		}
		public AttackRangesVars[] AttackRanges;

        //Attack Delay: (global)
        public float DelayTime = 0.0f; //how much time before the actual attack is launched?
        float DelayTimer;
        public bool UseDelayTrigger = false; //if we want to use another component to trigger the attack, then enable this.
        [HideInInspector]
        public bool AttackTriggered = false;
        public bool PlayAnimInDelay = false; //should the character play the animation while waiting for the delay?

        //DoT:
        [System.Serializable]
        public class DoTVars
        {
            public bool Enabled = false; //enable Damage over time?
            [HideInInspector]
            public float Damage; //How much damage will be dealt to the target? In the attack component, this is chosen from the other damage settings
            public bool Infinite = false; //if set to true, the DoT won't stop until the target is dead or another component requires that.
            public float Duration = 20.0f; //the duration of the DoT attack.
            public float Cycle = 4.0f; //When will damage be applied.
            public GameObject Source; //the attacker who launched this attack
        }
        public DoTVars DoT;

        public enum AttackTypes {Random, InOrder}; //in the case of having a lot of attack sources, there are two mods, the first is to choose  and the second is attacking from all sources in order
		public AttackTypes AttackType = AttackTypes.Random & AttackTypes.InOrder;
		[System.Serializable]
		public class AttackSourceVars
		{
			public float AttackDelay = 0.2f;
			public EffectObj AttackObj; //attack object prefab (must have both the EffectObj and AttackObject components).
			public float AttackObjDestroyTime = 3.0f; //life duration of the attack object
			public Transform AttackObjSource; //Where will the attack object be sent from?
			public float AttackObjSpeed = 10.0f; //how fast is the attack object moving
			public bool DamageOnce = true; //should the attack object do damage once it hits a building/unit and then do no more damage.
			public bool DestroyAttackObjOnDamage = true; //should the attack object get destroyed after it has caused damage.
		}
		public AttackSourceVars[] AttackSources;
		[HideInInspector]
		public Vector3 MvtVector; //The attack object movement direction
		public float AttackStepTimer;
		public int AttackStep;

        public GameObject WeaponObj; //When assigned, this object will be rotated depending on the target's position.
        public bool FreezeRotX = false;
        public bool FreezeRotY = false;
        public bool FreezeRotZ = false;
        public bool SmoothRotation = true; //allow smooth rotation?
        public float RotationDamping = 2.0f; //rotation damping (when smooth rotation is enabled)
        public Vector3 WeaponIdleAngles;
        public Quaternion WeaponIdleRotation;

        //Attacking anim timer:
        public float AttackAnimTime = 0.2f; //Must be lower than the duration of the attacking animation.
		[HideInInspector]
		public float AttackAnimTimer;

		//is the attacker a building or a unit?
        public enum Attackers {Unit, Building}
        public Attackers Attacker;

        [HideInInspector]
		public Unit UnitMgr;
        [HideInInspector]
        public Building BuildingMgr;

		GameManager GameMgr;

		//Other scripts:
		EffectObjPool ObjPool;

		//Army Unit ID:
		[HideInInspector]
		public int ArmyUnitID = -1;

		//Audio:
		public AudioClip AttackOrderSound; //played when the unit is ordered to attack
		public AudioClip AttackSound; //played each time the unit attacks.

		void Awake ()
		{
            CheckAttacker(); //check if the attacker setting matches the object where this component is at.

            AttackID = -1; //reset the attack ID (the multiple attack, if existant will set it later in Start () )

            //if it attacks all targets, clear the list:
            if (AttackAllTypes == true)
                AttackCategoriesList.Clear();

            //Set the DoT source if we're ever going to use that:
            DoT.Source = gameObject;
        }

        //check if the attacker setting matches the object where this component is at.
        void CheckAttacker()
        {
            //attempt to get the unit and building components
            UnitMgr = gameObject.GetComponent<Unit>();
            BuildingMgr = gameObject.GetComponent<Building>();

            //check if they are valid for the chosen attacker setting
            if (UnitMgr == null && Attacker == Attackers.Unit)
            {
                Debug.LogError("Attack is set to 'Unit' but there's no 'Unit.cs' component.");
            }
            else if (BuildingMgr == null && Attacker == Attackers.Building)
            {
                Debug.LogError("Attack is set to 'Building' but there's no 'Building.cs' component.");
            }
        }

        //determines if the attacker is dead or not
        bool CanAttack ()
        {
            if (Attacker == Attackers.Unit) //if the attacker is a unit
            {
                if (UnitMgr.Dead == true) //and it's dead
                {
                    return false; //nope
                }
            }
            else if (Attacker == Attackers.Building) //if the attacker is a building
            {
                if(BuildingMgr.Health <= 0.0f || BuildingMgr.IsBuilt == false) //if the building's health is null (destroyed or getting destroyed) or the building is not built
                {
                    return false; //nope
                }
            }

            //if we reach this stage then
            return true; //yeah
        }

        //cancels the attack:
        void CancelAttack ()
        {
            if (Attacker == Attackers.Unit) //if the attacker is a unit
            {
                UnitMgr.CancelAttack();
                if(UnitMgr.Moving == true) //if the unit is moving
                {
                    UnitMgr.StopMvt(); //stop
                }
            }

            //for both:
            AttackTarget = null; //set the target to null.
        }

        //is the attacker idle (when searching for an attack target, we need to check if the attacker is not doing something else):
        bool IsIdle ()
        {
            if(Attacker == Attackers.Unit) //concerns units only
            {
                return UnitMgr.IsIdle();
            }

            return true;
        }

        //gets the attacker's faction ID
        int AttackerFactionID()
        {
            if (Attacker == Attackers.Unit) //if the attacker is a unit
            {
                return UnitMgr.FactionID;
            }
            else if (Attacker == Attackers.Building) //if the attacker is a building
            {
                return BuildingMgr.FactionID;
            }

            return -1;
        }

        void Start () {
            //get the ame manager script
            GameMgr = GameManager.Instance;

			//default values for the timers:
			AttackTimer = 0.0f;
			SearchTimer = 0.0f;

            ObjPool = EffectObjPool.Instance;

            //Unit only:
            if (Attacker == Attackers.Unit)
            {
                if (UnitMgr.NavAgent != null)
                {
                    //If the min unit/building attacking distance is smaller than the nav mesh agent diameter then it would be impossible for the unit to reach its target, so we set it to the minimal possible value:
                    if (MinUnitStoppingDistance < UnitMgr.NavAgent.radius * 2)
                    {
                        MinUnitStoppingDistance = UnitMgr.NavAgent.radius * 2;
                    }
                    if (MinBuildingStoppingDistance < UnitMgr.NavAgent.radius * 2)
                    {
                        MinBuildingStoppingDistance = UnitMgr.NavAgent.radius * 2;
                    }
                }
            }

			if (AttackAllTypes == false) { //if it can not attack all the units
				AttackManager.Instance.AssignAttackCategories (AttackCategories, ref AttackCategoriesList);
			}	

            //if there's a weapon object:
            if(WeaponObj)
            {
                WeaponIdleRotation.eulerAngles = WeaponIdleAngles;
            }
		}

		void Update () {

            //Unit only:
            if (Attacker == Attackers.Unit)
            {
                //the attack animation timer:
                if (AttackAnimTimer > 0)
                {
                    AttackAnimTimer -= Time.deltaTime;
                }
                if (AttackAnimTimer < 0)
                {
                    AttackAnimTimer = 0;
                    //stop showing the attacking animation when the timer is done.
                    if (UnitMgr.AnimMgr)
                    {
                        if (UnitMgr.AnimMgr.GetBool("IsAttacking") == true)
                        {
                            UnitMgr.AnimMgr.SetBool("IsAttacking", false);
                        }
                    }
                }
            }

            //Attack timer:
            if (AttackTimer > 0)
            {
                AttackTimer -= Time.deltaTime;
            }

            if (CanAttack() == true) //If the attacker is still not dead/destroyed
			{
				if(GameMgr.PeaceTime <= 0) //if we're not in the peace time
				{
                    if (AttackTarget == null)
                    { //If the target is not set yet
                        if (WeaponObj != null) //if there's a weapon object
                        {
                            if (SmoothRotation == false)
                            { //if the rotation is automatically changed
                                WeaponObj.transform.localRotation = WeaponIdleRotation;
                            }
                            else
                            {
                                //smooth rotation here:
                                WeaponObj.transform.localRotation = Quaternion.Slerp(WeaponObj.transform.localRotation, WeaponIdleRotation, Time.deltaTime * RotationDamping);
                            }
                        }

                        if (GameManager.MultiplayerGame == false || (GameManager.MultiplayerGame == true && GameManager.PlayerFactionID == AttackerFactionID()))
                        { //if this is an offline game or online but this is the local player
                            if (AttackInRange == true && IsIdle () == true) //if the attacker can attack in range and it's also in idle state
                            {
                                //if the faction is a NPC or a local player and having a target is required.
                                if (GameManager.PlayerFactionID != AttackerFactionID() || (GameManager.PlayerFactionID == AttackerFactionID() && RequireAttackTarget == true))
                                {

                                    //Search timer:
                                    if (SearchTimer > 0)
                                    {
                                        SearchTimer -= Time.deltaTime;
                                    }
                                    else
                                    {

                                        //Search if there are enemy units in range:
                                        bool Found = false;

                                        float Size = AttackRange;
                                        Vector3 SearchFrom = transform.position;

                                        //only for NPC factions:

                                        //if there's no city center to protect:
                                        if (AttackRangeCenter == null)
                                        {
                                            AttackRangeFromCenter = false; //we're not defending any city center then:
                                        }
                                        //if there's a city center to protect
                                        if (AttackRangeFromCenter == true && AttackRangeCenter != null)
                                        {
                                            SearchFrom = AttackRangeCenter.transform.position; //the search pos is the city center
                                            Size = AttackRangeCenter.GetComponent<Border>().Size; //and the search size is the whole city border size:
                                        }

                                        Collider[] ObjsInRange = Physics.OverlapSphere(SearchFrom, Size);
                                        int i = 0; //counter
                                        while (i < ObjsInRange.Length && Found == false)
                                        {

                                            Unit UnitInRange = ObjsInRange[i].gameObject.GetComponent<Unit>();
                                            if (UnitInRange)
                                            { //If it's a unit object 
                                                if (UnitInRange.enabled == true)
                                                { //if the unit comp is enabled
                                                  //If this unit and the target have different teams and make sure it's not dead.
                                                    if (UnitInRange.FactionID != AttackerFactionID() && UnitInRange.Dead == false)
                                                    {
                                                        //if the unit is visible:
                                                        if (UnitInRange.IsInvisible == false)
                                                        {
                                                            if (AttackTarget == null)
                                                            {
                                                                if (AttackManager.Instance.CanAttackTarget(this, ObjsInRange[i].gameObject, AttackCategoriesList))
                                                                { //if the unit can attack the target.
                                                                  //Set this unit as the target 
                                                                    SetAttackTarget(ObjsInRange[i].gameObject);
                                                                    Found = true;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            i++;
                                        }

                                        if (Found == false)
                                        {
                                            SearchTimer = SearchReload; //No enemy units found? search again.
                                        }
                                        else
                                        {
                                            //if the attacker is a unit:
                                            if (Attacker == Attackers.Unit)
                                            {
                                                //Follow the taraget:
                                                UnitMgr.CheckUnitPath(Vector3.zero, AttackTarget, MaxUnitStoppingDistance + AttackTarget.GetComponent<Unit>().NavAgent.radius + UnitMgr.NavAgent.radius, 0, true);
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
					else
                    { 
						//if the target went invisible:
						//Checking whether the target is dead or not (if it's a unit or a building.
						bool Dead = false;

						if (AttackTarget.GetComponent<Unit> ()) { 
							//if the target went invisible:
							if (AttackTarget.GetComponent<Unit> ().IsInvisible == true) {
								//stop attacking it:
								CancelAttack ();
								return;
							} else if (AttackTarget.GetComponent<Unit> ().FactionID == AttackerFactionID()) {
								CancelAttack ();
								return;
							}
							Dead = AttackTarget.GetComponent<Unit> ().Dead;
						} else if (AttackTarget.GetComponent<Building> ()) {
							if (AttackTarget.GetComponent<Building> ().FactionID == AttackerFactionID()) {
								CancelAttack ();
								return;
							}
							//Dead = !(Target.GetComponent<Building> ().Health > 0);
						}

						if (Dead == true && AttackTimer <= 0) {
							//if the target unit is dead, cancel everything
							CancelAttack ();
						} else {
							//If the current unit has a target,
							if (Vector3.Distance (this.transform.position, AttackTarget.transform.position) > FollowRange && AttackRangeFromCenter == false && AttackTarget.GetComponent<Unit> () && WasInTargetRange == true) { //This means that the target has left the follow range of the unit.
                                CancelAttack();
                                //This unit doesn't have a target anymore.
                            } else {

                                if (Attacker == Attackers.Unit) //if the attacker is a unit
                                {
                                    //if this is the local player in a MP game or simply an offline game:
                                    if (GameManager.MultiplayerGame == false || (GameManager.MultiplayerGame == true && GameManager.PlayerFactionID == AttackerFactionID()))
                                    {

                                        //if the attack target went away:
                                        if (Vector3.Distance(UnitMgr.NavAgent.destination, AttackTarget.transform.position) > ChangeMvtDistance && UnitMgr.Moving == true)
                                        {
                                            UnitMgr.CheckUnitPath(Vector3.zero, AttackTarget, UnitMgr.NavAgent.stoppingDistance, 0, true);
                                        }

                                        //if destination is reached but the unit is still far away from the target.
                                        if (Vector3.Distance(UnitMgr.NavAgent.destination, AttackTarget.transform.position) > UnitMgr.NavAgent.stoppingDistance && UnitMgr.DestinationReached == true)
                                        { //THE PROBLEM IS HERE: When unit reaches the destination but max distance is probably more 
                                            UnitMgr.DestinationReached = false;
                                        }

                                        if (UnitMgr.DestinationReached == false && UnitMgr.Moving == false)
                                        { //If the unit didn't reach its target and it looks like it's not moving:
                                          //Follow the target:
                                            UnitMgr.CheckUnitPath(Vector3.zero, AttackTarget, UnitMgr.NavAgent.stoppingDistance, 0, true);
                                        }
                                    }

                                    //if the attacker is in the correct range of his target
                                    if (UnitMgr.DestinationReached == true)
                                    {
                                        if(MoveOnAttack == false && UnitMgr.Moving == true) //if move on attack is disabled and the unit is still moving
                                        {
                                            return; //stop
                                        }
                                    }
                                    else
                                    {
                                        return; //if the unit didn't reach the target then don't proceed
                                    }

                                }

                                //reaching this stage means that the attacker is in range of the target and it's all good to go:

                                WasInTargetRange = true;
                                //Make the attack object look at the target:
                                if (WeaponObj != null)
                                {
                                    Vector3 LookAt = AttackTarget.transform.position - WeaponObj.transform.position;
                                    //which axis should not be rotated? 
                                    if (FreezeRotX == true)
                                        LookAt.x = 0.0f;
                                    if (FreezeRotY == true)
                                        LookAt.y = 0.0f;
                                    if (FreezeRotZ == true)
                                        LookAt.z = 0.0f;
                                    Quaternion TargetRotation = Quaternion.LookRotation(LookAt);
                                    if (SmoothRotation == false)
                                    { //if the rotation is automatically changed
                                        WeaponObj.transform.rotation = TargetRotation;
                                    }
                                    else
                                    {
                                        //smooth rotation here:
                                        WeaponObj.transform.rotation = Quaternion.Slerp(WeaponObj.transform.rotation, TargetRotation, Time.deltaTime * RotationDamping);
                                    }

                                }

                                //Delay timer here: 
                                if (DelayTimer > 0)
                                {
                                    DelayTimer -= Time.deltaTime;
                                }

                                if (Attacker == Attackers.Unit)
                                {
                                    //Playing animation:
                                    if (UnitMgr.AnimMgr && (PlayAnimInDelay == true || (PlayAnimInDelay == false && DelayTimer <= 0.0f && AttackTriggered == true)))
                                    {
                                        if (UnitMgr.AnimMgr.GetBool("IsAttacking") == false)
                                        {
                                            UnitMgr.AnimMgr.SetBool("IsAttacking", true);
                                        }
                                    }
                                }

                                //If the attack delay is over, launch the attack
                                if (DelayTimer <= 0.0f && AttackTriggered == true)
                                {

                                    if (AttackTimer <= 0)
                                    { //if the attack timer is ready:

                                        //Custom event:
                                        if (GameMgr.Events)
                                            GameMgr.Events.OnAttackPerformed(this, AttackTarget, AttackID);

                                        //Is this a direct attack (no use of attack objects)?
                                        if (DirectAttack == true)
                                        {
                                            if (AreaDamage == true)
                                            {
                                                //launch area damage and provide all arguments:
                                                AttackManager.Instance.LaunchAreaDamage(AttackTarget.transform.position, AttackRanges, AttackerFactionID(), AttackEffect, AttackEffectTime, AttackCategoriesList, DoT);
                                            }
                                            else if (DoT.Enabled == true)
                                            {
                                                Unit TargetUnit = AttackTarget.GetComponent<Unit>();
                                                //only for units currently?
                                                if (TargetUnit)
                                                {
                                                    //DoT settings:
                                                    TargetUnit.DoT = DoT;
                                                    TargetUnit.DoT.Damage = AttackManager.GetDamage(AttackTarget, CustomDamage, UnitDamage);
                                                }
                                            }
                                            else
                                            {
                                                //if this is no areal damage and no DoT
                                                //deal damage to unit/building:
                                                if (AttackTarget.GetComponent<Unit>())
                                                {
                                                    AttackTarget.GetComponent<Unit>().AddHealth(-AttackManager.GetDamage(AttackTarget, CustomDamage, UnitDamage), this.gameObject);
                                                    //Spawning the damage effect object:
                                                    AttackManager.Instance.SpawnEffectObj(AttackTarget.GetComponent<Unit>().DamageEffect, AttackTarget, 0.0f, true);
                                                    //spawn attack effect object: units only currently
                                                    AttackManager.Instance.SpawnEffectObj(AttackEffect, AttackTarget, AttackEffectTime, false);
                                                }
                                                else if (AttackTarget.GetComponent<Building>())
                                                {
                                                    AttackTarget.GetComponent<Building>().AddHealth(-AttackManager.GetDamage(AttackTarget, CustomDamage, BuildingDamage), this.gameObject);
                                                    //Spawning the damage effect object:
                                                    AttackManager.Instance.SpawnEffectObj(AttackTarget.GetComponent<Building>().DamageEffect, AttackTarget, 0.0f, true);
                                                }
                                            }

                                            //Play the attack audio:
                                            AudioManager.PlayAudio(gameObject, AttackSound, false);

                                            if (Attacker == Attackers.Unit)
                                            {
                                                UnitMgr.SetAnimState(UnitAnimState.Attacking);
                                            }

                                            //reload the timers

                                            AttackTimer = AttackReload;
                                            AttackAnimTimer = AttackAnimTime;
                                        }
                                        else
                                        { //If the unit can launch attack objs towards the target unit
                                            if (AttackStep < AttackSources.Length && AttackSources.Length > 0)
                                            { //if we haven't already launched attacks from all sources

                                                if (AttackStepTimer > 0)
                                                {
                                                    AttackStepTimer -= Time.deltaTime;
                                                }
                                                if (AttackStepTimer <= 0)
                                                {
                                                    AttackTimer = AttackReload;
                                                    AttackAnimTimer = AttackAnimTime;

                                                    AttackObject AttackObj = ObjPool.GetEffectObj(EffectObjPool.EffectObjTypes.AttackObj, AttackSources[AttackStep].AttackObj).GetComponent<AttackObject>();
                                                    AttackObj.transform.position = AttackSources[AttackStep].AttackObjSource.transform.position; //Set the attack object's position:
                                                    AttackObj.gameObject.SetActive(true); //Activate the attack object

                                                    AttackObj.DefaultUnitDamage = UnitDamage;
                                                    AttackObj.DefaultBuildingDamage = BuildingDamage;
                                                    AttackObj.CustomDamage = CustomDamage;

                                                    Vector3 TargetPos = AttackTarget.transform.position;

                                                    if (AttackTarget.GetComponent<Unit>())
                                                    {
                                                        AttackObj.TargetFactionID = AttackTarget.GetComponent<Unit>().FactionID;
                                                        TargetPos = AttackTarget.GetComponent<Unit>().PlayerSelection.transform.position;
                                                    }
                                                    else if (AttackTarget.GetComponent<Building>())
                                                    {
                                                        AttackObj.TargetFactionID = AttackTarget.GetComponent<Building>().FactionID;
                                                        TargetPos = AttackTarget.GetComponent<Building>().PlayerSelection.transform.position;
                                                    }

                                                    AttackObj.DamageOnce = AttackSources[AttackStep].DamageOnce;
                                                    AttackObj.DestroyOnDamage = AttackSources[AttackStep].DestroyAttackObjOnDamage;

                                                    AttackObj.Source = gameObject;
                                                    AttackObj.SourceFactionID = AttackerFactionID();

                                                    if (AreaDamage == false)
                                                    {
                                                        AttackObj.DidDamage = false;
                                                        AttackObj.DoDamage = !DirectAttack;
                                                        AttackObj.AreaDamage = false;
                                                    }
                                                    else
                                                    {
                                                        AttackObj.DamageOnce = true;
                                                        AttackObj.DidDamage = false;
                                                        AttackObj.AreaDamage = true;
                                                        AttackObj.AttackRanges = AttackRanges;
                                                        AttackObj.AttackCategoriesList = AttackCategoriesList;

                                                    }

                                                    //Damage over time:
                                                    AttackObj.DoT = DoT;

                                                    //Attack object movement:
                                                    AttackObj.MvtVector = (TargetPos - AttackSources[AttackStep].AttackObjSource.transform.position) / Vector3.Distance(AttackTarget.transform.position, AttackSources[AttackStep].AttackObjSource.transform.position);
                                                    AttackObj.Speed = AttackSources[AttackStep].AttackObjSpeed;

                                                    //Set the attack obj's rotation so that it looks at the target:
                                                    AttackObj.transform.rotation = Quaternion.LookRotation(TargetPos - AttackObj.transform.position);

                                                    //Hide the attack object after some time:
                                                    AttackObj.DestroyTimer = AttackSources[AttackStep].AttackObjDestroyTime;
                                                    AttackObj.ShowAttackObjEffect();

                                                    //Play the attack audio:
                                                    AudioManager.PlayAudio(gameObject, AttackSound, false);

                                                    //-----------------------------------------------------------------------------------------------

                                                    //search for the next attack object:
                                                    if (AttackType == AttackTypes.InOrder)
                                                    { //if the attack types is in order
                                                        AttackStep++;
                                                    }

                                                    if (AttackStep >= AttackSources.Length || AttackType == AttackTypes.Random)
                                                    { //end of attack round:
                                                      //Reload the attack timer:
                                                        AttackStep = 0;
                                                        AttackStepTimer = AttackSources[AttackStep].AttackDelay;
                                                    }
                                                    else
                                                    {
                                                        AttackStepTimer = AttackSources[AttackStep].AttackDelay;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
							}
						}
					} 
				}
			}
		}

		//Set attack target:
		public void SetAttackTarget (GameObject Obj)
		{
            //if the attacker is a unit:
            if (Attacker == Attackers.Unit)
            {
                UnitMgr.DestinationReached = false; //to make the unit move to the target.
            }

            AttackTarget = Obj;
            AttackTimer = 0.0f; //first attack comes directly, then timer will start.

            LastTargetPos = AttackTarget.transform.position;


            if (DirectAttack == false) {
				//other settings here:
				if (AttackType == AttackTypes.Random) { //if the attack type is random
					AttackStep = Random.Range (0, AttackSources.Length); //pick a random source
				} else if (AttackType == AttackTypes.InOrder) { //if it's in order
					AttackStep = 0; //start with the first attack source:
				}

				AttackStepTimer = AttackSources [AttackStep].AttackDelay;
			}

			WasInTargetRange = false;

            //set the attack delay options:
            DelayTimer = DelayTime;
            //do we have an attack trigger option?
            if (UseDelayTrigger == true)
            {
                AttackTriggered = false;
            }
            else
            {
                AttackTriggered = true;
            }

            //Custom event:
            if (GameMgr.Events)
                GameMgr.Events.OnAttackTargetLocked(this, AttackTarget, AttackID);
        }

		public void ResetAttack () //reset the values of the attack:
		{
			AttackStep = 0;
			AttackStepTimer = 0.0f;
			AttackTimer = 0.0f;
			AttackTarget = null;
			WasInTargetRange = false;
		}
	}
}