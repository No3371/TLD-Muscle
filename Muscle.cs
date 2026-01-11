using System.Collections;
using Moment;
using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gear;
using Il2CppTLD.Scenes;
using MelonLoader;
using MelonLoader.TinyJSON;
using ModData;
using UnityEngine;
using Il2CppTLD.News;
using Il2CppTLD.IntBackedUnit;

namespace Muscle
{
    internal class Muscle : MelonMod, Moment.IScheduledEventExecutor
    {
		internal static Muscle Instance { get; private set; }
		internal ModDataManager ModSave { get; private set; }
		internal MuscleData ModData { get; set; }

        public override void OnInitializeMelon()
		{
			Settings.OnLoad();
			Instance = this;

			ModSave = new ModDataManager(nameof(Muscle));
			ModData = new MuscleData();

			uConsole.RegisterCommand("reset_muscle", new Action(() => {
				ModData.AppliedCarryWeight = Settings.options.init;
			}));

			uConsole.RegisterCommand("set_muscle", new Action(() => {
				if (uConsole.GetNumParameters() == 1 && uConsole.TryGetFloat(out var f))
					ModData.AppliedCarryWeight = f;
			}));

			IEnumerator DelayedRegistration () { // Maybe this can fix a dodgy launch crash? If it's because of this?
				yield return new WaitForSeconds(5);
				Moment.Moment.RegisterExecutor(this);
			}
			MelonCoroutines.Start(DelayedRegistration());
		}

