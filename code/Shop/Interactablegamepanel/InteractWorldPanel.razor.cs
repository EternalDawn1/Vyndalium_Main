// ShopWorldPanel.cs
using GeneralGame;
using Sandbox;

namespace GeneralGame.HUD

{
    [StyleSheet]
  
    public partial class InteractWorldPanel : PanelComponent
    {
       

       protected override void OnStart()
        {
            base.OnStart();
            SetupShopUI();
        }
        private void SetupShopUI()
        {
            if (Player.Local == null)
                return;

            
            
        }


        public void OpenShop()
        {
            // Your code to open the shop goes here
        }


       


    }
}
