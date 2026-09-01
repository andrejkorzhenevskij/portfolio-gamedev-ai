using Hanabi.Core;

internal static class TableMessages
{
    private static readonly string[] Steps = ["первый", "второй", "третий", "четвёртый", "пятый"];
    private static readonly string[] EnglishSteps = ["first", "second", "third", "fourth", "fifth"];
    private static readonly string[] StepsAt = ["первом", "втором", "третьем", "четвёртом", "пятом"];

    public static string ChooseAction => GameText.Pick("Выберите карту или действие.", "Choose a card or an action.");
    public static string Waiting => GameText.Pick("Ожидание.", "Waiting.");
    public static string CardSelected => GameText.Pick("Карта выбрана.", "Card selected.");
    public static string AwaitingAction => GameText.Pick("Ждём вашего решения.", "Your move.");
    public static string PlayerDiscarded => GameText.Pick("Вы отправили карту в отбой.", "You tossed a card into the discard pile.");
    public static string PlayerHandRefilled => GameText.Pick("Рука снова полна.", "Your hand is full again.");
    public static string PartnerDiscarded => GameText.Pick("Партнёр отправил карту в отбой.", "Your partner tossed a card.");
    public static string PartnerHandRefilled => GameText.Pick("Он добрал новую карту.", "He drew a new card.");
    public static string CardsMarked => GameText.Pick("Подходящие карты отмечены.", "Every matching card is marked.");
    public static string AdviceRefused => GameText.Pick("Партнёр промолчал.", "Your partner kept quiet.");
    public static string NoGoodMove => GameText.Pick("Хорошего хода он не видит.", "He can't see a good move.");
    public static string PartnerSuggestedCard => GameText.Pick("Партнёр указал на одну из ваших карт.", "Your partner pointed to one of your cards.");
    public static string ChoosePartnerCard => GameText.Pick("Укажите карту или велите ему не ходить.", "Pick a card, or tell him to hold.");
    public static string PartnerRefusedHint => GameText.Pick("Партнёр решил промолчать.", "Your partner decided to keep quiet.");
    public static string PartnerDiscardedInstead => GameText.Pick("Вместо подсказки он сбросил карту.", "He discarded a card instead of giving a hint.");
    public static string NoAdvice => GameText.Pick("Ничего не подскажу", "I got nothing for you");
    public static string LogsGathered => GameText.Pick("Записи собраны.", "Notes gathered.");

    public static string Played(bool partner, string plan, int rank)
    {
        return GameText.English
            ? partner
                ? $"Your partner laid down the {EnglishStep(rank)} step of \"{plan}\"."
                : $"You laid down the {EnglishStep(rank)} step of \"{plan}\"."
            : partner
                ? $"Партнёр дал {Step(rank)} шаг в плане «{plan}»."
                : $"Вы дали {Step(rank)} шаг в плане «{plan}».";
    }

    public static string PlayResult(string plan, int rank, bool success, bool gameOver)
    {
        if (success)
        {
            return GameText.Pick("Всё идёт по плану.", "Everything is going according to plan.");
        }

        return GameText.English
            ? gameOver
                ? $"The \"{plan}\" plan broke down on step {rank}. The job is over."
                : $"The \"{plan}\" plan broke down on step {rank}."
            : gameOver
                ? $"План «{plan}» сорван на {StepAt(rank)} шаге. Дело провалено."
                : $"План «{plan}» сорван на {StepAt(rank)} шаге.";
    }

    public static string Hint(bool partner, CardColor? color, int? rank)
    {
        var actor = partner
            ? GameText.Pick("Партнёр подсказал", "Your partner hinted")
            : GameText.Pick("Вы подсказали", "You hinted");
        return color is not null
            ? GameText.Pick($"{actor} цвет: {ColorName(color.Value)}.", $"{actor} a color: {ColorName(color.Value)}.")
            : GameText.Pick($"{actor} ранг: {rank}.", $"{actor} a rank: {rank}.");
    }

    public static string AdviceGiven(int cost)
    {
        return GameText.Pick($"Партнёр дал совет за {cost}% доли.", $"Your partner sold you advice for {cost}% of the take.");
    }

