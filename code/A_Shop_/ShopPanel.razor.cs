using Sandbox;
using Sandbox.ui.Hud;
using System;

namespace GeneralGame.HUD
{
    [StyleSheet]
    public partial class ShopPanel : Panel
    {
        public int currentPage = 0;
        public int itemsPerPage = 20; // Anzahl der Items pro Seite
        public int totalPages => (int)Math.Ceiling( (double)Player.Local.Inventory.StorageItems.Count / itemsPerPage );
        public ItemComponent upgradeItem;
        public ItemComponent aspectItem;
        private bool isUpgradePanelVisible = false;
        private int upgradeCost = 1500;
        private Action ConfirmUpgradeAction { get; set; }
        private bool ShowUpgradeConfirmationDialog { get; set; }
        private bool ShowUpgradeResultDialog { get; set; }
        private string UpgradeResultMessage { get; set; }
        private string UpgradeResultClass { get; set; }
        private string statusText = "Success Chance:";
        private string statusClass = "visible";
        private bool upgradeSuccessful;
        private bool upgradeDestroyed;
        
       
        private void ShowUpgradeConfirmation( Action confirmAction )
        {
            var requiredMaterials = GetRequiredMaterials( upgradeItem.ItemLevel, upgradeItem.Tier );

            if ( !HasEnoughMaterials( requiredMaterials ) )
            {
                UpgradeResultMessage = "Not enough materials!";
                ShowUpgradeResultDialog = true;
                StateHasChanged();
                return;
            }

            ConfirmUpgradeAction = confirmAction;
            ShowUpgradeConfirmationDialog = true;
            StateHasChanged();
        }
        
        private bool HasEnoughMaterials( Dictionary<string, int> requiredMaterials )
        {
            foreach ( var material in requiredMaterials )
            {
                if ( !Player.Local.Inventory.BackpackHasMaterial( material.Key, material.Value ) )
                {
                    return false;
                }
            }
            return true;
        }
       private void ConfirmUpgrade()
    {
        ConfirmUpgradeAction?.Invoke();
        ShowUpgradeConfirmationDialog = false;
        ShowUpgradeResultDialog = true;

        // Hier die tatsächliche Logik für den Erfolg, Misserfolg oder Zerstörung des Upgrades hinzufügen
        // Beispielhafte Logik, ersetzen Sie dies durch Ihre tatsächliche Logik
        StateHasChanged();
        if (upgradeSuccessful)
        {
            UpgradeResultMessage = "Upgrade Successful!";
            UpgradeResultClass = "success";
        }
        else if (upgradeDestroyed)
        {
            UpgradeResultMessage = "Upgrade Destroyed!";
            UpgradeResultClass = "destroyed";
        }
        else
        {
            UpgradeResultMessage = "Upgrade Failed!";
            UpgradeResultClass = "failed";
        }

        StateHasChanged();
    }

        private void CancelUpgrade()
        {
            ShowUpgradeConfirmationDialog = false;
            StateHasChanged();
        }
        private void CloseUpgradeResult()
        {
            ShowUpgradeResultDialog = false;
            StateHasChanged();
        }

        private SortDirection currentSortDirection = SortDirection.Ascending;
        private bool showStorageSortOptions = false;
        private bool showInventorySortOptions = false;
        private void ToggleStorageSortOptions()
        {
            showStorageSortOptions = !showStorageSortOptions;
            
        }

        private void ToggleInventorySortOptions()
        {
            showInventorySortOptions = !showInventorySortOptions;
        }
        private void SortBackPackItems( SortOption sortOption )
        {
            // Umschalten der Sortierrichtung
            currentSortDirection = currentSortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;

            Player.Local.Inventory?.SortBackpackItems( sortOption );
        }

        private void SortStorageItems( SortOption sortOption )
        {
            // Umschalten der Sortierrichtung
            currentSortDirection = currentSortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;

            Player.Local.Inventory?.SortStorageItems( sortOption );
        }

        
        protected async Task OnAfterRenderAsync( bool firstRender )
        {
            if ( firstRender && upgradeItem != null && upgradeItem.SuccessChance == 0 )
            {
                await ChangeStatusText();
            }
           

        }

