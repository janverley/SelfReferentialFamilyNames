using System.Net.Http.Json;
using System.Text.Json;

var urlForGemeentes = "https://api.basisregisters.vlaanderen.be/v1/gemeenten";

using var c = new HttpClient();
var res = await c.GetAsync(urlForGemeentes);

var result = await res.Content.ReadFromJsonAsync<Root>();

using var c2 = new HttpClient();
c2.DefaultRequestHeaders.Referrer = new Uri("https://familienaam.be/Peeters"); // for authentication 

foreach (var gemeente in result.gemeenten.Where(g => g.gemeenteStatus == "inGebruik"))
{
    var gemeenteNaam = gemeente.gemeentenaam.geografischeNaam.spelling;

    var urlForFamilyNames = $"http://familienaam.be/api/2008/Van_{gemeenteNaam}";

    var responseMessage = await c2.GetAsync(urlForFamilyNames);

    var responseContentString = await responseMessage.Content.ReadAsStringAsync();

         
    if (!responseContentString.Contains("\"d\":[]")) // ugly : polymorphic json
    {
        var naamData = JsonSerializer.Deserialize<FamillieNaamData>(responseContentString);

        if (naamData != null && naamData.d.ContainsKey(gemeenteNaam))
        {
            Console.WriteLine($"In {gemeenteNaam} wonen {naamData.d[gemeenteNaam][0]} mensen die \"Van {gemeenteNaam}\" heten");
        }
    }
}

public class FamillieNaamData
{
    public int y { get; set; }
    public bool o { get; set; }
    public Dictionary<string, List<int>> d { get; set; }
}