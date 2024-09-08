

Random random = new Random();
Dictionary<string, int> stocks = new Dictionary<string, int>
		{
			{ "AAPL", random.Next(100, 500) },
			{ "GOOGL", random.Next(100, 500) },
			{ "MSFT", random.Next(100, 500) },
			{ "AMZN", random.Next(100, 500) },
			{ "TSLA", random.Next(100, 500) }
		};

Dictionary<string, int> playerStocks = new Dictionary<string, int>
		{
			{ "AAPL", 0 },
			{ "GOOGL", 0 },
			{ "MSFT", 0 },
			{ "AMZN", 0 },
			{ "TSLA", 0 }
		};

Dictionary<string, int> totalSpent = new Dictionary<string, int>
		{
			{ "AAPL", 0 },
			{ "GOOGL", 0 },
			{ "MSFT", 0 },
			{ "AMZN", 0 },
			{ "TSLA", 0 }
		};

Dictionary<string, List<int>> priceHistory = new Dictionary<string, List<int>>
		{
			{ "AAPL", new List<int>() },
			{ "GOOGL", new List<int>() },
			{ "MSFT", new List<int>() },
			{ "AMZN", new List<int>() },
			{ "TSLA", new List<int>() }
		};

int balance = 30000;
int totalTurns = 300;
int currentTurn = 0;
int targetBalance = 1000000;
bool running = true;
int selectedIndex = 0;
string[] menuItems = { "Купить акции", "Продать акции", "Конец хода", "Выйти" };

while (running)
{
	Console.Clear();
	Console.WriteLine($"Ход {currentTurn}/{totalTurns}");
	Console.WriteLine("Добро пожаловать на биржу!");
	Console.WriteLine($"Ваш баланс: ${balance}");
	Console.WriteLine("Текущие цены на акции:");

	foreach (var stock in stocks)
	{
		double averageCost = playerStocks[stock.Key] > 0 ? (double)totalSpent[stock.Key] / playerStocks[stock.Key] : 0;
		bool hasDividends = priceHistory[stock.Key].Count == 10 && priceHistory[stock.Key].Average() < stocks[stock.Key];
		string dividendsInfo = hasDividends ? " | Дивиденды: Да" : " | Дивиденды: Нет";

		Console.Write($"{stock.Key}: $");

		ConsoleColor color = ConsoleColor.White;
		if (priceHistory[stock.Key].Count > 1)
		{
			int previousPrice = priceHistory[stock.Key][priceHistory[stock.Key].Count - 2];
			int currentPrice = stocks[stock.Key];

			if (currentPrice > previousPrice)
			{
				color = ConsoleColor.Green;
			}
			else if (currentPrice < previousPrice)
			{
				color = ConsoleColor.Red;
			}
		}

		Console.ForegroundColor = color;
		Console.Write($"{stocks[stock.Key]}");
		Console.ResetColor();

		Console.WriteLine($" | У вас: {playerStocks[stock.Key]} акций | Средняя стоимость: ${averageCost:F2}{dividendsInfo}");
	}

	// Добавляем отображение стоимости портфеля
	decimal portfolioValue = CalculatePortfolioValue(playerStocks, stocks);
	Console.WriteLine($"\nСтоимость вашего портфеля: ${portfolioValue:F2}");

	Console.WriteLine("\nВыберите действие:");

	for (int i = 0; i < menuItems.Length; i++)
	{
		if (i == selectedIndex)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"> {menuItems[i]}");
			Console.ResetColor();
		}
		else
		{
			Console.WriteLine($"  {menuItems[i]}");
		}
	}

	var key = Console.ReadKey(true).Key;

	switch (key)
	{
		case ConsoleKey.UpArrow:
			selectedIndex = (selectedIndex == 0) ? menuItems.Length - 1 : selectedIndex - 1;
			break;
		case ConsoleKey.DownArrow:
			selectedIndex = (selectedIndex == menuItems.Length - 1) ? 0 : selectedIndex + 1;
			break;
		case ConsoleKey.Enter:
			switch (selectedIndex)
			{
				case 0:
					SelectStock(stocks, playerStocks, totalSpent, ref balance, true);
					break;
				case 1:
					SelectStock(stocks, playerStocks, totalSpent, ref balance, false);
					break;
				case 2:
					UpdateStockPrices(stocks, priceHistory, random);
					PayDividends(stocks, playerStocks, priceHistory, ref balance);
					currentTurn++; // Увеличиваем счетчик ходов только при завершении хода
					break;
				case 3:
					running = false;
					break;
			}
			break;
	}

	if (currentTurn >= totalTurns)
	{
		running = false;
	}
}

if (balance >= targetBalance)
{
	Console.WriteLine("Поздравляем! Вы достигли цели и заработали 1 миллион долларов!");
}
else
{
	Console.WriteLine("Игра окончена. Вы не достигли цели.");
}

Console.WriteLine("Спасибо за игру!");






