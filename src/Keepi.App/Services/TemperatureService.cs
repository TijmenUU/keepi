namespace Keepi.App.Services;

public interface ITemperatureService
{
    double CelciusToFahrenheit(double degreesCelcius);
}

internal sealed class TemperatureService : ITemperatureService
{
    public double CelciusToFahrenheit(double degreesCelcius) => degreesCelcius * (9.0 / 5.0) + 32.0;
}
