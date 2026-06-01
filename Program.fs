namespace CliSlidingPuzzle

open System

// Directions for moving the empty tile
type Direction = Up | Down | Left | Right

// Game modes
type GameMode = Playing | CheckSolution

module PuzzleGame =

  // Requirement 5:
  // Winning board layout
  let TargetBoard = [1; 2; 3; 4; 5; 6; 7; 8; 0]

  // Requirement 1:
  // Display a 3x3 board in the terminal
  let printBoard (board : int list) (nextBoardInSolution : int list option) (mode : GameMode) =
    Console.Clear()
    printfn "=== CLI Sliding Puzzle ==="

    if board = TargetBoard then
      printfn "Congratulations, you solved the puzzle!"
      printfn "--------------------------------------------------"
      printfn "Press 'R' to Reset and try this board again"
      printfn "Press 'A' to view the Answer guide for this board"
      printfn "Press 'N' to play a New game board"
      printfn "Press 'Q' to Quit the game"
      printfn "--------------------------------------------------\n"
    else
      match mode with
      | Playing ->
          printfn "PLAYING MODE"
          printfn "Manual : Use Arrow Keys to move Empty Space(_)"
          printfn "Commands : [R] Reset Board | [A] Answer Guide | [N] New Board | [Q] Quit\n"
      | CheckSolution ->
          printfn "ANSWER GUIDE MODE"
          printfn "Manual : Follow the guide board layout below"
          printfn "Commands : [A] Return to Playing Mode | [Q] Quit\n"

    for i in 0 .. 2 do
      let row = board |> List.skip (i * 3) |> List.take 3
      let renderCell x = if x = 0 then "_" else string x

      let c0 = List.item 0 row
      let c1 = List.item 1 row
      let c2 = List.item 2 row

      printfn " %s | %s | %s " (renderCell c0) (renderCell c1) (renderCell c2)
      if i < 2 then printfn "---+---+---"
    
    printfn ""

    if mode = CheckSolution && board <> TargetBoard then
      match nextBoardInSolution with
      | Some nextLayout ->
          printfn "Next Step Guide:"

          for i in 0 .. 2 do
            let row = nextLayout |> List.skip (i * 3) |> List.take 3
            let renderCell x = if x = 0 then "_" else string x

            let c0 = List.item 0 row
            let c1 = List.item 1 row
            let c2 = List.item 2 row

            printfn "   %s | %s | %s " (renderCell c0) (renderCell c1) (renderCell c2)
            if i < 2 then printfn "  ---+---+---"

          printfn ""

      | None ->
          printfn "Next Step Guide:"
          printfn "=> Press the correct arrow key to make the final move!"

  // Find the position of the empty tile
  let getEmptyPosition (board : int list) =
    let idx = board |> List.findIndex (fun x -> x = 0)
    (idx / 3, idx % 3)

  // Requirement 3 & 4:
  // Move the empty tile and ignore invalid moves
  let move (dir : Direction) (board : int list) =
    let r, c = getEmptyPosition board

    let tr, tc =
      match dir with
      | Up -> r - 1, c
      | Down -> r + 1, c
      | Left -> r, c - 1
      | Right -> r, c + 1

    if tr >= 0 && tr < 3 && tc >= 0 && tc < 3 then
      let emptyIdx = r*3 + c
      let targetIdx = tr*3 + tc

      let newBoard =
        board
        |> List.mapi (fun idx x ->
            if idx = emptyIdx then List.item targetIdx board
            elif idx = targetIdx then List.item emptyIdx board
            else x)

      Some newBoard
    else None

  // Remove duplicate states from the stored solution path
  let optimizePath (path : int list list) =
    let rec compress acc remaining =
      match remaining with
      | [] -> List.rev acc
      | head :: tail ->
          if List.contains head acc then
            let shortenedAcc = acc |> List.skipWhile (fun b -> b <> head)
            compress shortenedAcc tail
          else compress (head :: acc) tail

    compress [] path

  // Requirement 2:
  // Generate a random board that is always solvable
  let generateSolvableBoard () =
    let rand = Random()
    let directions = [Up; Down; Left; Right]

    let rec shuffle steps currentBoard boardHistory =
      if steps = 0 then
        let optimized = optimizePath boardHistory
        (optimized.Head, optimized)
      else
        let randomDir = List.item (rand.Next(4)) directions

        match move randomDir currentBoard with
        | Some nextBoard -> shuffle (steps - 1) nextBoard (nextBoard :: boardHistory)
        | None -> shuffle steps currentBoard boardHistory

    // Start from the solved board and shuffle it
    shuffle 60 TargetBoard [TargetBoard]

  // Main game loop
  let rec gameLoop
      (board : int list)
      (historyPath : int list list)
      (mode : GameMode)
      (initBoard : int list)
      (initPath : int list list) =

    let nextLayout, remainingHistory =
      if historyPath.Length >= 2 && historyPath.Head = board then
        let next = List.item 1 historyPath
        let tail = List.skip 1 historyPath
        Some next, tail
      elif historyPath.Length = 1 && historyPath.Head = board then None, []
      else None, historyPath

    printBoard board nextLayout mode

    let keyInfo = Console.ReadKey(true)

    if keyInfo.Key = ConsoleKey.Q then printfn "Game Exited. Thank you!"
    else
      if board = TargetBoard then
        match keyInfo.Key with
        | ConsoleKey.R -> gameLoop initBoard initPath Playing initBoard initPath
        | ConsoleKey.A -> gameLoop initBoard initPath CheckSolution initBoard initPath
        | ConsoleKey.N ->
            let newBoard, newPath = generateSolvableBoard ()
            gameLoop newBoard newPath Playing newBoard newPath
        | _ -> gameLoop board historyPath Playing initBoard initPath

      else
        match mode with
        | Playing ->
            let handlePlayerMove dir =
              match move dir board with
              | Some nextBoard -> gameLoop nextBoard historyPath Playing initBoard initPath
              | None -> gameLoop board historyPath Playing initBoard initPath

            match keyInfo.Key with
            | ConsoleKey.UpArrow -> handlePlayerMove Up
            | ConsoleKey.DownArrow -> handlePlayerMove Down
            | ConsoleKey.LeftArrow -> handlePlayerMove Left
            | ConsoleKey.RightArrow -> handlePlayerMove Right
            | ConsoleKey.R -> gameLoop initBoard initPath Playing initBoard initPath
            | ConsoleKey.A -> gameLoop initBoard initPath CheckSolution initBoard initPath
            | ConsoleKey.N ->
                let newBoard, newPath = generateSolvableBoard ()
                gameLoop newBoard newPath Playing newBoard newPath
            | _ -> gameLoop board historyPath Playing initBoard initPath

        | CheckSolution ->
            let handlePlayerMove dir =
              match move dir board with
              | Some nextBoard ->
                  match nextLayout with
                  | Some expected when nextBoard = expected ->
                      // Only allow the next correct move in guide mode
                      gameLoop nextBoard remainingHistory CheckSolution initBoard initPath
                  | _ -> gameLoop board historyPath CheckSolution initBoard initPath
              | None -> gameLoop board historyPath CheckSolution initBoard initPath

            match keyInfo.Key with
            | ConsoleKey.UpArrow -> handlePlayerMove Up
            | ConsoleKey.DownArrow -> handlePlayerMove Down
            | ConsoleKey.LeftArrow -> handlePlayerMove Left
            | ConsoleKey.RightArrow -> handlePlayerMove Right
            | ConsoleKey.A -> gameLoop board historyPath Playing initBoard initPath
            | _ -> gameLoop board historyPath CheckSolution initBoard initPath

module Program =

  [<EntryPoint>]
  let main argv =
    Console.CursorVisible <- false

    let initialBoard, historyPath =
      PuzzleGame.generateSolvableBoard ()

    PuzzleGame.gameLoop
      initialBoard
      historyPath
      Playing
      initialBoard
      historyPath

    Console.CursorVisible <- true
    0