        private async Task ChangeStatusText()
        {
            statusClass = "fade";
            StateHasChanged();
            await Task.Delay( 1000 );

            statusText = "Failed";
            statusClass = "visible";
            StateHasChanged();
            await Task.Delay( 3000 );

            statusClass = "fade";
            StateHasChanged();
            await Task.Delay( 1000 );

            statusText = "Success Chance:";
            statusClass = "visible";
            StateHasChanged();
        }



        public void CheckUpgradeSlot()
        {
            if ( shopInteractable == null )
            {
                Log.Warning( "shopInteractable ist null." );
                return;
            }
           

            

            // Überprüfen, ob es gültige Upgrade-Items im Inventar des Spielers gibt
            isUpgradePanelVisible = Player.Local.Inventory.UpgradeItems.Any( item => item != null && item.ItemLevel < 27 );
            if ( isUpgradePanelVisible )
            {
                upgradeItem = Player.Local.Inventory.UpgradeItems.FirstOrDefault( item => item != null && item.ItemLevel < 27 );
                if ( upgradeItem != null )
                {
                    // Hier können Sie die Upgrade-Kosten basierend auf dem Item-Level berechnen
                    upgradeCost = CalculateUpgradeCost( upgradeItem.ItemLevel, upgradeItem.Tier );
                    SetSuccessChance();
                }
                else
                {
                    Log.Warning( "Kein gültiges Upgrade-Item gefunden." );
                }
            }
            else
            {
                upgradeItem = null;
            }
        }
        private bool isAspectPanelVisible;
     
     
   
      
        private int aspectCost;
        private float aspectSuccessChance;
        public void CheckAspectSlot()
        {
            if ( shopInteractable == null )
            {
                return;
            }

            var aspectItems = Player.Local.Inventory.AspectItems;
            var upgradeItems = Player.Local.Inventory.UpgradeItems;

            if ( aspectItems == null || upgradeItems == null )
            {
                Log.Warning( "AspectItems or UpgradeItems is null." );
                return;
            }

            isAspectPanelVisible = aspectItems.Any( item => item != null );
            if ( isAspectPanelVisible )
            {
                aspectItem = aspectItems.FirstOrDefault( item => item != null );
                var upgradeItem = upgradeItems.FirstOrDefault( item => item != null ); // Sicherstellen, dass upgradeItem nicht null ist
                if ( aspectItem != null && upgradeItem != null )
                {
                    isAspectPanelVisible = true;
                    aspectCost = CalculateAspectCost( aspectItem.Tier ); // Explizite Konvertierung
                    aspectSuccessChance = CalculateAspectSuccessChance( (int)aspectItem.Tier ); // Explizite Konvertierung
                }
                else
                {
                    Log.Warning( "AspectItem or UpgradeItem is null." );
                }
            }
            else
            {
                aspectItem = null;
            }
        }
        private void ShowAspectConfirmation( ItemComponent aspectItem )
        {
           
            PerformUpgrade();
        }
        private int CalculateAspectCost( GeneralGame.Tier tier )
        {
            // Basis-Kosten
            int baseCost = 5000;

            // Kosten pro Tier-Level
            int costPerTier = 1000;

            // Berechne die Gesamtkosten
            int totalCost = baseCost + ((int)tier * costPerTier);

            return totalCost;
        }
        private float CalculateAspectSuccessChance( int tier )
        {
            // Berechnen Sie die Erfolgsrate basierend auf dem Tier des Aspekt-Items
            return 1.0f - (tier * 0.1f); // Beispielberechnung
        }
        private void PerformUpgrade()
        {
            Log.Info( "PerformUpgrade called" );

            if ( aspectItem == null || upgradeItem == null )
            {
                return;
            }

            

            if ( aspectItem != null )
            {
                // Logik zum Ausführen des Upgrades
                ApplyAspectToUpgradeItem( upgradeItem, aspectItem ); // Übertrage die Aspekt-Informationen auf das Upgrade-Item

                // Berechne die Kosten für das Upgrade
                int tier = (int)upgradeItem.Tier; // Angenommen, das Upgrade-Item hat eine Tier-Eigenschaft
                int cost = CalculateAspectCost( (GeneralGame.Tier)tier );

                // Überprüfen, ob der Spieler genug Guthaben hat
                if ( Player.Local.Vyndalium >= cost )
                {
                    // Ziehe die Kosten vom Guthaben des Spielers ab
                    Player.Local.Vyndalium -= cost;

                    // Setze das Aspekt-Item auf null (zerstören)
                    Player.Local.Inventory?.RemoveItem( aspectItem );
                    CheckAspectSlot();
                    StateHasChanged();

                    // Aktualisiere die Anzeige
                    Inventory.Instance?.OnChanged();
                    Hudmaster.Instance.ShowNotification( "Upgrade erfolgreich durchgeführt.", "/ui/hud/success.gif" );
                }
                else
                {
                    Hudmaster.Instance.ShowNotification( "Nicht genügend Guthaben für das Upgrade.", "/ui/hud/error.gif" );
                }
                StateHasChanged();
                CheckAspectSlot();
            }
        }
        private void ApplyAspectToUpgradeItem( ItemComponent upgradeItem, ItemComponent aspectItem )
        {
            upgradeItem.ApplyAspect( aspectItem ); // Übertrage die Aspekt-Informationen auf das Upgrade-Item
        }

