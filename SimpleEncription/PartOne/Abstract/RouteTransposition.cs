using ConsoleLibrary.ConsoleExtensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using static ConsoleLibrary.ConsoleExtensions.ConsoleTableExtension;

namespace SimpleEncription.PartOne.Abstract
{
    public abstract class RouteTransposition: WorkInvoker.Abstract.WorkBase
    {
        protected int[] TranslationColumn { get; private set; }
        protected int[] TranslationRow { get; private set; }
        protected abstract RouteTranspositionType Type { get; }

        public override async Task Start(CancellationToken token)
        {
            Console.SetDecorationOneElem();
            await Console.WriteLine("Если в матрицу текст не поместиться он будет автоматически обрезан");
            var (countRows, countColumn) = await GetMatrixSize(token);
            int maxCountChars = countRows * countColumn;
            await Console.WriteLine($"Данный размер матрицы обеспечивает работу с {maxCountChars}");
            string text = await Console.ReadLine("Введите текст", token: token);
            if (text.Length != maxCountChars)
            {
                if(Type == RouteTranspositionType.Encryption)
                {
                    text = EditInputText(text, maxCountChars);
                    await Console.WriteLine($"Изменён текст: {text}");
                }
                else
                {
                    await Console.WriteLine("Ошибка: некорректная длина текста при расшифровке", ConsoleIOExtension.TextStyle.IsTitle | ConsoleIOExtension.TextStyle.IsError);
                    return;
                }
            }
            await LoadTranslationdata(token, countColumn, countRows);
            await TextTranslate(text, token);
        }
        