		public void ClearMuscle ()
		{
			var encumber = GameManager.m_Encumber;
            float appliedCarryWeight = Muscle.Instance.ModData.AppliedCarryWeight;
			Muscle.Instance.LoggerInstance?.Msg($"Clearing muscle: { appliedCarryWeight } ({ encumber.GetMaxCarryCapacityKG() })");
            ItemWeight appliedCarryWeightWeight = ItemWeight.FromKilograms(appliedCarryWeight);
            encumber.m_MaxCarryCapacity              -= appliedCarryWeightWeight;
            encumber.m_MaxCarryCapacityWhenExhausted -= appliedCarryWeightWeight;
            encumber.m_NoSprintCarryCapacity         -= appliedCarryWeightWeight;
            encumber.m_NoWalkCarryCapacity           -= appliedCarryWeightWeight;
            encumber.m_EncumberLowThreshold          -= appliedCarryWeightWeight;
            encumber.m_EncumberMedThreshold          -= appliedCarryWeightWeight;
            encumber.m_EncumberHighThreshold         -= appliedCarryWeightWeight;

			var fatigue = GameManager.m_Fatigue;
			fatigue.m_FatigueIncreasePerHourStanding = Mathf.Clamp(fatigue.m_FatigueIncreasePerHourStanding + appliedCarryWeight * 0.08f, 1f, 5);
			fatigue.m_FatigueIncreasePerHourWalking = Mathf.Clamp(fatigue.m_FatigueIncreasePerHourWalking + appliedCarryWeight * 0.08f, 1f, 10);
			fatigue.m_FatigueIncreasePerHourSprintingMin = Mathf.Clamp(fatigue.m_FatigueIncreasePerHourSprintingMin + appliedCarryWeight * 0.7f, 1f, 100);
			fatigue.m_FatigueIncreasePerHourSprintingMax = Mathf.Clamp(fatigue.m_FatigueIncreasePerHourSprintingMax + appliedCarryWeight * 0.7f, 1f, 200);

			fatigue.m_RestedThreshold = Mathf.Clamp(fatigue.m_RestedThreshold - appliedCarryWeight, 0, 99.9f);
			fatigue.m_SlightlyTiredThreshold = Mathf.Clamp(fatigue.m_SlightlyTiredThreshold - appliedCarryWeight, 0, 99.9f);
			fatigue.m_TiredThreshold = Mathf.Clamp(fatigue.m_TiredThreshold - appliedCarryWeight, 0, 99.9f);
			fatigue.m_VeryTiredThreshold = Mathf.Clamp(fatigue.m_VeryTiredThreshold - appliedCarryWeight, 0, 99.9f);
			fatigue.m_ExhaustedThreshold = Mathf.Clamp(fatigue.m_ExhaustedThreshold - appliedCarryWeight, 0, 99.9f);
			Muscle.Instance.LoggerInstance?.Msg($"Fatigue increase: { fatigue.m_FatigueIncreasePerHourStanding } / { fatigue.m_FatigueIncreasePerHourWalking } / { fatigue.m_FatigueIncreasePerHourSprintingMin } ~ { fatigue.m_FatigueIncreasePerHourSprintingMax } ");
			Muscle.Instance.LoggerInstance?.Msg($"Fatigue thresholds: { fatigue.m_RestedThreshold } / { fatigue.m_SlightlyTiredThreshold } / { fatigue.m_TiredThreshold } / { fatigue.m_VeryTiredThreshold } / { fatigue.m_ExhaustedThreshold }");

			var climb = GameManager.m_PlayerClimbRope;
			climb.m_FatigueDrainPerSecondClimbingUp = Mathf.Clamp(climb.m_FatigueDrainPerSecondClimbingUp + appliedCarryWeight * 0.02f, 0.1f, 1);
			climb.m_FatigueDrainPerSecondClimbingHolding = Mathf.Clamp(climb.m_FatigueDrainPerSecondClimbingHolding + appliedCarryWeight * 0.01f, 0.1f, 1);
			climb.m_FatigueDrainPerSecondClimbingDown = Mathf.Clamp(climb.m_FatigueDrainPerSecondClimbingDown + appliedCarryWeight * 0.01f, 0.1f, 1);
			Muscle.Instance.LoggerInstance?.Msg($"Climb: { climb.m_FatigueDrainPerSecondClimbingUp } / {climb.m_FatigueDrainPerSecondClimbingHolding} / { climb.m_FatigueDrainPerSecondClimbingDown }");

			var move = GameManager.m_PlayerMovement;
			move.m_WindMovementSpeedMultiplierMin = Mathf.Clamp(move.m_WindMovementSpeedMultiplierMin - appliedCarryWeight * 0.02f, 0.1f, 0.85f);
			move.m_DeepSnowSpeedMultiplierMin = Mathf.Clamp(move.m_DeepSnowSpeedMultiplierMin - appliedCarryWeight * 0.01f, 0.1f, 0.5f);
			move.m_DeepSnowSpeedMultiplierMax = Mathf.Clamp(move.m_DeepSnowSpeedMultiplierMax - appliedCarryWeight * 0.01f, 0.1f, 0.9f);
			Muscle.Instance.LoggerInstance?.Msg($"Move wind: { move.m_WindMovementSpeedMultiplierMin } ~ { move.m_WindMovementSpeedMultiplierMax }");
			Muscle.Instance.LoggerInstance?.Msg($"Move Deepsnow: { move.m_DeepSnowSpeedMultiplierMin } ~ { move.m_DeepSnowSpeedMultiplierMax }");

			var struggle = GameManager.m_PlayerStruggle;
			struggle.m_FleeChanceOnHit = Mathf.Clamp(struggle.m_FleeChanceOnHit - appliedCarryWeight * 0.2f, 1f, 5f);
			struggle.m_TapDecreasePerSecond = Mathf.Clamp(struggle.m_TapDecreasePerSecond + appliedCarryWeight * 0.4f, 10f, 30f);
			Muscle.Instance.LoggerInstance?.Msg($"Struggle onHit: { struggle.m_FleeChanceOnHit } / decrease per sec: { struggle.m_TapDecreasePerSecond }");

			Muscle.Instance.LoggerInstance?.Msg($"Cleared muscle: { encumber.GetMaxCarryCapacityKG() }");
		}
		public void ApplyMuscle (float muscle)
		{
			var encumber = GameManager.m_Encumber;
			Muscle.Instance.LoggerInstance?.Msg($"Applying muscle: { muscle } ({ encumber.GetMaxCarryCapacityKG() })");
            ItemWeight muscleWeight = ItemWeight.FromKilograms(muscle);
            encumber.m_MaxCarryCapacity              += muscleWeight;
            encumber.m_MaxCarryCapacityWhenExhausted += muscleWeight;
            encumber.m_NoSprintCarryCapacity         += muscleWeight;
            encumber.m_NoWalkCarryCapacity           += muscleWeight;
            encumber.m_EncumberLowThreshold          += muscleWeight;
            encumber.m_EncumberMedThreshold          += muscleWeight;
            encumber.m_EncumberHighThreshold         += muscleWeight;

			var fatigue = GameManager.m_Fatigue;
			fatigue.m_FatigueIncreasePerHourStanding = Mathf.Clamp(fatigue.m_FatigueIncreasePerHourStanding - muscle * 0.08f, 1f, 5);
			fatigue.m_FatigueIncreasePerHourWalking = Mathf.Clamp(fatigue.m_FatigueIncreasePerHourWalking - muscle * 0.08f, 1f, 10);
			fatigue.m_FatigueIncreasePerHourSprintingMin = Mathf.Clamp(fatigue.m_FatigueIncreasePerHourSprintingMin - muscle * 0.7f, 1f, 100);
			fatigue.m_FatigueIncreasePerHourSprintingMax = Mathf.Clamp(fatigue.m_FatigueIncreasePerHourSprintingMax - muscle * 0.7f, 1f, 200);

			fatigue.m_RestedThreshold = Mathf.Clamp(fatigue.m_RestedThreshold + muscle, 0, 99.9f);
			fatigue.m_SlightlyTiredThreshold = Mathf.Clamp(fatigue.m_SlightlyTiredThreshold + muscle, 0, 99.9f);
			fatigue.m_TiredThreshold = Mathf.Clamp(fatigue.m_TiredThreshold + muscle, 0, 99.9f);
			fatigue.m_VeryTiredThreshold = Mathf.Clamp(fatigue.m_VeryTiredThreshold + muscle, 0, 99.9f);
			fatigue.m_ExhaustedThreshold = Mathf.Clamp(fatigue.m_ExhaustedThreshold + muscle, 0, 99.9f);

			Muscle.Instance.LoggerInstance?.Msg($"Fatigue increase: { fatigue.m_FatigueIncreasePerHourStanding } / { fatigue.m_FatigueIncreasePerHourWalking } / { fatigue.m_FatigueIncreasePerHourSprintingMin } ~ { fatigue.m_FatigueIncreasePerHourSprintingMax } ");
			Muscle.Instance.LoggerInstance?.Msg($"Fatigue thresholds: { fatigue.m_RestedThreshold } / { fatigue.m_SlightlyTiredThreshold } / { fatigue.m_TiredThreshold } / { fatigue.m_VeryTiredThreshold } / { fatigue.m_ExhaustedThreshold }");

			var climb = GameManager.m_PlayerClimbRope;
			climb.m_FatigueDrainPerSecondClimbingUp = Mathf.Clamp(climb.m_FatigueDrainPerSecondClimbingUp - muscle * 0.02f, 0.1f, 1);
			climb.m_FatigueDrainPerSecondClimbingHolding = Mathf.Clamp(climb.m_FatigueDrainPerSecondClimbingHolding - muscle * 0.01f, 0.1f, 1);
			climb.m_FatigueDrainPerSecondClimbingDown = Mathf.Clamp(climb.m_FatigueDrainPerSecondClimbingDown - muscle * 0.01f, 0.1f, 1);
			Muscle.Instance.LoggerInstance?.Msg($"Climb: { climb.m_FatigueDrainPerSecondClimbingUp } / {climb.m_FatigueDrainPerSecondClimbingHolding} / { climb.m_FatigueDrainPerSecondClimbingDown }");

			var move = GameManager.m_PlayerMovement;
			move.m_WindMovementSpeedMultiplierMin = Mathf.Clamp(move.m_WindMovementSpeedMultiplierMin + muscle * 0.02f, 0.1f, 0.85f);
			move.m_DeepSnowSpeedMultiplierMin = Mathf.Clamp(move.m_DeepSnowSpeedMultiplierMin + muscle * 0.01f, 0.1f, 0.5f);
			move.m_DeepSnowSpeedMultiplierMax = Mathf.Clamp(move.m_DeepSnowSpeedMultiplierMax + muscle * 0.01f, 0.1f, 0.9f);
			Muscle.Instance.LoggerInstance?.Msg($"Move wind: { move.m_WindMovementSpeedMultiplierMin } ~ { move.m_WindMovementSpeedMultiplierMax }");
			Muscle.Instance.LoggerInstance?.Msg($"Move Deepsnow: { move.m_DeepSnowSpeedMultiplierMin } ~ { move.m_DeepSnowSpeedMultiplierMax }");

			var struggle = GameManager.m_PlayerStruggle;
			struggle.m_FleeChanceOnHit = Mathf.Clamp(struggle.m_FleeChanceOnHit + muscle * 0.2f, 1f, 5f);
			struggle.m_TapDecreasePerSecond = Mathf.Clamp(struggle.m_TapDecreasePerSecond - muscle * 0.4f, 10f, 30f);
			Muscle.Instance.LoggerInstance?.Msg($"Struggle onHit: { struggle.m_FleeChanceOnHit } / decrease per sec: { struggle.m_TapDecreasePerSecond }");

			Muscle.Instance.LoggerInstance.Msg($"Applied muscle: { encumber.GetMaxCarryCapacityKG() }");
		}


