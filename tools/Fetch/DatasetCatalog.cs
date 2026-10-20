namespace Fetch;

public sealed record DatasetDefinition(string Code, string Id, string Slug, string Label, bool IsReserve);

// Datasets from prompt 02. Reserves are used in this order when a main dataset is rejected.
public static class DatasetCatalog
{
    public static readonly IReadOnlyList<DatasetDefinition> Main =
    [
        new("s01", "a53f258b2c21f503a1bea267cc912bb033db0d0db90ee1483f66323771634127", "metrorower-popularne-polaczenia", "Metrorower: popularne połączenia", false),
        new("s02", "b161e77ded1548e6044a0f52796df030fe96755c3e1470e49d8524084682d464", "metrorower-stacje", "Metrorower: stacje aktualne", false),
        new("s03", "4e4f0a8bdce9bb2857c6ef27eceb7a8308127b738dfd7c51decc88287d4b9084", "rowery-dostepnosc-gminy", "Dzienna dostępność rowerów w gminach", false),
        new("s04", "dea28113f214eb0fb3fc9fcc9ec149f6cd5f61b7382e422645bd2fb52a60c67b", "bytom-rzeczy-znalezione", "Bytom: rzeczy znalezione", false),
        new("s05", "565dc7a79264b260f100cd7e07e40a06e5bd073b5d0721e8ea6619e721178b8c", "dabrowa-rzeczy-znalezione", "Dąbrowa Górnicza: rzeczy znalezione", false),
        new("s06", "498f49f43a2b0c7b39c26ee824eae5bcdb95c1001277727318ffe297ba02f7d1", "dabrowa-pomniki-przyrody", "Dąbrowa Górnicza: pomniki przyrody", false),
        new("s07", "e0f4337467ef2f60fc31f553213fe5283db5070d2b896df5c999bc988d12f2fb", "siewierz-wydarzenia", "Siewierz: wydarzenia", false),
        new("s08", "ae2be0e5ed4ec1863d7cb72e2397cd98805667e1acc6aaf975ae411d3056b9f7", "ck-victoria-wydarzenia", "CK Victoria: wydarzenia", false),
        new("s09", "09524009d1dd5bc2f80c409d8849c8edc3222c8deda8188963cab53456eb4180", "pok-wydarzenia", "Przystanek Otwartej Kultury: wydarzenia", false),
        new("s10", "05bc52c0eed46e917739a13ee88cc94f45a672c3ffaa9ef5bc88f4ad6bdb660b", "gliwice-boiska-stadiony", "Gliwice: boiska i stadiony", false),
        new("s11", "1def0df4cbc60ff042636848df79479a71a3b87247138be2615f72499323ac64", "katowice-miejsca-pamieci", "Katowice: miejsca pamięci narodowej", false),
        new("s12", "77714e3bb8cad342bc54cfe6d77c478c4d2141b97aefc1cb6df6cc0719ce52a3", "bytom-zabytki", "Bytom: zabytki", false),
        new("s13", "1ea36ecce32f5766c78654f9cd1358a75830a7799d8a002fc251bec6b7359f47", "sck-zajecia-sekcyjne", "SCK Siemianowice: zajęcia sekcyjne", false),
        new("s14", "lista-automatow-biletowych", "ztm-automaty-biletowe", "ZTM: automaty biletowe", false),
        new("s15", "ed990d78435a34a373b870f349aad5b56987d961840a5ecf8062bdb1bd7fcc53", "gliwice-kina-teatry", "Gliwice: kina i teatry", false),
    ];

    public static readonly IReadOnlyList<DatasetDefinition> Reserves =
    [
        new("R1", "6d8bcaf6e7076ea75fa5006178bb6f9044ef90573bb0ba0cfe87269c6bee5023", "gliwice-muzea", "Gliwice: muzea", true),
        new("R2", "b78bdf8a194b381025a5c2f4a79b9daa01d335f107519145604fb35412178299", "zabrze-miejsca-pamieci", "Zabrze: miejsca pamięci", true),
        new("R3", "73ad234d3f98dc1c34a87474bb54446510ac7c104fbb9e6e02e6a63fe3b1f945", "gliwice-biblioteki", "Gliwice: biblioteki", true),
        new("R4", "f310c9b971378c90071de753fc47e60d0b7b7fa52a9a0408bc5937c9373f6bd3", "gliwice-obiekty-sportowe", "Gliwice: obiekty sportowe", true),
        new("R5", "27d74bc7362cbcb3aeaf9b41b3635cc331c227cf122431be9a6aab11e29ac975", "metrorower-rezerwacje", "Metrorower: rezerwacje", true),
    ];
}
