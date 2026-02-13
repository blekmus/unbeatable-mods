using Arcade.UI.SongSelect;
using HarmonyLib;
using MelonLoader;
using System.Reflection;
using UnityEngine;


[assembly: MelonInfo(typeof(CUSTOM_PLAYLIST.Core), "CUSTOM_PLAYLIST", "1.0.0", "blekmus", null)]
[assembly: MelonGame("D-CELL GAMES", "UNBEATABLE")]

namespace CUSTOM_PLAYLIST
{
    public class Core : MelonMod
    {
        private bool inArcadeMode = false;
        private PlaylistSongs playlist;
        public static Core Instance;

        public override void OnInitializeMelon()
        {
            Instance = this;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            inArcadeMode = sceneName == "ArcadeModeMenu";
            if (!inArcadeMode) return;

            this.LoadPlaylist();
        }

        // adds custom playlist category to the song selection menu
        [HarmonyPatch(typeof(ArcadeSongDatabase), "Awake")]
        private static class Patch_Awake
        {
            private static void Postfix(ArcadeSongDatabase __instance)
            {
                __instance.SelectableCategories?.Add(new("playlist", "Playlist songs", 0));
            }
        }

        // reimplemented RefreshSongList filtering and sorting when playlist category is selected
        // this was easier in dnSpy through inline code injection, not so much in melonLoader
        // i sure as hell hope this method has matured, i dont wonna edit the mod with each update
        // all in the name of Distribution and Reproducibility, as they say
        [HarmonyPatch(typeof(ArcadeSongDatabase), "RefreshSongList")]
        private static class Patch_RefreshSongList
        {
            // getting around the read-only nature of ArcadeSongDatabase's _songList private variable 
            static readonly FieldInfo songListField = AccessTools.Field(typeof(ArcadeSongDatabase), "_songList");

            private static bool Prefix(ArcadeSongDatabase __instance)
            {
                if (ArcadeSongDatabase.SelectedCategory.ToString() != "playlist")
                    return true;

                int[] playlistSongs = Core.Instance?.playlist?.songs;

                // if no songs in playlist return empty songlist
                if (playlistSongs == null || playlistSongs.Length == 0)
                {
                    songListField.SetValue(__instance, new List<ArcadeSongDatabase.BeatmapItem>());
                    __instance.OnSongListRefreshed?.Invoke();
                    return false;
                }

                // hashset for optimized lookup
                HashSet<int> playlistSet = [.. playlistSongs];
                string selectedDifficulty = ArcadeSongDatabase.SelectedDifficulty;

                // filter songs by difficulty, unlock status and if added to playlist
                IEnumerable<ArcadeSongDatabase.BeatmapItem> filteredSongs = __instance.SongDatabase.Values
                    .Where(s => s.BeatmapInfo.difficulty == selectedDifficulty
                             && s.Unlocked
                             && playlistSet.Contains(s.internalIndex));

                // sort songs
                IEnumerable<ArcadeSongDatabase.BeatmapItem> sortedSongs = ArcadeSongDatabase.SelectedSort switch
                {
                    ArcadeSongDatabase.SortingMode.Name => filteredSongs.OrderBy(s => s.Beatmap.metadata.titleUnicode),
                    ArcadeSongDatabase.SortingMode.Level => filteredSongs.OrderBy(s => s.Beatmap.metadata.tagData.Level),
                    ArcadeSongDatabase.SortingMode.Grade => filteredSongs.OrderBy(s =>
                    {
                        string leaderboard = HighScoreList.GetModifiersLeaderboard(StorableBeatmapOptions.GetModifierMask());
                        return s.Highscore.TryGetValue(leaderboard, out var highScoreItem) ? highScoreItem.score : 0;
                    }),
                    _ => filteredSongs.OrderBy(s => s.internalIndex)
                };

                songListField.SetValue(__instance, sortedSongs.ToList());

                // refresh frontend songlist
                __instance.OnSongListRefreshed?.Invoke();

                // disable execusion of existing RefreshSongList method
                return false;
            }


        }

        // resolve and cache save file path for playlist
        private string cachedPlaylistPath;
        private string PlaylistPath
        {
            get
            {
                if (cachedPlaylistPath != null) return cachedPlaylistPath;

                string profilePath = FileStorageController.GetProfilesPath();
                ProfileFolderList profileFolders = JsonUtility.FromJson<ProfileFolderList>(File.ReadAllText(Path.Combine(profilePath, "profile-order.json")));
                string guid = Guid.ParseExact(profileFolders.folders[0], "D").ToString("D").ToUpper();
                cachedPlaylistPath = Path.Combine(profilePath, guid, "playlist-songs.json");
                return cachedPlaylistPath;
            }
        }

        private void LoadPlaylist()
        {
            try
            {
                string text = File.ReadAllText(this.PlaylistPath);
                this.playlist = JsonUtility.FromJson<PlaylistSongs>(text);
            }
            catch (Exception)
            {
                this.playlist = new PlaylistSongs();
                this.SavePlaylist();
            }
        }

        private void SavePlaylist()
        {
            File.WriteAllText(this.PlaylistPath, JsonUtility.ToJson(this.playlist));
        }

        // F3 key toggles songs add/remove from playlist
        public override void OnUpdate()
        {
            if (!inArcadeMode)
                return;

            if (Input.GetKeyDown(KeyCode.F3))
            {
                ArcadeSongDatabase.BeatmapItem selectedSong = ArcadeSongList.Instance?.GetSelectedSong();
                if (selectedSong == null) return;

                playlist.songs ??= [];
                int songId = selectedSong.internalIndex;
                bool isInPlaylist = Array.IndexOf(playlist.songs, songId) != -1;

                if (!isInPlaylist)
                {
                    // add
                    playlist.songs = [.. playlist.songs, songId];
                    ArcadeNotification.Show<ArcadeUnlockNotification>("Unlock", null).Fill("> user playlist.", "song added to playlist", $"// {selectedSong.Song.name}", "", null);
                }
                else
                {
                    // remove
                    playlist.songs = [.. playlist.songs.Where(id => id != songId)];
                    ArcadeNotification.Show<ArcadeUnlockNotification>("Unlock", null).Fill("> user playlist.", "song removed from playlist", $"// {selectedSong.Song.name}", "", null);
                }

                this.SavePlaylist();
                ArcadeSongDatabase.Instance?.RefreshSongList(false);
            }
        }
    }
}

[Serializable]
public class PlaylistSongs
{
    public int[] songs;
}