        public string ScheduledEventExecutorId => "BAStudio.Muscle";

        public void Execute(TLDDateTime time, string eventType, string? eventId, string? eventData)
        {
            switch (eventType)
			{
				case "losingMuscle":
					OnCheckLosingMuscle();
					Moment.Moment.ScheduleRelative(this, new EventRequest((0, Settings.options.reductionFreq, 0), "losingMuscle"));
					break;
			}
        }

		void OnCheckLosingMuscle ()
		{
			float eatenSince = GameManager.m_PlayerGameStats.m_CaloriesEaten - Muscle.Instance.ModData.LastCaloriesEaten_Losing;
			float burnedSince = GameManager.m_PlayerGameStats.m_CaloriesBurned - Muscle.Instance.ModData.LastCaloriesBurned_Losing;
			float slept24hrs = GameManager.m_Rest.GetNumHoursSleptInLastTwentyFour();
			Muscle.Instance.LoggerInstance?.Msg($"Muscle reduction attempt: { Moment.Moment.Now }");
			var passedHours = (Moment.Moment.Now - ModData.LastLosingTime).TotalMinutes/60f;
            float requiredEaten = 35 * passedHours + 15 * passedHours * Settings.options.reductionDifficulty * Muscle.Instance.ModData.AppliedCarryWeight;
            float requiredEaten2 = 25 * passedHours + 12 * passedHours * Settings.options.reductionDifficulty * Muscle.Instance.ModData.AppliedCarryWeight;
			
			if (GameManager.m_Hypothermia.HasHypothermia())
			{
				requiredEaten *= 1.1f;
				requiredEaten2 *= 1.1f;
			}
			if (GameManager.m_Infection.HasInfection())
			{
				requiredEaten *= 1.1f;
				requiredEaten2 *= 1.1f;
			}
			if (GameManager.m_IntestinalParasites.HasIntestinalParasites())
			{
				requiredEaten *= 1.05f;
				requiredEaten2 *= 1.05f;
			}
			requiredEaten *= (1 + GameManager.GetBrokenRibComponent().GetBrokenRibCount() * 0.01f);
			requiredEaten2 *= (1 + GameManager.GetBrokenRibComponent().GetBrokenRibCount() * 0.01f);
            Muscle.Instance.LoggerInstance?.Msg($"Slept last 24 hours: { slept24hrs }hrs, eatenSince: { eatenSince } / {requiredEaten} / {requiredEaten2}, {GameManager.m_Condition.GetConditionLevel().ToString()} / {ConditionLevel.VeryInjured.ToString()}");
			
			var impact = 0f;
			if (slept24hrs <= 4 || eatenSince < requiredEaten || burnedSince < 20 * passedHours + 100 * Muscle.Instance.ModData.AppliedCarryWeight || GameManager.m_Condition.GetConditionLevel() >= ConditionLevel.VeryInjured)
			{
				impact -= 0.02f * (passedHours/22f);
				if (slept24hrs <= 6 && (eatenSince < requiredEaten || GameManager.m_Condition.GetConditionLevel() >= ConditionLevel.Injured))
					impact -= 0.02f * (passedHours/22f);
				if (eatenSince < requiredEaten && GameManager.m_Condition.GetConditionLevel() >= ConditionLevel.Injured)
					impact -= 0.02f * (passedHours/22f);
				if (slept24hrs <= 2 || eatenSince < requiredEaten2 || GameManager.m_Condition.GetConditionLevel() >= ConditionLevel.NearDeath)
					impact -= 0.02f * (passedHours/22f);
				Muscle.Instance.LoggerInstance?.Msg($"Muscle reduced: {impact * Settings.options.reductionScale}kg...");
			}
			if (impact != 0)
			{
				ClearMuscle();
				Muscle.Instance.ModData.AppliedCarryWeight += impact;
				ApplyMuscle(Muscle.Instance.ModData.AppliedCarryWeight);
			}
			ModData.LastCaloriesBurned_Losing = GameManager.m_PlayerGameStats.m_CaloriesBurned;
			ModData.LastCaloriesEaten_Losing = GameManager.m_PlayerGameStats.m_CaloriesEaten;
			ModData.LastLosingTime = Moment.Moment.Now;
		}
    }

