using Sandbox;
using Sandbox.ui.Hud;
using System;

namespace GeneralGame.HUD
{
    [StyleSheet]
    public partial class ShopPanel : Panel
    {
        public int currentPage = 0;
        private int itemsPerPage = 12; // Anzahl der Items pro Seite
        private int totalPages => (int)Math.Ceiling( (double)Player.Local.Inventory.StorageItems.Count / itemsPerPage );
        public ItemComponent upgradeItem;
        private bool isUpgradePanelVisible = false;
        private int upgradeCost = 1500;


        public void CheckUpgradeSlot()
        {
            isUpgradePanelVisible = Player.Local.Inventory.UpgradeItems.Any( item => item != null && item.ItemLevel < 27 );
            if ( isUpgradePanelVisible )
            {
                upgradeItem = Player.Local.Inventory.UpgradeItems.First( item => item != null && item.ItemLevel < 27 );
                // Hier können Sie die Upgrade-Kosten basierend auf dem Item-Level berechnen
                upgradeCost = CalculateUpgradeCost( upgradeItem.ItemLevel , upgradeItem.Tier );
            }
            else
            {
                upgradeItem = null;
                upgradeCost = 0;
                isUpgradePanelVisible = false;
            }
        }

        private int CalculateUpgradeCost( int itemLevel, GeneralGame.Tier tier )
        {
            // Beispielhafte Berechnung der Upgrade-Kosten basierend auf dem Item-Level und Tier
            int baseCost = itemLevel switch
            {
                1 => 500,
                2 => 1500,
                3 => 5000,
                24 => 10000,
                25 => 15000,
                26 => 20000,
                27 => 25000,
                _ => (int)(500 * Math.Pow( 1.3, itemLevel - 1 )), // Exponentielle Berechnung für andere Level
            };

            // Zusätzliche Kosten basierend auf dem Tier
            double tierMultiplier = Math.Pow( 1.6, (double)tier - 1 );
            return (int)(baseCost * tierMultiplier);
        }

        private void UpgradeItem()
        {
            if ( upgradeItem != null && Player.Local.Vyndalium >= upgradeCost && upgradeItem.ItemLevel < 27 )
            {
                
                if ( upgradeItem.DMG > 0 ) Player.Local.AttackValue += (float)(upgradeItem.DMG * 0.3);
                if ( upgradeItem.Health > 0 ) Player.Local.Health += (float)(upgradeItem.Health * 0.3);
                if ( upgradeItem.Armor > 0 ) Player.Local.Armor += (float)(upgradeItem.Armor * 0.3);
                if ( upgradeItem.STG > 0 ) Player.Local.STG += (float)(upgradeItem.STG * 0.3);
                if ( upgradeItem.HE > 0 ) Player.Local.HE += (float)(upgradeItem.HE * 0.3);
                if ( upgradeItem.DEX > 0 ) Player.Local.DEX += (float)(upgradeItem.DEX * 0.3);
                if ( upgradeItem.PER > 0 ) Player.Local.PER += (float)(upgradeItem.PER * 0.3);
                if ( upgradeItem.INT > 0 ) Player.Local.INT += (float)(upgradeItem.INT * 0.3);
                if ( upgradeItem.Mana > 0 ) Player.Local.MaxMana += (float)(upgradeItem.Mana * 0.3);
                if ( upgradeItem.CritHitDamage > 0 ) Player.Local.IncreaseCritHitDamage( (float)(upgradeItem.CritHitDamage * 0.3) );
                if ( upgradeItem.CritHitChance > 0 ) Player.Local.IncreaseCritHitChance( (float)(upgradeItem.CritHitChance * 0.3) );
                if ( upgradeItem.AbilityHaste > 0 ) Player.Local.AbilityHaste += (float)(upgradeItem.AbilityHaste * 0.3);
                if ( upgradeItem.AttackPower > 0 ) Player.Local.AttackPower += (float)(upgradeItem.AttackPower * 0.3);
                if ( upgradeItem.MagicPower > 0 ) Player.Local.MagicPower += (float)(upgradeItem.MagicPower * 0.3);
                if ( upgradeItem.AttackSpeed > 0 ) Player.Local.AttackSpeed += (float)(upgradeItem.AttackSpeed * 0.3);
                if ( upgradeItem.MoveSpeed > 0 ) Player.Local.MoveSpeed += (float)(upgradeItem.MoveSpeed * 0.3);
                if ( upgradeItem.MagicDefense > 0 ) Player.Local.MagicDefense += (float)(upgradeItem.MagicDefense * 0.3);
                if ( upgradeItem.Evasion > 0 ) Player.Local.Evasion += (float)(upgradeItem.Evasion * 0.3);
                if ( upgradeItem.Cover > 0 ) Player.Local.Block += (float)(upgradeItem.Cover * 0.3);
                if ( upgradeItem.BonusEXP > 0 ) Player.Local.BonusEXPGain += (float)(upgradeItem.BonusEXP * 0.3);
                if ( upgradeItem.BonusScore > 0 ) Player.Local.BonusScore += (float)(upgradeItem.BonusScore * 0.3);
                if ( upgradeItem.BonusVyndalium > 0 ) Player.Local.BonusVyndalium += (float)(upgradeItem.BonusVyndalium * 0.3);
                if ( upgradeItem.Tenacity > 0 ) Player.Local.Tenacity += (float)(upgradeItem.Tenacity * 0.3);
                if ( upgradeItem.StunResistance > 0 ) Player.Local.StunResist += (float)(upgradeItem.StunResistance * 0.3);
                if ( upgradeItem.BlindResistance > 0 ) Player.Local.BlindResist += (float)(upgradeItem.BlindResistance * 0.3);
                if ( upgradeItem.BleedResistance > 0 ) Player.Local.BleedResist += (float)(upgradeItem.BleedResistance * 0.3);
                if ( upgradeItem.SlowResistence > 0 ) Player.Local.SlowResist += (float)(upgradeItem.SlowResistence * 0.3);
                if ( upgradeItem.FireResistence > 0 ) Player.Local.FireResist += (float)(upgradeItem.FireResistence * 0.3);
                if ( upgradeItem.PoisonResistence > 0 ) Player.Local.PoisonResist += (float)(upgradeItem.PoisonResistence * 0.3);
                if ( upgradeItem.IceResistence > 0 ) Player.Local.IceResist += (float)(upgradeItem.IceResistence * 0.3);
                if ( upgradeItem.LightningResistence > 0 ) Player.Local.LightningResist += (float)(upgradeItem.LightningResistence * 0.3);
                if ( upgradeItem.HolyResistence > 0 ) Player.Local.LightResist += (float)(upgradeItem.HolyResistence * 0.3);
                Player.Local.Vyndalium -= upgradeCost;
                upgradeItem.ItemLevel++;
                Hudmaster.Instance.ShowNotification( $"Item {upgradeItem.Name} have been upgraded to {upgradeItem.ItemLevel} !" , "/ui/hud/success.gif" );
                CheckUpgradeSlot(); // Aktualisieren Sie den Panel-Zustand
            }
            else
            {
                Hudmaster.Instance.ShowNotification( "Not enough Money" , "/ui/hud/error.gif" );
            }
        }




