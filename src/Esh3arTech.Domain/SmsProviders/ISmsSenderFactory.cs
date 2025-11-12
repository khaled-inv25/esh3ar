namespace Esh3arTech.SmsProviders
{
    public interface ISmsSenderFactory
    {
        ISmsSender Create(string provider);
    }
}