    internal class MuscleData
    {
        private float appliedCarryWeight;
        public void LoadData ()
		{
			CalculatedHours = int.Parse(Muscle.Instance.ModSave.Load("calculatedHours") ?? "0");
			AppliedCarryWeight = float.Parse(Muscle.Instance.ModSave.Load("appliedCarryweight") ?? Settings.options.init.ToString());
			LastCaloriesEaten = float.Parse(Muscle.Instance.ModSave.Load("lastCaloriesEaten") ?? "0");
			LastCaloriesBurned = float.Parse(Muscle.Instance.ModSave.Load("lastCaloriesBurned") ?? "0");
			LastCaloriesEaten_Losing = float.Parse(Muscle.Instance.ModSave.Load("lastCaloriesEatenDaily") ?? "0");
			LastCaloriesBurned_Losing = float.Parse(Muscle.Instance.ModSave.Load("lastCaloriesBurnedDaily") ?? "0");
			LastLosingTime = new TLDDateTime(0, 0, int.Parse(Muscle.Instance.ModSave.Load("lastLosing") ?? "0"));
		}

		public void SaveData ()
		{
			Muscle.Instance.ModSave.Save(CalculatedHours.ToString(), "calculatedHours");
			Muscle.Instance.ModSave.Save(AppliedCarryWeight.ToString(), "appliedCarryweight");
			Muscle.Instance.ModSave.Save(LastCaloriesEaten.ToString(), "lastCaloriesEaten");
			Muscle.Instance.ModSave.Save(LastCaloriesBurned.ToString(), "lastCaloriesBurned");
			Muscle.Instance.ModSave.Save(LastCaloriesEaten_Losing.ToString(), "lastCaloriesEatenDaily");
			Muscle.Instance.ModSave.Save(LastCaloriesBurned_Losing.ToString(), "lastCaloriesBurnedDaily");
			Muscle.Instance.ModSave.Save(LastLosingTime.TotalMinutes.ToString(), "lastLosing");
		}
		internal uint GameId { get; set; }
        internal int CalculatedHours { get; set;}
        internal float AppliedCarryWeight
		{
			get => appliedCarryWeight;
			set => appliedCarryWeight = Mathf.Clamp(MathF.Round(value, 3), Settings.options.min, Settings.options.max);
		}
        internal float LastCaloriesEaten { get; set; }
        internal float LastCaloriesBurned { get; set; }
        internal float LastCaloriesEaten_Losing { get; set; }
        internal float LastCaloriesBurned_Losing { get; set; }
        internal TLDDateTime LastLosingTime { get; set; }
    }

