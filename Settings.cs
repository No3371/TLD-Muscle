using ModSettings;
using UnityEngine;

namespace Muscle
{
	internal class MuscleSettings : JsonModSettings
	{
		[Name("Initial Muscle")]
		[Description("The muscle you start with")]
		[Slider(-5f, 5f)]
		public float init = 0;
		[Name("Max Muscle")]
		[Description("The max muscle you could have")]
		[Slider(0, 5f)]
		public float max = 5;
		[Name("Min Muscle")]
		[Description("The min muscle you could have")]
		[Slider(-5, 0)]
		public float min = -3;
		[Name("Reduction Threshold Scale")]
		[Description("Higher means it's easier to trigger daily muscle reduction.")]
		[Slider(0.5f, 2f)]
		public float reductionDifficulty = 1;
		[Name("Reduction Impact Scale")]
		[Description("Higher means it shrink more when reduction is triggered.")]
		[Slider(0.25f, 3f)]
		public float reductionScale = 1;
		[Name("Reduction Frequency")]
		[Description("How many hours between each reduction attempt. Lower value does not speed up reduction, only makes it smoother.")]
		[Slider(8, 22)]
		public int reductionFreq = 22;
		[Name("Growth Threshold Scale")]
		[Description("Higher means it's harder to trigger muscle growth.")]
		[Slider(0.5f, 2f)]
		public float growthDifficulty = 1;
		[Name("Growth Impact Scale")]
		[Description("Higher means it grows more when growth is triggered.")]
		[Slider(0.5f, 3f)]
		public float growthScale = 1;
	}
	internal static class Settings
	{
		internal static MuscleSettings options;

		public static void OnLoad()
		{
			options = new MuscleSettings();
			options.AddToModSettings("Muscle");
		}
	}
}