        private int CalculateUpgradeCost( int itemLevel, GeneralGame.Tier tier )
        {
            // Beispielhafte Berechnung der Upgrade-Kosten basierend auf dem Item-Level und Tier
            int baseCost = itemLevel switch
            {
                1 => 200,
                2 => 500,
                3 => 2000,
                24 => 5000,
                25 => 15000,
                26 => 20000,
                27 => 25000,
                _ => (int)(500 * Math.Pow( 1.3, itemLevel - 1 )), // Exponentielle Berechnung für andere Level
            };

            // Zusätzliche Kosten basierend auf dem Tier
            double tierMultiplier = Math.Pow( 1.6, (double)tier - 1 );
            return (int)(baseCost * tierMultiplier);
        }
        private void SetSuccessChance()
        {
            if ( upgradeItem != null )
            {
                upgradeItem.SuccessChance = CalculateSuccessChance( upgradeItem.ItemLevel );
            }
        }
        public void PlaySuccessSoundFromPath( string soundEventPath, float volume )
        {
            // SoundEvent anhand des Pfads laden
            var soundEvent = ResourceLibrary.Get<SoundEvent>( soundEventPath );
            if ( soundEvent != null )
            {
                // SoundEvent abspielen
                var soundHandle = Sound.Play( soundEvent );
                if ( soundHandle.IsValid() )
                {
                    // Lautstärke und Position einstellen
                    soundHandle.Volume = volume; // Lautstärke einstellen
                    soundHandle.Position = Vector3.Zero; // Position auf (0,0,0) setzen
                    soundHandle.ListenLocal = true; // Sound lokal abspielen
                }
                else
                {
                    
                }
            }
            else
            {
                // Fehlerbehandlung, falls das SoundEvent nicht gefunden wird
                Log.Warning( $"SoundEvent '{soundEventPath}' konnte nicht gefunden werden." );
            }
        }
        

