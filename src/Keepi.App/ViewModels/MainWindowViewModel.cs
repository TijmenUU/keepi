using Keepi.App.Services;

namespace Keepi.App.ViewModels;

public partial class MainWindowViewModel(ITemperatureService temperatureService) : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";

    private string celcius = "";
    public string Celcius
    {
        get => celcius;
        set
        {
            if (celcius != value)
            {
                celcius = value;
                OnPropertyChanged();
                OnPropertyChanged(propertyName: nameof(Fahrenheit));
            }
        }
    }

    public string Fahrenheit
    {
        get
        {
            if (double.TryParse(celcius, out var degreesCelcius))
            {
                return temperatureService.CelciusToFahrenheit(degreesCelcius).ToString("0.0");
            }

            return "";
        }
    }
}
