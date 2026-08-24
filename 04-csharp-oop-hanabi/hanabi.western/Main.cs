using Godot;
using Hanabi.Core;

public partial class Main : Control
{
    private const int MaxMistakes = 3;
    private const int MaxHintTokens = 8;
    private const string LogPath = "hanabi_log.txt";
    private static readonly Vector2 ScreenSize = new(1152, 648);
    private static readonly Vector2 LayoutPadding = new(18, 10);
    private static readonly Vector2 CardSize = new(72, 96);
    private static readonly Vector2 PlanLaneSize = new(116, 214);
    private static readonly string[] PlanLanes = ["Маршрут", "Оружие", "Охрана", "Цель", "Отход"];
    private static readonly CardColor[] LaneColors =
    [
        CardColor.Red,
        CardColor.Green,
        CardColor.Blue,
        CardColor.Yellow,
        CardColor.White
    ];

    private Hanabi.Core.Game game = null!;
    private readonly int[] laneProgress = new int[5];
    private readonly int?[] burnedFromRank = new int?[5];
    private readonly List<string> actionLog = [];
    private readonly Random random = new();
    private int mistakesLeft = MaxMistakes;
    private int hintTokens = MaxHintTokens;
    private int playerShare = 50;
    private int partnerShare = 50;
    private int turnNumber = 1;
    private int currentPlayerIndex = 0;
    private string lastSelected = "Нет";
    private string lastAction = "Выберите карту или действие.";
    private string lastResult = "Ожидание.";
    private int? selectedHandIndex;
    private int? selectedCardIndex;
    private int? partnerAdviceCardIndex;
    private Card? partnerAdviceCard;
    private int? hintedHandIndex;
    private CardColor? hintedColor;
    private int? hintedRank;

    public override void _Ready()
    {
        GetWindow().Size = (Vector2I)ScreenSize;
        game = new Hanabi.Core.Game(CreateStartingCards());
        AddLog("Prototype started");
        RefreshTable();
    }

