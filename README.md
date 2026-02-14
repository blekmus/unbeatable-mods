# UNBEATABLE MODS

MelonLoader mods for [UNBEATABLE](https://store.steampowered.com/app/2240620/UNBEATABLE/) by D-CELL GAMES.

## Arcade Mode Custom Playlist

As much as I love all the songs in the game, there are some that I always come back to. Considering how far my obsession for seeing only the things I want to see has gone, I made my own solution to it. I might've had to learn Unity, C#, and reverse engineering from the ground up, but it was worth it. Here's my fix for: *What if only my favorite songs were displayed?*

### [Read about how I made this mod](https://dinil.dev/longform/unbeatable-mod-notes?utm_source=github)

> Currently only tested on the windows version of the game.

Adds a new **playlist** category in the filters menu that works with all existing sorting methods.

![category](./CUSTOM_PLAYLIST/screenshots/category.png)

Press `F3` to toggle songs in and out of the playlist.

![toggle](./CUSTOM_PLAYLIST/screenshots/toggle.png)

The playlist is saved alongside your save files and syncs to Steam Cloud. It's stored in its own file, so if the game updates and breaks the mod, your save file won't get corrupted.

![savefile](./CUSTOM_PLAYLIST/screenshots/savefile.png)

## Installation

1. Install [MelonLoader](https://github.com/LavaGang/MelonLoader.Installer) for UNBEATABLE.
2. Launch game once for MelonLoader to initialize.
3. Download `.rar` mod file from [Releases](https://github.com/blekmus/unbeatable-mods/releases/latest).
4. Unzip and place the DLL in your `Mods` folder in the game dir. 
5. Launch the game.

## License

MIT License - See [LICENSE](LICENSE.txt) for details

## Credits

Created by blekmus (Dinil Fernando)
