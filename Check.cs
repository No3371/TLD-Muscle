using Il2Cpp;

namespace Muscle
{
    public static class Check
	{
		public static bool InGame => !GameManager.IsMainMenuActive() && !GameManager.IsBootSceneActive() && !GameManager.IsEmptySceneActive();

	}
}
