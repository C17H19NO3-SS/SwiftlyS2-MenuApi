namespace MenuApi.API;

public class MenuTheme
{
    private string _fontSizeClass = "fontSize-s";
    private string _titleFontSizeClass = "fontSize-sm";
    private int? _fontSizePx;
    private int? _titleFontSizePx;
    private int? _subtitleFontSizePx;

    /// <summary>
    /// Menü paneli ana font boyutu sınıfı: fontSize-xxs, fontSize-xs, fontSize-s, fontSize-sm, fontSize-m vb.
    /// Eğer değer "16px" veya saf sayı ("16") olarak girilirse otomatik olarak FontSizePx ayarlanır.
    /// </summary>
    public string FontSizeClass
    {
        get => _fontSizeClass;
        set
        {
            if (TryParsePx(value, out int px))
            {
                _fontSizePx = px;
            }
            _fontSizeClass = value;
        }
    }

    /// <summary>
    /// Menü seçenekleri ve içeriği için piksel cinsinden font boyutu (Örn: 14, 16, 18, 20).
    /// Tanımlandığında FontSizeClass yerine inline style (style="font-size: {px}px;") kullanılır.
    /// </summary>
    public int? FontSizePx
    {
        get => _fontSizePx;
        set => _fontSizePx = value;
    }

    /// <summary>
    /// Başlık font boyutu sınıfı: fontSize-xs, fontSize-s, fontSize-sm, fontSize-m, fontSize-l vb.
    /// Eğer değer "20px" veya saf sayı ("20") olarak girilirse otomatik olarak TitleFontSizePx ayarlanır.
    /// </summary>
    public string TitleFontSizeClass
    {
        get => _titleFontSizeClass;
        set
        {
            if (TryParsePx(value, out int px))
            {
                _titleFontSizePx = px;
            }
            _titleFontSizeClass = value;
        }
    }

    /// <summary>
    /// Başlık için piksel cinsinden font boyutu (Örn: 18, 20, 24).
    /// Tanımlandığında TitleFontSizeClass yerine inline style (style="font-size: {px}px;") kullanılır.
    /// </summary>
    public int? TitleFontSizePx
    {
        get => _titleFontSizePx;
        set => _titleFontSizePx = value;
    }

    /// <summary>
    /// Alt başlık için piksel cinsinden font boyutu (Örn: 12, 14).
    /// </summary>
    public int? SubtitleFontSizePx
    {
        get => _subtitleFontSizePx;
        set => _subtitleFontSizePx = value;
    }

    /// <summary>
    /// Öğe ve metin font boyutu HTML niteliğini döner. CS2 Panorama sınıfı ve inline stili birlikte uygular.
    /// </summary>
    public string GetItemFontAttribute()
    {
        if (_fontSizePx.HasValue && _fontSizePx.Value > 0)
        {
            string cls = MapPxToClass(_fontSizePx.Value);
            return $"class=\"{cls}\" style=\"font-size: {_fontSizePx.Value}px;\"";
        }
        return $"class=\"{_fontSizeClass}\"";
    }

    /// <summary>
    /// Başlık font boyutu HTML niteliğini döner. CS2 Panorama sınıfı ve inline stili birlikte uygular.
    /// </summary>
    public string GetTitleFontAttribute()
    {
        if (_titleFontSizePx.HasValue && _titleFontSizePx.Value > 0)
        {
            string cls = MapPxToClass(_titleFontSizePx.Value);
            return $"class=\"{cls}\" style=\"font-size: {_titleFontSizePx.Value}px;\"";
        }
        return $"class=\"{_titleFontSizeClass}\"";
    }

    /// <summary>
    /// Alt başlık font boyutu HTML niteliğini döner.
    /// </summary>
    public string GetSubtitleFontAttribute()
    {
        if (_subtitleFontSizePx.HasValue && _subtitleFontSizePx.Value > 0)
        {
            string cls = MapPxToClass(_subtitleFontSizePx.Value);
            return $"class=\"{cls}\" style=\"font-size: {_subtitleFontSizePx.Value}px;\"";
        }
        return "class=\"fontSize-xs\"";
    }

    public static string MapPxToClass(int px)
    {
        return px switch
        {
            <= 7 => "fontSize-xxs",
            <= 10 => "fontSize-xs",
            <= 13 => "fontSize-s",
            <= 16 => "fontSize-sm",
            <= 20 => "fontSize-m",
            <= 28 => "fontSize-l",
            <= 36 => "fontSize-xl",
            _ => "fontSize-xxl"
        };
    }