    [HarmonyPatch(nameof(Rest), nameof(Rest.EndSleeping))]
	internal static class EndSleepingPatch
	{
		internal static void Postfix ()
		{

			float conslept = GameManager.m_Rest.m_NumSecondsSleeping/3600f;
			if (conslept < 5)
			{
				Muscle.Instance.ModData.LastCaloriesBurned += 75 * conslept * Settings.options.growthDifficulty;
				Muscle.Instance.ModData.LastCaloriesEaten += 70 * conslept * Settings.options.growthDifficulty;
				return;
			}

            int totalHours = Moment.Moment.Now.TotalHours, sinceLast = totalHours - Muscle.Instance.ModData.CalculatedHours;
			float eatenSince = GameManager.m_PlayerGameStats.m_CaloriesEaten - Muscle.Instance.ModData.LastCaloriesEaten;
			float burnedSince = GameManager.m_PlayerGameStats.m_CaloriesBurned - Muscle.Instance.ModData.LastCaloriesBurned;
            Muscle.Instance.LoggerInstance?.Msg($"{Moment.Moment.Now} Total Hours: {totalHours} ({sinceLast})");
			Muscle.Instance.LoggerInstance?.Msg($"Burned: {GameManager.m_PlayerGameStats.m_CaloriesBurned} cals ({ burnedSince:+#;-#}), Eaten: {GameManager.m_PlayerGameStats.m_CaloriesEaten} cals ({ eatenSince:+#;-#})");
			Muscle.Instance.ModData.CalculatedHours = totalHours;
			Muscle.Instance.ModData.LastCaloriesBurned = GameManager.m_PlayerGameStats.m_CaloriesBurned;
			Muscle.Instance.ModData.LastCaloriesEaten = GameManager.m_PlayerGameStats.m_CaloriesEaten;

            float eatenThreshold1 = 250 + (140 + Muscle.Instance.ModData.AppliedCarryWeight * 10) * sinceLast * Settings.options.growthDifficulty;
            float eatenThreshold2 = 250 + (160 + Muscle.Instance.ModData.AppliedCarryWeight * 10) * sinceLast * Settings.options.growthDifficulty;
            float eatenThreshold3 = 300 + (180 + Muscle.Instance.ModData.AppliedCarryWeight * 10) * sinceLast * Settings.options.growthDifficulty;
            Muscle.Instance.LoggerInstance?.Msg($"Slept: { conslept } / 4hrs, eatenSince: { eatenSince } / {eatenThreshold1} / {eatenThreshold2} / {eatenThreshold3} cals, {GameManager.m_Condition.GetConditionLevel().ToString()} / {ConditionLevel.Injured.ToString()}");

			if (conslept <= 4)
			{
				Muscle.Instance.LoggerInstance.Msg($"Not growing muscle... reason: slept not long enough hours.");
				return;
			}
			else if (eatenSince < eatenThreshold1)
			{
				Muscle.Instance.LoggerInstance.Msg($"Not growing muscle... reason: eaten not enough calories.");
				return;
			}
			else if (burnedSince < 1000 + 150 * Settings.options.growthDifficulty)
			{
				Muscle.Instance.LoggerInstance.Msg($"Not growing muscle... reason: burned not enough calories.");
				return;
			}
			else if (GameManager.m_Condition.GetConditionLevel() >= ConditionLevel.Injured)
			{
				Muscle.Instance.LoggerInstance.Msg($"Not growing muscle... reason: not healthy enough.");
				return;
			}
			if (conslept > 10) conslept = 10;
			float impact = 0;
			float scale = eatenSince >= eatenThreshold3? 1.2f : eatenSince >= eatenThreshold2? 1.1f : 1f;
			scale *= Settings.options.growthScale;
			for (int i = 4; i < conslept; i++)
			{
				eatenSince -= 600 + 150 * Settings.options.growthDifficulty;
				burnedSince -= 550 + 150 * Settings.options.growthDifficulty;
				if (eatenSince < 0 || burnedSince < 0) break;
				if (i >= 8) impact += 0.015f * scale;
				else impact += 0.01f * scale;
			}
			Muscle.Instance.LoggerInstance.Msg($"Growing { impact }kg...");

			Muscle.Instance.ClearMuscle();

			Muscle.Instance.ModData.AppliedCarryWeight += impact;

			Muscle.Instance.ApplyMuscle(Muscle.Instance.ModData.AppliedCarryWeight);


			// if (GameManager.m_TimeOfDay.m_DaysSurvivedLastFrame == 0 && Muscle.Instance.ModData.CalculatedHHours <= GameManager.m_TimeOfDay.m_DaysSurvivedLastFrame)
			// 	return;
		}
	}

