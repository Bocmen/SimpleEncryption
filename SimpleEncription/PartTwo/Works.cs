using ConsoleLibrary.ConsoleExtensions;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CoreRandomGenerators.Extension;

namespace SimpleEncription.PartTwo
{
    [WorkInvoker.Attributes.LoaderWorkBase("Шифрование константой", "", Const.NameGroupKeyEncription)]
    public class ConstWork : Abstract.GammaEncryption
    {
        public override async Task<BitArray> GetGamma(CancellationToken token, int textLength)
        {
            ushort value = (ushort)(await Console.ReadULong("Введите 16 битную константу", token: token, defaultValue: 10, endRange: ushort.MaxValue, startRange: ushort.MinValue));
            return new BitArray(new byte[] { (byte)(value >> 8), (byte)value });
        }
    }
    [WorkInvoker.Attributes.LoaderWorkBase("Шифрование LFSR генератором", "", Const.NameGroupKeyEncription)]
    public class LFSRWork : Abstract.GammaEncryption
    {
        public override Task<BitArray> GetGamma(CancellationToken token, int textLength)
        {
            var randomGenerator = new CoreRandomGenerators.LFSR();
            byte[] buffer = new byte[textLength];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = randomGenerator.NextByte();
            return Task.FromResult(new BitArray(buffer));
        }
    }
    [WorkInvoker.Attributes.LoaderWorkBase("Шифрование текстом", "", Const.NameGroupKeyEncription)]
    public class TextWork : Abstract.GammaEncryption
    {
        public override async Task<BitArray> GetGamma(CancellationToken token, int countBit) => new BitArray(Encoding.UTF8.GetBytes(await Console.ReadLine("Введите ключ-текст", token: token, defaultValue: "Testtetststkjhdksfhsd;f")));
    }
}
