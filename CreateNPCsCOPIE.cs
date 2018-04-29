using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNPCs : MonoBehaviour {

	public void createNPCs (){  
		Debug.Log ("Creating NPCs");

		MyMethods text = ScriptableObject.CreateInstance ("MyMethods") as MyMethods;
		CreateDistricts districts = ScriptableObject.CreateInstance ("CreateDistricts") as CreateDistricts;

		string out_filepath = @"C:\Users\henri\Desktop\Project\InScape\Assets\WorldBuilding\StoryGeneration\OutsideDistricts.txt";
		string in_filepath = @"C:\Users\henri\Desktop\Project\InScape\Assets\WorldBuilding\StoryGeneration\InsideDistricts.txt";
		string npc_tags = @"C:\Users\henri\Desktop\Project\InScape\Assets\WorldBuilding\StoryGeneration\NPCtags.txt";
		string npc_infos = @"C:\Users\henri\Desktop\Project\InScape\Assets\WorldBuilding\StoryGeneration\NPCInfos.txt";
		string in_text  = File.ReadAllText (in_filepath);	
		string out_text  = File.ReadAllText (out_filepath);
		string tag_text  = File.ReadAllText (npc_tags);
		string info_text  = File.ReadAllText (npc_infos);

		List<List<string>> infos = new List<List<string>> ();
		infos = districts.createDistricts ();

		foreach (List<string> lst_str in infos) {
			foreach (string str in lst_str) {
				Debug.Log (str);
			}
		}

		//Declarations
		//--------------------------------------------------------------------------------------------------------------------------
		//Get dictionary for outside

		Debug.Log ("Now getting dic for out");

		Dictionary<string, List<NPCClass>> outside_DistrictsWithInhabitants = new Dictionary<string, List<NPCClass>> ();

		foreach (string outsideD in infos[2]){
			List<NPCClass> npcList = new List<NPCClass> ();
			List<string> enablednpcTaglist = new List<string> ();
			List<string> neednpcTaglist = new List<string> ();
			List<string> enablednpcInfolist = new List<string> ();
			List<string> neednpcInfolist = new List<string> ();
			int howmany = int.Parse (text.getSubstringBetween(text.getSubstringBetween(out_text, @"{/ID}" + outsideD + @"{/ID}"), @"{/notMoreThanThisMuchNPC}"));

			string neededTagText = text.getSubstringBetween (out_text, @"{/ID}" + outsideD + @"{/ID}");
			string needTagString = text.getSubstringBetween (neededTagText, @"{/needTags}");
			neednpcTaglist = text.getTagsIn (needTagString);

			string enabledTagText = text.getSubstringBetween (out_text, @"{/ID}" + outsideD + @"{/ID}");
			string enabledTagString = text.getSubstringBetween (enabledTagText, @"{/enableTags}");
			neednpcTaglist = text.getTagsIn (enabledTagString);

			//get all possible tags from the districts and translate them into infos
			foreach (string tag in neednpcTaglist) {
				List<string> infolist = new List<string> ();
				infolist = NpcTagToInfo (tag, tag_text);
				neednpcInfolist.AddRange (infolist);
			}
			foreach (string tag in enablednpcTaglist) {
				List<string> infolist = new List<string> ();
				infolist = NpcTagToInfo (tag, tag_text);
				enablednpcInfolist.AddRange (infolist);
			}

			//check whether the infos are suitable
			foreach (string info in neednpcInfolist) {
				string info_subtext = text.getSubstringBetween (info_text, @"{/type}" + info + @"{/type}");
				string whitelisttext = text.getSubstringBetween (info_subtext, @"{/whitelist}");
				string blacklisttext = text.getSubstringBetween (info_subtext, @"{/blacklist}");
				List<string> whitelist = new List<string> ();
				List<string> blacklist = new List<string> ();

				if (whitelisttext == @"{}") {
					//all are suitable
				} else if (whitelisttext != "") {
					whitelist = text.getTagsIn (whitelisttext);
					neednpcInfolist = text.whitelist (neednpcInfolist, whitelist);
				} else {
					blacklist = text.getTagsIn (blacklisttext);
					neednpcInfolist = text.blacklist (neednpcInfolist, blacklist);
				}
				
			}

			foreach (string info in enablednpcInfolist) {
				string info_subtext = text.getSubstringBetween (info_text, @"{/type}" + info + @"{/type}");
				string whitelisttext = text.getSubstringBetween (info_subtext, @"{/whitelist}");
				string blacklisttext = text.getSubstringBetween (info_subtext, @"{/blacklist}");
				List<string> whitelist = text.getTagsIn (whitelisttext);
				List<string> blacklist = text.getTagsIn (blacklisttext);

				if (whitelisttext == @"{}") {
					//all are suitable
				} else if (whitelisttext != "") {
					whitelist = text.getTagsIn (whitelisttext);
					enablednpcTaglist = text.whitelist (enablednpcInfolist, whitelist);
				} else {
					blacklist = text.getTagsIn (blacklisttext);
					enablednpcTaglist = text.blacklist (enablednpcInfolist, blacklist);
				}

			}

			int counter1 = 1;
			int counter2 = 1;
			//fill them up
			foreach (string info in neednpcInfolist) {
				int howmanyOfInfo = int.Parse (text.getSubstringBetween(text.getSubstringBetween(info_text,  @"{/type}" + info + @"{/type}") , @"{/maxCount}"));
				while (howmanyOfInfo > 0 && counter1 <= 1000) {
					Debug.Log ("counter1: " + counter1.ToString());
					counter1++;
					howmanyOfInfo = howmanyOfInfo - 1;
					howmany = howmany - 1;
					NPCClass npc = new NPCClass (info);
					npcList.Add (npc);
				}
			}

			int howmanyAreEnabled = enablednpcInfolist.Count ();
			int endIfCannotAddMore = howmanyAreEnabled;
			while (howmany > 0 && endIfCannotAddMore > 0 && counter1 <=1000) {
				Debug.Log ("counter2: " + counter2.ToString());
				counter2++;
				List<int> list = new List<int> ();
				foreach(string str in enablednpcInfolist){
					list.Add (0);
				}
				for (int i = howmanyAreEnabled; i > 0; i--) {
					int howmanyOfInfo = int.Parse (text.getSubstringBetween (text.getSubstringBetween (info_text, @"{/type}" + enablednpcInfolist.ElementAt (i) + @"{/type}"), @"{/maxCount}"));
					if (list.ElementAt (i) < howmanyOfInfo) {
						NPCClass npc = new NPCClass (enablednpcInfolist.ElementAt (i));
						npcList.Add (npc);
						howmany = howmany - 1;
					} else { 
						endIfCannotAddMore--;
					}
				}


			}


			outside_DistrictsWithInhabitants.Add (outsideD, npcList);

			foreach (NPCClass npcc in npcList) {
				Debug.Log (outsideD + ": " + npcc.NPCtype);
			}
		}

		//Get dictionary for outside
		//--------------------------------------------------------------------------------------------------------------------------
		//Get dictionary for inside



		//Get dictionary for inside
		//--------------------------------------------------------------------------------------------------------------------------
		//Return

		returnClassNPC ret = new returnClassNPC ();
		ret.texts = infos [0];
		ret.AtoS4 = infos [1];

		//return ret;

	}

	public class returnClassNPC {

		public List<string> texts;
		public List<string> AtoS4;
		public Dictionary<string, List<NPCClass>> outside_DistrictsWithInhabitants;
		public Dictionary<string, List<NPCClass>> inside_DistrictsWithInhabitants;

	}

	public List<string> NpcTagToInfo(string tag, string npcTags){

		MyMethods text = ScriptableObject.CreateInstance ("MyMethods") as MyMethods;
		List<string> returnlist = new List<string> ();

		string thisTagsText = text.getSubstringBetween(npcTags, @"{/type}" + tag + @"{/type}");
		string tagsString = text.getSubstringBetween (thisTagsText, @"{/tags}");

		returnlist = text.getTagsIn (tagsString);

		return returnlist;
	}
}




























