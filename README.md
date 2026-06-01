# CLI-Sliding-Puzzle
This project is a command-line sliding puzzle game. The player is presented with a 3x3 grid containing one empty space and tiles numbered 1 to 8. The player slides tiles into the empty space to arrange them in order.

# How to Run
1. Ensure you have the **.NET 10 SDK** installed on your machine
2. Navigate to the directory where `CliSlidingPuzzle.fsproj` and `Program.fs` are located.
3. Open terminal and exeucte 'dotnet run'

# Game Rules
## Playing Mode
The game starts in this state. The player use keyboard to play.
### Movement
Use Arrow keys(Up, Down, Left, Right) to move empty space(_) and swap with neighbor tiles
### Command
- [R] Reset Board : Make the current puzzle back to the initial state.
- [A] Answer Guide : Turn into Answer guide mode which shows you sample answer(ensures that the puzzle can be solved).
- [N] New Board : Generate new random puzzle.
- [Q] Quit : Instantly terminates.

## Answer Guide Mode
When pressed [A] during Playing mode.
### Mechanism
It brings board to the intial(unshuffled) state by reversing the shuffling order.
### Constraints
To prevent path desynchronization, the input matrix is locked down; only the correct arrow key following the guide is allowed.
### Command
- [A] Return : Turns off the Answer guide mode and returns to Playing mode.
- [Q] Quit : Instantly terminates.
