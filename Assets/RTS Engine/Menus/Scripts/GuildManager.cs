using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSEngine
{
public class GuildManager : MonoBehaviour {

		public class Guild {
			public string guildName = "Guild0";
			public string guildCode = "guild0";
			public Building[] ExtraBuildings; //A list of extra buildings that only this faction can place. 

			//criar um script nos buildings que muda as sprites automaticamente? com base na guildID.
			//for NPC factions, if one of the buildings below is unique for the faction, it must be precised (leave empty if the faction don't have special forms of these buildings)
			public Building CapitalBuilding;	//starter town center
			public Building BuildingCenter;		//post town center adquired
			public Building PopulationBuilding;	//houses
			public Building DropOffBuilding;	//storage
			//public Unit Civilian;
			//make the rest of units prefab;
			//image of guild
			//description: "Focus on economy."
			//bonus description:"Storage do stuff."
			//list - Feats: 1-9 + feat description
			//
		}

	}
}