	[HarmonyPatch(typeof(QualitySettingsManager), nameof(QualitySettingsManager.ApplyCurrentQualitySettings))]
	internal static class Scene
	{
		internal static string CurrentScene { get; private set; }
		internal static bool InGame { get; private set;}
		static void Postfix ()
		{
			CurrentScene = null!;
			CurrentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
			InGame = false;
			if (!Check.InGame)
				return;
			InGame = true;
		}
	}

    [HarmonyPatch(nameof(SaveGameSystem), nameof(SaveGameSystem.RestoreGlobalData))]
	internal static class RestoreGlobalData
	{
    	[HarmonyPriority(Priority.Low)]
		internal static void Postfix ()
		{
            bool inGame = Check.InGame;
			// Muscle.Instance.LoggerInstance?.Msg($"---RestoreGlobalData---(InGame: {inGame})");
            if (!inGame) return;
			if (Muscle.Instance.ModSave.Load("calculatedHours") == null)
			{
				Muscle.Instance.ModData.CalculatedHours = Moment.Moment.Now.TotalHours;
				Muscle.Instance.LoggerInstance?.Msg($"Reset calculated hours.");
			}
			else
				Muscle.Instance.ModData.LoadData();
			Muscle.Instance.LoggerInstance?.Msg($"Loaded Muscle: { Muscle.Instance.ModData.AppliedCarryWeight }, E: { GameManager.m_Encumber.GetMaxCarryCapacityKG() }");
			Muscle.Instance.ApplyMuscle(Muscle.Instance.ModData.AppliedCarryWeight);
			if (!Moment.Moment.IsScheduled(Muscle.Instance.ScheduledEventExecutorId, "losingMuscle"))
				Moment.Moment.ScheduleRelative(Muscle.Instance, new EventRequest((0, Settings.options.reductionFreq, 0), "losingMuscle"));
		}
	}