static void SelectStock(Dictionary<string, int> stocks, Dictionary<string, int> playerStocks, Dictionary<string, int> totalSpent, ref int balance, bool isBuying)
{
	int selectedIndex = 0;
	string[] stockSymbols = stocks.Keys.ToArray();

	while (true)
	{
		Console.Clear();
		Console.WriteLine("Выберите акцию:");

		for (int i = 0; i < stockSymbols.Length; i++)
		{
			if (i == selectedIndex)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"> {stockSymbols[i]}");
				Console.ResetColor();
			}
			else
			{
				Console.WriteLine($"  {stockSymbols[i]}");
			}
		}

		var key = Console.ReadKey(true).Key;

		switch (key)
		{
			case ConsoleKey.UpArrow:
				selectedIndex = (selectedIndex == 0) ? stockSymbols.Length - 1 : selectedIndex - 1;
				break;
			case ConsoleKey.DownArrow:
				selectedIndex = (selectedIndex == stockSymbols.Length - 1) ? 0 : selectedIndex + 1;
				break;
			case ConsoleKey.Enter:
				if (isBuying)
				{
					BuyStocks(stocks, playerStocks, totalSpent, ref balance, stockSymbols[selectedIndex]);
				}
				else
				{
					SellStocks(stocks, playerStocks, totalSpent, ref balance, stockSymbols[selectedIndex]);
				}
				return;
			case ConsoleKey.Escape:
				return;
		}
	}
}



static void BuyStocks(Dictionary<string, int> stocks, Dictionary<string, int> playerStocks, Dictionary<string, int> totalSpent, ref int balance, string symbol)
{
	Console.Clear();
	int maxQuantity = balance / stocks[symbol];
	Console.WriteLine($"Сколько акций {symbol} вы хотите купить? (Максимум: {maxQuantity})");
	int quantity = int.Parse(Console.ReadLine());
	int cost = stocks[symbol] * quantity;

	if (balance >= cost)
	{
		balance -= cost;
		playerStocks[symbol] += quantity;
		totalSpent[symbol] += cost;
		Console.WriteLine($"Вы купили {quantity} {GetStockWord(quantity)} {symbol} за ${cost}.");
	}
	else
	{
		Console.WriteLine("Недостаточно средств.");
	}
	Console.WriteLine("Нажмите любую клавишу, чтобы продолжить.");
	Console.ReadKey();
}



static void SellStocks(Dictionary<string, int> stocks, Dictionary<string, int> playerStocks, Dictionary<string, int> totalSpent, ref int balance, string symbol)
{
	if (playerStocks[symbol] > 0)
	{
		Console.Clear();
		Console.WriteLine($"Сколько акций {symbol} вы хотите продать?");
		int quantity = int.Parse(Console.ReadLine());

		if (playerStocks[symbol] >= quantity)
		{
			int revenue = stocks[symbol] * quantity;
			balance += revenue;
			playerStocks[symbol] -= quantity;
			totalSpent[symbol] -= (int)(totalSpent[symbol] * ((double)quantity / (playerStocks[symbol] + quantity)));
			Console.WriteLine($"Вы продали {quantity} {GetStockWord(quantity)} {symbol} за ${revenue}.");
		}
		else
		{
			Console.WriteLine("У вас недостаточно акций для продажи.");
		}
	}
	else
	{
		Console.WriteLine("У вас нет акций для продажи.");
	}
	Console.WriteLine("Нажмите любую клавишу, чтобы продолжить.");
	Console.ReadKey();
}


static void UpdateStockPrices(Dictionary<string, int> stocks, Dictionary<string, List<int>> priceHistory, Random random)
{
	foreach (var stock in stocks.Keys.ToList())
	{
		int previousPrice = stocks[stock];
		int change = random.Next(-10, 11);
		if (random.Next(0, 10) == 0) // 10% шанс на резкое изменение
		{
			change = random.Next(-50, 51);
		}

		stocks[stock] = Math.Max(1, stocks[stock] + change); // Цена не может быть меньше 1
		priceHistory[stock].Add(stocks[stock]);

		if (priceHistory[stock].Count > 10)
		{
			priceHistory[stock].RemoveAt(0);
		}
	}
}



static void PayDividends(Dictionary<string, int> stocks, Dictionary<string, int> playerStocks, Dictionary<string, List<int>> priceHistory, ref int balance)
{
	foreach (var stock in stocks.Keys)
	{
		if (priceHistory[stock].Count == 10 && priceHistory[stock].Average() < stocks[stock])
		{
			int dividend = (int)(stocks[stock] * playerStocks[stock] * 0.01); // Дивиденды 1% от стоимости акций
			balance += dividend;
			Console.WriteLine($"Вы получили дивиденды в размере ${dividend} от {stock}.");
		}
	}
	Console.WriteLine("Дивиденды начислены. Нажмите любую клавишу, чтобы продолжить.");
	Console.ReadKey();
}

static decimal CalculatePortfolioValue(Dictionary<string, int> playerStocks, Dictionary<string, int> stocks)
{
	decimal totalValue = 0;
	foreach (var stock in playerStocks)
	{
		totalValue += stock.Value * stocks[stock.Key];
	}
	return totalValue;
}

static string GetStockWord(int quantity)
{
	if (quantity % 100 >= 11 && quantity % 100 <= 19)
	{
		return "акций";
	}
	switch (quantity % 10)
	{
		case 1:
			return "акция";
		case 2:
		case 3:
		case 4:
			return "акции";
		default:
			return "акций";
	}
}

