using System;
using System.IO;
using Game.Save;
using MiMieSaver;

string outputRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "CreatSaveTest"));

if (Directory.Exists(outputRoot))
    Directory.Delete(outputRoot, true);

Directory.CreateDirectory(outputRoot);

var archiveMgr = new ArchiveMgr(outputRoot);
archiveMgr.RegisterModule<DemoPlayerModule>(new DemoPlayerModule("喵咪勇者"));
archiveMgr.RegisterModule<DemoEquipmentModule>(new DemoEquipmentModule("新手剑"));

Console.WriteLine($"存档根目录: {outputRoot}");
Console.WriteLine();

var slot1 = archiveMgr.CreatSlot("主存档");
Console.WriteLine($"创建槽位 1: {slot1.DisplayName}  Id={slot1.SlotId}");
archiveMgr.Save();
Console.WriteLine($"  -> {slot1.SlotId}_SaveData.dat");

var slot2 = archiveMgr.CreatSlot("二周目");
Console.WriteLine($"创建槽位 2: {slot2.DisplayName}  Id={slot2.SlotId}");
archiveMgr.Save();
Console.WriteLine($"  -> {slot2.SlotId}_SaveData.dat");

archiveMgr.SwitchSlot(slot1.SlotId);
Console.WriteLine();
Console.WriteLine($"切回槽位 1 并二次保存");
archiveMgr.Save();

Console.WriteLine();
Console.WriteLine("生成文件:");
foreach (string file in Directory.GetFiles(outputRoot))
    Console.WriteLine($"  {Path.GetFileName(file)}  ({new FileInfo(file).Length} bytes)");

internal class DemoPlayerModule : IArchiveModule
{
    private readonly string playerName;

    public DemoPlayerModule(string playerName)
    {
        this.playerName = playerName;
    }

    public string ModuleName => nameof(DemoPlayerModule);

    public void CreateArchive(SaveData saveData)
    {
        saveData.Meta ??= new MetaSave
        {
            Version = 1,
            CreatTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        saveData.Player = new PlayerModuleSave
        {
            PlayerId = Guid.NewGuid().ToString(),
            PlayerName = playerName,
            CreateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public void FromArchive(SaveData saveData) { }

    public void ToArchive(SaveData saveData)
    {
        if (saveData.Player == null)
            CreateArchive(saveData);
        else
            saveData.Player.PlayerName = playerName;
    }
}

internal class DemoEquipmentModule : IArchiveModule
{
    private readonly string itemName;

    public DemoEquipmentModule(string itemName)
    {
        this.itemName = itemName;
    }

    public string ModuleName => nameof(DemoEquipmentModule);

    public void CreateArchive(SaveData saveData)
    {
        saveData.Equpment = new EquipmentModuleSave();
        saveData.Equpment.Items.Add(new EquipmentItem
        {
            InstanceId = Guid.NewGuid().ToString(),
            Level = 1,
            SlotType = 1
        });
    }

    public void FromArchive(SaveData saveData) { }

    public void ToArchive(SaveData saveData)
    {
        if (saveData.Equpment == null)
            CreateArchive(saveData);
    }
}
