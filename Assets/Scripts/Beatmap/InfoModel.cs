using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class Info
{
    [JsonProperty("_version")] public string Version { get; set; } = "2.1.0";

    [JsonProperty("_songName")] public string SongName { get; set; } = "";

    [JsonProperty("_songSubName")] public string SongSubName { get; set; } = "";

    [JsonProperty("_songAuthorName")] public string SongAuthorName { get; set; } = "";

    [JsonProperty("_levelAuthorName")] public string LevelAuthorName { get; set; } = "";

    [JsonProperty("_beatsPerMinute")] public float BeatsPerMinute { get; set; } = 100;

    [JsonProperty("_shuffle")] public float Shuffle { get; set; }

    [JsonProperty("_shufflePeriod")] public float ShufflePeriod { get; set; }

    [JsonProperty("_previewStartTime")] public float PreviewStartTime { get; set; }

    [JsonProperty("_previewDuration")] public float PreviewDuration { get; set; } = 10;

    [JsonProperty("_songFilename")] public string SongFilename { get; set; } = "";

    [JsonProperty("_coverImageFilename")] public string CoverImageFilename { get; set; }

    [JsonProperty("_environmentName")] public string EnvironmentName { get; set; } = "DefaultEnvironment";

    [JsonProperty("_songTimeOffset")] public float SongTimeOffset { get; set; }

    [JsonProperty("_customData")] public InfoCustomData CustomData { get; set; } = new();

    [JsonProperty("_difficultyBeatmapSets")]
    public List<DifficultyBeatmapSet> DifficultyBeatmapSets { get; set; } = new();
}

[Serializable]
public class InfoCustomData
{
    [JsonProperty("_editors")] public Editors Editors { get; set; } = new Editors();
}

[Serializable]
public class Editors
{
    [JsonProperty("_lastEditedBy")] public string LastEditedBy { get; set; } = "Micro Mapper";
}

[Serializable]
public class DifficultyBeatmapSet
{
    [JsonProperty("_beatmapCharacteristicName")]
    public string BeatmapCharacteristicName { get; set; } = "Standard";

    [JsonProperty("_difficultyBeatmaps")] public List<DifficultyBeatmap> DifficultyBeatmaps { get; set; } = new();
}

[Serializable]
public class DifficultyBeatmap
{
    [JsonProperty("_difficulty")] public string Difficulty { get; set; } = "ExpertPlus";

    [JsonProperty("_difficultyRank")] public int DifficultyRank { get; set; } = 7;

    [JsonProperty("_beatmapFilename")] public string BeatmapFilename { get; set; } = "ExpertPlusStandard.dat";

    [JsonProperty("_noteJumpMovementSpeed")]
    public float NoteJumpMovementSpeed { get; set; } = 20;

    [JsonProperty("_noteJumpStartBeatOffset")]
    public float NoteJumpStartBeatOffset { get; set; }

    [JsonProperty("_beatmapColorSchemeIdx")]
    public int BeatmapColorSchemeIdx { get; set; }

    [JsonProperty("_environmentNameIdx")] public int EnvironmentNameIdx { get; set; }

    [JsonProperty("_customData")] public DifficultyBeatmapCustomData CustomData { get; set; } = new();
}

public class DifficultyBeatmapCustomData
{
    [JsonProperty("_requirements")] public List<string> Requirements { get; set; } = new();

    [JsonProperty("_difficultyLabel")] public string DifficultyLabel { get; set; } = "";
}