        private void UpgradeItem()
        {
            if ( upgradeItem != null && Player.Local.Vyndalium >= upgradeCost && upgradeItem.ItemLevel < 27 )
            {
                if ( upgradeItem.IsPotion || upgradeItem.IsMaterial )
                {
                    Hudmaster.Instance.ShowNotification( "you cannot upgrade that.", "/ui/hud/exit.gif" );
                    PlaySuccessSoundFromPath( "sounds/upgrade/notenoughmoney.sound", 0.0125f );
                    return;
                }
                // Ab Level 4 werden Materialien benötigt
                if ( upgradeItem.ItemLevel >= 14 )
                {
                    var requiredMaterials = GetRequiredMaterials( upgradeItem.ItemLevel, upgradeItem.Tier );
                    if ( !HasRequiredMaterials( requiredMaterials ) )
                    {
                        Hudmaster.Instance.ShowNotification( "Not enough materials", "/ui/hud/exit.gif" );
                        PlaySuccessSoundFromPath( "sounds/upgrade/notenoughmoney.sound", 0.0125f );
                        return;
                    }
                    RemoveRequiredMaterials( requiredMaterials );
                }

                // Berechne die Erfolgschance
                double successChance = CalculateSuccessChance( upgradeItem.ItemLevel );
                
                Random random = new Random();
                if ( random.NextDouble() <= successChance )
                {
                    

                    Player.Local.Vyndalium -= upgradeCost;
                    upgradeItem.ItemLevel++;
                    Hudmaster.Instance.ShowNotification( $"Item {upgradeItem.Name} has been upgraded to {upgradeItem.ItemLevel}!", "/ui/hud/success.gif" );
                    
                    upgradeItem.SellPrice += (int)(upgradeCost * 0.5);
                    if ( upgradeItem.MinAttackValue > 0 )
                    {
                        upgradeItem.MinAttackValue += (int)2.6;
                    }

                    if ( upgradeItem.MaxAttackValue > 0 )
                    {
                        upgradeItem.MaxAttackValue += (int)2.6;
                    }

                    if ( upgradeItem.Health > 0 )
                        upgradeItem.Health += (int)(upgradeItem.Health * 0.3);

                    if ( upgradeItem.Armor > 0 )
                        upgradeItem.Armor += (int)(upgradeItem.Armor * 0.3);

                    if ( upgradeItem.STG > 0 )
                        upgradeItem.STG += (int)(upgradeItem.STG * 0.3);

                    if ( upgradeItem.HE > 0 )
                        upgradeItem.HE += (int)(upgradeItem.HE * 0.3);

                    if ( upgradeItem.DEX > 0 )
                        upgradeItem.DEX += (int)(upgradeItem.DEX * 0.3);

                    if ( upgradeItem.PER > 0 )
                        upgradeItem.PER += (int)(upgradeItem.PER * 0.3);

                    if ( upgradeItem.INT > 0 )
                        upgradeItem.INT += (int)(upgradeItem.INT * 0.3);

                    if ( upgradeItem.Mana > 0 )
                        upgradeItem.Mana += (int)(upgradeItem.Mana * 0.3);

                    if ( upgradeItem.CritHitDamage > 0 )
                        upgradeItem.CritHitDamage += (int)(upgradeItem.CritHitDamage * 0.3);

                    if ( upgradeItem.CritHitChance > 0 )
                        upgradeItem.CritHitChance += (int)(upgradeItem.CritHitChance * 0.3);

                    if ( upgradeItem.AbilityHaste > 0 )
                        upgradeItem.AbilityHaste += (int)(upgradeItem.AbilityHaste * 0.3);

                    if ( upgradeItem.AttackPower > 0 )
                        upgradeItem.AttackPower += (int)(upgradeItem.AttackPower * 0.3);

                    if ( upgradeItem.MagicPower > 0 )
                        upgradeItem.MagicPower += (int)(upgradeItem.MagicPower * 0.3);

                    if ( upgradeItem.AttackSpeed > 0 )
                        upgradeItem.AttackSpeed += (int)(upgradeItem.AttackSpeed * 0.3);

                    if ( upgradeItem.MoveSpeed > 0 )
                        upgradeItem.MoveSpeed += (int)(upgradeItem.MoveSpeed * 0.3);

                    if ( upgradeItem.MagicDefense > 0 )
                        upgradeItem.MagicDefense += (int)(upgradeItem.MagicDefense * 0.3);

                    if ( upgradeItem.Evasion > 0 )
                        upgradeItem.Evasion += (int)(upgradeItem.Evasion * 0.3);

                    if ( upgradeItem.Cover > 0 )
                        upgradeItem.Cover += (int)(upgradeItem.Cover * 0.3);

                    if ( upgradeItem.BonusEXP > 0 )
                        upgradeItem.BonusEXP += (int)(upgradeItem.BonusEXP * 0.3);

                    if ( upgradeItem.BonusScore > 0 )
                        upgradeItem.BonusScore += (int)(upgradeItem.BonusScore * 0.3);

                    if ( upgradeItem.BonusVyndalium > 0 )
                        upgradeItem.BonusVyndalium += (int)(upgradeItem.BonusVyndalium * 0.3);

                    if ( upgradeItem.Tenacity > 0 )
                        upgradeItem.Tenacity += (int)(upgradeItem.Tenacity * 0.3);

                    if ( upgradeItem.StunResistance > 0 )
                        upgradeItem.StunResistance += (int)(upgradeItem.StunResistance * 0.3);

                    if ( upgradeItem.BlindResistance > 0 )
                        upgradeItem.BlindResistance += (int)(upgradeItem.BlindResistance * 0.3);

                    if ( upgradeItem.BleedResistance > 0 )
                        upgradeItem.BleedResistance += (int)(upgradeItem.BleedResistance * 0.3);

                    if ( upgradeItem.SlowResistence > 0 )
                        upgradeItem.SlowResistence += (int)(upgradeItem.SlowResistence * 0.3);

                    if ( upgradeItem.FireResistence > 0 )
                        upgradeItem.FireResistence += (int)(upgradeItem.FireResistence * 0.3);

                    if ( upgradeItem.PoisonResistence > 0 )
                        upgradeItem.PoisonResistence += (int)(upgradeItem.PoisonResistence * 0.3);

                    if ( upgradeItem.IceResistence > 0 )
                        upgradeItem.IceResistence += (int)(upgradeItem.IceResistence * 0.3);

                    if ( upgradeItem.LightningResistence > 0 )
                        upgradeItem.LightningResistence += (int)(upgradeItem.LightningResistence * 0.3);

                    if ( upgradeItem.HolyResistence > 0 )
                        upgradeItem.HolyResistence += (int)(upgradeItem.HolyResistence * 0.3);


                    PlaySuccessSoundFromPath( "sounds/upgrade/noti.sound",0.0125f );
                    upgradeSuccessful = true;

                    CheckUpgradeSlot(); // Aktualisieren Sie den Panel-Zustand
                }
                else
                {
                    if ( upgradeItem.ItemLevel <= 10 )
                    {
                        upgradeItem.ItemLevel--;
                        if ( upgradeItem.MinAttackValue > 0 )
                        {
                            upgradeItem.MinAttackValue -= (int)2.6;
                        }

                        if ( upgradeItem.MaxAttackValue > 0 )
                        {
                            upgradeItem.MaxAttackValue -= (int)2.6;
                        }

                        if ( upgradeItem.Health > 0 )
                            upgradeItem.Health -= (int)(upgradeItem.Health * 0.3);

                        if ( upgradeItem.Armor > 0 )
                            upgradeItem.Armor -= (int)(upgradeItem.Armor * 0.3);

                        if ( upgradeItem.STG > 0 )
                            upgradeItem.STG -= (int)(upgradeItem.STG * 0.3);

                        if ( upgradeItem.HE > 0 )
                            upgradeItem.HE -= (int)(upgradeItem.HE * 0.3);

                        if ( upgradeItem.DEX > 0 )
                            upgradeItem.DEX -= (int)(upgradeItem.DEX * 0.3);

                        if ( upgradeItem.PER > 0 )
                            upgradeItem.PER -= (int)(upgradeItem.PER * 0.3);

                        if ( upgradeItem.INT > 0 )
                            upgradeItem.INT -= (int)(upgradeItem.INT * 0.3);

                        if ( upgradeItem.Mana > 0 )
                            upgradeItem.Mana -= (int)(upgradeItem.Mana * 0.3);

                        if ( upgradeItem.CritHitDamage > 0 )
                            upgradeItem.CritHitDamage -= (int)(upgradeItem.CritHitDamage * 0.3);

                        if ( upgradeItem.CritHitChance > 0 )
                            upgradeItem.CritHitChance -= (int)(upgradeItem.CritHitChance * 0.3);

                        if ( upgradeItem.AbilityHaste > 0 )
                            upgradeItem.AbilityHaste -= (int)(upgradeItem.AbilityHaste * 0.3);

                        if ( upgradeItem.AttackPower > 0 )
                            upgradeItem.AttackPower -= (int)(upgradeItem.AttackPower * 0.3);

                        if ( upgradeItem.MagicPower > 0 )
                            upgradeItem.MagicPower -= (int)(upgradeItem.MagicPower * 0.3);

                        if ( upgradeItem.AttackSpeed > 0 )
                            upgradeItem.AttackSpeed -= (int)(upgradeItem.AttackSpeed * 0.3);

                        if ( upgradeItem.MoveSpeed > 0 )
                            upgradeItem.MoveSpeed -= (int)(upgradeItem.MoveSpeed * 0.3);

                        if ( upgradeItem.MagicDefense > 0 )
                            upgradeItem.MagicDefense -= (int)(upgradeItem.MagicDefense * 0.3);

                        if ( upgradeItem.Evasion > 0 )
                            upgradeItem.Evasion -= (int)(upgradeItem.Evasion * 0.3);

                        if ( upgradeItem.Cover > 0 )
                            upgradeItem.Cover -= (int)(upgradeItem.Cover * 0.3);

                        if ( upgradeItem.BonusEXP > 0 )
                            upgradeItem.BonusEXP -= (int)(upgradeItem.BonusEXP * 0.3);

                        if ( upgradeItem.BonusScore > 0 )
                            upgradeItem.BonusScore -= (int)(upgradeItem.BonusScore * 0.3);

                        if ( upgradeItem.BonusVyndalium > 0 )
                            upgradeItem.BonusVyndalium -= (int)(upgradeItem.BonusVyndalium * 0.3);

                        if ( upgradeItem.Tenacity > 0 )
                            upgradeItem.Tenacity -= (int)(upgradeItem.Tenacity * 0.3);

                        if ( upgradeItem.StunResistance > 0 )
                            upgradeItem.StunResistance -= (int)(upgradeItem.StunResistance * 0.3);

                        if ( upgradeItem.BlindResistance > 0 )
                            upgradeItem.BlindResistance -= (int)(upgradeItem.BlindResistance * 0.3);

                        if ( upgradeItem.BleedResistance > 0 )
                            upgradeItem.BleedResistance -= (int)(upgradeItem.BleedResistance * 0.3);

                        if ( upgradeItem.SlowResistence > 0 )
                            upgradeItem.SlowResistence -= (int)(upgradeItem.SlowResistence * 0.3);

                        if ( upgradeItem.FireResistence > 0 )
                            upgradeItem.FireResistence -= (int)(upgradeItem.FireResistence * 0.3);

                        if ( upgradeItem.PoisonResistence > 0 )
                            upgradeItem.PoisonResistence -= (int)(upgradeItem.PoisonResistence * 0.3);

                        if ( upgradeItem.IceResistence > 0 )
                            upgradeItem.IceResistence -= (int)(upgradeItem.IceResistence * 0.3);

                        if ( upgradeItem.LightningResistence > 0 )
                            upgradeItem.LightningResistence -= (int)(upgradeItem.LightningResistence * 0.3);

                        if ( upgradeItem.HolyResistence > 0 )
                            upgradeItem.HolyResistence -= (int)(upgradeItem.HolyResistence * 0.3);
                        upgradeItem.SellPrice -= (int)(upgradeCost * 0.5);
                        Hudmaster.Instance.ShowNotification( $"Upgrade failed. Item {upgradeItem.Name} has been downgraded to {upgradeItem.ItemLevel}.", "/ui/hud/exit.gif" );
                        PlaySuccessSoundFromPath("sounds/upgrade/error.sound", 0.0125f);
                    }
                    else
                    {
                        
                        Player.Local.Inventory?.RemoveItem( upgradeItem );
                        Hudmaster.Instance.ShowNotification( $"Upgrade failed. Item {upgradeItem.Name} has been destroyed.", "/ui/hud/exit.gif" );
                        PlaySuccessSoundFromPath( "sounds/upgrade/failing.sound", 0.0125f );
                        
                        StateHasChanged();
                        CheckUpgradeSlot();
                        //upgradeItem = null;
                        upgradeDestroyed = true;
                    }
                    upgradeSuccessful = false;
                }

                StateHasChanged();
                CheckUpgradeSlot();
                SetSuccessChance();
            }
            else
            {
                
                Hudmaster.Instance.ShowNotification( "Not enough Money", "/ui/hud/exit.gif" );
                PlaySuccessSoundFromPath( "sounds/upgrade/notenoughmoney.sound", 0.025f );
            }
        }
        
       

