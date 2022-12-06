using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ConsoleApp3
{
	class Program
	{
		static void Main(string[] args)
		{
			MainCore a = new MainCore();

			a.doPrintDebugMessange = false; // Если true - выводит в консоль текстовое сопровождение шагов поиска хороших символов

			a.ReadFile();
			a.printGrammar();
			a.startProcessDeletingChainRules();
		}

		public class MainCore
		{
			public List<string> NoTerminals = new List<string>();	// Нетерминалы
			public List<string> Terminals = new List<string>();		// Терминалы
			List<List<string>> Rules = new List<List<string>>();	// Правила
			public string Axioma;									// Начальный символ

			public List<string> GoodSimvols = new List<string>();	// Хорошие нетерминалы
			
			bool isProgrammEnd = false;     // Если true - то все рекурсивные вызовы завершаются
			bool isValPrint = false;        // Если true - то ответ больше не печатается
			public bool doPrintDebugMessange = true;   // Если true - выводит в консоль текстовое сопровождение шагов поиска хороших символов

			public int countDeletedCainRules = 0; // Кол-во удалённых цепных правил

			/*
				Например правило A -> BC, Bc, c;
				Будет выглядеть: Rules[n] = [["A", "BC", "Bc", "c"]], где n - это номер этого правила
				Лямбда - будет & 
			*/

			/*
				Определения:

				• Правило: A -> BC, Bc, c					// Как оно записывается на бумаге
				• Набор правила: ["A", "BC", "Bc", "c"]		// Как оно выглядит в массивах внутри программы
				• Элемент правила: A, BC, Bc, c

				• Условие правила: A						// То, с чего правила начинается
				• Исходы правила: BC, Bc, c					// То, что чледует из правила
			*/

			#region chainRules // Код, который удаляет цепные правила

			bool[] delRules; // Если delRules[i] == true, то правило удалено, и мы не рассматриваем его для замены элементов

			public void startProcessDeletingChainRules()
			{
				//changeElementRules(0);

				delRules = new bool[Rules.Count];

				for (int i = 0; i < Rules.Count; i++)
				{
					delRules[i] = false;
				}

				checkGramm();

				for (int i = 0; i < Rules.Count; i++) // Здесь перебираем все наборы правил
				{
					if (delRules[i] == false)
					{
						changeElementRules(i);
					}
				}

				if (isProgrammEnd == false)
				{
					cout();
					cout("Программа успешно удалила все цепные правила. Всего было удалено " + countDeletedCainRules + "шт. правил");
					cout();
					cout("-------");
					cout();

					overWritingRules(); // Удаляю удалённые правила из правил

					printNewRules();
				}
			}

			bool checkGramm() // Проверяет, нет ли в грамматике лямбда-правил
			{
				for (int i = 0; i < Rules.Count; i++)
				{
					for (int j = 1; j < Rules[i].Count; j++)
					{
						if (Rules[i][j] == "&")
						{
							cout("Ошибка грамматики: Найдено правило с лямбда-исходом");
							cout();
							cout("Алгоритм будет остановлен");
							cout("----------");
							isProgrammEnd = true;
							return false;
						}
					}
				}

				return true;
			}

			// Пока работает алгоримт changeElementRules, я не удаляю правила, а только помечаю их удалёнными
			// И уже здесь окончательно их удаляю
			void overWritingRules() 
			{
				List<List<string>> newRules = new List<List<string>>();

				for (int i = 0; i < Rules.Count; i++)
				{
					if (delRules[i] == false)
					{
						newRules.Add(Rules[i]);
					}
				}

				Rules = newRules;
			}

			// Заменяет элемент правила, если он является еденичным терминалом
			// Принимает на вход номер правила, которое будет обрабатывать
			bool changeElementRules(int i)
			{
				if ((isProgrammEnd == false) && (delRules[i] == false))
				{
					if (doPrintDebugMessange) cout(". Рассматриваем правило " + Rules[i][0] + " -> ...");
					for (int j = 1; j < Rules[i].Count; j++) // Здесь перебираем каждый элемент правила
					{
						if (Rules[i][j].Length == 1) // Если элемент правила имеет длинну = 1
						{
							if (doPrintDebugMessange) cout(".. Длинна элемента правила " + Rules[i][j] + " = 1");

							for (int k = 0; k < NoTerminals.Count; k++)
							{
								if (Rules[i][j] == NoTerminals[k]) // Если элемент правила равняетя еденичному терминалу
								{
									if (doPrintDebugMessange) cout("... Элемент правила " + Rules[i][j] + " равен нетерминалу " + NoTerminals[k]);

									for (int u = 0; u < Rules.Count; u++) // Здесь перебираем все наборы правил
									{
										if (Rules[u][0] == NoTerminals[k])
										{
											if (doPrintDebugMessange) cout(".... Правило " + Rules[u][0] + " -> ... начинается с нетерминала " + NoTerminals[k]);

											// Нашли правило, которе начинается с еденичного терминала, который мы нашли в элементе другого правила

											// • Удаляем рассматриваемый элемент правила из рассматриваемого правила

											string lookElemetnOfLookRules = Rules[i][j];
											Rules[i].RemoveAt(j);

											// • Добавляем все исходы найденного правила на индекс удалённого элемента

											for (int r = 1; r < Rules[u].Count; r++)
											{
												Rules[i].Insert(j, Rules[u][r]);
											}

											// • Удаляем найденное правило

											if (delRules[u] == false) countDeletedCainRules++;

											if (doPrintDebugMessange) cout(".... Удаляем правило " + Rules[u][0] + " -> ...");
											//Rules.RemoveAt(u);
											delRules[u] = true;

											// • Проверяем, не появилось ли рекурсии в рассматриваемом правиле. Если да - завершение программы

											for (int r = 1; r < Rules[i].Count; r++)
											{
												//cout("lookElemetnOfLookRules : " + lookElemetnOfLookRules + " != Rules[i][r] : " + Rules[i][r]);
												if (lookElemetnOfLookRules == Rules[i][r])
												{
													if (doPrintDebugMessange) cout("..... В изменённом правиле " + Rules[i][0] + 
														"-> ... обнаружена рекурсия элемента правила: " + Rules[i][r] + 
														" и удалённого элемента правила: " + lookElemetnOfLookRules);

													cout();
													cout("----------");
													cout("Ошибка грамматики: Найдено рекурсивное правило");
													cout();
													cout("Алгоритм будет остановлен");
													cout("----------");
													isProgrammEnd = true;
													return false;
												}

												if (Rules[i][0] == Rules[i][r])
												{
													if (doPrintDebugMessange) cout("..... В изменённом правиле " + Rules[i][0] +
														"-> ... обнаружена рекурсия начала: " + Rules[i][0] +
														" и его элемента: " + Rules[i][r]);

													cout();
													cout("----------");
													cout("Ошибка грамматики: Найдено рекурсивное правило");
													cout();
													cout("Алгоритм будет остановлен");
													cout("----------");
													isProgrammEnd = true;
													return false;
												}
											}											

											// • Если всё ок, то ещё раз рекуррентно посылаем на обработку в эту процедуру это правило

											changeElementRules(i);

											return true;
										}
									}
								}
							}
						}
					}
				}
				return false;
			}

			#endregion

			#region wordIsEmpty // Код, который проверяет, пуст ли язык [сейчас закомментирован]
			/*
            bool termWord = false;          // Если false - то язык пуст

			// Находит все терминалы в правилах , и отправляет их в Finder
			public void checkIsHaveTermOnRules()
			{
				for (int i = 0; i < Rules.Count; i++) // Здесь перебираем все наборы правил
				{
					for (int j = 0; j < Rules[i].Count; j++) // Здесь перебираем кадый элемент правила
					{
						for (int k = 0; k < Terminals.Count; k++) // Проходим по всем терминалам
						{
							bool isHaveNoTerminalSimvols = false;	
							// Если элемент правила состоит из нескольких смиволов, эта переменная будет = false только тогда, когда все эти символы - терминалы

							for (int u = 0; u < Rules[i][j].Length; u++)
							{
								for (int k1 = 0; k1 < NoTerminals.Count; k1++) // Проходим по всем нетерминалам
								{
									if (Rules[i][j][u] == NoTerminals[k1][0]) // Ищем нетерминал в нашем элемента правила
									{
										isHaveNoTerminalSimvols = true;
										break; 
									}
								}

								if (isHaveNoTerminalSimvols == false)
								{
									if (Rules[i][j][u] == Terminals[k][0]) // Сравниваем каждый элемент
									{
										// Нашли, что в каком-то правиле есть терминал

										string ruleCondition = Rules[i][0];

										if (GoodSimvols.IndexOf(Rules[i][0]) == -1) // Если его нет в хороших символах
										{ 
											GoodSimvols.Add(ruleCondition); //хорошие терминалы
										}

										if (doPrintDebugMessange) Console.WriteLine("При начальном обходе нашли терминал в правилах, это: " + Rules[i][j]);
										Finder(ruleCondition); // Отправляем его в Finder
									}
								}								
							}
						}
					}
				}
			}			

			// Получает на вход лексему, и ищет существует ли правило, из которого эту лексему можно получить
			// Если такое правило есть, то рекурсивно отправляет в самого себя это правило, в качестве входного значения
			// Если это найденное правило является аксиомой - то язык не пуст
			void Finder(string findCell)
			{
				if (isProgrammEnd == false)
				{
					if (doPrintDebugMessange) Console.WriteLine("Ищем элемент " + findCell + " во всех правилах");
					for (int i = 0; i < Rules.Count; i++)
					{
						for (int j = 1; j < Rules[i].Count; j++)
						{
							for (int u = 0; u < Rules[i][j].Length; u++)
							{
								// Вот тут, если совпал один символ элемента, и в элементе больше 1 символа								
								if (Rules[i][j][u].ToString() == findCell)	
								{
									bool isValueElement = true; // = true, если из каждого нетерминала элемента правила выводится терминальная цепочка

									if (Rules[i][j].Length > 1)
									{
										// То проверяем на то, что бы все остальные правила из элемента этого правила были в хороших символах
										for (int u2 = 0; u2 < Rules[i][j].Length; u2++)
										{
											if (u != u2)
                                            {
												if (NoTerminals.IndexOf(Rules[i][j][u2].ToString()) != -1) // Если рассматриваемый символ является нетерминалом
												{
													if (GoodSimvols.IndexOf(Rules[i][j][u2].ToString()) == -1) // Если его нет в хороших символах
													{
														isValueElement = false;
														if (doPrintDebugMessange) coutnn("В элементе правила есть нетерминалы, которых нет в хороших символах: "
															+ Rules[i][j][u2].ToString());
														break;
													}
												}
                                            }
                                        }
									}

									if (isValueElement == true)
									{
										if (GoodSimvols.IndexOf(Rules[i][0]) == -1)
										{
											if (doPrintDebugMessange) Console.Write("Нашли. ");
											if (doPrintDebugMessange) Console.WriteLine("Начало правила: " + Rules[i][0] + ". Теперь ищем правило, из которого этот элемент получался бы");
											GoodSimvols.Add(Rules[i][0]);
											Finder(Rules[i][0]);
										}
										else
										{
											if (doPrintDebugMessange) coutnn("Обнаружено рекурсивное правило: " + Rules[i][0] + " -> ...");
										}
									}
								}
							}
						}
					}
				}
			}

			// Ищет во множестве хороших смиволов Аксимоу
			void isLangEmpty()
			{
				for (int i = 0; i < GoodSimvols.Count; i++)
				{
					if (GoodSimvols[i] == Axioma)
					{
						cout("В множестве хороших смиволов нашли аксиому. Значит язык не пуст");
						termWord = true;
						break;
					}
				}
			}

			// Печатает, пуст ли язык
			public void printCanBelanguage()
			{
				if (isValPrint == false)
				{
					isLangEmpty();
					cout(" ");
					isValPrint = true;
					if (termWord == true) cout("--- Язык не пуст ---");
					else cout("--- Язык пуст ---");
				}
			}
			*/

			#endregion

			#region ReadFile // Код, который считывает грамматику из файла
			public void ReadFile()
			{
				string curfile = @"Grammar.txt";

				if (File.Exists(curfile)) //проверяем открыт ли файл
				{
					StreamReader file = new StreamReader("Grammar.txt"); // говорим, что переменная file теперь ссылается на наш файл

					string value; //терминал или нетерминал
					value = ""; //нужно, чтобы коректно работал код			
					int j = 0; //для поиска *
					string finish = "^"; // стопсивол
					string litter = "*"; //разделитель
					bool theend = true; //чтобы цикл while останавливался
					List<string> expressions; //каждое новое правило сначала заноситься в промежуточный лист

					Axioma = file.ReadLine(); //первая строка начальный символ
					string buff = file.ReadLine(); //вторая строка файла нетерминалы

					while (theend) //пока не нашли ^ и не дошли до конца
					{

						while (buff[j] != litter[0] && buff[j] != finish[0]) j++; //отмеряем размер строки до *
						if (buff[j] == litter[0])
						{
							value = buff.Substring(0, j); //копируем строку до * в отдельную переменную
							NoTerminals.Add(value); //добавляет строку в лист
							buff = buff.Remove(0, j + 1); //удаляем строку вместе с *
							j = 0; //начинаем с начала строки
							theend = true;

						}
						else if (buff[j] == finish[0]) //нашли стопсимвол и выходим из цикла
						{
							theend = false;
						}
						else
						{
							Console.WriteLine("Something went wrong while reading the first line");
							theend = false; // в любой непонятной ситуайии выходим из цикла
						}
					}
					int size_buff = buff.Length; //осталась последняя строка
					value = buff.Substring(0, size_buff - 1); //копируем последнюю строку
					NoTerminals.Add(value); //добавляем строку в лист

					buff = file.ReadLine(); //читаем третью строку файла. терминалы
					theend = true; //запускаем цикл

					while (theend) //пока не нашли ^ и не дошли до конца
					{
						while (buff[j] != litter[0] && buff[j] != finish[0]) j++; //отмеряем размер строки до *
						if (buff[j] == litter[0])
						{
							value = buff.Substring(0, j); //копируем строку до * в отдельную переменную
							Terminals.Add(value); //добавляет строку в лист
							buff = buff.Remove(0, j + 1); //удаляем строку вместе с *
							j = 0; //начинаем с начала строки
							theend = true;

						}
						else if (buff[j] == finish[0]) //нашли стопсимвол и выходим из цикла
						{
							theend = false;
						}
						else
						{
							Console.WriteLine("Something went wrong while reading the second line");
							theend = false; // в любой непонятной ситуайии выходим из цикла
						}
					}
					size_buff = buff.Length; //осталась последняя строка
					value = buff.Substring(0, size_buff - 1); //копируем последнюю строку
					Terminals.Add(value); //добавляем строку в лист

					while (!file.EndOfStream) //пока не конец файла
					{
						buff = file.ReadLine(); //читаем строку. правиа
						theend = true; //запускаем цикл
						expressions = new List<string>(); //создаем новую строку таблицы листов

						while (theend) //пока не нашли ^
						{
							while (buff[j] != litter[0] && buff[j] != finish[0]) j++; //отмеряем размер строки до *
							if (buff[j] == litter[0]) //нашли разделяющий символ
							{
								value = buff.Substring(0, j); //копируем строку до * в отдельную переменную
								expressions.Add(value); //добавляет строку в лист
								buff = buff.Remove(0, j + 1); //удаляем строку вместе с *							
								theend = true;
							}
							else if (buff[j] == finish[0]) //нашли стопсимвол и выходим из цикла
							{
								theend = false;
							}
							else
							{
								Console.WriteLine("something went wrong while reading the Rules");
								theend = false; // в любой непонятной ситуайии выходим из цикла
							}
							j = 0; //начинаем с начала строки
						}
						size_buff = buff.Length; //осталась последняя строка
						value = buff.Substring(0, size_buff - 1); //копируем последнюю строку
						expressions.Add(value); //добавляем строку в лист
						Rules.Add(expressions);
					}

					file.Close(); //закрываем файл
					cout("Данные успешно загружены из внешнего файла.");
				}
				else
				{
					cout("При загрузке данных из внешнего файла произошла ошибка. Проверьте, находится ли файл " + curfile + " в директории программы.");
				}
				cout();
			}

            #endregion

            public static void cout<Type>(Type Input)
			{
				Console.WriteLine(Input);
			}

			public static void cout()
			{
				Console.WriteLine(" ");
			}

			public static void coutnn<Type>(Type Input)
			{
				Console.WriteLine(Input);
			}

			// Выводит в консоль Нетерминалы, Терминалы, Правила и Аксиому
			public void printGrammar()
			{
				cout("Нетерминалы: ");

				for (int i = 0; i < NoTerminals.Count; i++)
				{
					if (i > 0) Console.Write(", ");
					Console.Write(NoTerminals[i]);
				}
				Console.WriteLine();

				cout();
				cout("Терминалы: ");

				for (int i = 0; i < Terminals.Count; i++)
				{
					if (i > 0) Console.Write(", ");
					Console.Write(Terminals[i]);
				}
				Console.WriteLine();

				cout();
				cout("Правила вывода: ");

				for (int i = 0; i < Rules.Count; i++) // Чуть сложного кода для красивого вывода
				{
					Console.Write("    " + Rules[i][0] + " -> ");
					for (int j = 1; j < Rules[i].Count; j++) 
					{
						if (j > 1) Console.Write(", ");
						Console.Write(Rules[i][j]);
					}
					Console.WriteLine(" ");
				}

				cout();
				cout("Аксиома: ");

				Console.WriteLine(Axioma);
				cout();
			}

			public void printNewRules() // Выводит в консоль только правила
			{
				cout("Новые правила вывода: ");

				for (int i = 0; i < Rules.Count; i++) // Чуть сложного кода для красивого вывода
				{
					Console.Write("    " + Rules[i][0] + " -> ");
					for (int j = 1; j < Rules[i].Count; j++)
					{
						if (j > 1) Console.Write(", ");
						Console.Write(Rules[i][j]);
					}
					Console.WriteLine(" ");
				}

				cout();
			}

			public void printGoodSimvols()
			{
				cout();
				cout("Выводим все хорошие смиволы, которые нашли:");
				cout();
				//Console.WriteLine(this.GoodSimvols.Count);
				for (int i = 0; i < this.GoodSimvols.Count; i++)
				{
					if (i > 0) Console.Write(", ");
					Console.Write(this.GoodSimvols[i]);
				}
				Console.WriteLine();
			}
		}
	}
}
