using System;
using System.IO;
using Game.Save;

namespace MiMieSaver.Tests;

public class ArchiveTests : IDisposable
{
    private readonly string rootPath;
    private readonly ArchiveMgr archiveMgr;

    public ArchiveTests()
    {
        rootPath = Path.Combine(Path.GetTempPath(), "MiMieSaverTests", Guid.NewGuid().ToString());
        archiveMgr = new ArchiveMgr(rootPath);
        archiveMgr.RegisterModule<TestPlayerModule>(new TestPlayerModule());
    }

    public void Dispose()
    {
        if (Directory.Exists(rootPath))
            Directory.Delete(rootPath, true);
    }

    [Fact]
    public void CreatSlot_ShouldIncreaseCount()
    {
        var slot = archiveMgr.CreatSlot("测试槽位");
        Assert.NotNull(slot);
        Assert.Equal(1, archiveMgr.GetAllSlotIndex().Count);
    }

    [Fact]
    public void SaveAndLoad_ShouldPersistPlayerData()
    {
        archiveMgr.CreatSlot("主存档");
        archiveMgr.Save();
        archiveMgr.Load();

        Assert.True(archiveMgr.TryGetModule(out TestPlayerModule player));
        Assert.Equal("测试玩家", player.PlayerName);
    }

    [Fact]
    public void HasSaveData_ShouldReturnTrueAfterSave()
    {
        archiveMgr.CreatSlot("存档1");
        archiveMgr.Save();
        Assert.True(archiveMgr.HasSaveData());
    }
}

internal class TestPlayerModule : IArchiveModule
{
    public string PlayerName { get; private set; }

    public string ModuleName => nameof(TestPlayerModule);

    public void CreateArchive(SaveData saveData)
    {
        saveData.Player = new PlayerModuleSave
        {
            PlayerId = Guid.NewGuid().ToString(),
            PlayerName = "测试玩家",
            CreateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public void FromArchive(SaveData saveData)
    {
        PlayerName = saveData.Player?.PlayerName;
    }

    public void ToArchive(SaveData saveData)
    {
        if (saveData.Player == null)
            CreateArchive(saveData);
    }
}