        private string GetBackgroundColor( double? successChance )
        {
            if ( successChance.HasValue )
            {
                return successChance.Value > 0.5 ? "green" : "red";
            }
            return "gray";
        }

        private Dictionary<string, int> GetRequiredMaterials( int itemLevel, GeneralGame.Tier tier )
        {
            // Beispielhafte Berechnung der benötigten Materialien basierend auf dem Item-Level und Tier
            var materials = new Dictionary<string, int>();
            if ( itemLevel >= 14 )
            {
                materials["Wood"] = tier switch
                {
                    GeneralGame.Tier.C => 5,
                    GeneralGame.Tier.B => 10,
                    GeneralGame.Tier.A => 15,
                    GeneralGame.Tier.S => 20,
                    GeneralGame.Tier.SS => 25,
                    GeneralGame.Tier.SSS => 30,
                    GeneralGame.Tier.Ultimate => 35,

                    _ => 0
                };
            }
            return materials;
        }

        private bool HasRequiredMaterials( Dictionary<string, int> requiredMaterials )
        {
            foreach ( var material in requiredMaterials )
            {
                if ( !Player.Local.Inventory.BackpackHasMaterial( material.Key, material.Value ) )
                {
                    return false;
                }
            }
            return true;
        }
        private double CalculateSuccessChance( int itemLevel )
        {
            // Beispielhafte Berechnung der Erfolgschance basierend auf dem Item-Level
            return itemLevel switch
            {
                0 => 1.0,
                1 => 0.95,
                2 => 0.90,
                3 => 0.85,
                4 => 0.80,
                5 => 0.75,
                6 => 0.70,
                7 => 0.65,
                8 => 0.60,
                9 => 0.55,
                10 => 0.50,
                11 => 0.45,
                12 => 0.40,
                13 => 0.35,
                14 => 0.30,
                15 => 0.25,
                16 => 0.20,
                17 => 0.15,
                18 => 0.10,
                19 => 0.05,
                20 => 0.03,
                21 => 0.02,
                22 => 0.01,
                23 => 0.005,
                24 => 0.003,
                25 => 0.002,
                26 => 0.001,
                27 => 0.0005,
                _ => 0.0, // Standardwert für nicht unterstützte Level
            };
        }