    public static string PartnerAsksAdvice(int cost)
    {
        return GameText.Pick($"Партнёр просит совета за {cost}% доли.", $"Your partner wants advice for {cost}% of the take.");
    }

    public static string LootAmount(int share)
    {
        var dollars = share * 250;
        return dollars < 1000 ? $"${dollars}" : $"${dollars / 1000} {dollars % 1000:000}";
    }

    public static string LootReply(int share)
    {
        return GameText.English
            ? share switch
            {
                <= 0 => "You won't see a cent. You will stay alive. We leaving anyway?",
                <= 15 => "Well... enough for a house, a horse, and time to lie low. You sure?",
                <= 30 => "Land, a house, a small herd. Call it a deal?",
                <= 45 => "That's a real ranch. No more hiring yourself out to other men.",
                <= 60 => "A new life: land, cattle, men on your payroll. We stop here?",
                <= 75 => "With that kind of haul, you can disappear and come back owning a fine ranch.",
                <= 90 => "Near enough a fortune. Anything more, and we're not risking it for the money.",
                _ => "You're taking near all of it. He'll remember that. We leaving anyway?"
            }
            : share switch
            {
                <= 0 => "Тебе не достанется ни цента. Зато останешься жив. Всё равно уходим?",
                <= 15 => "Ну... хватит на домик, лошадь и время залечь на дно. Уверен?",
                <= 30 => "Купишь землю, дом и небольшое стадо. На том и порешим?",
                <= 45 => "Это уже настоящее ранчо. Можно больше не наниматься к чужим.",
                <= 60 => "Новая жизнь: земля, скот и люди в найме. Останавливаемся?",
                <= 75 => "С таким кушем можно исчезнуть и объявиться хозяином большого ранчо.",
                <= 90 => "Почти состояние. Дальше мы рискуем уже не ради денег.",
                _ => "Ты забираешь почти всё. Он это запомнит. Всё равно уходим?"
            };
    }

    public static string LootEnding(int share)
    {
        return GameText.English
            ? share switch
            {
                <= 0 => "Not a cent to your name. But you made it out alive.",
                <= 15 => "Enough for a house, a horse, and time to lie low.",
                <= 30 => "Enough for land, a house, and a small herd.",
                <= 45 => "A real ranch. No more hiring yourself out to other men.",
                <= 60 => "A new life: land, cattle, men on your payroll.",
                <= 75 => "Enough to disappear and come back owning a fine ranch.",
                <= 90 => "Near enough a fortune. Anything more would not have been about the money.",
                _ => "You took near all of it. He'll remember that."
            }
            : share switch
            {
                <= 0 => "Ни цента за душой. Зато ты выбрался живым.",
                <= 15 => "Хватит на домик, лошадь и время залечь на дно.",
                <= 30 => "Хватит на землю, дом и небольшое стадо.",
                <= 45 => "Настоящее ранчо. Больше не придётся наниматься к чужим.",
                <= 60 => "Новая жизнь: земля, скот и люди в найме.",
                <= 75 => "С таким кушем можно исчезнуть и объявиться хозяином большого ранчо.",
                <= 90 => "Почти состояние. Дальше мы рисковали бы уже не ради денег.",
                _ => "Ты забрал почти всё. Он это запомнит."
            };
    }

    public static string LogSaved(string path)
    {
        return GameText.Pick($"Записи сохранены в {path}.", $"Notes saved to {path}.");
    }

    private static string Step(int rank) => Steps[Math.Clamp(rank, 1, 5) - 1];

    private static string EnglishStep(int rank) => EnglishSteps[Math.Clamp(rank, 1, 5) - 1];

    private static string StepAt(int rank) => StepsAt[Math.Clamp(rank, 1, 5) - 1];

    private static string ColorName(CardColor color)
    {
        return color switch
        {
            CardColor.Red => GameText.Pick("красный", "red"),
            CardColor.Green => GameText.Pick("зелёный", "green"),
            CardColor.Blue => GameText.Pick("синий", "blue"),
            CardColor.Yellow => GameText.Pick("жёлтый", "yellow"),
            CardColor.White => GameText.Pick("белый", "white"),
            _ => GameText.Pick("неизвестный", "unknown")
        };
    }
}