    public MenuTheme SetFontSize(int px)
    {
        FontSizePx = px;
        return this;
    }

    public MenuTheme SetFontSize(string sizeOrClass)
    {
        FontSizeClass = sizeOrClass;
        return this;
    }

    public MenuTheme SetTitleFontSize(int px)
    {
        TitleFontSizePx = px;
        return this;
    }

    public MenuTheme SetTitleFontSize(string sizeOrClass)
    {
        TitleFontSizeClass = sizeOrClass;
        return this;
    }

    public MenuTheme SetFontSizePx(int px)
    {
        FontSizePx = px;
        return this;
    }

    public MenuTheme SetTitleFontSizePx(int px)
    {
        TitleFontSizePx = px;
        return this;
    }

    private static bool TryParsePx(string? value, out int px)
    {
        px = 0;
        if (string.IsNullOrWhiteSpace(value)) return false;
        string trimmed = value.Trim().ToLowerInvariant();
        if (trimmed.EndsWith("px"))
        {
            trimmed = trimmed[..^2].Trim();
        }
        return int.TryParse(trimmed, out px) && px > 0;
    }

    /// <summary>
    /// Başlık rengi (HEX / HTML color)
    /// </summary>
    public string TitleColor { get; set; } = "#FFB000";

    /// <summary>
    /// Başlık parantez / çerçeve karakterleri. [ ve ]
    /// </summary>
    public string TitlePrefix { get; set; } = "[ ";
    public string TitleSuffix { get; set; } = " ]";

    /// <summary>
    /// Alt başlık rengi
    /// </summary>
    public string SubtitleColor { get; set; } = "#B0C4DE";

    /// <summary>
    /// Seçeneklerin slot numarası rengi (Örn: 1. 2.)
    /// </summary>
    public string SlotNumberColor { get; set; } = "#FFB000";

    /// <summary>
    /// Aktif / seçilebilir öğelerin metin rengi
    /// </summary>
    public string ItemTextColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// Devre dışı / kilitli öğelerin rengi
    /// </summary>
    public string DisabledTextColor { get; set; } = "#777777";

    /// <summary>
    /// Bilgi metni ve ayraçların rengi
    /// </summary>
    public string InfoTextColor { get; set; } = "#888888";

    /// <summary>
    /// Sayfa numarası bilgi metni rengi (Sayfa 1/2)
    /// </summary>
    public string PageInfoColor { get; set; } = "#888888";

    /// <summary>
    /// 8. Geri butonu metni ve renkleri
    /// </summary>
    public string BackText { get; set; } = "Geri";
    public string BackActiveColor { get; set; } = "#FFFFFF";
    public string BackDisabledColor { get; set; } = "#888888";

    /// <summary>
    /// 9. İleri butonu metni ve renkleri
    /// </summary>
    public string NextText { get; set; } = "Ileri";
    public string NextActiveColor { get; set; } = "#FFFFFF";
    public string NextDisabledColor { get; set; } = "#888888";

    /// <summary>
    /// 0. Çıkış butonu metni ve rengi
    /// </summary>
    public string ExitText { get; set; } = "Cikis";
    public string ExitColor { get; set; } = "#FFFFFF";
    public string ExitSlotColor { get; set; } = "#FFB000";

    /// <summary>
    /// Varsayılan altın sarısı nostaljik tema
    /// </summary>
    public static MenuTheme Default => new();

    /// <summary>
    /// Mavi / Buz teması
    /// </summary>
    public static MenuTheme Blue => new()
    {
        TitleColor = "#00BFFF",
        SlotNumberColor = "#1E90FF",
        ExitSlotColor = "#1E90FF"
    };

    /// <summary>
    /// Yeşil / Askeri tema
    /// </summary>
    public static MenuTheme Green => new()
    {
        TitleColor = "#00FF7F",
        SlotNumberColor = "#32CD32",
        ExitSlotColor = "#32CD32"
    };

    /// <summary>
    /// Kırmızı / Ateşli tema
    /// </summary>
    public static MenuTheme Red => new()
    {
        TitleColor = "#FF4500",
        SlotNumberColor = "#FF6347",
        ExitSlotColor = "#FF6347"
    };

    /// <summary>
    /// Mor / VIP teması
    /// </summary>
    public static MenuTheme Purple => new()
    {
        TitleColor = "#DA70D6",
        SlotNumberColor = "#BA55D3",
        ExitSlotColor = "#BA55D3"
    };
}