    [HarmonyPatch(nameof(SaveGameSystem), nameof(SaveGameSystem.SaveGlobalData))]
	internal static class SaveGlobalData
	{
    	[HarmonyPriority(Priority.Low)]
		internal static void Postfix (SlotData slot)
		{
            bool inGame = Check.InGame;
			// Muscle.Instance.LoggerInstance?.Msg($"---SaveGlobalData---(InGame: {inGame}, { slot.m_GameId } / { slot.m_BaseName } / { slot.m_InternalName } / { slot.m_DisplayName })");
            if (!inGame) return;
			if (Muscle.Instance.ModSave.Load("calculatedHours") == null) // First entry
			{
				Muscle.Instance.LoggerInstance?.Msg($"First entry... calculatedHours: {Muscle.Instance.ModSave.Load("calculatedHours")}");
				Muscle.Instance.ModData = new(); // Clear data in memory
				Muscle.Instance.ModData.CalculatedHours = Moment.Moment.Now.TotalHours;
				Muscle.Instance.LoggerInstance?.Msg($"Cleared in memory muscle data.");
			}
			Muscle.Instance.ModData.SaveData();
			Muscle.Instance.LoggerInstance?.Msg($"Saved Muscle: { Muscle.Instance.ModData.AppliedCarryWeight }, E: { GameManager.m_Encumber.GetMaxCarryCapacityKG() }");
			if (!Moment.Moment.IsScheduled(Muscle.Instance.ScheduledEventExecutorId, "losingMuscle"))
				Moment.Moment.ScheduleRelative(Muscle.Instance, new EventRequest((0, Settings.options.reductionFreq, 0), "losingMuscle"));
		}
	}
}
