namespace SimpleEncription.PartOne
{
    [WorkInvoker.Attributes.LoaderWorkBase("Шифрование", "", Const.NameGroupRouteTransposition)]
    public class Encryption : Abstract.RouteTransposition
    {
        protected override RouteTranspositionType Type => RouteTranspositionType.Encryption;
    }
    [WorkInvoker.Attributes.LoaderWorkBase("Дешифрование", "", Const.NameGroupRouteTransposition)]
    public class Decryption : Abstract.RouteTransposition
    {
        protected override RouteTranspositionType Type => RouteTranspositionType.Decryption;
    }
}
