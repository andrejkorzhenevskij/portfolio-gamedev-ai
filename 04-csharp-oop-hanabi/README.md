# Hanabi - C# OOP Exercise

A completed C#/.NET 8 implementation of a simplified two-player Hanabi assignment focused on object-oriented design.

- Simplified two-player Hanabi
- Console application
- Explicit ordered input deck
- Game rules isolated from input/output
- Developed with tests

## Why this project is interesting

The main modeling challenge is that players cannot see their own cards. The program must distinguish between the actual hidden `Card` state and the player's `CardKnowledge` about that card.

Successful plays can also be classified as risky based on whether the player's current knowledge guaranteed that the play was legal.

This is a simplified assignment implementation, not a full commercial game or complete standard Hanabi ruleset.

## Architecture

`Hanabi.Core` contains the domain model:

- `Card` / `CardColor`
- `Deck`
- `Hand` / `HandSlot` / `CardKnowledge`
- `Tableau`
- `DiscardPile`
- `Game`

`Hanabi.Cli` handles parsing console commands and formatting the final game result. It contains no game rules.

`Hanabi.Tests` contains xUnit tests for domain rules and CLI parsing/output.

`Game` coordinates actions and turn state, `Tableau` owns playability, and `CardKnowledge` represents what a player knows about a hidden card.

## Implemented rules

These are the simplified assignment rules, not full standard Hanabi rules.

- Exactly two players
- First five input cards go to player 1, next five to player 2
- `Play`
- `Drop`
- `Tell color`
- `Tell rank`
- Complete hints are required
- Invalid play or hint ends the game
- Drawing the final deck card ends the game immediately
- 25 successfully played cards ends the game
- Commands after game over are ignored
- Successful risky plays are counted

## Input Format

```text
Start new game with deck R1 G2 B3 W4 Y5 R1 R1 B1 B2 W1 W2 W1
Play card 0
Drop card 4
Tell color Red for cards 0 1 2 3 4
Tell rank 1 for cards 2 4
```

Card notation uses `R`, `G`, `B`, `Y`, or `W` followed by rank `1` through `5`.

Hand indices are zero-based.

## Output Format

The assignment specifies the three output values but does not specify punctuation or labels. This implementation uses exactly three space-separated integers:

```text
<move-number> <correctly-played-card-count> <successful-risky-play-count>
```

Example:

```text
1 0 0
```

## Running

From `04-csharp-oop-hanabi`:

```bash
dotnet run --project Hanabi.Cli
```

Example:

```bash
printf '%s\n' \
  'Start new game with deck R1 G2 B3 W4 Y5 R1 R1 B1 B2 W1 W2' \
  'Play card 1' \
  | dotnet run --project Hanabi.Cli
```

## Tests

```bash
dotnet test
```

The current suite contains 94 passing tests.

## Design Notes

- `HandSlot` binds the actual `Card` with its `CardKnowledge` to avoid parallel collections drifting out of sync.
- `Tableau` stores only the highest successfully played rank per color.
- Guaranteed playability evaluates all cards consistent with `CardKnowledge`.
- `Game` receives an explicit ordered deck; it does not depend on standard Hanabi deck construction or shuffling.

## Possible Next Step

The same `Hanabi.Core` could later be reused by a graphical or web frontend without moving game rules into UI code.
