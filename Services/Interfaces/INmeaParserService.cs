using PoligonMaui.Models;

namespace PoligonMaui.Services.Interfaces;

public interface INmeaParserService
{
    GpsPosition? ParseGprmc(string nmeaString);
    bool ValidateChecksum(string nmeaString);
    double ConvertDegreeMinutesToDecimal(string degreeMinutes, string hemisphere);
}