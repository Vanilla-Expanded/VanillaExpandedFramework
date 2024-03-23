using HarmonyLib;
using LudeonTK;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFECore;

public class ForcedMusicManager : GameComponent
{
    private static readonly AccessTools.FieldRef<MusicManagerPlay, SongDef> currentSong = AccessTools.FieldRefAccess<MusicManagerPlay, SongDef>("currentSong");
    private static readonly AccessTools.FieldRef<MusicManagerPlay, bool> songWasForced = AccessTools.FieldRefAccess<MusicManagerPlay, bool>("songWasForced");
    private static bool patchesApplied;

    private int currentPriority = -1;
    private HashSet<SongDef> forcedSongs = new();
    private Dictionary<int, ForcedSongsBox> prioritySongs = new();

    public int Priority => currentPriority;
    public IEnumerable<SongDef> Songs => forcedSongs;
    public IReadOnlyDictionary<int, ForcedSongsBox> AllSongs => prioritySongs;

    public static ForcedMusicManager Instance;

    public ForcedMusicManager(Game game) => Instance = this;

    public static void ApplyPatches()
    {
        if (patchesApplied) return;
        VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(MusicManagerPlay), "ChooseNextSong"), new HarmonyMethod(typeof(ForcedMusicManager), nameof(ChooseNextSong_Prefix)));
        VFECore.harmonyInstance.Patch(AccessTools.PropertyGetter(typeof(MusicManagerPlay), "DangerMusicMode"), new HarmonyMethod(typeof(ForcedMusicManager), nameof(DangerMusicMode_Prefix)));
        patchesApplied = true;
    }

    public static void ForceSong(SongDef def, int priority)
    {
        if (Instance.forcedSongs.Count == 0)
        {
            ForceStopMusic();
            Instance.forcedSongs.Add(def);
            Instance.currentPriority = priority;
        }
        else if (priority == Instance.currentPriority) Instance.forcedSongs.Add(def);
        else if (priority > Instance.currentPriority)
        {
            ForceStopMusic();
            Instance.prioritySongs.Add(Instance.currentPriority, new ForcedSongsBox(Instance.forcedSongs));
            Instance.forcedSongs = new HashSet<SongDef> { def };
            Instance.currentPriority = priority;
        }
        else if (Instance.prioritySongs.TryGetValue(priority, out var songs))
        {
            songs.forcedSongs.Add(def);
            Instance.prioritySongs[priority] = songs;
        }
        else Instance.prioritySongs.Add(priority, new ForcedSongsBox(new HashSet<SongDef> { def }));
    }

    public static void EndSong(SongDef def)
    {
        if (Instance.forcedSongs.Remove(def))
        {
            if (currentSong(Find.MusicManagerPlay) == def) ForceStopMusic();
            if (Instance.forcedSongs.Count == 0)
                if (Instance.prioritySongs.Any())
                {
                    var (priority, songs)    = Instance.prioritySongs.MaxBy(kv => kv.Key);
                    Instance.currentPriority = priority;
                    Instance.forcedSongs     = songs.forcedSongs;
                    Instance.prioritySongs.Remove(priority);
                }
                else Instance.currentPriority = -1;
        }

        foreach (var (_, (songs, _)) in Instance.prioritySongs) songs.Remove(def);

        Instance.prioritySongs.RemoveAll(kv => kv.Value.forcedSongs.Count == 0);
    }

    private static void ForceStopMusic()
    {
        songWasForced(Find.MusicManagerPlay) = false;
        Find.MusicManagerPlay.ForceFadeoutAndSilenceFor(1f, 1f);
    }

    public static bool ChooseNextSong_Prefix(ref SongDef __result, MusicManagerPlay __instance)
    {
        if (Instance.forcedSongs.TryRandomElement(out var song))
        {
            __result = song;
            songWasForced(__instance) = true;
            return false;
        }

        return true;
    }

    public static bool DangerMusicMode_Prefix(ref bool __result)
    {
        if (Instance.forcedSongs.Count > 0)
        {
            __result = true;
            return false;
        }

        return true;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref currentPriority, nameof(currentPriority));
        Scribe_Collections.Look(ref forcedSongs, nameof(forcedSongs), LookMode.Def);
        Scribe_Collections.Look(ref prioritySongs, nameof(prioritySongs), LookMode.Value, LookMode.Deep);
    }

    public class ForcedSongsBox : IExposable
    {
        public HashSet<SongDef> forcedSongs;

        public ForcedSongsBox() { }
        public ForcedSongsBox(HashSet<SongDef> forcedSongs) => this.forcedSongs = forcedSongs;

        public void ExposeData()
        {
            Scribe_Collections.Look(ref forcedSongs, nameof(forcedSongs), LookMode.Def);
        }

        public void Deconstruct(out HashSet<SongDef> item, out int length)
        {
            item = forcedSongs;
            length = forcedSongs.Count;
        }
    }

    [DebugAction("Music", "End forced music")]
    public static void StopAll()
    {
        Instance.currentPriority = -1;
        Instance.forcedSongs.Clear();
        Instance.prioritySongs.Clear();
        ForceStopMusic();
    }
}

public class QuestNode_ForceMusic : QuestNode
{
    public QuestNode_ForceMusic()
    {
        ForcedMusicManager.ApplyPatches();
    }

    public SlateRef<string> inSignalEnable;
    public SlateRef<string> inSignalDisable;
    public SlateRef<List<SongDef>> possibleSongs;
    public SlateRef<int> priority;
    protected override void RunInt()
    {
        var slate = QuestGen.slate;
        QuestGen.quest.AddPart(new QuestPart_ForcedMusic
        {
            inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? slate.Get<string>("inSignal"),
            inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate)),
            possibleSongs = possibleSongs.GetValue(slate),
            priority = priority.GetValue(slate),
            signalListenMode = QuestPart.SignalListenMode.Always
        });
    }

    protected override bool TestRunInt(Slate slate) => true;
}

public class QuestPart_ForcedMusic : QuestPart
{
    public string inSignalEnable;
    public string inSignalDisable;
    public List<SongDef> possibleSongs;
    public int priority;
    
    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag == inSignalEnable)
            foreach (var def in possibleSongs)
                ForcedMusicManager.ForceSong(def, priority);

        if (signal.tag == inSignalDisable)
            foreach (var def in possibleSongs)
                ForcedMusicManager.EndSong(def);
    }

    public override void Cleanup()
    {
        base.Cleanup();
        foreach (var def in possibleSongs)
            ForcedMusicManager.EndSong(def);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignalEnable, nameof(inSignalEnable));
        Scribe_Values.Look(ref inSignalDisable, nameof(inSignalDisable));
        Scribe_Values.Look(ref priority, nameof(priority));
        Scribe_Collections.Look(ref possibleSongs, nameof(possibleSongs), LookMode.Def);
    }
}