        private (string totalMaterialsText, string missingMaterialsText) GetRequiredMaterialsText()
        {
            if ( upgradeItem == null || upgradeItem.ItemLevel < 14 )
            {
                return ("No materials required.", string.Empty);
            }

            var requiredMaterials = GetRequiredMaterials( upgradeItem.ItemLevel, upgradeItem.Tier );
            var missingMaterials = new Dictionary<string, int>();
            var totalMaterialsText = "Total materials:\n";

            foreach ( var material in requiredMaterials )
            {
                int currentAmount = Player.Local.Inventory.GetBackpackMaterialCount( material.Key );
                int neededAmount = material.Value - currentAmount;
                totalMaterialsText += $"{material.Key}: {currentAmount}/{material.Value}\n";
                if ( neededAmount > 0 )
                {
                    missingMaterials[material.Key] = neededAmount;
                }
            }

            if ( missingMaterials.Count == 0 )
            {
                return (totalMaterialsText + "All required materials are available.", string.Empty);
            }

            var missingMaterialsText = "Missing materials:\n";
            foreach ( var material in missingMaterials )
            {
                missingMaterialsText += $"{material.Key}: {material.Value}\n";
            }

            return (totalMaterialsText, missingMaterialsText);
        }

        private void RemoveRequiredMaterials( Dictionary<string, int> requiredMaterials )
        {
            foreach ( var material in requiredMaterials )
            {
                Player.Local.Inventory.RemoveBackpackMaterial( material.Key, material.Value );
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
            // Prefabs beim Erstellen des Panels laden
        }

        protected void OnAwake()
        {
            if ( shopStorage == null )
            {
                
                shopStorage.IsOpened = false;
                IsVisible = false;
               
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
        private void SortBackpackItems( SortOption sortOption )
        {
            // Umschalten der Sortierrichtung
            currentSortDirection = currentSortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;

            Player.Local.Inventory?.SortBackpackItems( sortOption );
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
            int hash = HashCode.Combine(
                IsVisible,
                Player.Local.Inventory.BackpackItems.HashCombine( i => i?.GetHashCode() ?? -1 ),
                shopStorage?.AvailableItems.HashCombine( i => i?.GetHashCode() ?? -1 ) ?? 0,
                isUpgradePanelVisible,
                upgradeItem?.GetHashCode() ?? 0,
                ShowUpgradeConfirmationDialog,
             
                Player.Local.Vyndalium,
                statusText?.GetHashCode() ?? 0
                
                
            );


            UpgradeResultMessage?.GetHashCode();
            hash  = HashCode.Combine( hash, statusClass?.GetHashCode() ?? 0 );
           

            return hash;
        }

        public void SetPanelVisibility( bool isVisible )
        {
            IsVisible = isVisible;
        }
    }
}
