#### Provides an in song leaderboard comparing your score as you play to the scores on the active leaderboard page as of starting the song

## Configuration
- Any True/False settings can be toggled in game through the left side of the modifiers panel, below Disappearing arrows, faster song, etc. Simply go to the 'LeaderboardInSong' submenu
- All of the options can be configured in UserData/LeaderboardInSong.ini
```ini
[Options]
Enabled = True
SortByAcc = True
X = -2
Y = 0.5
Z = 7
Scale = 0.3
SimpleNames = False
RefreshTime = 0.5
```
| Option | About |
| - | - |
| `Enabled` | Enables/Disables the Leaderboard |
| `SortByAcc ` | Orders scores on the leaderboard by accuracy instead of simply using current score, useful if you want the leaderboard to be more dynamic |
| `X` | X position of the Leaderboard (Default is -2) |
| `Y` | Y position of the Leaderboard (Default is 0.5) |
| `Z` | Z position of the Leaderboard (Default is 7) |
| `Scale` | Scale multiplier for the leaderboard (Default is 0.3) |
| `SimpleNames` | When true, removes excess information from player names such as percentage, modifiers, or performance points |
| `RefreshTime` | How often the leaderboard refreshes (Default is 0.5) Lower values means the board refreshes more often, however can also become taxing performance wise at very low values |
