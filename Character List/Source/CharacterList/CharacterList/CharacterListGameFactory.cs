
using TenCrowns.ClientCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;

namespace CharacterList
{
    internal class CharacterListGameFactory : GameFactory
    {
        public CharacterListGameFactory() : base()
        {
            return;
        }

        public override ClientUI CreateClientUI(ClientManager pClientManager)
        {
            return (ClientUI)new CharacterListClientUI(pClientManager);
        }
    }
}
