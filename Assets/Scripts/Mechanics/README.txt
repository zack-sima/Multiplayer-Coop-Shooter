
BASIC GUIDE TO THE INVENTORY <-> IMPLEMENTATION LOOP

#1) PlayerDataHandler

    Stored on PlayerDataHandler, which exists on all scenes.
    Debug console pulls from these csvs to create its own copy.
    NetworkedEntity pulls from PlayerDataHandler and runs the static methods in CSVParser, InfoInits, and InfoHandlers.

#2) PlayerDataHandler.equippedInfos

    Stores all the equipped items, using CSVType and CSVId as a 2 key dictionary which stores the level of that item.
    Debug console pulls from this to show you all equipped items and their stats.
    EquippedInfos gets pulled at start of game to determine the upgrades and abilities and stuff.
    Push string ids in to equip.
    Clear equipped to remove all entries.

#3) Core/CSVParser

    Parses the CSVs into InventoryInfo classes that store information regarding stats and upgrades.

#4) Core/InfoInits

    Initializes the above InventoryInfos into implemented items and shit on NetworkedEntity and etc.

#5) Core/InfoHandlers

    Handles systick and other stats and implementation side shit for abilities.

#6) Core/CSVEnums

    Stores all the enums that are used for the CSV parsing. Use consists of nameof(CSVId.---) or nameof(CSVMd.---), with 'Md' standing for modification.

#7) Core/AbilityInterfaces

    All interfaces for abilities are stored here, as well as some generic TryGetters.

#8) Upgrades/UpgradesCatalog

    Where the upgrades are stored, the tree created, and the ui for the in-game upgrades catalog done.