        private async Task TextTranslate(string text, CancellationToken token)
        {
            Console.StartCollectionDecorate();
            char[] chars = new char[TranslationRow.Length * TranslationColumn.Length];
            for (int r = 0; r < TranslationRow.Length; r++)
            {
                for (int c = 0; c < TranslationColumn.Length; c++)
                {
                    int indexOriginal = r * TranslationColumn.Length + c;
                    if (indexOriginal < text.Length)
                    {
                        int indexTo = TranslationRow[r] * TranslationColumn.Length + TranslationColumn[c];
                        chars[indexTo] = text[indexOriginal];
                    }
                    else
                    {
                        r = int.MaxValue - 1;
                        break;
                    }
                }
            }
            await Console.WriteLine("Результат", ConsoleIOExtension.TextStyle.IsTitle);
            await Console.ReadLine("Преобразованный текст", defaultValue: new string(chars), token: token);
        }    
        private string EditInputText(string text, int length)
        {
            if (text.Length < length)
            {
                char[] chars = new char[length];
                for (int i = 0; i < chars.Length; i++)
                    chars[i] = text[i % text.Length];
                return new string(chars);
            }
            else
                return text.Substring(0, length);
        }
        private IEnumerable<NodeTranslation> GenerateNodeTranslation(string key, int size)
        {
            if (string.IsNullOrEmpty(key)) key = ((char)0).ToString();
            HashSet<char> unique = new HashSet<char>();
            char lastVal = char.MinValue;
            for (int i = 0; i < size; i++)
            {
                char currentChar = key[i % key.Length];
                if (!unique.Add(currentChar))
                {
                    for (; true; lastVal++)
                    {
                        if (!key.Contains(lastVal))
                        {
                            unique.Add(lastVal);
                            lastVal++;
                            break;
                        }
                    }
                }
            }
            var data = unique.Select((x, i) => (x, i)).OrderBy(x => x.x).Select((x, i) => (i, x.i, x.x));
            return (Type == RouteTranspositionType.Encryption ? data.Select(x => new NodeTranslation(key[x.Item2 % key.Length], x.x, x.Item1, x.Item2)) : data.Select(x => new NodeTranslation(key[x.Item2 % key.Length], x.x, x.Item2, x.Item1))).OrderBy(x => x.InitialIndex);
        }
        private async Task LoadTranslationdata(CancellationToken token, int countColumn, int countRows)
        {
            Console.StartCollectionDecorate();
            string keyColumn = await Console.ReadLine("Введите первый ключ - столбец (пустая строка отменит применения ключа)", token: token, defaultValue: "987654321");
            string keyRow = await Console.ReadLine("Введите второй ключ - строка (пустая строка отменит применения ключа)", token: token, defaultValue: "987654321");

            IEnumerable<NodeTranslation> translationColumn = GenerateNodeTranslation(keyColumn, countColumn);
            IEnumerable<NodeTranslation> translationRow = GenerateNodeTranslation(keyRow, countRows);

            await Console.DrawTableUseGrid(GetTableDataUseTextKey(translationColumn, translationRow));

            TranslationColumn = translationColumn.Select(x => x.TranslateIndex).ToArray();
            TranslationRow = translationRow.Select(x => x.TranslateIndex).ToArray();
        }
        private IEnumerable<ViewInsertFullInfo> GetTableDataUseTextKey(IEnumerable<NodeTranslation> translatioColumn, IEnumerable<NodeTranslation> translatioRow)
        {
            List<ViewInsertFullInfo> tableData = new List<ViewInsertFullInfo>()
            {
                new ViewInsertFullInfo()
                {
                    Column = 0,
                    Row = 0,
                    ViewInsertSpan = new ViewInsertSpanInfo()
                    {
                        ColumnSpan = 4,
                        SetViewAutoDetect = "Столбец"
                    }
                },
                new ViewInsertFullInfo()
                {
                    Column = 4,
                    Row = 0,
                    ViewInsertSpan = new ViewInsertSpanInfo()
                    {
                        ColumnSpan = 4,
                        SetViewAutoDetect = "Строка"
                    }
                },
            };
            void addTableTranslations(IEnumerable<NodeTranslation> nodeTranslations, int columnOffset)
            {
                tableData.Add(new ViewInsertFullInfo()
                {
                    Column = 0 + columnOffset,
                    Row = 1,
                    ViewInsertSpan = "Char"
                });
                tableData.Add(new ViewInsertFullInfo()
                {
                    Column = 1 + columnOffset,
                    Row = 1,
                    ViewInsertSpan = "Id"
                });
                tableData.Add(new ViewInsertFullInfo()
                {
                    Column = 2 + columnOffset,
                    Row = 1,
                    ViewInsertSpan = "Индекс элемента"
                });
                tableData.Add(new ViewInsertFullInfo()
                {
                    Column = 3 + columnOffset,
                    Row = 1,
                    ViewInsertSpan = "Транслируется в индекс"
                });
                int rowOffset = 2;
                foreach (var node in nodeTranslations)
                {
                    tableData.Add(new ViewInsertFullInfo()
                    {
                        Column = 0 + columnOffset,
                        Row = rowOffset,
                        ViewInsertSpan = $"'{node.InputCharKey}' -> {(uint)node.InputCharKey}"
                    });
                    tableData.Add(new ViewInsertFullInfo()
                    {
                        Column = 1 + columnOffset,
                        Row = rowOffset,
                        ViewInsertSpan = $"'{node.EditCharKey}' -> {(uint)node.EditCharKey}"
                    });
                    tableData.Add(new ViewInsertFullInfo()
                    {
                        Column = 2 + columnOffset,
                        Row = rowOffset,
                        ViewInsertSpan = node.InitialIndex.ToString()
                    });
                    tableData.Add(new ViewInsertFullInfo()
                    {
                        Column = 3 + columnOffset,
                        Row = rowOffset,
                        ViewInsertSpan = node.TranslateIndex.ToString()
                    });
                    rowOffset++;
                }
            }
            addTableTranslations(translatioColumn, 0);
            addTableTranslations(translatioRow, 4);
            return tableData;
        }
        private async Task<(int countRows, int countColumn)> GetMatrixSize(CancellationToken token)
        {
            var matrixSize = await Console.ReadArrayInt("Введите размер матрицы ('Строка'x'Столбец') 5x5", startRange: 1, count: 2, separator: 'x', defaultsValue: new int[] { 5, 5 }, token: token);
            return (matrixSize[0], matrixSize[1]);
        }
        public enum RouteTranspositionType : byte
        {
            /// <summary>
            /// Шифрование
            /// </summary>
            Encryption,
            /// <summary>
            /// Расшифровка
            /// </summary>
            Decryption
        }
        private struct NodeTranslation
        {
            public char InputCharKey { get; private set; }
            public char EditCharKey { get; private set; }
            public int InitialIndex { get; private set; }
            public int TranslateIndex { get; private set; }

            public NodeTranslation(char inputCharKey, char editCharKey, int initialIndex, int translateIndex)
            {
                InputCharKey=inputCharKey;
                EditCharKey=editCharKey;
                InitialIndex=initialIndex;
                TranslateIndex=translateIndex;
            }
        }
    }
}