    private void RefreshTable()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }

        var table = new Panel { CustomMinimumSize = ScreenSize, Size = ScreenSize };
        table.AddThemeStyleboxOverride("panel", PanelStyle(new Color("6f4527"), new Color("3f2515")));
        AddChild(table);

        var layoutSize = ScreenSize - LayoutPadding * 2;
        var layout = new VBoxContainer
        {
            Position = LayoutPadding,
            CustomMinimumSize = layoutSize,
            Size = layoutSize
        };
        layout.AddThemeConstantOverride("separation", 8);
        AddChild(layout);

        layout.AddChild(CreatePartnerHandArea());
        layout.AddChild(CreateCenterRow());
        layout.AddChild(CreateActionLogArea());
        layout.AddChild(CreatePlayerHandArea());
        layout.AddChild(CreateActionsArea());
    }

    private static Card[] CreateStartingCards()
    {
        return
        [
            .. from rank in Enumerable.Range(1, 5)
               from color in LaneColors
               from _ in Enumerable.Range(0, rank == 1 ? 3 : rank == 5 ? 1 : 2)
               select new Card(color, rank)
        ];
    }

    private Control CreatePartnerHandArea()
    {
        var area = CreateZone("PartnerHand", new Vector2(1, 118));
        AddTitle(area, "Партнёр");
        AddHand(area, game.Hands[1], handIndex: 1, hideCards: false);
        return area;
    }

    private Control CreateCenterRow()
    {
        var row = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            CustomMinimumSize = new Vector2(1, 248),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        row.AddThemeConstantOverride("separation", 12);
        row.AddChild(CreateMistakesArea());
        row.AddChild(CreatePlanArea());
        row.AddChild(CreateDeckArea());
        return row;
    }

    private Control CreatePlayerHandArea()
    {
        var area = CreateZone("PlayerHand", new Vector2(1, 100));
        AddHand(area, game.Hands[0], handIndex: 0, hideCards: true);
        return area;
    }

    private Control CreateMistakesArea()
    {
        var area = CreateZone("Mistakes", new Vector2(128, 1));
        AddTitle(area, "Ошибки");
        area.AddChild(CreatePile("Осталось", mistakesLeft.ToString(), new Color("5a2630")));
        return area;
    }

    private Control CreatePlanArea()
    {
        var area = CreateZone("Plan", new Vector2(650, 1));
        AddTitle(area, "План");

        var lanes = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        lanes.AddThemeConstantOverride("separation", 8);

        for (var lane = 0; lane < PlanLanes.Length; lane++)
        {
            lanes.AddChild(CreatePlanLane(lane));
        }

        area.AddChild(lanes);
        return area;
    }

    private Control CreateDeckArea()
    {
        var area = CreateZone("Deck", new Vector2(142, 1));
        AddTitle(area, "Запасы");
        area.AddChild(CreatePile("Колода", game.DrawDeck.Count.ToString(), new Color("4a3428")));
        area.AddChild(CreateHintTokens(hintTokens));
        area.AddChild(CreateShareCounter());
        return area;
    }

    private Control CreateActionLogArea()
    {
        var panel = new Panel
        {
            CustomMinimumSize = new Vector2(1, 72),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        panel.AddThemeStyleboxOverride(
            "panel",
            PanelStyle(IsGameOver() ? new Color("4a2020") : new Color("2f2a24"), IsGameOver() ? new Color("ff9b72") : new Color("d6c07a")));

        var label = new Label
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            Text = $"{(IsGameOver() ? "Plan failed\n" : "")}Карта: {SelectedText()}\nДействие: {lastAction} | Итог: {lastResult} | Ошибки: {mistakesLeft}/{MaxMistakes} | Ход {turnNumber}, игрок {currentPlayerIndex + 1}",
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeFontSizeOverride("font_size", 16);
        panel.AddChild(label);
        return panel;
    }

    private Control CreateActionsArea()
    {
        var area = CreateZone("Actions", new Vector2(1, 42));
        var row = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        row.AddThemeConstantOverride("separation", 6);

        row.AddChild(CreateActionButton("Сыграть", CanPlaySelected(), PlaySelected));
        row.AddChild(CreateActionButton("Сбросить", CanDropSelected(), DropSelected));
        row.AddChild(CreateActionButton("Цвет", CanHintSelected(), HintSelectedColor));
        row.AddChild(CreateActionButton("Ранг", CanHintSelected(), HintSelectedRank));
        row.AddChild(CreateActionButton("Ask advice", CanUseAdvice(), AskForAdvice));
        row.AddChild(CreateActionButton("Export Log", true, ExportLog));
        area.AddChild(row);
        return area;
    }

    private bool CanPlaySelected()
    {
        return !IsGameOver() &&
            selectedHandIndex == 0 &&
            selectedCardIndex is not null;
    }

    private bool CanDropSelected()
    {
        return !IsGameOver() && selectedHandIndex == 0 && selectedCardIndex is not null;
    }

    private bool CanHintSelected()
    {
        return !IsGameOver() &&
            hintTokens > 0 &&
            selectedHandIndex == HintTargetHandIndex() &&
            selectedCardIndex is not null;
    }

    private bool CanUseAdvice()
    {
        return !IsGameOver() && hintTokens > 0 && game.Hands[0].Count > 0 && partnerAdviceCardIndex is null;
    }

    private void PlaySelected()
    {
        if (!CanPlaySelected())
        {
            return;
        }

        var hand = game.Hands[0];
        var playedIndex = selectedCardIndex!.Value;
        var card = hand.Slots[playedIndex].Card;
        var acceptedAdvice = partnerAdviceCardIndex == playedIndex;
        lastSelected = CardText(card);
        AddLog($"ACTION: Player {currentPlayerIndex + 1} played {CardText(card)}");
        if (acceptedAdvice)
        {
            ApplyShareChange(1, 10);
            AddLog("Accepted partner advice");
            AddLog("Cost: 10% share");
            ClearAdvice();
        }
        else
        {
            ClearIgnoredAdvice($"played {CardText(card)}");
        }

        hand.RemoveAt(selectedCardIndex.Value);

        var lane = LaneFor(card.Color);
        if (burnedFromRank[lane] is null && card.Rank == laneProgress[lane] + 1)
        {
            laneProgress[lane] = card.Rank;
            lastAction = $"Сыграна {CardText(card)} в {PlanLanes[lane]}";
            lastResult = $"Успех: закрыта ячейка {card.Rank}.";
            AddLog($"SUCCESS: {PlanLanes[lane]} progress {laneProgress[lane]}/5");
        }
        else
        {
            mistakesLeft--;
            burnedFromRank[lane] ??= laneProgress[lane] + 1;
            RemoveColorFromHands(card.Color);
            lastAction = $"Сыграна {CardText(card)} в {PlanLanes[lane]}";
            lastResult = IsGameOver()
                ? $"Ошибка: дорожка сожжена с {burnedFromRank[lane]}, план провален."
                : $"Ошибка: дорожка сожжена с {burnedFromRank[lane]}.";
            AddLog("FAILURE: wrong card");
            AddLog($"Burned lane: {PlanLanes[lane]}");
            AddLog($"Mistakes left: {mistakesLeft}");
        }

        RefillHand(hand);
        ClearSelection();
        AdvanceTurn();
        RefreshTable();
    }

    private void DropSelected()
    {
        if (!CanDropSelected())
        {
            return;
        }

        var hand = game.Hands[0];
        var droppedIndex = selectedCardIndex!.Value;
        lastSelected = CardText(hand.Slots[droppedIndex].Card);
        AddLog($"ACTION: Player {currentPlayerIndex + 1} discarded {lastSelected}");
        hintTokens = Math.Min(MaxHintTokens, hintTokens + 1);
        AddLog("Hint token restored");
        ClearIgnoredAdvice($"discarded {lastSelected}");
        hand.RemoveAt(droppedIndex);
        RefillHand(hand);
        lastAction = $"Сброшена {lastSelected}";
        lastResult = "Карта убрана, рука добрана.";
        AddLog("Hand refilled");
        ClearSelection();
        AdvanceTurn();
        RefreshTable();
    }

    private void HintSelectedColor()
    {
        if (!CanHintSelected())
        {
            return;
        }

        var targetHandIndex = HintTargetHandIndex();
        var card = game.Hands[targetHandIndex].Slots[selectedCardIndex!.Value].Card;
        hintTokens--;
        ApplyShareChange(currentPlayerIndex);
        ApplyVisualHint(handIndex: targetHandIndex, color: card.Color, rank: null);
        lastSelected = CardText(card);
        lastAction = $"Подсказка по цвету: {WesternSymbol(card.Color)}";
        lastResult = $"Отмечены: {MarkedCardsText(targetHandIndex)}.";
        AddLog($"ACTION: Player {currentPlayerIndex + 1} gave color hint: {card.Color}");
        AddLog($"Marked cards: {MarkedCardsText(targetHandIndex)}");
        ClearIgnoredAdvice($"gave color hint {card.Color}");
        ClearSelection();
        AdvanceTurn();
        RefreshTable();
    }

    private void HintSelectedRank()
    {
        if (!CanHintSelected())
        {
            return;
        }

        var targetHandIndex = HintTargetHandIndex();
        var card = game.Hands[targetHandIndex].Slots[selectedCardIndex!.Value].Card;
        hintTokens--;
        ApplyShareChange(currentPlayerIndex);
        ApplyVisualHint(handIndex: targetHandIndex, color: null, rank: card.Rank);
        lastSelected = CardText(card);
        lastAction = $"Подсказка по рангу: {card.Rank}";
        lastResult = $"Отмечены: {MarkedCardsText(targetHandIndex)}.";
        AddLog($"ACTION: Player {currentPlayerIndex + 1} gave rank hint: {card.Rank}");
        AddLog($"Marked cards: {MarkedCardsText(targetHandIndex)}");
        ClearIgnoredAdvice($"gave rank hint {card.Rank}");
        ClearSelection();
        AdvanceTurn();
        RefreshTable();
    }

    private void AskForAdvice()
    {
        if (!CanUseAdvice())
        {
            return;
        }

        hintTokens--;
        partnerAdviceCardIndex = random.Next(game.Hands[0].Count);
        partnerAdviceCard = game.Hands[0].Slots[partnerAdviceCardIndex.Value].Card;
        lastSelected = $"Партнёр выбрал карту {partnerAdviceCardIndex.Value + 1}";
        lastAction = "Ask for advice";
        lastResult = "Partner suggests playing this card.";
        AddLog("Advice requested:");
        AddLog($"Suggested card: {CardText(partnerAdviceCard)}");
        AddLog("Cost: 1 hint token");
        ClearSelection();
        RefreshTable();
    }

    private void ExportLog()
    {
        SaveLog();
        lastAction = "Export Log";
        lastResult = $"{LogPath} сохранён.";
        RefreshTable();
    }

    private void RefillHand(Hand hand)
    {
        while (hand.Count < 5 && TryDrawPlayableCard(out var card))
        {
            hand.Add(card);
        }
    }

    private bool TryDrawPlayableCard(out Card card)
    {
        while (!game.DrawDeck.IsEmpty)
        {
            card = game.DrawDeck.Draw();
            if (burnedFromRank[LaneFor(card.Color)] is null)
            {
                return true;
            }
        }

        card = new Card(CardColor.Red, 1);
        return false;
    }

    private void RemoveColorFromHands(CardColor color)
    {
        foreach (var hand in game.Hands)
        {
            for (var i = hand.Count - 1; i >= 0; i--)
            {
                if (hand.Slots[i].Card.Color == color)
                {
                    hand.RemoveAt(i);
                }
            }

            RefillHand(hand);
        }
    }

    private void SelectCard(int handIndex, int cardIndex)
    {
        var alreadySelected = selectedHandIndex == handIndex && selectedCardIndex == cardIndex;
        selectedHandIndex = handIndex;
        selectedCardIndex = cardIndex;
        lastSelected = SelectedText();
        lastAction = "Выбрана карта";
        lastResult = "Ожидается действие.";
        var card = game.Hands[handIndex].Slots[cardIndex].Card;
        if (!alreadySelected)
        {
            AddLog($"CARD SELECT: Player {currentPlayerIndex + 1} selected card {CardText(card)}");
        }

        RefreshTable();
    }

    private void ClearSelection()
    {
        selectedHandIndex = null;
        selectedCardIndex = null;
    }

    private void ApplyVisualHint(int handIndex, CardColor? color, int? rank)
    {
        hintedHandIndex = handIndex;
        hintedColor = color;
        hintedRank = rank;
    }

    private void ApplyShareChange(int earningPlayerIndex, int percent = 5)
    {
        if (earningPlayerIndex == 0)
        {
            playerShare = Math.Min(100, playerShare + percent);
            partnerShare = Math.Max(0, partnerShare - percent);
        }
        else
        {
            partnerShare = Math.Min(100, partnerShare + percent);
            playerShare = Math.Max(0, playerShare - percent);
        }
    }

    private int HintTargetHandIndex()
    {
        return 1 - currentPlayerIndex;
    }

    private void ClearAdvice()
    {
        partnerAdviceCardIndex = null;
        partnerAdviceCard = null;
    }

    private void ClearIgnoredAdvice(string actualAction)
    {
        if (partnerAdviceCard is null)
        {
            return;
        }

        AddLog("Advice ignored");
        AddLog($"Suggested card: {CardText(partnerAdviceCard)}");
        AddLog($"Actual action: {actualAction}");
        ClearAdvice();
    }

    private void AdvanceTurn()
    {
        turnNumber++;
        currentPlayerIndex = 1 - currentPlayerIndex;

        if (currentPlayerIndex == 1 && !IsGameOver())
        {
            RunPartnerTurn();
            if (!IsGameOver())
            {
                turnNumber++;
                currentPlayerIndex = 0;
            }
        }
    }

    private void RunPartnerTurn()
    {
        if (TryFindPartnerPlayableCard(out var playableIndex))
        {
            AddLog("Partner decision:");
            AddLog("Reason: playable card available");
            PartnerPlay(playableIndex);
            return;
        }

        if (hintTokens > 0 && game.Hands[0].Count > 0)
        {
            AddLog("Partner decision:");
            AddLog("Reason: no playable cards, giving hint");
            PartnerHint();
            return;
        }

        AddLog("Partner decision:");
        AddLog("Reason: no useful action, discarding");
        PartnerDiscard();
    }

    private bool TryFindPartnerPlayableCard(out int index)
    {
        var hand = game.Hands[1];
        for (var i = 0; i < hand.Count; i++)
        {
            var card = hand.Slots[i].Card;
            var lane = LaneFor(card.Color);
            if (burnedFromRank[lane] is null && card.Rank == laneProgress[lane] + 1)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }

    private void PartnerPlay(int index)
    {
        var hand = game.Hands[1];
        if (index < 0 || index >= hand.Count)
        {
            return;
        }

        var card = hand.Slots[index].Card;
        AddLog($"ACTION: Player 2 played {CardText(card)}");
        hand.RemoveAt(index);

        var lane = LaneFor(card.Color);
        if (burnedFromRank[lane] is null && card.Rank == laneProgress[lane] + 1)
        {
            laneProgress[lane] = card.Rank;
            lastAction = $"Партнёр сыграл {CardText(card)}";
            lastResult = $"Успех: {PlanLanes[lane]} {laneProgress[lane]}/5.";
            AddLog($"SUCCESS: {PlanLanes[lane]} progress {laneProgress[lane]}/5");
        }
        else
        {
            mistakesLeft--;
            burnedFromRank[lane] ??= laneProgress[lane] + 1;
            RemoveColorFromHands(card.Color);
            lastAction = $"Партнёр сыграл {CardText(card)}";
            lastResult = IsGameOver()
                ? $"Ошибка: {PlanLanes[lane]} сожжена, план провален."
                : $"Ошибка: {PlanLanes[lane]} сожжена.";
            AddLog("FAILURE: wrong card");
            AddLog($"Burned lane: {PlanLanes[lane]}");
            AddLog($"Mistakes left: {mistakesLeft}");
        }

        RefillHand(hand);
    }

    private void PartnerDiscard()
    {
        var hand = game.Hands[1];
        if (hand.Count == 0)
        {
            AddLog("Partner skipped: no cards");
            return;
        }

        var index = random.Next(hand.Count);
        var card = hand.Slots[index].Card;
        hand.RemoveAt(index);
        RefillHand(hand);
        lastAction = $"Партнёр сбросил {CardText(card)}";
        lastResult = "Рука партнёра добрана.";
        AddLog($"ACTION: Player 2 discarded {CardText(card)}");
        AddLog("Hand refilled");
    }

    private void PartnerHint()
    {
        var card = game.Hands[0].Slots[random.Next(game.Hands[0].Count)].Card;
        var givesColor = random.Next(2) == 0;
        hintTokens--;
        ApplyShareChange(1);
        ApplyVisualHint(0, givesColor ? card.Color : null, givesColor ? null : card.Rank);
        lastAction = givesColor ? "Партнёр дал подсказку по цвету" : "Партнёр дал подсказку по рангу";
        lastResult = $"Отмечены: {MarkedCardsText(0)}.";
        AddLog(givesColor
            ? $"ACTION: Player 2 gave color hint: {card.Color}"
            : $"ACTION: Player 2 gave rank hint: {card.Rank}");
        AddLog($"Marked cards: {MarkedCardsText(0)}");
    }

    private bool IsGameOver()
    {
        return mistakesLeft <= 0;
    }

    private string SelectedText()
    {
        if (selectedHandIndex is null || selectedCardIndex is null)
        {
            return lastSelected;
        }

        var hand = game.Hands[selectedHandIndex.Value];
        if (selectedCardIndex.Value < 0 || selectedCardIndex.Value >= hand.Count)
        {
            return lastSelected;
        }

        var card = hand.Slots[selectedCardIndex.Value].Card;
        return selectedHandIndex == 0
            ? $"Игрок: карта {selectedCardIndex.Value + 1} ({CardText(card)})"
            : $"Партнёр: {CardText(card)}";
    }

    private void AddLog(string entry)
    {
        actionLog.Add($"Turn {turnNumber}:");
        actionLog.Add(entry);
        actionLog.Add("");
        SaveLog();
    }

    private void SaveLog()
    {
        File.WriteAllText(LogPath, string.Join('\n', actionLog));
    }

    private string MarkedCardsText(int handIndex)
    {
        var marked = game.Hands[handIndex].Slots
            .Select((slot, index) => IsHintedCard(slot.Card, handIndex) ? $"{index + 1}:{CardText(slot.Card)}" : "")
            .Where(text => text.Length > 0);
        return string.Join(", ", marked.DefaultIfEmpty("нет"));
    }

    private static VBoxContainer CreateZone(string name, Vector2 minimumSize)
    {
        return new VBoxContainer
        {
            Name = name,
            Alignment = BoxContainer.AlignmentMode.Center,
            CustomMinimumSize = minimumSize,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
    }

    private static void AddTitle(VBoxContainer layout, string text)
    {
        var label = new Label
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        label.AddThemeFontSizeOverride("font_size", 18);
        label.AddThemeColorOverride("font_color", Colors.White);
        layout.AddChild(label);
    }

    private void AddHand(VBoxContainer layout, Hand hand, int handIndex, bool hideCards)
    {
        var row = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        row.AddThemeConstantOverride("separation", 8);

        for (var index = 0; index < hand.Slots.Count; index++)
        {
            row.AddChild(CreateCard(hand.Slots[index].Card, handIndex, index, hideCards));
        }

        layout.AddChild(row);
    }

    private Button CreateCard(Card card, int handIndex, int cardIndex, bool hidden)
    {
        var selected = selectedHandIndex == handIndex && selectedCardIndex == cardIndex;
        var advised = handIndex == 0 && partnerAdviceCardIndex == cardIndex;
        var blocked = burnedFromRank[LaneFor(card.Color)] is not null;
        var hinted = IsHintedCard(card, handIndex);
        var text = hidden
            ? hinted ? $"◆\n{HintMarker()}" : "◆"
            : hinted ? $"{WesternSymbol(card.Color)}\n{card.Rank}\n{HintMarker()}" : $"{WesternSymbol(card.Color)}\n{card.Rank}";
        var button = new Button
        {
            CustomMinimumSize = CardSize,
            Text = text,
            Disabled = blocked || IsGameOver()
        };
        button.AddThemeFontSizeOverride("font_size", hinted ? 22 : 26);
        button.AddThemeStyleboxOverride(
            "normal",
            hidden
                ? PanelStyle(blocked ? new Color("2b2528") : hinted || advised ? new Color("355c42") : new Color("26324f"), hinted || selected || advised ? new Color("f0d35f") : new Color("d6c07a"))
                : PanelStyle(blocked ? new Color("382a2a") : CardTint(card.Color), hinted || selected || advised ? new Color("f0d35f") : Colors.White));
        button.AddThemeStyleboxOverride("pressed", PanelStyle(hidden ? new Color("26324f") : CardTint(card.Color), new Color("f0d35f")));
        button.Pressed += () => SelectCard(handIndex, cardIndex);
        return button;
    }

    private bool IsHintedCard(Card card, int handIndex)
    {
        return hintedHandIndex == handIndex &&
            ((hintedColor is not null && card.Color == hintedColor) ||
             (hintedRank is not null && card.Rank == hintedRank));
    }

    private string HintMarker()
    {
        return hintedColor is not null ? WesternSymbol(hintedColor.Value) : hintedRank?.ToString() ?? "";
    }

    private Control CreatePlanLane(int lane)
    {
        var panel = new Panel
        {
            CustomMinimumSize = PlanLaneSize,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        panel.AddThemeStyleboxOverride("panel", PanelStyle(new Color("2f3f38"), new Color("d6c07a")));

        var column = new VBoxContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        column.AddThemeConstantOverride("separation", 4);
        panel.AddChild(column);

        var label = new Label { Text = PlanLanes[lane], HorizontalAlignment = HorizontalAlignment.Center };
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeFontSizeOverride("font_size", 15);
        column.AddChild(label);

        for (var rank = 1; rank <= 5; rank++)
        {
            column.AddChild(CreateLaneCell(lane, rank));
        }

        return panel;
    }

    private Control CreateLaneCell(int lane, int rank)
    {
        var burned = burnedFromRank[lane] is not null && rank >= burnedFromRank[lane];
        var completed = rank <= laneProgress[lane];
        var cell = new Panel { CustomMinimumSize = new Vector2(1, 30) };
        cell.AddThemeStyleboxOverride(
            "panel",
            PanelStyle(
                burned ? new Color("3a2020") : completed ? CardTint(LaneColors[lane]) : new Color("1f2926"),
                burned ? new Color("d46a4a") : new Color("d6c07a")));

        var label = new Label
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            Text = burned ? "X" : completed ? $"{WesternSymbol(LaneColors[lane])}{rank}" : rank.ToString(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        label.AddThemeFontSizeOverride("font_size", 18);
        label.AddThemeColorOverride("font_color", burned ? new Color("ffb08f") : Colors.White);
        cell.AddChild(label);
        return cell;
    }

    private Button CreateActionButton(string text, bool enabled, Action pressed)
    {
        var button = new Button
        {
            Text = text,
            Disabled = !enabled,
            CustomMinimumSize = new Vector2(104, 36)
        };
        button.Pressed += pressed;
        return button;
    }

    private static Control CreatePile(string title, string value, Color color)
    {
        var pile = new Panel { CustomMinimumSize = new Vector2(108, 76) };
        pile.AddThemeStyleboxOverride("panel", PanelStyle(color, Colors.White));

        var labels = new VBoxContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        pile.AddChild(labels);

        var titleLabel = new Label { Text = title, HorizontalAlignment = HorizontalAlignment.Center };
        titleLabel.AddThemeColorOverride("font_color", Colors.White);
        labels.AddChild(titleLabel);

        var valueLabel = new Label { Text = value, HorizontalAlignment = HorizontalAlignment.Center };
        valueLabel.AddThemeFontSizeOverride("font_size", 24);
        valueLabel.AddThemeColorOverride("font_color", Colors.White);
        labels.AddChild(valueLabel);

        return pile;
    }

    private static Control CreateHintTokens(int available)
    {
        var grid = new GridContainer { Columns = 4 };
        grid.AddThemeConstantOverride("h_separation", 6);
        grid.AddThemeConstantOverride("v_separation", 6);

        for (var i = 0; i < MaxHintTokens; i++)
        {
            var token = new Panel { CustomMinimumSize = new Vector2(24, 24) };
            token.AddThemeStyleboxOverride(
                "panel",
                PanelStyle(i < available ? new Color("d2a532") : new Color("3a3328"), new Color("fff0a8")));
            grid.AddChild(token);
        }

        return grid;
    }

    private Control CreateShareCounter()
    {
        var label = new Label
        {
            Text = $"Доля\n{partnerShare}/{playerShare}",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeFontSizeOverride("font_size", 16);
        return label;
    }

    private static StyleBoxFlat PanelStyle(Color background, Color border)
    {
        return new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            ContentMarginBottom = 6,
            ContentMarginLeft = 6,
            ContentMarginRight = 6,
            ContentMarginTop = 6
        };
    }

    private static int LaneFor(CardColor color)
    {
        return Array.IndexOf(LaneColors, color);
    }

    private static Color CardTint(CardColor color)
    {
        return color switch
        {
            CardColor.Red => new Color("c73e3a"),
            CardColor.Green => new Color("3f8f52"),
            CardColor.Blue => new Color("2f6fb8"),
            CardColor.Yellow => new Color("d2a532"),
            CardColor.White => new Color("e8e4d7"),
            _ => Colors.Gray
        };
    }

    private static string WesternSymbol(CardColor color)
    {
        return color switch
        {
            CardColor.Red => "R",
            CardColor.Green => "^",
            CardColor.Blue => "~",
            CardColor.Yellow => "$",
            CardColor.White => "*",
            _ => "?"
        };
    }

    private static string CardText(Card card)
    {
        return $"{WesternSymbol(card.Color)}{card.Rank}";
    }
}
