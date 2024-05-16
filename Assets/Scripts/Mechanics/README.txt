
BASIC GUIDE TO THE INVENTORY <-> IMPLEMENTATION LOOP

#1) GARAGECSVs

    Stored on PlayerDataHandler, which exists on all scenes.
    Debug console pulls from these csvs to create its own copy.


#2) PlayerDataHandler.equippedInfos

    Stores all the equipped items, using CSVType and CSVId as a 2 key dictionary which stores the level of that item.
    Debug console pulls from this to show you all equipped items and their stats.

#3) NetworkedEntity.playerInstance.Spawned()

    On game start, networkedEntity initializes all the items in PlayerDataHandler.equippedInfos

    First it gets parsed via CSVParser.cs
    
    These get pushed to the following in this order:

    InventoryInit.UpdateInventory()
        -> ActiveInit.InitActive()
            -> NetworkedEntity.playerInstance.Add(IAbility)
            -> UpgradeInit.InitUpgrade()
                -> UpgradeCatalog.playerUpgrades.Add(UpgradeNode)
                    -> if Upgrade == unlocked
                        -> NetworkedEntity.playerInstance.AddUpgradeModi(UpgradeNode)
                            -> UpgradeHandler.PushToUpgradeHandler()
                                -> UpgradeHandler.PushUpgrade()
                                    -> Apply Upgrade stats/etc. to NetworkedEntity
                                    -> NetworkedEntity.playerInstance.upgradeNodes.Add(UpgradeNode)
        -> GadgetInit.InitActive()
            -> NetworkedEntity.playerInstance.Add(IAbility)
            -> UpgradeInit.InitUpgrade()
                -> UpgradeCatalog.AddUpgrade()
                    -> NetworkedEntity.playerInstance.Add(UpgradeNode)

> ActiveInit, IIAbility, CSVEnums, DescriptionInit, AbilitiesStatHandler, PlayerDataHandler, UpgradeHandler <