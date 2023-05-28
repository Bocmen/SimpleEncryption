using ConsoleLibrary.ConsoleExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleEncription.PartTwo.Abstract
{
    public abstract class GammaEncryption: WorkInvoker.Abstract.WorkBase
    {
        static GammaEncryption() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        public abstract Task<BitArray> GetGamma(CancellationToken token, int textLength);
        private IEnumerable<byte> MaskCollection(BitArray bitArray)
        {
            byte[] xorMask = new byte[(int)Math.Ceiling(bitArray.Length / 8.0)];
            BitArrayToLength(ref bitArray, xorMask.Length * 8);
            bitArray.CopyTo(xorMask, 0);
            while (true)
                foreach (var value in xorMask)
                    yield return value;
        }
        private void BitArrayToLength(ref BitArray bitArray, int length)
        {
            if (bitArray.Length == length) return;
            int startLength = bitArray.Length;
            bitArray.Length = length;
            int indexRead = 0;
            int currentIndex = startLength;
            while (currentIndex < length)
            {
                bitArray[currentIndex] = bitArray[indexRead];

                currentIndex++;
                indexRead++;
                if (indexRead >= startLength) indexRead = 0;
            }
        }
        public override async Task Start(CancellationToken token)
        {
            string text = await Console.ReadLine("Введите текст для шифрования", token: token, defaultValue: "А также некоторые особенности внутренней политики призывают нас к новым свершениям, которые, в свою очередь, должны быть представлены в исключительно положительном свете. Предварительные выводы неутешительны: синтетическое тестирование играет важную роль в формировании инновационных методов управления процессами. Предварительные выводы неутешительны: высокотехнологичная концепция общественного уклада не даёт нам иного выбора, кроме определения позиций, занимаемых участниками в отношении поставленных задач. Таким образом, начало повседневной работы по формированию позиции предполагает независимые способы реализации распределения внутренних резервов и ресурсов. Мы вынуждены отталкиваться от того, что повышение уровня гражданского сознания предоставляет широкие возможности для новых предложений. Принимая во внимание показатели успешности, дальнейшее развитие различных форм деятельности играет важную роль в формировании модели развития.");
            BitArray gamma = await GetGamma(token, text.Length);
            text = XorMask(text, MaskCollection(gamma).GetEnumerator(), Encoding.GetEncoding(1251));
            await Console.ReadLine("Результат", token: token, defaultValue: text);
        }
        public string XorMask(string text, IEnumerator<byte> xorMaskData, Encoding encoding)
        {
            var bytes = encoding.GetBytes(text);
            var str = encoding.GetString(bytes).Select(x => (x, (int)x));
            for (int i = 0; i < bytes.Length; i++)
            {
                if (!xorMaskData.MoveNext()) return null;
                bytes[i] ^= xorMaskData.Current;
            }
            return encoding.GetString(bytes);
        }
        public string XorMask(string text, IEnumerator<byte> xorMaskData) => XorMask(text, xorMaskData, Encoding.Unicode);
    }
}
