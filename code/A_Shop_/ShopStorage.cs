using GeneralGame;
using Sandbox;
using System.Collections.Generic;
using GeneralGame.HUD;

namespace GeneralGame
{
    public class ShopStorage : Component
    {
        public bool IsOpened { get; set; }
        public bool IsDoorOpen { get; set; }

        private ShopPanel shopPanel { get; set; }
       
    

        protected override void OnAwake()
        {
            base.OnAwake();
            if ( shopPanel == null )
            {
                shopPanel = new ShopPanel();
            }
        }

        public ShopStorage() { }



        public void OpenShop()
        {
            if ( !IsOpened )
            {
                

               

                FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.ShopPanel);
                Player.Local.BlockInputs = true;
            }
            else
            {
                CloseShop();
            }
        }

        public void CloseShop()
        {
            Player.Save();
            Player.Local.BlockInputs = false;
            FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.InGameHud);
        }
    }
}