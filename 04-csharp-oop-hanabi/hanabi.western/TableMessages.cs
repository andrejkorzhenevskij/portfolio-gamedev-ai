using Hanabi.Core;

internal static class TableMessages
{
    private static readonly string[] Steps = ["первый", "второй", "третий", "четвёртый", "пятый"];
    private static readonly string[] StepsAt = ["первом", "втором", "третьем", "четвёртом", "пятом"];

    public const string ChooseAction = "Выберите карту или действие.";
    public const string Waiting = "Ожидание.";
    public const string CardSelected = "Карта выбрана.";
    public const string AwaitingAction = "Ждём вашего решения.";
    public const string PlayerDiscarded = "Вы отправили карту в отбой.";
    public const string PlayerHandRefilled = "Рука снова полна.";
    public const string PartnerDiscarded = "Партнёр отправил карту в отбой.";
    public const string PartnerHandRefilled = "Он добрал новую карту.";
    public const string CardsMarked = "Подходящие карты отмечены.";
    public const string AdviceRefused = "Партнёр промолчал.";
    public const string NoGoodMove = "Хорошего хода он не видит.";
    public const string PartnerSuggestedCard = "Партнёр указал на одну из ваших карт.";
    public const string ChoosePartnerCard = "Укажите карту или велите ему не ходить.";
    public const string PartnerRefusedHint = "Партнёр решил промолчать.";
    public const string PartnerDiscardedInstead = "Вместо подсказки он сбросил карту.";
    public const string NoAdvice = "Ничего не подскажу";
    public const string LogsGathered = "Записи собраны.";

    public static string Played(bool partner, string plan, int rank)
    {
        return partner
            ? $"Партнёр дал {Step(rank)} шаг в плане «{plan}»."
            : $"Вы дали {Step(rank)} шаг в плане «{plan}».";
    }

    public static string PlayResult(string plan, int rank, bool success, bool gameOver)
    {
        if (success)
        {
            return "Всё идёт по плану.";
        }

        return gameOver
            ? $"План «{plan}» сорван на {StepAt(rank)} шаге. Дело провалено."
            : $"План «{plan}» сорван на {StepAt(rank)} шаге.";
    }

    public static string Hint(bool partner, CardColor? color, int? rank)
    {
        var actor = partner ? "Партнёр подсказал" : "Вы подсказали";
        return color is not null
            ? $"{actor} цвет: {ColorName(color.Value)}."
            : $"{actor} ранг: {rank}.";
    }

    public static string AdviceGiven(int cost)
    {
        return $"Партнёр дал совет за {cost}% доли.";
    }

    public static string PartnerAsksAdvice(int cost)
    {
        return $"Партнёр просит совета за {cost}% доли.";
    }

    public static string LootAmount(int share)
    {
        var dollars = share * 250;
        return dollars < 1000 ? $"${dollars}" : $"${dollars / 1000} {dollars % 1000:000}";
    }

    public static string LootReply(int share)
    {
        return share switch
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

    public static string LogSaved(string path)
    {
        return $"Записи сохранены в {path}.";
    }

    private static string Step(int rank) => Steps[Math.Clamp(rank, 1, 5) - 1];

    private static string StepAt(int rank) => StepsAt[Math.Clamp(rank, 1, 5) - 1];

    private static string ColorName(CardColor color)
    {
        return color switch
        {
            CardColor.Red => "красный",
            CardColor.Green => "зелёный",
            CardColor.Blue => "синий",
            CardColor.Yellow => "жёлтый",
            CardColor.White => "белый",
            _ => "неизвестный"
        };
    }
}
