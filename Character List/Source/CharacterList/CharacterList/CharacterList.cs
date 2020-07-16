using TenCrowns.AppCore;
using TenCrowns.GameCore;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterList
{
    public class CharacterList : ModEntryPointAdapter
    {
        public override void Initialize(ModSettings modSettings)
        {
            Debug.Log((object)"Character List DLL initialising");
            base.Initialize(modSettings);
            modSettings.Factory = new CharacterListGameFactory();
        }
    }
}