        private void PreviousPage()
        {
            if ( currentPage > 0 )
            {
                currentPage--;
            }
        }

        private void NextPage()
        {
            if ( currentPage < totalPages - 1 )
            {
                currentPage++;
            }
        }
       

        public static bool IsDragging { get; private set; }
        public static new bool IsVisible { get; set; }
        public ShopStorage shopStorage { get; private set; }
        private ShopInteractable shopInteractable;
        private bool isInitialized = false;
        public static ShopPanel Instance { get; private set; }

        public enum PanelType
        {
            Inventory,
            StoreItems,
            Weapons,
            Armor,
            Accessories
        }

        public PanelType currentPanel = PanelType.Inventory;

        public void ShowPanel( PanelType panel )
        {
            currentPanel = panel;
        }

        public ShopPanel()
        {
            Instance = this;
            shopStorage = new ShopStorage();
            shopInteractable = new ShopInteractable();
            IsVisible = false;
            shopStorage.LoadPrefabs();
            CheckUpgradeSlot(); // Prefabs beim Erstellen des Panels laden
        }

        protected void OnAwake()
        {
            if ( shopStorage == null )
            {
                shopStorage = new ShopStorage();
                shopStorage.IsOpened = false;
                IsVisible = false;
                shopStorage.LoadPrefabs();
                CheckUpgradeSlot(); // Prefabs beim Erwachen laden
            }
        }

        public void OnUpdate()
        {
            if ( !isInitialized )
            {
                shopStorage.IsOpened = false;
                IsVisible = false;
                isInitialized = true;
            }
            else
            {
                bool isOpened = shopStorage?.IsOpened ?? false;

                if ( isOpened != IsVisible )
                {
                    ToggleVisibility();
                }
            }
            
        }

        public void ToggleVisibility()
        {
            if ( shopStorage == null )
            {
                return;
            }

            bool newState = !shopStorage.IsOpened;
            shopStorage.IsOpened = newState;
            IsVisible = newState;
        }

        public void OpenShop()
        {
            if ( shopStorage != null && !shopStorage.IsOpened )
            {
                shopStorage.IsOpened = true;
                IsVisible = true;
            }
        }

        public void CloseShop()
        {
            if ( shopStorage != null && shopStorage.IsOpened )
            {
                shopStorage.IsOpened = false;
                IsVisible = false;
            }
        }

        public void ClosePanel()
        {
            CloseShop();
        }

        public void ResetVisibility()
        {
            IsVisible = false;
        }

        protected override int BuildHash()
        {
            return HashCode.Combine(
                IsVisible,
                Player.Local.Inventory.BackpackItems.HashCombine( i => i?.GetHashCode() ?? -1 ),
                shopStorage?.AvailableItems.HashCombine( i => i?.GetHashCode() ?? -1 ) ?? 0,
                isUpgradePanelVisible
                
            );
        }

        public void SetPanelVisibility( bool isVisible )
        {
            IsVisible = isVisible;
        }
    }
}